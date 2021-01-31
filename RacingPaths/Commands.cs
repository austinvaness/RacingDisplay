using avaness.RacingPaths.Data;
using avaness.RacingPaths.Storage;
using Sandbox.Game;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VRage.Game.ModAPI;
using VRage.Utils;

namespace avaness.RacingPaths
{
    public class Commands
    {
        private PathStorage paths;

        public Commands(PathStorage paths)
        {
            this.paths = paths;
            MyAPIGateway.Utilities.MessageEnteredSender += Utilities_MessageEnteredSender;
        }

        public void Unload()
        {
            MyAPIGateway.Utilities.MessageEnteredSender -= Utilities_MessageEnteredSender;
        }

        private void Utilities_MessageEnteredSender(ulong sender, string messageText, ref bool sendToOthers)
        {
            long identityId = MyAPIGateway.Players.TryGetIdentityId(sender);
            if (identityId == 0)
                return;

            if (!messageText.StartsWith("/ghost") && !messageText.StartsWith("/rg"))
                return;

            sendToOthers = false;

            string[] cmd = messageText.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (cmd.Length < 2)
                return;

            switch (cmd[1])
            {
                case "top":
                    if(cmd.Length == 2)
                    {
                        SortedSet<Path> sorted = new SortedSet<Path>(paths.Select(i => i.Data), new PathComparer());
                        StringBuilder sb = new StringBuilder();
                        sb.Append("Top 10:").AppendLine();
                        if(sorted.Count > 0)
                        {
                            int nameLen = sorted.Take(10).Max(p => p.DisplayName.Length);
                            foreach (Path p in sorted.Take(10))
                            {
                                string name = p.DisplayName;
                                sb.Append(name);
                                if (name.Length < nameLen)
                                    sb.Append(' ', nameLen - name.Length);
                                sb.Append(' ').Append(p.Length.ToString("mm\\:ss\\.ff")).AppendLine();
                            }
                        }
                        else
                        {
                            sb.AppendLine("No racers.");
                        }
                        MyVisualScriptLogicProvider.SendChatMessage(sb.ToString(), "Leaderboard", identityId);
                    }
                    break;
                case "clear":
                    if(cmd.Length == 2)
                        paths.ClearAll();
                    break;
                case "remove":
                case "del":
                    if(cmd.Length >= 3)
                    {
                        string name = BuildString(cmd, 2);
                        IMyPlayer p = GetPlayer(name);
                        paths.Remove(p.SteamUserId);
                    }
                    break;
            }
        }

        private string BuildString(string[] cmd, int start)
        {
            if(start == cmd.Length - 1)
                return cmd[start];

            StringBuilder sb = new StringBuilder();
            sb.Append(cmd[start]);
            for(int i = start + 1; i < cmd.Length; i++)
                sb.Append(' ').Append(cmd[i]);
            return sb.ToString();
        }

        private IMyPlayer GetPlayer(string name)
        {
            List<IMyPlayer> temp = new List<IMyPlayer>();
            MyAPIGateway.Players.GetPlayers(temp, p => p.DisplayName.Contains(name));
            return temp.FirstOrDefault();
        }
    }
}
