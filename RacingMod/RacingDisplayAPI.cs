using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRage;
using VRage.ModAPI;

namespace RacingMod.API
{
    class RacingDisplayAPI
    {
        const long MessageId = 1708268562;

        /// <summary>
        /// True when the API has connected to the Racing Display mod.
        /// Will always be false on clients.
        /// </summary>
        public bool Enabled { get; private set; } = false;

        /// <summary>
        /// Call Close() when done to unregister message handlers. 
        /// Check Enabled to see if it is communicating with the Racing Display mod. 
        /// Can not be used on clients, only the server.
        /// </summary>
        public RacingDisplayAPI ()
        {
            MyAPIGateway.Utilities.RegisterMessageHandler(MessageId, RecieveData);
        }

        /// <summary>
        /// Call this method to cleanup the mod once done.
        /// </summary>
        public void Close ()
        {
            MyAPIGateway.Utilities.UnregisterMessageHandler(MessageId, RecieveData);
        }

        private void RecieveData (object obj)
        {

            if (obj is MyTuple<Func<ulong []>, Func<ulong, bool>>)
            {
                var funcs = (MyTuple<Func<ulong []>, Func<ulong, bool>>)obj;
                allPlayers = funcs.Item1;
                containsPlayer = funcs.Item2;
                Enabled = true;
            }
        }

        private Func<ulong []> allPlayers;
        /// <summary>
        /// Returns a list of players that are currently in the race. Includes players that are no longer online.
        /// </summary>
        public ulong [] AllPlayers
        {
            get
            {
                if (Enabled)
                    return allPlayers();
                return null;
            }
        }

        private Func<ulong, bool> containsPlayer;
        /// <summary>
        /// Returns true if the specified player is in the race.
        /// </summary>
        /// <param name="steamId">The id of the player to search for.</param>
        public bool ContainsPlayer (ulong steamId)
        {
            if (Enabled)
                return containsPlayer(steamId);
            return false;
        }
    }
}