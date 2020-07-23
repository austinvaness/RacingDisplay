using avaness.RacingMod.Paths;
using ProtoBuf;
using Sandbox.Game;
using Sandbox.ModAPI;
using SpaceEngineers.Game.ModAPI;
using System;
using VRage.Game.ModAPI;

namespace avaness.RacingMod
{
    public partial class RacingSession
    {
        private void MessageEntered (string messageText, ref bool sendToOthers)
        {
            try
            {
                IMyPlayer p = MyAPIGateway.Session?.Player;
                if (p == null || string.IsNullOrEmpty(messageText))
                    return;
                ProcessCommand(p, messageText, ref sendToOthers);
            }
            catch (Exception e)
            {
                RacingTools.ShowError(e, GetType());
            }
        }

        void ProcessCommand (IMyPlayer p, string command, ref bool sendToOthers)
        {
            bool redirect = false;
            command = command.ToLower().Trim();
            if (!command.StartsWith("/rcd"))
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
                case "ui":
                    if (activeRacersHud != null)
                        ToggleUI();
                    break;
                case "j":
                case "join":
                    if (RacingConstants.IsServer)
                    {
                        if(cmd.Length > 2 && IsPlayerAdmin(p, false))
                        {
                            IMyPlayer temp = RacingTools.GetPlayer(cmd [2]);
                            if (temp == null)
                                ShowAdminMsg(p, $"No racer was found with a name containing '{cmd [2]}'.");
                            else
                                JoinRace(temp, true);
                        }
                        else
                        {
                            JoinRace(p);
                        }
                    }
                    else
                    {
                        redirect = true;
                    }
                    break;
                case "l":
                case "leave":
                    if (RacingConstants.IsServer)
                        LeaveRace(p);
                    else
                        redirect = true;
                    break;
                case "rejoin":
                    if (RacingConstants.IsServer)
                    {
                        LeaveRace(p, false);
                        JoinRace(p);
                    }
                    else
                    {
                        redirect = true;
                    }
                    break;
                case "aj":
                case "autojoin":
                    if(RacingConstants.IsServer)
                    {
                        if(MapSettings.TimedMode)
                        {
                            if (!activePlayers.Contains(p.SteamUserId) && !JoinRace(p))
                                return;

                            StaticRacerInfo info = GetStaticInfo(p);
                            if (info.AutoJoin)
                                ShowMsg(p, "You will leave the race after finishing.");
                            else
                                ShowMsg(p, "Your timer will reset after finishing.");
                            info.AutoJoin = !info.AutoJoin;
                        }
                        else
                        {
                            ShowMsg(p, "Auto join only works for timed races.");
                        }
                    }
                    else
                    {
                        redirect = true;
                    }
                    break;
                case "clear":
                    if (!IsPlayerAdmin(p, true))
                        return;

                    if (RacingConstants.IsServer)
                    {
                        if (cmd.Length == 2)
                        {
                            finishers.Clear();
                            ShowAdminMsg(p, "Cleared all finishers.");
                            ClearAllRecorders();
                        }
                        else if (cmd.Length == 3)
                        {
                            StaticRacerInfo info;
                            if (!GetStaticInfo(cmd[2], out info))
                            {
                                ShowAdminMsg(p, $"No racer was found with a name containing '{cmd [2]}'.");
                                return;
                            }
                            else
                            {
                                ShowAdminMsg(p, $"Removed {info.Name} from the finalists.");
                                info.Recorder?.ClearData();
                                finishers.Remove(info);
                            }
                        }
                        else
                        {
                            ShowAdminMsg(p, "Usage:\n/rcd clear [name]: Removes all or a specific racer from finalists.");
                            return;

                        }
                    }
                    else
                    {
                        redirect = true;
                    }
                    break;
                case "debug":
                    if(!RacingConstants.IsServer)
                        ShowAdminMsg(p, "Debug view only works for the server host.");
                    debug = !debug;
                    break;
                case "mode":
                    if (!IsPlayerAdmin(p, true))
                        return;

                    if (RacingConstants.IsServer)
                    {
                        MapSettings.TimedMode = !MapSettings.TimedMode;
                        if (MapSettings.TimedMode)
                            ShowAdminMsg(p, "Mode is now timed.");
                        else
                            ShowAdminMsg(p, "Mode is now normal.");
                    }
                    else
                    {
                        redirect = true;
                    }
                    break;
                case "strictstart":
                    if (!IsPlayerAdmin(p, true))
                        return;

                    if (RacingConstants.IsServer)
                    {
                        MapSettings.StrictStart = !MapSettings.StrictStart;
                        if (MapSettings.StrictStart)
                            ShowAdminMsg(p, "Starting on the track is no longer allowed.");
                        else
                            ShowAdminMsg(p, "Starting on the track is now allowed.");
                    }
                    else
                    {
                        redirect = true;
                    }
                    break;
                case "grant":
                    if (!IsPlayerAdmin(p, true))
                        return;

                    if (cmd.Length != 3)
                    {
                        ShowAdminMsg(p, "Usage:\n/rcd grant <name>: Fixes a 'missing' error on the ui.");
                        return;
                    }

                    if (RacingConstants.IsServer)
                    {
                        StaticRacerInfo info;
                        if (GetStaticInfo(cmd[2], out info) && info.OnTrack)
                        {
                            ShowAdminMsg(p, $"Reset {info.Name}'s missing status.");
                            Nodes.ResetPosition(info);
                        }
                        else
                        {
                            ShowAdminMsg(p, $"No racer was found with a name containing '{cmd [2]}'.");
                        }
                        return;
                    }
                    else
                    {
                        redirect = true;
                    }
                    break;
                case "laps":
                    if (!IsPlayerAdmin(p, true))
                        return;

                    if (cmd.Length != 3)
                    {
                        ShowAdminMsg(p, "Usage:\n/rcd laps <number>: Changes the number of laps.");
                        return;
                    }

                    if (RacingConstants.IsServer)
                    {
                        int laps;
                        if (int.TryParse(cmd [2], out laps))
                        {
                            MapSettings.NumLaps = laps;
                            ShowAdminMsg(p, $"Number of laps is now {laps}.");
                        }
                        else
                        {
                            ShowAdminMsg(p, $"'{cmd [2]}' is not a valid number.");
                        }
                        return;
                    }
                    else
                    {
                        redirect = true;
                    }
                    break;
                case "kick":
                    if (!IsPlayerAdmin(p, true))
                        return;

                    if (cmd.Length != 3)
                    {
                        ShowAdminMsg(p, "Usage:\n/rcd kick <name>: Removes a player from the race.");
                        return;
                    }

                    if (RacingConstants.IsServer)
                    {
                        IMyPlayer result = RacingTools.GetPlayer(cmd [2]);
                        if (result == null)
                        {
                            ShowAdminMsg(p, $"No racer was found with a name containing '{cmd [2]}'.");
                        }
                        else
                        {
                            if (activePlayers.Contains(result.SteamUserId))
                            {
                                ShowAdminMsg(p, $"Removed {result.DisplayName} from the race.");
                                LeaveRace(result);
                                ShowMsg(result, "You have been kicked from the race.");
                            }
                            else
                            {
                                ShowAdminMsg(p, $"{result.DisplayName} is not in the race.");
                            }
                        }
                        return;
                    }
                    else
                    {
                        redirect = true;
                    }
                    break;
                case "timers":
                    if (!IsPlayerAdmin(p, true))
                        return;

                    if(RacingConstants.IsServer)
                    {
                        foreach(IMyTimerBlock t in StartTimers)
                            t.Trigger();
                        ShowAdminMsg(p, $"Started {StartTimers.Count} timers.");
                        RacingTools.SendAPIMessage(RacingConstants.apiRaceStarted); // API: Race started
                    }
                    else
                    {
                        redirect = true;
                    }
                    break;
                case "looped":
                    if (!IsPlayerAdmin(p, true))
                        return;

                    if (RacingConstants.IsServer)
                    {
                        if(MapSettings.NumLaps > 1)
                        {
                            ShowAdminMsg(p, "Race tracks with more than one lap are always looped.");
                        }
                        else
                        {
                            MapSettings.Looped = !MapSettings.Looped;
                            if (MapSettings.Looped)
                                ShowAdminMsg(p, "The race track is now looped.");
                            else
                                ShowAdminMsg(p, "The race track is no longer looped.");
                        }
                    }
                    else
                    {
                        redirect = true;
                    }
                    break;
                case "finish":
                    if (!IsPlayerAdmin(p, true))
                        return;

                    if (cmd.Length < 3 || cmd.Length > 4)
                    {
                        ShowAdminMsg(p, "Usage:\n/rcd finish <name> [position]: Adds a finisher to the list.");
                        return;
                    }

                    if (RacingConstants.IsServer)
                    {
                        IMyPlayer result = RacingTools.GetPlayer(cmd [2]);
                        if (result == null)
                        {
                            ShowAdminMsg(p, $"No racer was found with a name containing '{cmd [2]}'.");
                        }
                        else
                        {
                            StaticRacerInfo info = GetStaticInfo(result);
                            LeaveRace(result, false);
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
                        return;
                    }
                    else
                    {
                        redirect = true;
                    }
                    break;
                case "admin":
                    if (IsPlayerAdmin(p, false))
                        ShowAdminHelp(p);
                    return;
                case "recorder":
                case "record":
                case "rec":
                    if (RacingConstants.IsServer)
                    {
                        if(MapSettings.TimedMode)
                        {
                            StaticRacerInfo info = GetStaticInfo(p);
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
                    }
                    else
                    {
                        redirect = true;
                    }
                    break;
                default:
                    ShowChatHelp(p);
                    return;
            }

            if (redirect)
            {
                byte [] data = MyAPIGateway.Utilities.SerializeToBinary(new CommandInfo(command, p.SteamUserId));
                MyAPIGateway.Multiplayer.SendMessageToServer(RacingConstants.packetCmd, data);
            }
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
            string s = "\nCommands:\n/rcd join: Joins the race.\n/rcd leave: Leaves the race.\n" +
                "/rcd rejoin: Shortcut to leave and join the race.\n/rcd ui: Toggles the on screen UIs.\n" +
                "/rcd autojoin: Rejoin a timed race after it has been completed.\n" +
                "/rcd record: Start recording your fastest path in timed races.";
            if (IsPlayerAdmin(p, false))
                s += "\nTo view admin commands:\n/rcd admin";
            MyVisualScriptLogicProvider.SendChatMessage(s, "rcd", p.IdentityId, "Blue");
        }

        void ShowAdminHelp (IMyPlayer p)
        {
            string s = "\nAdmin Commands:\n" +
                    "/rcd clear [name]: Removes finalist(s).\n" +
                    "/rcd grant <name>: Fixes a racer's 'missing' status.\n/rcd mode: Toggles timed mode.\n" +
                    "/rcd kick <name>: Removes a racer from the race.\n/rcd strictstart: Toggles if starting on the track is allowed.\n" +
                    "/rcd laps <number>: Changes the number of laps.\n/rcd looped: Toggles if the track is looped.\n" +
                    "/rcd timers: Triggers all timers prefixed with [racetimer].\n/rcd finish <name> [position]: Force a racer to finish.";
            MyVisualScriptLogicProvider.SendChatMessage(s, "rcd admin", p.IdentityId, "Red");
        }


        [ProtoContract]
        public class CommandInfo
        {
            [ProtoMember(1)]
            public string cmd;
            [ProtoMember(2)]
            public ulong steamId;

            public CommandInfo ()
            {
            }

            public CommandInfo (string cmd, ulong steamId)
            {
                this.cmd = cmd;
                this.steamId = steamId;
            }
        }

    }
}
