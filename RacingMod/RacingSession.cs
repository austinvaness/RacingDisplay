using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Draygo.API;
using Sandbox.Game;
using Sandbox.ModAPI;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.ModAPI;
using VRage.Utils;
using VRageMath;
using ProtoBuf;
using System.Collections.Concurrent;
using BlendTypeEnum = VRageRender.MyBillboard.BlendTypeEnum;
using KlimeDraygo.RelativeSpectator.API;

namespace RacingMod
{
    [MySessionComponentDescriptor(MyUpdateOrder.AfterSimulation)]
    public partial class RacingSession : MySessionComponentBase
    {
        public static RacingSession Instance;
        public int Runtime = 0;
        public RacingMapSettings MapSettings = new RacingMapSettings();

        bool debug = false;
        
        private readonly List<RacingBeacon> nodes = new List<RacingBeacon>();
        private RacingBeacon finish;
        private double[] nodeDistances = { };


        private readonly StringBuilder activeRacersText = new StringBuilder("Initializing...");
        private readonly StringBuilder infoHudText = new StringBuilder();
        private string finalRacersString;


        private bool running = false;

        private readonly Dictionary<ulong, StaticRacerInfo> staticRacerInfo = new Dictionary<ulong, StaticRacerInfo>();
        
        private readonly HashSet<ulong> finishers = new HashSet<ulong>();
        private readonly HashSet<ulong> activePlayers = new HashSet<ulong>();
        private int numRacersPreviousTick = 0;

        readonly ConcurrentDictionary<ulong, HashSet<ulong>> nextPlayerRequests = new ConcurrentDictionary<ulong, HashSet<ulong>>();
        readonly ConcurrentDictionary<ulong, HashSet<ulong>> prevPlayerRequests = new ConcurrentDictionary<ulong, HashSet<ulong>>();

        SpecCamAPI Spec;

        private string hudHeader;

        private int gateWaypointGps;
        
        // Temporary
        private readonly List<IMyPlayer> playersTemp = new List<IMyPlayer>();
        private readonly StringBuilder tempSb = new StringBuilder();

        RacingPreferences config = new RacingPreferences();

        public RacingSession ()
        {
            Instance = this;
            UpdateHeader();
        }

        private void Start()
        {
            if(textApi == null)
                textApi = new HudAPIv2(CreateHudItems);

            if(Spec == null)
                Spec = new SpecCamAPI();

            if (RacingConstants.IsPlayer && MyAPIGateway.Session.Player == null)
                return;

            gateWaypointGps = MyAPIGateway.Session.GPS.Create(RacingConstants.gateWaypointName, RacingConstants.gateWaypointDescription, Vector3D.Zero, true).Hash;

            MyAPIGateway.Utilities.MessageEntered += MessageEntered;
            if (RacingConstants.IsServer)
            {
                MyVisualScriptLogicProvider.RemoveGPSForAll(RacingConstants.gateWaypointName);
                MyAPIGateway.Multiplayer.RegisterMessageHandler(RacingConstants.packetCmd, ReceiveCmd);
                MyAPIGateway.Multiplayer.RegisterMessageHandler(RacingConstants.packetSpecRequest, ReceiveSpecRequest);
                MapSettings = RacingMapSettings.LoadFile();
                UpdateUI_Admin();
            }
            else
            {
                MyAPIGateway.Session.GPS.RemoveLocalGps(gateWaypointGps);
                MyAPIGateway.Multiplayer.RegisterMessageHandler(RacingConstants.packetMainId, ReceiveActiveRacers);
                MyAPIGateway.Multiplayer.RegisterMessageHandler(RacingConstants.packetSpecResponse, ReceiveSpecResponse);
                MyAPIGateway.Multiplayer.SendMessageToServer(RacingConstants.packetSettingsInit, BitConverter.GetBytes(MyAPIGateway.Session.Player.SteamUserId));
            }
            MyAPIGateway.Multiplayer.RegisterMessageHandler(RacingConstants.packetSettings, ReceiveSettings);
            MyAPIGateway.Multiplayer.RegisterMessageHandler(RacingConstants.packetSettingsInit, ReceiveSettingsInit);

            config = RacingPreferences.LoadFile();
            UpdateUI_Spectator();

            MyLog.Default.WriteLineAndConsole("Racing Display started.");
            running = true;
        }

