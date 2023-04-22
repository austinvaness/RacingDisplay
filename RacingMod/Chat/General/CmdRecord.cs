using avaness.RacingMod.Race;
using VRage.Game.ModAPI;

namespace avaness.RacingMod.Chat.General
{
    public class CmdRecord : ChatCommand
    {
        public override string Id => "record";

        public override string Usage => ": (obsolete)";

        public override bool Hidden => true;

        protected override void Execute(IMyPlayer p, bool admin, string[] cmd, Track race)
        {
            ShowChatMsg(p, "Use '/race ghost' instead.");
        }
    }
}
