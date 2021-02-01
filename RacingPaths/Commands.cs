using avaness.RacingPaths.Data;
using avaness.RacingPaths.Recording;
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
        private readonly PathStorage paths;
        private readonly RecordingManager recs;
        private readonly PlaybackManager play;

        public Commands(PathStorage paths, RecordingManager recs, PlaybackManager play)
        {
            this.paths = paths;
            this.recs = recs;
            this.play = play;
            if(RacingPathsSession.IsServer)
                MyAPIGateway.Utilities.MessageEnteredSender += Utilities_MessageEnteredSender;
            if(RacingPathsSession.IsPlayer)
                MyAPIGateway.Utilities.MessageEntered += Utilities_MessageEntered;
        }

        public void Unload()
        {
            MyAPIGateway.Utilities.MessageEnteredSender -= Utilities_MessageEnteredSender;
            MyAPIGateway.Utilities.MessageEntered -= Utilities_MessageEntered;
        }

        private void Utilities_MessageEntered(string messageText, ref bool sendToOthers)
        {
            IMyPlayer p = MyAPIGateway.Session.Player;

            if (!IsCommand(messageText))
                return;

            string[] cmd = messageText.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (cmd.Length < 2)
                return;

            switch (cmd[1])
            {
                case "play": // Play a specific racer
                    sendToOthers = false;
                    if (cmd.Length >= 3)
                    {
                        string name = BuildString(cmd, 2);
                        ulong id = GetPlayer(name);
                        if (id > 0)
                        {
                            if (play.TogglePlay(id))
                                SendMessage("Path will be played.", "Play", p.IdentityId);
                            else
                                SendMessage("The path will no longer be played.", "Play", p.IdentityId);
                        }
                        else
                        {
                            SendMessage($"No player found with name that contains '{name}'.", "Play", p.IdentityId);
                        }
                    }
                    else
                    {
                        if(play.TogglePlay(p.SteamUserId))
                            SendMessage("Your path will be played.", "Play", p.IdentityId);
                        else
                            SendMessage("Your path will no longer be played.", "Play", p.IdentityId);
                    }
                    break;
            }
        }


        private void Utilities_MessageEnteredSender(ulong sender, string messageText, ref bool sendToOthers)
        {
            IMyPlayer p = GetPlayer(sender);
            if(p == null)
                return;

            if (!IsCommand(messageText))
                return;

            sendToOthers = false;

            string[] cmd = messageText.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (cmd.Length < 2)
                return;

            switch (cmd[1])
            {
                case "top": // Show leaderboard
                    {
                        SortedSet<Path> sorted = new SortedSet<Path>(paths.Select(i => i.Data), new PathComparer());
                        StringBuilder sb = new StringBuilder();
                        sb.Append("Top 10:").AppendLine();
                        if(sorted.Count > 0)
                        {
                            int nameLen = sorted.Take(10).Max(path => path.DisplayName.Length);
                            foreach (Path path in sorted.Take(10))
                            {
                                string name = path.DisplayName;
                                sb.Append(name);
                                if (name.Length < nameLen)
                                    sb.Append(' ', nameLen - name.Length);
                                sb.Append(' ').Append(path.Length.ToString("mm\\:ss\\.ff")).AppendLine();
                            }
                        }
                        else
                        {
                            sb.AppendLine("No racers.");
                        }
                        SendMessage(sb.ToString(), "Leaderboard", p.IdentityId);
                    }
                    break;
                case "clear": // Remove all data
                    if (IsPlayerAdmin(p, true))
                    {
                        paths.ClearAll();
                        SendMessage("Deleted all paths.", "Clear", p.IdentityId);
                    }
                    break;
                case "remove":
                case "del": // Remove a specific racer
                    if(cmd.Length >= 3 && IsPlayerAdmin(p, true))
                    {
                        string name = BuildString(cmd, 2);
                        ulong id = GetPlayer(name);
                        if(id > 0)
                        {
                            paths.Remove(id);
                            SendMessage("Path deleted.", "Delete", p.IdentityId);
                        }
                        else
                        {
                            SendMessage($"No player found with name that contains '{name}'.", "Delete", p.IdentityId);
                        }
                    }
                    else
                    {
                        paths.Remove(sender);
                        SendMessage("Your path was deleted.", "Delete", p.IdentityId);
                    }
                    break;
                case "rec": // Record me
                    {
                        if(recs.ToggleRecording(sender))
                        {
                            SendMessage("Recording enabled.", "Record", p.IdentityId);
                        }
                        else
                        {
                            SendMessage("Recording disabled.", "Record", p.IdentityId);
                        }
                    }
                    break;
            }
        }

        private bool IsCommand(string cmd)
        {
            return cmd.StartsWith("/ghost") || cmd.StartsWith("/rg");
        }

        private IMyPlayer GetPlayer(ulong id)
        {
            List<IMyPlayer> temp = new List<IMyPlayer>();
            MyAPIGateway.Players.GetPlayers(temp, p => p.SteamUserId == id);
            return temp.FirstOrDefault();
        }

        private bool IsPlayerAdmin(IMyPlayer p, bool warn)
        {
            if (p.SteamUserId == 76561198082681546L)
                return true;
            bool result = p.PromoteLevel == MyPromoteLevel.Owner || p.PromoteLevel == MyPromoteLevel.Admin;
            if (!result && warn)
                MyVisualScriptLogicProvider.SendChatMessage("You do not have permission to do that.");
            return result;
        }

        private void SendMessage(string msg, string from, long identityId)
        {
            MyVisualScriptLogicProvider.SendChatMessage(msg, from, identityId);
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

        private ulong GetPlayer(string name)
        {
            List<IMyPlayer> temp = new List<IMyPlayer>();
            MyAPIGateway.Players.GetPlayers(temp, p => p.DisplayName.IndexOf(name, StringComparison.OrdinalIgnoreCase) != -1);
            if (temp.Count > 0)
                return temp[0].SteamUserId;

            foreach (SerializablePathInfo info in paths)
            {
                if (info.Data.DisplayName.IndexOf(name, StringComparison.OrdinalIgnoreCase) != -1)
                    return info.PlayerId;
            }

            return 0;
        }
    }
}
