using avaness.RacingMod.Race;
using Sandbox.Game;
using System;
using System.Text;
using VRage.Game.ModAPI;

namespace avaness.RacingMod.Chat
{
    public abstract class ChatCommand
    {
        public abstract string Id { get; }
        public abstract string Usage { get; }
        public virtual string AdminUsage => Usage;
        
        public void Run(IMyPlayer p, bool admin, string[] cmd, Track race)
        {
            if (ValidateLength(cmd.Length, admin))
            {
                Execute(p, admin, cmd, race);
            }
            else
            {
                string usage = admin ? AdminUsage : Usage;
                if(usage.Length > 0 && usage[0] == ':')
                    ShowChatMsg(p, $"Usage:\n{RacingCommands.prefix} {Id}{usage}");
                ShowChatMsg(p, $"Usage:\n{RacingCommands.prefix} {Id} {usage}");
            }
        }

        protected abstract void Execute(IMyPlayer p, bool admin, string[] cmd, Track race);

        protected virtual bool ValidateLength(int len, bool admin)
        {
            return len == 2;
        }

        protected void ShowMsg(IMyPlayer p, string msg)
        {
            MyVisualScriptLogicProvider.ShowNotification(msg, RacingConstants.defaultMsgMs, "White", p.IdentityId);
        }

        protected void ShowChatMsg(IMyPlayer p, string msg)
        {
            MyVisualScriptLogicProvider.SendChatMessage(msg, "rcd", p.IdentityId, "Red");
        }

        protected void WarnPermission(IMyPlayer p)
        {
            ShowChatMsg(p, "You do not have permission to do that.");
        }

        public override string ToString()
        {
            return $"{RacingCommands.prefix} {Id}";
        }

        protected string BuildString(string[] args, int start)
        {
            if (start == args.Length - 1)
                return args[start];

            StringBuilder sb = new StringBuilder();
            for(int i = start; i < args.Length; i++)
            {
                if (i > start)
                    sb.Append(' ');
                sb.Append(args[i]);
            }
            return sb.ToString();
        }
    }
}
