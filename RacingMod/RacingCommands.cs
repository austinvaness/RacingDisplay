using ProtoBuf;
using Sandbox.Game;
using Sandbox.ModAPI;
using SpaceEngineers.Game.ModAPI;
using System;
using VRage.Game.ModAPI;

namespace RacingMod
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
                case "join":
                    if (RacingConstants.IsServer)
                        JoinRace(p);
                    else
                        redirect = true;
                    break;
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
                case "autojoin":
                    if(RacingConstants.IsServer)
                    {
                        if(MapSettings.TimedMode)
                        {
                            if (!activePlayers.Contains(p.SteamUserId) && !JoinRace(p))
                                return;

                            StaticRacerInfo info = GetStaticInfo(p);
                            if (info.AutoJoin)
                                MyVisualScriptLogicProvider.ShowNotification("You will leave the race after finishing.", RacingConstants.defaultMsgMs, "White", p.IdentityId);
                            else
                                MyVisualScriptLogicProvider.ShowNotification("Your timer will reset after finishing.", RacingConstants.defaultMsgMs, "White", p.IdentityId);
                            info.AutoJoin = !info.AutoJoin;
                        }
                        else
                        {
                            MyVisualScriptLogicProvider.ShowNotification("Auto join only works for timed races.", RacingConstants.defaultMsgMs, "White", p.IdentityId);
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
                            MyVisualScriptLogicProvider.SendChatMessage("Cleared all finishers.", "rcd", p.IdentityId, "Red");
                        }
                        else if (cmd.Length == 3)
                        {
                            StaticRacerInfo info;
                            if (!GetStaticInfo(cmd[2], out info))
                            {
                                MyVisualScriptLogicProvider.SendChatMessage($"No racer was found with a name containing '{cmd [2]}'.", "rcd", p.IdentityId, "Red");
                                return;
                            }
                            else
                            {
                                MyVisualScriptLogicProvider.SendChatMessage($"Removed {info.Name} from the finalists.", "rcd", p.IdentityId, "Red");
                                finishers.Remove(info);
                            }
                        }
                        else
                        {
                            MyVisualScriptLogicProvider.SendChatMessage("Usage:\n/rcd clear [name]: Removes all or a specific racer from finalists.", "rcd", p.IdentityId, "Red");
                            return;

                        }
                    }
                    else
                    {
                        redirect = true;
                    }
                    break;
                case "cleartimer":
                    if (!IsPlayerAdmin(p, true))
                        return;

                    if (RacingConstants.IsServer)
                    {
                        if (cmd.Length == 2)
                        {
                            foreach (StaticRacerInfo info in staticRacerInfo.Values)
                                info.Timer.Reset(true);
                            MyVisualScriptLogicProvider.SendChatMessage("Reset all race timers.", "rcd", p.IdentityId, "Red");
                        }
                        else if (cmd.Length == 3)
                        {
                            StaticRacerInfo info;
                            if (GetStaticInfo(cmd[2], out info) && info.OnTrack)
                            {
                                info.Timer.Reset(true);
                                MyVisualScriptLogicProvider.SendChatMessage($"Reset {info.Name}'s race timer.", "rcd", p.IdentityId, "Red");
                                
                            }
                            else
                            {
                                MyVisualScriptLogicProvider.SendChatMessage($"No racer was found with a name containing '{cmd [2]}'.", "rcd", p.IdentityId, "Red");
                                return;
                            }
                        }
                        else
                        {
                            MyVisualScriptLogicProvider.SendChatMessage("Usage:\n/rcd cleartimer [name]: Resets all or a specific race timer.", "rcd", p.IdentityId, "Red");
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
                        MyVisualScriptLogicProvider.SendChatMessage("Debug view only works for the server host.", "rcd", p.IdentityId, "Red");
                    debug = !debug;
                    break;
                case "mode":
                    if (!IsPlayerAdmin(p, true))
                        return;

                    if (RacingConstants.IsServer)
                    {
                        MapSettings.TimedMode = !MapSettings.TimedMode;
                        if (MapSettings.TimedMode)
                            MyVisualScriptLogicProvider.SendChatMessage("Mode is now timed.", "rcd", p.IdentityId, "Red");
                        else
                            MyVisualScriptLogicProvider.SendChatMessage("Mode is now normal.", "rcd", p.IdentityId, "Red");
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
                            MyVisualScriptLogicProvider.SendChatMessage("Starting on the track is no longer allowed.", "rcd", p.IdentityId, "Red");
                        else
                            MyVisualScriptLogicProvider.SendChatMessage("Starting on the track is now allowed.", "rcd", p.IdentityId, "Red");
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
                        MyVisualScriptLogicProvider.SendChatMessage("Usage:\n/rcd grant <name>: Fixes a 'missing' error on the ui.", "rcd", p.IdentityId, "Red");
                        return;
                    }

                    if (RacingConstants.IsServer)
                    {
                        StaticRacerInfo info;
                        if (GetStaticInfo(cmd[2], out info) && info.OnTrack)
                        {
                            MyVisualScriptLogicProvider.SendChatMessage($"Reset {info.Name}'s missing status.", "rcd", p.IdentityId, "Red");
                            Nodes.ResetPosition(info);
                        }
                        else
                        {
                            MyVisualScriptLogicProvider.SendChatMessage($"No racer was found with a name containing '{cmd [2]}'.", "rcd", p.IdentityId, "Red");
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
                        MyVisualScriptLogicProvider.SendChatMessage("Usage:\n/rcd laps <number>: Changes the number of laps.", "rcd", p.IdentityId, "Red");
                        return;
                    }

                    if (RacingConstants.IsServer)
                    {
                        int laps;
                        if (int.TryParse(cmd [2], out laps))
                        {
                            MapSettings.NumLaps = laps;
                            MyVisualScriptLogicProvider.SendChatMessage($"Number of laps is now {laps}.", "rcd", p.IdentityId, "Red");
                        }
                        else
                        {
                            MyVisualScriptLogicProvider.SendChatMessage($"'{cmd [2]}' is not a valid number.", "rcd", p.IdentityId, "Red");
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
                        MyVisualScriptLogicProvider.SendChatMessage("Usage:\n/rcd kick <name>: Removes a player from the race.", "rcd", p.IdentityId, "Red");
                        return;
                    }

                    if (RacingConstants.IsServer)
                    {
                        IMyPlayer result = RacingTools.GetPlayer(cmd [2]);
                        if (result == null)
                        {
                            MyVisualScriptLogicProvider.SendChatMessage($"No racer was found with a name containing '{cmd [2]}'.", "rcd", p.IdentityId, "Red");
                        }
                        else
                        {
                            if (activePlayers.Contains(result.SteamUserId))
                            {
                                MyVisualScriptLogicProvider.SendChatMessage($"Removed {result.DisplayName} from the race.", "rcd", p.IdentityId, "Red");
                                LeaveRace(result);
                                MyVisualScriptLogicProvider.ShowNotification("You have been kicked from the race.", RacingConstants.defaultMsgMs, "White", result.IdentityId);
                            }
                            else
                            {
                                MyVisualScriptLogicProvider.SendChatMessage($"{result.DisplayName} is not in the race.", "rcd", p.IdentityId, "Red");
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
                        MyVisualScriptLogicProvider.SendChatMessage($"Started {StartTimers.Count} timers.", "rcd", p.IdentityId, "Red");
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
                            MyVisualScriptLogicProvider.SendChatMessage("Race tracks with more than one lap are always looped.", "rcd", p.IdentityId, "Red");
                        }
                        else
                        {
                            MapSettings.Looped = !MapSettings.Looped;
                            if (MapSettings.Looped)
                                MyVisualScriptLogicProvider.SendChatMessage("The race track is now looped.", "rcd", p.IdentityId, "Red");
                            else
                                MyVisualScriptLogicProvider.SendChatMessage("The race track is no longer looped.", "rcd", p.IdentityId, "Red");
                        }
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
                MyVisualScriptLogicProvider.SendChatMessage("You do not have permission to do that.", "rcd", p.IdentityId, "Red");
            return result;
        }

        void ShowChatHelp (IMyPlayer p)
        {
            string s = "\nCommands:\n/rcd join: Joins the race.\n/rcd leave: Leaves the race.\n" +
                "/rcd rejoin: Shortcut to leave and join the race.\n/rcd ui: Toggles the on screen UIs.\n" +
                "/rcd autojoin: Rejoin a timed race after it has been completed.";
            if (IsPlayerAdmin(p, false))
                s += "\nTo view admin commands:\n/rcd admin";
            MyVisualScriptLogicProvider.SendChatMessage(s, "rcd", p.IdentityId, "Red");
        }

        void ShowAdminHelp (IMyPlayer p)
        {
            string s = "\nAdmin Commands:\n/rcd start: Starts the race.\n" +
                    "/rcd clear [name]: Removes finalist(s).\n" +
                    "/rcd cleartimer [name]: Resets a racer(s) timer.\n" +
                    "/rcd grant <name>: Fixes a racer's 'missing' status.\n/rcd mode: Toggles timed mode.\n" +
                    "/rcd kick <name>: Removes a racer from the race.\n/rcd strictstart: Toggles if starting on the track is allowed.\n" +
                    "/rcd laps <number>: Changes the number of laps.\n/rcd looped: Toggles if the track is looped.\n" +
                    "/rcd timers: Triggers all timers prefixed with [racetimer].";
            MyVisualScriptLogicProvider.SendChatMessage(s, "rcd", p.IdentityId, "Red");
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
