using avaness.RacingMod.Race;
using System;
using VRage.Game.ModAPI;

namespace avaness.RacingMod.Chat.Admin
{
    public abstract class AdminCommand : ChatCommand
    {
        protected override void Execute(IMyPlayer p, bool admin, string[] cmd, Track race)
        {
            if (admin)
                ExecuteAdmin(p, cmd, race);
            else
                WarnPermission(p);
        }

        protected abstract void ExecuteAdmin(IMyPlayer p, string[] cmd, Track race);


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
