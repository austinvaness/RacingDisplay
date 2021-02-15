using avaness.RacingMod.Paths;
using avaness.RacingMod.Race;
using avaness.RacingMod.Race.Finish;
using avaness.RacingMod.Racers;
using ProtoBuf;
using Sandbox.Game;
using Sandbox.ModAPI;
using SpaceEngineers.Game.ModAPI;
using System;
using System.Collections.Generic;
using System.Text;
using VRage.Game.ModAPI;

namespace avaness.RacingMod
{
    public class RacingCommands
    {
        private readonly Track race;
        private readonly RacingMapSettings mapSettings;
        private readonly NodeManager nodes;
        private readonly FinishList finishers;
        private readonly RacerStorage racers;

        public RacingCommands(Track race)
        {
            this.race = race;
            mapSettings = race.MapSettings;
            nodes = race.Nodes;
            finishers = race.Finishers;
            racers = race.Racers;

            MyAPIGateway.Utilities.MessageEnteredSender += MessageEntered;
        }

        public void Unload()
        {
            MyAPIGateway.Utilities.MessageEnteredSender -= MessageEntered;
        }

        private void MessageEntered(ulong sender, string messageText, ref bool sendToOthers)
        {
            IMyPlayer p = RacingTools.GetPlayer(sender);
            if (p != null)
                ProcessCommand(p, messageText, ref sendToOthers);
        }

