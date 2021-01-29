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

        public APILogic(Track race)
        {
            if(RacingConstants.IsServer)
            {
                this.race = race;
                MyAPIGateway.Utilities.SendModMessage(RacingConstants.ModMessageId,
                    new MyTuple<Func<IEnumerable<MyTuple<ulong, TimeSpan>>>, Func<IEnumerable<MyTuple<ulong, double>>>, Func<IMyPlayer, bool, bool>, Func<IMyPlayer, bool>>
                    (GetFinishers, GetActiveRacers, RequestJoin, RequestLeave));
            }
        }

        public void Unload()
        {

        }

        private IEnumerable<MyTuple<ulong, TimeSpan>> GetFinishers()
        {
            return race.Finishers.Select(x => new MyTuple<ulong, TimeSpan>(x.Id, x.BestTime));
        }

        private IEnumerable<MyTuple<ulong, double>> GetActiveRacers()
        {
            return race.GetListedRacers().Select(p => new MyTuple<ulong, double>(p.Id, p.Distance));
        }

        private bool RequestJoin(IMyPlayer p, bool force)
        {
            return race.JoinRace(p, force);
        }

        private bool RequestLeave(IMyPlayer p)
        {
            return race.LeaveRace(p, false);
        }

        public void SendEvent(ulong id, RacingDisplayAPI.PlayerEvent pEvent)
        {
            MyAPIGateway.Utilities.SendModMessage(RacingConstants.ModMessageId, new MyTuple<ulong, int>(id, (int)pEvent));
        }
    }
}
