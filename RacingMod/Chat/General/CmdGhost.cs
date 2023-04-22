using avaness.RacingMod.Race;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRage.Game.ModAPI;

namespace avaness.RacingMod.Chat.General
{
    public class CmdGhost : ChatCommand
    {
        public override string Usage => ": Manage your race ghost.";

        public override string Id => "ghost";

        protected override void Execute(IMyPlayer p, bool admin, string[] cmd, Track race)
        {
            switch (cmd[2])
            {
                case "record":
                    break;
                case "play":
                    break;
                case "stop":
                    break;
                default:
                    ShowChatMsg(p, "Options: record, play, stop");
                    break;
            }
        }

        protected override bool ValidateLength(int len, bool admin)
        {
            return len > 2;
        }
    }
}
