using avaness.RacingMod.Race;
using VRage.Game.ModAPI;

namespace avaness.RacingMod.Chat.General
{
    public class CmdLeave : ChatCommand
    {
        public override string Id => "leave";

        public override string Usage => ": Leave the race.";

        protected override void Execute(IMyPlayer p, bool admin, string[] cmd, Track race)
        {
            race.LeaveRace(p);
        }
    }
}
