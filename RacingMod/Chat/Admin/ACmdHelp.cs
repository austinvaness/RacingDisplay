using avaness.RacingMod.Race;
using VRage.Game.ModAPI;

namespace avaness.RacingMod.Chat.Admin
{
    public class ACmdHelp : AdminCommand
    {
        public override string Id => "admin";

        public override string Usage => ": Show admin commands.";

        private readonly string msg;
        public ACmdHelp(string msg)
        {
            this.msg = msg;
        }

        protected override void ExecuteAdmin(IMyPlayer p, string[] cmd, Track track)
        {
            ShowChatMsg(p, msg);
        }

        protected override bool ValidateLength(int len)
        {
            return true;
        }
    }
}
