using avaness.RacingMod.Race;
using Sandbox.ModAPI;
using System;
using VRage;
using System.Linq;
using VRage.Game.ModAPI;
using System.Collections.Generic;

namespace avaness.RacingMod.API
{
    public class APILogic
    {
        private readonly Track race;
        private readonly List<IMyPlayer> playersTemp = new List<IMyPlayer>();

        public APILogic(Track race)
        {
            if(RacingConstants.IsServer)
            {
                this.race = race;
                MyAPIGateway.Utilities.SendModMessage(RacingConstants.ModMessageId,
                    new MyTuple<Func<MyTuple<ulong, TimeSpan>[]>, Func<ulong[]>, Func<ulong, bool, bool>, Func<IMyPlayer, bool, bool>>
                    (GetFinishers, GetActiveRacers, RequestJoin, RequestJoin));
                race.Finishers.OnFinishersModified += Finishers_OnFinishersModified;
            }
        }

        public void Unload()
        {
            if(race != null)
                race.Finishers.OnFinishersModified -= Finishers_OnFinishersModified;
        }

        private void SendEvent(int id)
        {
            MyAPIGateway.Utilities.SendModMessage(RacingConstants.ModMessageId, id);
        }

        private void Finishers_OnFinishersModified()
        {
            SendEvent(0);
        }

        private MyTuple<ulong, TimeSpan>[] GetFinishers()
        {
            return race.Finishers.Select(x => new MyTuple<ulong, TimeSpan>(x.Id, x.BestTime)).ToArray();
        }

        private ulong[] GetActiveRacers()
        {
            MyAPIGateway.Players.GetPlayers(playersTemp, p => race.Contains(p.SteamUserId));
            ulong[] players = playersTemp.Select(p => p.SteamUserId).ToArray();
            playersTemp.Clear();
            return players;
        }

        private bool RequestJoin(ulong id, bool force)
        {
            IMyPlayer p = RacingTools.GetPlayer(id);
            if (p != null)
                return RequestJoin(p, force);
            return false;
        }

        private bool RequestJoin(IMyPlayer p, bool force)
        {
            return race.JoinRace(p, force);
        }
    }
}