        public override void UpdateAfterSimulation ()
        {
            if (MyAPIGateway.Session == null)
                return;
            if (!running)
                Start();
            if (!running)
                return;

            Runtime++;
            try
            {
                if (RacingConstants.IsPlayer)
                    Spectator();

                if (RacingConstants.IsServer)
                {
                    ProcessValues();
                    BroadcastData(activeRacersText, RacingConstants.packetMainId);
                    DrawDebug();
                }
            }
            catch (Exception e)
            {
                RacingTools.ShowError(e, GetType());
            }
        }

        protected override void UnloadData ()
        {
            MyAPIGateway.Utilities.MessageEntered -= MessageEntered;

            if (RacingConstants.IsServer)
            {
                MyAPIGateway.Multiplayer.UnregisterMessageHandler(RacingConstants.packetCmd, ReceiveCmd);
                MyAPIGateway.Multiplayer.UnregisterMessageHandler(RacingConstants.packetSpecRequest, ReceiveSpecRequest);
            }
            else
            {
                MyAPIGateway.Multiplayer.UnregisterMessageHandler(RacingConstants.packetMainId, ReceiveActiveRacers);
                MyAPIGateway.Multiplayer.UnregisterMessageHandler(RacingConstants.packetSpecResponse, ReceiveSpecResponse);
            }
            MyAPIGateway.Multiplayer.UnregisterMessageHandler(RacingConstants.packetSettings, ReceiveSettings);
            MyAPIGateway.Multiplayer.UnregisterMessageHandler(RacingConstants.packetSettingsInit, ReceiveSettingsInit);

            textApi?.Unload();
            Spec?.Close();

            Instance = null;
        }

        public void UpdateHeader()
        {
            tempSb.Clear();
            tempSb.Append(RacingConstants.headerColor).Append("#".PadRight(RacingConstants.numberWidth + 1));
            tempSb.Append("Name".PadRight(RacingConstants.nameWidth + 1));
            tempSb.Append("Position".PadRight(RacingConstants.distWidth + 1));
            if (MapSettings.NumLaps > 1)
                tempSb.Append("Lap");
            tempSb.AppendLine().Append(RacingConstants.colorWhite);
            hudHeader = tempSb.ToString();
        }

        public void UpdateInfoHud()
        {
            if (infoHud == null)
                return;
            Vector2D v = infoHud.GetTextLength();
            v.Y = 0;
            v.X *= -1;
            infoHud.Offset = v;
        }

        public void RegisterNode(RacingBeacon node)
        {
            if (!node.Valid || node.Type == RacingBeacon.BeaconType.IGNORED)
                return;

            if (node.Type == RacingBeacon.BeaconType.FINISH)
            {
                if(finish == null)
                {
                    if (!float.IsNaN(node.NodeNumber))
                    {
                        MapSettings.NumLaps = (int)node.NodeNumber;
                        UpdateUI_NumLaps();
                        node.Beacon.CustomData = "";
                    }
                    nodes.Add(node);
                    finish = node;
                }
                else
                {
                    node.Beacon.CustomName = "";
                    return;
                }
            }
            else if (RacingTools.AddSorted(nodes, node))
            {
                if (finish != null && finish.Beacon == node.Beacon)
                    finish = null;
            }
            else
            {
                // checkpoint with this id already existed
                node.Beacon.CustomData = "";
                return;
            }
            
            RebuildNodeInformation();
        }

        public void RemoveNode(RacingBeacon node)
        {
            int index = nodes.FindIndex(other => other.Entity.EntityId == node.Entity.EntityId); 
            if(index >= 0)
                nodes.RemoveAt(index);

            if (finish != null && finish.Beacon == node.Beacon)
            {
                UpdateHeader();
                finish = null;
            }
            RebuildNodeInformation();
        }

        private void RebuildNodeInformation()
        {
            // rebuild position cache
            Vector3[] nodePositions = nodes.Select(b => b.Coords).ToArray();
            
            // rebuild distance cache
            nodeDistances = new double[nodePositions.Length];
            if (nodeDistances.Length == 0)
                return;

            foreach (StaticRacerInfo info in staticRacerInfo.Values)
                info.NextNode = null;
            
            double cumulative = 0;
            nodeDistances [0] = 0;
            nodes [0].Index = 0;
            Vector3D prev = nodePositions[0];
            for (int i = 1; i < nodePositions.Length; i++)
            {
                nodes [i].Index = i;
                Vector3D v = nodePositions[i];
                cumulative += Vector3.Distance(prev, v);
                nodeDistances[i] = cumulative;
                prev = nodePositions[i];
            }
        }