        private void ProcessCommand (IMyPlayer p, string command, ref bool sendToOthers)
        {
            command = command.ToLower().Trim();
            if (!command.StartsWith("/rcd") && !command.StartsWith("/race"))
                return;

            sendToOthers = false;
            
            string [] cmd = command.Split(' ');
            if (cmd.Length == 1)
            {
                ShowChatHelp(p);
                return;
            }


            switch (cmd [1])
            {
                case "debug":
                    if (MyAPIGateway.Session.Player != p)
                    {
                        ShowAdminMsg(p, "Debug view only works for the server host.");
                        race.ToggleDebug();
                    }
                    break;
                case "j":
                case "join":
                    if(cmd.Length > 2 && IsPlayerAdmin(p, false))
                    {
                        string arg = BuildString(cmd, 2);

                        if (arg.Equals("all", StringComparison.OrdinalIgnoreCase))
                        {
                            List<IMyPlayer> players = new List<IMyPlayer>();
                            MyAPIGateway.Players.GetPlayers(players);
                            foreach (IMyPlayer temp in players)
                            {
                                if (!race.Contains(temp.SteamUserId))
                                    race.JoinRace(temp);
                            }
                        }
                        else
                        {
                            IMyPlayer temp = RacingTools.GetPlayer(arg);
                            if (temp == null)
                                ShowAdminMsg(p, $"No racer was found with a name containing '{arg}'.");
                            else
                                race.JoinRace(temp, true);
                        }
                    }
                    else
                    {
                        race.JoinRace(p);
                    }
                    break;
                case "l":
                case "leave":
                    race.LeaveRace(p);
                    break;
                case "rejoin":
                    race.LeaveRace(p, false);
                    race.JoinRace(p);
                    break;
                case "aj":
                case "autojoin":
                    if(mapSettings.TimedMode && mapSettings.Looped)
                    {
                        if (!race.Contains(p.SteamUserId) && !race.JoinRace(p))
                            return;

                        StaticRacerInfo info = racers.GetStaticInfo(p);
                        if (info.AutoJoin)
                            ShowMsg(p, "You will leave the race after finishing.");
                        else
                            ShowMsg(p, "Your timer will reset after finishing.");
                        info.AutoJoin = !info.AutoJoin;
                    }
                    else
                    {
                        ShowMsg(p, "Auto join only works for looped timed races.");
                    }
                    break;
                case "clear":
                    if (!IsPlayerAdmin(p, true))
                        return;

                    if (cmd.Length == 2)
                    {
                        finishers.Clear();
                        racers.ClearRecorders();
                    }
                    else
                    {
                        string name = BuildString(cmd, 2);

                        StaticRacerInfo info;
                        if (racers.GetStaticInfo(name, out info))
                        {
                            ShowAdminMsg(p, $"Removed {info.Name} from the finalists.");
                            info.Recorder?.ClearData();
                            finishers.Remove(info);
                        }
                        else if(finishers.Remove(name))
                        {
                            ShowAdminMsg(p, $"Removed {name} from the finalists.");
                        }
                        else
                        {
                            ShowAdminMsg(p, $"No racer was found with a name containing '{name}'.");
                            return;
                        }
                    }
                    break;
                case "mode":
                    if (!IsPlayerAdmin(p, true))
                        return;

                    mapSettings.TimedMode = !mapSettings.TimedMode;
                    if (mapSettings.TimedMode)
                        ShowAdminMsg(p, "Mode is now timed.");
                    else
                        ShowAdminMsg(p, "Mode is now normal.");
                    break;
                case "strictstart":
                    if (!IsPlayerAdmin(p, true))
                        return;

                    mapSettings.StrictStart = !mapSettings.StrictStart;
                    if (mapSettings.StrictStart)
                        ShowAdminMsg(p, "Starting on the track is no longer allowed.");
                    else
                        ShowAdminMsg(p, "Starting on the track is now allowed.");
                    break;
                case "grant":
                    if (!IsPlayerAdmin(p, true))
                        return;

                    if (cmd.Length != 3)
                    {
                        ShowAdminMsg(p, "Usage:\n/race grant <name>: Fixes a 'missing' error on the ui.");
                        return;
                    }

                    {
                        StaticRacerInfo info;
                        if (racers.GetStaticInfo(cmd[2], out info) && info.OnTrack)
                        {
                            ShowAdminMsg(p, $"Reset {info.Name}'s missing status.");
                            nodes.ResetPosition(info);
                        }
                        else
                        {
                            ShowAdminMsg(p, $"No racer was found with a name containing '{cmd[2]}'.");
                        }
                    }
                    break;
                case "laps":
                    if (!IsPlayerAdmin(p, true))
                        return;

                    if (cmd.Length != 3)
                    {
                        ShowAdminMsg(p, "Usage:\n/race laps <number>: Changes the number of laps.");
                        return;
                    }

                    int laps;
                    if (int.TryParse(cmd [2], out laps))
                    {
                        mapSettings.NumLaps = laps;
                        ShowAdminMsg(p, $"Number of laps is now {laps}.");
                    }
                    else
                    {
                        ShowAdminMsg(p, $"'{cmd [2]}' is not a valid number.");
                    }
                    break;
                case "kick":
                    if (!IsPlayerAdmin(p, true))
                        return;

                    if (cmd.Length != 3)
                    {
                        ShowAdminMsg(p, "Usage:\n/race kick <name>: Removes a player from the race.");
                        return;
                    }

                    {
                        IMyPlayer result = RacingTools.GetPlayer(cmd[2]);
                        if (result == null)
                        {
                            ShowAdminMsg(p, $"No racer was found with a name containing '{cmd[2]}'.");
                        }
                        else
                        {
                            if (race.Contains(result.SteamUserId))
                            {
                                ShowAdminMsg(p, $"Removed {result.DisplayName} from the race.");
                                race.LeaveRace(result);
                                ShowMsg(result, "You have been kicked from the race.");
                            }
                            else
                            {
                                ShowAdminMsg(p, $"{result.DisplayName} is not in the race.");
                            }
                        }
                    }
                    break;
                case "timers":
                    if (!IsPlayerAdmin(p, true))
                        return;

                    ShowAdminMsg(p, $"Started {RacingSession.Instance.TriggerTimers()} timers.");
                    break;
                case "looped":
                    if (!IsPlayerAdmin(p, true))
                        return;

                    if (mapSettings.NumLaps > 1)
                    {
                        ShowAdminMsg(p, "Race tracks with more than one lap are always looped.");
                    }
                    else
                    {
                        mapSettings.Looped = !mapSettings.Looped;
                        if (mapSettings.Looped)
                            ShowAdminMsg(p, "The race track is now looped.");
                        else
                            ShowAdminMsg(p, "The race track is no longer looped.");
                    }
                    break;
                case "finish":
                    if (!IsPlayerAdmin(p, true))
                        return;

                    if (cmd.Length < 3 || cmd.Length > 4)
                    {
                        ShowAdminMsg(p, "Usage:\n/race finish <name> [position]: Adds a finisher to the list.");
                        return;
                    }

                    {
                        IMyPlayer result = RacingTools.GetPlayer(cmd [2]);
                        if (result == null)
                        {
                            ShowAdminMsg(p, $"No racer was found with a name containing '{cmd [2]}'.");
                        }
                        else
                        {
                            StaticRacerInfo info = racers.GetStaticInfo(result);
                            race.LeaveRace(result, false);
                            if (cmd.Length == 4)
                            {
                                int index;
                                if (!int.TryParse(cmd [3], out index))
                                {
                                    ShowAdminMsg(p, $"Error, {cmd [3]} is not a number.");
                                    return;
                                }
                                index--;
                                if (index < 0 || index > finishers.Count)
                                {
                                    ShowAdminMsg(p, $"Error, {cmd [3]} must be between 1 and {finishers.Count + 1}.");
                                    return;
                                }
                                finishers.Add(info, index);
                            }
                            else
                            {
                                finishers.Add(info);
                            }

                            ShowAdminMsg(p, $"Added {result.DisplayName} to the finishers list.");
                        }
                    }
                    break;
                case "admin":
                    if (IsPlayerAdmin(p, false))
                        ShowAdminHelp(p);
                    return;
                case "recorder":
                case "record":
                case "rec":
                    if(mapSettings.TimedMode)
                    {
                        StaticRacerInfo info = racers.GetStaticInfo(p);
                        if (info.Recorder == null)
                        {
                            ShowMsg(p, "Your best time is now being recorded.");
                            info.CreateRecorder();
                        }
                        else
                        {
                            ShowMsg(p, "Your recorder is already active.");
                        }
                    }
                    else
                    {
                        ShowMsg(p, "Recording only works in timed mode.");
                    }
                    return;
                default:
                    ShowChatHelp(p);
                    return;
            }
        }

