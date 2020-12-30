using Sandbox.ModAPI;
using System;
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
        /// Call this method to cleanup the mod once you are done with it.
        /// </summary>
        public void Close()
        {
            MyAPIGateway.Utilities.UnregisterMessageHandler(MessageId, RecieveData);
            OnFinishersModified = null;
            finishers = null;
            racers = null;
            joinRace = null;
            joinRace2 = null;
            onEnabled = null;
        }

        private void RecieveData(object obj)
        {
            if (!Enabled && obj is MyTuple<Func<MyTuple<ulong, TimeSpan>[]>, Func<ulong[]>, Func<ulong, bool, bool>, Func<IMyPlayer, bool, bool>>)
            {
                // Initialization
                var funcs = (MyTuple<Func<MyTuple<ulong, TimeSpan>[]>, Func<ulong[]>, Func<ulong, bool, bool>, Func<IMyPlayer, bool, bool>>)obj;
                finishers = funcs.Item1;
                racers = funcs.Item2;
                joinRace = funcs.Item3;
                joinRace2 = funcs.Item4;
                Enabled = true;
                if (onEnabled != null)
                {
                    onEnabled.Invoke();
                    onEnabled = null;
                }
            }
            else if (Enabled && obj is int)
            {
                // Events
                int i = (int)obj;
                if (i == 0)
                {
                    if (OnFinishersModified != null)
                        OnFinishersModified.Invoke(finishers());
                }
            }
        }

        private Func<MyTuple<ulong, TimeSpan>[]> finishers;
        /// <summary>
        /// Returns a list of players that have finished the race, in order by rank.
        /// Each player consists of an id and a track finish time.
        /// </summary>
        public MyTuple<ulong, TimeSpan>[] Finishers
        {
            get
            {
                if (Enabled)
                    return finishers();
                return null;
            }
        }

        /// <summary>
        /// This event will be called when the display of finishers is modified.
        /// Each player consists of an id and a track finish time.
        /// Make sure to unsubscribe from the event when you are done with it!
        /// </summary>
        public event Action<MyTuple<ulong, TimeSpan>[]> OnFinishersModified;

        private Func<ulong[]> racers;
        /// <summary>
        /// Returns a list of players that are currently in the race, in order by rank.
        /// </summary>
        public ulong[] Racers
        {
            get
            {
                if (Enabled)
                    return racers();
                return null;
            }
        }

        private Func<ulong, bool, bool> joinRace;
        /// <summary>
        /// Requests that a player join the race.
        /// </summary>
        /// <param name="id">The id of the player.</param>
        /// <param name="force">Force the player to join the race even if they are in the middle of the track.</param>
        /// <returns>True if the player is now in the race.</returns>
        public bool JoinRace(ulong id, bool force = false)
        {
            if (Enabled)
                return joinRace(id, force);
            return false;
        }

        private Func<IMyPlayer, bool, bool> joinRace2;
        /// <summary>
        /// Requests that a player join the race.
        /// </summary>
        /// <param name="p">The player.</param>
        /// <param name="force">Force the player to join the race even if they are in the middle of the track.</param>
        /// <returns>True if the player is now in the race.</returns>
        public bool JoinRace(IMyPlayer p, bool force = false)
        {
            if (Enabled)
                return joinRace2(p, force);
            return false;
        }
    }
}