        private void ReceiveCmd (byte [] obj)
        {
            try
            {
                CommandInfo cmd = MyAPIGateway.Utilities.SerializeFromBinary<CommandInfo>(obj);
                IMyPlayer p = RacingTools.GetPlayer(cmd.steamId);
                bool temp = false;
                if (p != null)
                    ProcessCommand(p, cmd.cmd, ref temp);
            }
            catch (Exception e)
            {
                RacingTools.ShowError(e, GetType());
            }
        }

        private void ReceiveActiveRacers(byte[] obj)
        {
            DecodeString(obj, activeRacersText);
        }

        private void DecodeString (byte [] obj, StringBuilder sb)
        {
            try
            {
                string data = Encoding.ASCII.GetString(obj);
                sb.Clear();
                sb.Append(data);
            }
            catch (Exception e)
            {
                RacingTools.ShowError(e, GetType());
            }
        }

        private void MessageEntered(string messageText, ref bool sendToOthers)
        {
            try
            {
                IMyPlayer p = MyAPIGateway.Session?.Player;
                if (p == null || string.IsNullOrEmpty(messageText))
                    return;
                ProcessCommand(p, messageText, ref sendToOthers);
            }
            catch(Exception e)
            {
                RacingTools.ShowError(e, GetType());
            }
        }
        
