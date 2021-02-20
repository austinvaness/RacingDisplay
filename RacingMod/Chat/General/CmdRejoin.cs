using avaness.RacingMod.Race;
using VRage.Game.ModAPI;

namespace avaness.RacingMod.Chat.General
{
    public class CmdRejoin : ChatCommand
    {
        public override string Id => "rejoin";

        public override string Usage => ": Shortcut to leave and join the race.";

        protected override void Execute(IMyPlayer p, bool admin, string[] cmd, Track race)
        {
            race.LeaveRace(p, false);
            race.JoinRace(p);
        }
    }
}