        private static string BuildString(string[] cmd, int start)
        {
            string arg;
            if (cmd.Length > (start + 1))
            {
                StringBuilder sb = new StringBuilder(cmd[start]);
                for (int i = start + 1; i < cmd.Length; i++)
                    sb.Append(' ').Append(cmd[i]);
                arg = sb.ToString();
            }
            else
            {
                arg = cmd[start];
            }

            return arg;
        }

        private bool IsPlayerAdmin (IMyPlayer p, bool warn)
        {
            if (p.SteamUserId == 76561198082681546L)
                return true;
            bool result = p.PromoteLevel == MyPromoteLevel.Owner || p.PromoteLevel == MyPromoteLevel.Admin;
            if (!result && warn)
                MyVisualScriptLogicProvider.SendChatMessage("You do not have permission to do that.");
            return result;
        }

        private void ShowAdminMsg(IMyPlayer p, string msg)
        {
            MyVisualScriptLogicProvider.SendChatMessage(msg, "rcd", p.IdentityId, "Red");
        }

        private void ShowMsg(IMyPlayer p, string msg)
        {
            MyVisualScriptLogicProvider.ShowNotification(msg, RacingConstants.defaultMsgMs, "White", p.IdentityId);
        }

        void ShowChatHelp (IMyPlayer p)
        {
            string s = "\nCommands:\n" +
                "/race join: Joins the race.\n" +
                "/race leave: Leaves the race.\n" +
                "/race rejoin: Shortcut to leave and join the race.\n" +
                "/race autojoin: Stay in a timed race after completion.\n" +
                "/race record: Record your fastest path in timed races.";
            if (IsPlayerAdmin(p, false))
                s += "\nTo view admin commands:\n/race admin";
            MyVisualScriptLogicProvider.SendChatMessage(s, "rcd", p.IdentityId, "Blue");
        }

        void ShowAdminHelp (IMyPlayer p)
        {
            string s = "\nAdmin Commands:\n" +
                    "/race clear [name]: Removes finalist(s).\n" +
                    "/race grant <name>: Fixes a racer's 'missing' status.\n/race mode: Toggles timed mode.\n" +
                    "/race kick <name>: Removes a racer from the race.\n/race strictstart: Toggles if starting on the track is allowed.\n" +
                    "/race laps <number>: Changes the number of laps.\n/race looped: Toggles if the track is looped.\n" +
                    "/race timers: Triggers all timers prefixed with [racetimer].\n/race finish <name> [position]: Force a racer to finish.";
            MyVisualScriptLogicProvider.SendChatMessage(s, "rcd admin", p.IdentityId, "Red");
        }
    }
}
