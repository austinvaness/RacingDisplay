using Sandbox.Game;
using System;
using System.Text;
using VRage.Game.ModAPI;

namespace avaness.RacingLeaderboard.Chat
{
    public abstract class ChatCommand
    {
        private const int messageMs = 4000;

        public abstract string Id { get; }
        public abstract string Usage { get; }
        public virtual string AdminUsage => Usage;
        
        public void Run(IMyPlayer p, bool admin, string[] cmd)
        {
            if (ValidateLength(cmd.Length, admin))
            {
                Execute(p, admin, cmd);
            }
            else
            {
                string usage = admin ? AdminUsage : Usage;
                if(usage.Length > 0 && usage[0] == ':')
                    ShowChatMsg(p, $"Usage:\n{Commands.prefix} {Id}{usage}");
                ShowChatMsg(p, $"Usage:\n{Commands.prefix} {Id} {usage}");
            }
        }

        protected abstract void Execute(IMyPlayer p, bool admin, string[] cmd);

        protected virtual bool ValidateLength(int len, bool admin)
        {
            return len == 2;
        }

        protected void ShowMsg(IMyPlayer p, string msg)
        {
            MyVisualScriptLogicProvider.ShowNotification(msg, messageMs, "White", p.IdentityId);
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
            return $"{Commands.prefix} {Id}";
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
