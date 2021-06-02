using System;
using VRage.Game.ModAPI;

namespace avaness.RacingLeaderboard.Chat.Admin
{
    public abstract class AdminCommand : ChatCommand
    {
        protected override void Execute(IMyPlayer p, bool admin, string[] cmd)
        {
            if (admin)
                ExecuteAdmin(p, cmd);
            else
                WarnPermission(p);
        }

        protected abstract void ExecuteAdmin(IMyPlayer p, string[] cmd);


        protected override bool ValidateLength(int len, bool admin)
        {
            return !admin || ValidateLength(len);
        }

        protected virtual bool ValidateLength(int len)
        {
            return len == 2;
        }
    }
}