        void ProcessCommand(IMyPlayer p, string command, ref bool sendToOthers)
        {
            bool redirect = false;
            command = command.ToLower().Trim();
            if(!command.StartsWith("/rcd"))
                return;
            sendToOthers = false;
            string [] cmd = command.Split(' ');
            if(cmd.Length == 1)
            {
                ShowChatHelp(p);
                return;
            }
            switch (cmd[1])
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
                        LeaveRace(p);
                        JoinRace(p);
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
                        if(cmd.Length == 2)
                        {
                            ClearFinishers();
                            MyVisualScriptLogicProvider.SendChatMessage("Cleared all finishers.", "rcd", p.IdentityId, "Red");
                        }
                        else if(cmd.Length == 3)
                        {
                            IMyPlayer result = RacingTools.GetPlayer(cmd [2]);
                            if (result == null)
                            {
                                MyVisualScriptLogicProvider.SendChatMessage($"No racer was found with a name containing '{cmd [2]}'.", "rcd", p.IdentityId, "Red");
                                return;
                            }
                            else
                            {
                                MyVisualScriptLogicProvider.SendChatMessage($"Removed {result.DisplayName} from the finalists.", "rcd", p.IdentityId, "Red");
                                RemoveFinisher(result.SteamUserId);
                            }
                        }
                        else
                        {
                            MyVisualScriptLogicProvider.SendChatMessage("Usage:\n/rcd clear [name]: Removes all or a specific racer from finalists.", "rcd", p.IdentityId, "Red");
                            return;

                        }
                        FinishersUpdated();
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
                        if(cmd.Length == 2)
                        {
                            foreach(StaticRacerInfo info in staticRacerInfo.Values)
                                info.Timer.Reset(true);
                            MyVisualScriptLogicProvider.SendChatMessage("Reset all race timers.", "rcd", p.IdentityId, "Red");
                        }
                        else if(cmd.Length == 3)
                        {
                            IMyPlayer result = RacingTools.GetPlayer(cmd [2]);
                            if (result == null)
                            {
                                MyVisualScriptLogicProvider.SendChatMessage($"No racer was found with a name containing '{cmd [2]}'.", "rcd", p.IdentityId, "Red");
                                return;
                            }
                            else
                            {
                                StaticRacerInfo info;
                                if (staticRacerInfo.TryGetValue(p.SteamUserId, out info))
                                    info.Timer.Reset(true);
                                MyVisualScriptLogicProvider.SendChatMessage($"Reset {result.DisplayName}'s race timer.", "rcd", p.IdentityId, "Red");
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
                    debug = !debug;
                    break;
                case "mode":
                    if (!IsPlayerAdmin(p, true))
                        return;

                    if (RacingConstants.IsServer)
                    {
                        MapSettings.TimedMode = !MapSettings.TimedMode;
                        if(MapSettings.TimedMode)
                            MyVisualScriptLogicProvider.SendChatMessage("Mode is now timed.", "rcd", p.IdentityId, "Red");
                        else
                            MyVisualScriptLogicProvider.SendChatMessage("Mode is now normal.", "rcd", p.IdentityId, "Red");
                        FinishersUpdated();
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
                        IMyPlayer result = RacingTools.GetPlayer(cmd [2]);
                        if (result == null)
                        {
                            MyVisualScriptLogicProvider.SendChatMessage($"No racer was found with a name containing '{cmd [2]}'.", "rcd", p.IdentityId, "Red");
                        }
                        else
                        {
                            MyVisualScriptLogicProvider.SendChatMessage($"Reset {result.DisplayName}'s missing status.", "rcd", p.IdentityId, "Red");
                            StaticRacerInfo info;
                            if (GetStaticInfo(p, out info))
                                info.NextNode = null;
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
                        if (int.TryParse(cmd[2], out laps))
                        {
                            MapSettings.NumLaps = laps;
                            UpdateUI_NumLaps();
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
            if(!result && warn)
                MyVisualScriptLogicProvider.SendChatMessage("You do not have permission to do that.", "rcd", p.IdentityId, "Red");
            return result;
        }

        private void ToggleUI()
        {
            activeRacersHud.Visible = !activeRacersHud.Visible;
        }

        private bool GetStaticInfo (IMyPlayer p, out StaticRacerInfo info)
        {
            if (staticRacerInfo.TryGetValue(p.SteamUserId, out info))
                return true;
            info = null;
            return false;
        }

        private StaticRacerInfo GetStaticInfo(IMyPlayer p)
        {
            StaticRacerInfo info;
            if (staticRacerInfo.TryGetValue(p.SteamUserId, out info))
                return info;
            info = new StaticRacerInfo(p);
            staticRacerInfo.Add(p.SteamUserId, info);
            return info;
        }

        void ShowChatHelp(IMyPlayer p)
        {
            string s = "\nCommands:\n/rcd join: Joins the race.\n/rcd leave: Leaves the race.\n" +
                "/rcd rejoin: Shortcut to leave and join the race.\n/rcd ui: Toggles the on screen UIs.";
            if (IsPlayerAdmin(p, false))
                s = "\nAdmin Commands:\n/rcd clear [name]: Removes finalist(s).\n" +
                    "/rcd cleartimer [name]: Resets a racer(s) timer.\n" +
                    "/rcd grant <name>: Fixes a racer's 'missing' status.\n/rcd mode: Toggles timed mode.\n" +
                    "/rcd kick <name>: Removes a racer from the race.\n/rcd strictstart: Toggles if starting on the track is allowed.\n" +
                    "/rcd laps <number>: Changes the number of laps." + s;
            MyVisualScriptLogicProvider.SendChatMessage(s, "rcd", p.IdentityId, "Red");
        }

        private void DrawDebug ()
        {
            if (!debug)
                return;

            Vector4 color = Color.White.ToVector4();
            MyStringId mat = MyStringId.GetOrCompute("Square");

            for(int i = 1; i < nodes.Count; i++)
            {
                Vector3D origin = nodes [i - 1].Coords;
                Vector3 direction = nodes [i].Coords - origin;
                float len = direction.Length();
                if (len == 0)
                    continue;
                direction /= len;
                MyTransparentGeometry.AddLineBillboard(mat, color, origin, direction, len, 1, BlendTypeEnum.AdditiveTop);
            }
        }

        private void BroadcastData (StringBuilder sb, ushort packetId)
        {
            byte [] data = Encoding.ASCII.GetBytes(sb.ToString());

            List<IMyPlayer> players = new List<IMyPlayer>();
            MyAPIGateway.Players.GetPlayers(players);
            foreach (IMyPlayer p in players)
            {
                if (p.SteamUserId == MyAPIGateway.Multiplayer.ServerId)
                    continue;

                MyAPIGateway.Multiplayer.SendMessageTo(packetId, data, p.SteamUserId);
            }
        }

        bool IsPlayerActive(IMyPlayer p)
        {
            return activePlayers.Contains(p.SteamUserId) && p.Character?.Physics != null;
        }

        bool JoinRace(IMyPlayer p)
        {

            if (MapSettings.StrictStart)
            {
                int start;
                int end;
                double partial;
                GetClosestSegment(p.GetPosition(), out start, out end, out partial);
                if (end > 0 && end < nodes.Count)
                {
                    MyVisualScriptLogicProvider.ShowNotification("Move behind the starting line before joining the race.", RacingConstants.defaultMsgMs, "White", p.IdentityId);
                    return false;
                }
            }

            if (activePlayers.Add(p.SteamUserId))
            {
                StaticRacerInfo info = GetStaticInfo(p);
                info.NextNode = null;
                info.InFinish = false;
                info.Laps = 0;
                info.Timer.Reset(true);
                info.PreviousTick = null;
                if (!MapSettings.TimedMode && RemoveFinisher(p.SteamUserId))
                    FinishersUpdated();
                MyVisualScriptLogicProvider.ShowNotification("You have joined the race.", RacingConstants.defaultMsgMs, "White", p.IdentityId);
                return true;
            }
            else
            {
                MyVisualScriptLogicProvider.ShowNotification("You are already in the race.", RacingConstants.defaultMsgMs, "White", p.IdentityId);
                return false;
            }
        }

        bool RemoveFinisher(ulong id)
        {
            if(finishers.Remove(id))
            {
                StaticRacerInfo info;
                if (staticRacerInfo.TryGetValue(id, out info))
                    info.RemoveFinish();
                return true;
            }
            return false;
        }

        void ClearFinishers()
        {
            foreach (ulong id in finishers)
            {
                StaticRacerInfo info;
                if (staticRacerInfo.TryGetValue(id, out info))
                    info.RemoveFinish();
            }
            finishers.Clear();
        }

        bool LeaveRace(IMyPlayer p)
        {
            StaticRacerInfo info;
            if (GetStaticInfo(p, out info))
            {
                info.NextNode = null;
                info.InFinish = false;
                info.Laps = 0;
                info.Timer.Reset(true);
                info.PreviousTick = null;
            }

            RemoveWaypoint(p.IdentityId);

            if(activePlayers.Remove(p.SteamUserId))
            {
                MyVisualScriptLogicProvider.ShowNotification("You have left the race.", RacingConstants.defaultMsgMs, "White", p.IdentityId);
                return true;
            }
            else
            {
                MyVisualScriptLogicProvider.ShowNotification("You are not in the race.", RacingConstants.defaultMsgMs, "White", p.IdentityId);
                return false;
            }
        }

        void ProcessValues ()
        {
            if (nodes.Count < 2)
            {
                activeRacersText.Clear();
                activeRacersText.Append("Waiting for beacon nodes...").AppendLine();
                return;
            }

            SortedSet<RacerInfo> ranking = new SortedSet<RacerInfo>(new RacerDistanceComparer());
            playersTemp.Clear();
            MyAPIGateway.Players.GetPlayers(playersTemp, IsPlayerActive);
            foreach (IMyPlayer p in playersTemp)
            {
                StaticRacerInfo info = GetStaticInfo(p);

                // Find the closest node
                //int currLaps = GetNumLaps(p);
                bool missed;
                double dist;
                int? nextNode = GetNextNode(p, info, out missed, out dist);

                if (!nextNode.HasValue)
                {
                    info.Timer.Stop();
                    continue;
                }

                if (nextNode > 0 && nextNode < nodes.Count)
                    info.Timer.Start();
                else
                    info.Timer.Stop();
                    
                RacingBeacon destination = nodes[nextNode.Value];

                if (info.InFinish)
                {
                    missed = false; // When racer has crossed the finish, the racer will be ahead of the first node
                    if (finish == null || !finish.Contains(p))
                        info.InFinish = false;
                }

                // Check if racer is on the track
                if (dist >= 0)
                {
                    // Check if it's a finish
                    if(nextNode == nodes.Count - 1 && finish != null && finish.Contains(p))
                    {
                        // If the closest node is the last node, a finish exists, and the grid is intersecting with the finish
                        if (info.Laps >= MapSettings.NumLaps - 1)
                        {
                            // The racer has finished all laps
                            int rank = finishers.Count + 1;
                            finishers.Add(p.SteamUserId);
                            info.Finish();
                            FinishersUpdated();

                            if(MapSettings.TimedMode)
                                MyVisualScriptLogicProvider.ShowNotificationToAll($"{p.DisplayName} just finished with time {info.Timer.GetTime(RacingConstants.timerFormating)}", RacingConstants.defaultMsgMs, "White");
                            else
                                MyVisualScriptLogicProvider.ShowNotificationToAll($"{p.DisplayName} just finished in position {rank}", RacingConstants.defaultMsgMs, "White");
                            LeaveRace(p);
                        }
                        else
                        { 
                            // The racer needs more laps
                            if(!info.InFinish)
                            {
                                info.InFinish = true;
                                MyVisualScriptLogicProvider.ShowNotification($"Lap {info.Laps + 2} / {MapSettings.NumLaps}", RacingConstants.defaultMsgMs, "White", p.IdentityId);
                                info.Laps++;
                            }
                            info.NextNode = 0;
                            RacerInfo racer = new RacerInfo(p, dist, missed)
                            {
                                Destination = destination
                            };
                            ranking.Add(racer);
                        }
                    }
                    else
                    {
                        RacerInfo racer = new RacerInfo(p, dist, missed)
                        {
                            Destination = destination
                        };
                        ranking.Add(racer);
                    }
                }
                else
                {
                    RemoveWaypoint(p.IdentityId);
                }
            }

            BuildText(ranking);
        }

        int? GetNextNode(IMyPlayer p, StaticRacerInfo info, out bool missed, out double dist)
        {
            if (info.PreviousTick.HasValue && RacingTools.GetCockpit(p) == null)
            {
                missed = info.PreviousTick.Value.Missed;
                dist = info.PreviousTick.Value.Distance;
                return info.PreviousTick.Value.Destination.Index;
            }

            missed = false;
            dist = info.Laps * nodeDistances [nodes.Count - 1];

            int nextNode;
            bool isNew = !info.NextNode.HasValue;
            if (isNew)
            {
                info.NextNode = 0;
                nextNode = 0;
                if (MapSettings.NumLaps > 1)
                    MyVisualScriptLogicProvider.ShowNotification($"Lap {info.Laps + 1} / {MapSettings.NumLaps}", RacingConstants.defaultMsgMs, "White", p.IdentityId);
            }
            else
            {
                nextNode = info.NextNode.Value;
            }

            int start;
            int end;
            double partial;
            GetClosestSegment(p.GetPosition(), out start, out end, out partial);

            if (start < 0)
            {
                return nextNode;
                // Before start
                // end is 0
                // show whatever predefined nextNode there is
            }
            else if (start >= nodes.Count - 1)
            {
                // After end
                if (nextNode > 0)
                {
                    // Has been on the track previously
                    if (finish == null)
                    {
                        if (nextNode >= nodes.Count - 1 && NodeCleared(p, nodes.Count - 1))
                        {
                            // The previous node is the last node.
                            LeaveRace(p);
                            return null;
                        }
                        else
                        {
                            // The previous node is on the track somewhere
                            missed = true;
                            if (nextNode > nodes.Count - 1)
                                nextNode = nodes.Count - 1;
                            dist += nodeDistances [nextNode];
                            return nextNode;
                        }
                    }
                    else
                    {
                        if (nextNode >= nodes.Count - 1)
                        {
                            // The previous node is the last node.
                            // To get here, a collision with the finish must have failed.
                            missed = true;
                        }
                        dist += nodeDistances [nextNode];
                        return nextNode;
                    }
                }
                else
                {
                    // The racer has not yet entered the track, display the first node
                    return 0;
                }
            }
            else
            {
                // On the track
                if (isNew)
                {
                    info.NextNode = end;
                    nextNode = end;
                }

                if (end <= nextNode)
                {
                    // Racer is in expected position.
                    if(end == nextNode)
                    {
                        // This should help prevent collisions from not being detected by attempting the check earlier
                        RacingBeacon node = nodes [end];
                        if(node.Type == RacingBeacon.BeaconType.CHECKPOINT && node.Contains(p))
                            info.NextNode = end + 1;
                    }

                    dist += nodeDistances [start] + partial;
                    return nextNode;
                }
                else if (start == nextNode && NodeCleared(p, nextNode))
                {
                    // Racer has moved past one node.
                    info.NextNode = end;
                    dist += nodeDistances [start] + partial;
                    return end;
                }
                else
                {
                    // Racer has moved past multiple nodes, clamp them.
                    missed = start >= nextNode;

                    if (info.PreviousTick.HasValue)
                        dist = info.PreviousTick.Value.Distance;
                    else
                        dist += nodeDistances [nextNode];
                    return nextNode;
                }
            }
        }

        /// <summary>
        /// Use ONLY on server, not client!
        /// </summary>
        bool GetClosestRacer(Vector3D position, out IMyPlayer closest)
        {
            double minDistance = double.PositiveInfinity;
            closest = null;

            playersTemp.Clear();
            MyAPIGateway.Players.GetPlayers(playersTemp, (p) => activePlayers.Contains(p.SteamUserId));
            foreach (IMyPlayer p in playersTemp)
            {
                double dist = Vector3D.DistanceSquared(p.GetPosition(), position);
                if (dist < minDistance)
                {
                    closest = p;
                    minDistance = dist;
                }
            }
            return closest != null;
        }

        bool NodeCleared (IMyPlayer p, int reachedNode)
        {
            RacingBeacon node = nodes [reachedNode];
            return node.Type == RacingBeacon.BeaconType.NODE || node.Contains(p);
        }

        private void FinishersUpdated ()
        {
            // Build the final racer text
            tempSb.Clear();

            if (finishers.Count > 0)
            {
                tempSb.Append(RacingConstants.colorFinalist);
                
                IComparer<StaticRacerInfo> comp;
                if (MapSettings.TimedMode)
                    comp = new RacerBestTimeComparer();
                else
                    comp = new RacerRankComparer();
                SortedSet<StaticRacerInfo> sorted = new SortedSet<StaticRacerInfo>(comp);

                foreach (ulong current in finishers)
                    sorted.Add(staticRacerInfo [current]);

                int i = 0;
                foreach (StaticRacerInfo info in sorted)
                {
                    i++;
                    tempSb.Append(RacingTools.SetLength(i, RacingConstants.numberWidth)).Append(' ');
                    tempSb.Append(RacingTools.SetLength(info.Name, RacingConstants.nameWidth));
                    if(MapSettings.TimedMode)
                        tempSb.Append(' ').Append(info.BestTime.ToString(RacingConstants.timerFormating));
                    tempSb.AppendLine();
                }

                tempSb.Length--;
                tempSb.Append(RacingConstants.colorWhite).AppendLine();
            }
            finalRacersString = tempSb.ToString();
        }

        private void RemoveWaypoint (long identityId)
        {
            MyVisualScriptLogicProvider.RemoveGPS(RacingConstants.gateWaypointName, identityId);
        }

        void DrawRemoveWaypoint(RacerInfo info)
        {
            RemoveWaypoint(info.Racer.IdentityId);

            RacingBeacon node = info.Destination;

            MyVisualScriptLogicProvider.AddGPSObjective(RacingConstants.gateWaypointName, RacingConstants.gateWaypointDescription, node.Coords,
                RacingConstants.gateWaypointColor, 0, info.Racer.IdentityId);
        }

        void RenderWaypoint(RacerInfo current, RacerInfo previous)
        {
            if (current.Destination == null) // Before start or after finish
            {
                if (previous.Destination != null) // Player just left the track
                    RemoveWaypoint(current.Racer.IdentityId);
            }
            else // Player is on the track
            {
                // Player just took control, or Player just got on the track, or Player has crossed a node/checkpoint
                if (previous.Destination == null || current.Destination != previous.Destination)
                    DrawRemoveWaypoint(current);
            }
        }
        
        void GetClosestSegment(Vector3D racerPos, out int start, out int end, out double partialDist)
        {
            start = 0;
            end = -1;
            partialDist = 0;
            double minLinear2 = double.PositiveInfinity;

            if (nodes.Count < 2)
                return;

            int closest = 0;
            long minDistanceGrid = nodes [0].Beacon.CubeGrid.EntityId;
            double minDistance2 = Vector3D.DistanceSquared(nodes [0].Coords, racerPos);

            for (int i = 1; i < nodes.Count; i++)
            {
                Vector3D node = nodes [i].Coords;
                Vector3D segment = nodes [i - 1].Coords - node;
                double segmentLen = segment.Length();
                if (segmentLen <= 0)
                    continue;
                segment /= segmentLen;

                long beaconGrid = nodes [i].Beacon.CubeGrid.EntityId;
                Vector3D endToRacer = racerPos - node;
                double dist2 = endToRacer.LengthSquared();
                bool verifyGrid = true;
                if (dist2 < minDistance2 && beaconGrid != minDistanceGrid)
                {
                    verifyGrid = false;
                    minDistanceGrid = beaconGrid;
                    minDistance2 = dist2;
                    closest = i;
                }

                double scaler = RacingTools.ScalarProjection(endToRacer, segment);
                if (scaler <= 0 || scaler > segmentLen || (verifyGrid && beaconGrid == minDistanceGrid))
                    continue;
                double linear2 = (endToRacer - (scaler * segment)).LengthSquared();

                if(linear2 < minLinear2)
                {
                    start = i - 1;
                    end = i;
                    partialDist = segmentLen - scaler;
                    minLinear2 = linear2;
                }
            }

            if(minDistance2 < minLinear2)
            {
                if (closest > nodes.Count - 1)
                    closest = nodes.Count - 1; // Racer is past the last node
                if (closest <= 0)
                    closest = -1;
                start = closest;
                end = closest + 1;
                partialDist = 0;
            }
        }

        void BuildText (SortedSet<RacerInfo> ranking)
        {
            // Build the active racer text
            activeRacersText.Clear();
            if (ranking.Count == 0 && finishers.Count == 0)
            {
                activeRacersText.Append("No racers in range.");
                return;
            }

            activeRacersText.Append(hudHeader);
            if (finishers.Count > 0)
                activeRacersText.Append(finalRacersString);

            if (ranking.Count > 0)
            {
                bool drawWhite = false;

                int i = finishers.Count + 1;
                double previousDist = nodeDistances[nodes.Count - 1] * MapSettings.NumLaps;

                SendNextSpectatorResponse(ranking.Last().RacerId, ranking.First().Racer);
                SendPrevSpectatorResponse(ranking.First().RacerId, ranking.Last().Racer);

                ulong previousId = 0;
                IMyPlayer previousRacer = null;
                foreach (RacerInfo racer in ranking)
                {
                    StaticRacerInfo staticInfo = GetStaticInfo(racer.Racer);
                    RacerInfo current = racer;
                    current.Rank = i;

                    if (previousId != 0)
                    {
                        // Some spectators need this racer as their current racer.
                        SendNextSpectatorResponse(previousId, current.Racer);
                    }
                    if (previousRacer != null)
                    {
                        // Some spectators need this racer as their current racer.
                        SendPrevSpectatorResponse(racer.RacerId, previousRacer);
                    }

                    string drawnColor = null;
                    if (staticInfo.PreviousTick.HasValue)
                    {
                        RacerInfo previous = staticInfo.PreviousTick.Value;
                        RenderWaypoint(current, previous);

                        if (!Moved(current))
                        {
                            // stationary
                            drawnColor = RacingConstants.colorStationary;
                        }
                        else if (numRacersPreviousTick == ranking.Count && current.Rank < previous.Rank)
                        {
                            // just ranked up
                            current.RankUpFrame = Runtime;
                            drawnColor = RacingConstants.colorRankUp;
                        }
                        else if (previous.RankUpFrame > 0)
                        {
                            // recently ranked up
                            if (previous.RankUpFrame + RacingConstants.rankUpTime > Runtime)
                                current.RankUpFrame = previous.RankUpFrame;
                            drawnColor = RacingConstants.colorRankUp;
                        }
                    }
                    else
                    {
                        if (current.Destination != null)
                            DrawRemoveWaypoint(current);
                    }

                    if (drawnColor != null)
                    {
                        activeRacersText.Append(drawnColor);
                        drawWhite = true;
                    }
                    else if (drawWhite)
                    {
                        activeRacersText.Append(RacingConstants.colorWhite);
                        drawWhite = false;
                    }

                    // <num>
                    activeRacersText.Append(RacingTools.SetLength(i, RacingConstants.numberWidth)).Append(' ');

                    // <num> <name>
                    activeRacersText.Append(RacingTools.SetLength(current.Name, RacingConstants.nameWidth)).Append(' ');

                    // <num> <name> <distance>
                    if(current.Missed)
                    {
                        activeRacersText.Append(RacingTools.SetLength("missed", RacingConstants.distWidth));
                    }
                    else
                    {
                        if(MapSettings.TimedMode)
                        {
                            activeRacersText.Append(RacingTools.SetLength(staticInfo.Timer.GetTime(RacingConstants.timerFormating), RacingConstants.distWidth));
                        }
                        else
                        {
                            activeRacersText.Append(RacingTools.SetLength((int)(previousDist - current.Distance), RacingConstants.distWidth));
                            previousDist = current.Distance;
                        }
                    }

                    int lap = staticInfo.Laps;
                    if (MapSettings.NumLaps > 1)
                        activeRacersText.Append(' ').Append(lap + 1);

                    activeRacersText.AppendLine();

                    staticInfo.PreviousTick = current;

                    i++;
                    previousId = current.RacerId;
                    previousRacer = current.Racer;
                }
                numRacersPreviousTick = i - 1;
            }

            foreach(KeyValuePair<ulong, HashSet<ulong>> kv in nextPlayerRequests)
            {
                foreach(ulong requestor in kv.Value)
                    SendNextRacerInfo(requestor);
            }
            nextPlayerRequests.Clear();


            foreach (KeyValuePair<ulong, HashSet<ulong>> kv in prevPlayerRequests)
            {
                foreach (ulong requestor in kv.Value)
                    SendNextRacerInfo(requestor);
            }
            prevPlayerRequests.Clear();
        }

        bool Moved (RacerInfo value)
        {
            IMyCubeGrid grid = RacingTools.GetCockpit(value.Racer)?.CubeGrid;
            if (grid?.Physics == null)
                return false;
            return grid.Physics.LinearVelocity.LengthSquared() > RacingConstants.moveThreshold2;
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
