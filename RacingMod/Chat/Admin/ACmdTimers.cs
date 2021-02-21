using avaness.RacingMod.Race;
using VRage.Game.ModAPI;

namespace avaness.RacingMod.Chat.Admin
{
    public class ACmdTimers : AdminCommand
    {
        public override string Id => "timers";

        public override string Usage => ": Triggers all timers prefixed with [racetimer].";

        protected override void ExecuteAdmin(IMyPlayer p, string[] cmd, Track race)
        {
            ShowChatMsg(p, $"Started {RacingSession.Instance.TriggerTimers()} timers.");
        }
    }
}
