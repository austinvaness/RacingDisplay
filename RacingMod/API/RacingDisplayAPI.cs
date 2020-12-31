using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using VRage;
using VRage.Game.ModAPI;

namespace avaness.RacingMod.API
{
    public class RacingDisplayAPI
    {
        const long MessageId = 1708268562;

        /// <summary>
        /// True when the API has connected to the Racing Display mod.
        /// Will always be false on clients.
        /// </summary>
        public bool Enabled { get; private set; } = false;
        private Action onEnabled;

        /// <summary>
        /// Call Close() when done to unregister message handlers. 
        /// Check Enabled to see if the API is communicating with the Racing Display mod. 
        /// Can not be used on clients, only the server.
        /// </summary>
        /// <param name="onEnabled">Called once the API has connected to the Racing Display mod.</param>
        public RacingDisplayAPI(Action onEnabled = null)
        {
            this.onEnabled = onEnabled;
            MyAPIGateway.Utilities.RegisterMessageHandler(MessageId, RecieveData);
        }

        /// <summary>
        /// Call this method to cleanup once you are done with it.
        /// </summary>
        public void Close()
        {
            MyAPIGateway.Utilities.UnregisterMessageHandler(MessageId, RecieveData);
            finishers = null;
            racers = null;
            joinRace = null;
            leaveRace = null;
            onEnabled = null;
        }

        private void RecieveData(object obj)
        {
            if (!Enabled && obj is MyTuple<Func<IEnumerable<MyTuple<ulong, TimeSpan>>>, Func<IEnumerable<MyTuple<ulong, double>>>, Func<IMyPlayer, bool, bool>, Func<IMyPlayer, bool>>)
            {
                // Initialization
                var funcs = (MyTuple<Func<IEnumerable<MyTuple<ulong, TimeSpan>>>, Func<IEnumerable<MyTuple<ulong, double>>>, Func<IMyPlayer, bool, bool>, Func<IMyPlayer, bool>>)obj;
                finishers = funcs.Item1;
                racers = funcs.Item2;
                joinRace = funcs.Item3;
                leaveRace = funcs.Item4;
                Enabled = true;
                if (onEnabled != null)
                {
                    onEnabled.Invoke();
                    onEnabled = null;
                }
            }
        }

        private Func<IEnumerable<MyTuple<ulong, TimeSpan>>> finishers;
        /// <summary>
        /// Returns a list of players that have finished the race, in order by rank.
        /// Each player consists of an id and a track finish time.
        /// </summary>
        public IEnumerable<MyTuple<ulong, TimeSpan>> Finishers
        {
            get
            {
                if (Enabled)
                    return finishers();
                return null;
            }
        }

        private Func<IEnumerable<MyTuple<ulong, double>>> racers;
        /// <summary>
        /// Returns a list of players that are currently in the race, in order by rank.
        /// Each player consists of an id and a distance along the track from the start.
        /// </summary>
        public IEnumerable<MyTuple<ulong, double>> Racers
        {
            get
            {
                if (Enabled)
                    return racers();
                return null;
            }
        }

        private Func<IMyPlayer, bool, bool> joinRace;
        /// <summary>
        /// Requests that a player join the race.
        /// </summary>
        /// <param name="id">The id of the player.</param>
        /// <param name="force">Force the player to join the race even if they are in the middle of the track.</param>
        /// <returns>True if the player is now in the race.</returns>
        public bool JoinRace(ulong id, bool force = false)
        {
            if(Enabled)
            {
                IMyPlayer p = GetPlayer(id);
                if(p != null)
                    return joinRace(p, force);
            }
            return false;
        }

        /// <summary>
        /// Requests that a player join the race.
        /// </summary>
        /// <param name="p">The player.</param>
        /// <param name="force">Force the player to join the race even if they are in the middle of the track.</param>
        /// <returns>True if the player is now in the race.</returns>
        public bool JoinRace(IMyPlayer p, bool force = false)
        {
            if (Enabled)
                return joinRace(p, force);
            return false;
        }

        private Func<IMyPlayer, bool> leaveRace;
        /// <summary>
        /// Requests that a player leave the race.
        /// </summary>
        /// <param name="id">The id of the player.</param>
        /// <returns>True if the player is no longer in the race.</returns>
        public bool LeaveRace(ulong id)
        {
            if(Enabled)
            {
                IMyPlayer p = GetPlayer(id);
                if (p != null)
                    return leaveRace(p);
            }
            return false;
        }

        /// <summary>
        /// Requests that a player leave the race.
        /// </summary>
        /// <param name="p">The player.</param>
        /// <returns>True if the player is no longer in the race.</returns>
        public bool LeaveRace(IMyPlayer p)
        {
            if (Enabled)
                return leaveRace(p);
            return false;
        }

        private IMyPlayer GetPlayer(ulong steamId)
        {
            if (steamId == 0)
                return null;
            List<IMyPlayer> temp = new List<IMyPlayer>(1);
            MyAPIGateway.Players.GetPlayers(temp, (p) => p.SteamUserId == steamId);
            return temp.FirstOrDefault();
        }
    }
}
