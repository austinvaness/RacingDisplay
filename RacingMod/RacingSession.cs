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
using VRage.ModAPI;
using VRage.Utils;
using VRageMath;
using ProtoBuf;
using System.Collections.Concurrent;
using BlendTypeEnum = VRageRender.MyBillboard.BlendTypeEnum;
using KlimeDraygo.RelativeSpectator.API;
using VRage.Input;

namespace RacingMod
{
    [MySessionComponentDescriptor(MyUpdateOrder.AfterSimulation | MyUpdateOrder.BeforeSimulation)]
    public partial class RacingSession : MySessionComponentBase
    {
        public int Runtime = 0;

        bool timeMode = false;

        int numLaps = 1;

        bool debug = false;

        const string timerFormating = "mm\\:ss\\:ff";

        const int defaultMsgMs = 4000;

        const ushort packetMainId = 1337;
        const ushort packetCmd = 1339;

        public static RacingSession Instance;
        
        private readonly List<RacingBeacon> nodes = new List<RacingBeacon>();
        private RacingBeacon finish;
        private double[] nodeDistances = { };

        private HudAPIv2 textApi;
        private Vector2D activeHudPosition = new Vector2D(-0.95, 0.90);
        private Vector2D infoHudPosition = new Vector2D(1, 1);
        private HudAPIv2.HUDMessage activeRacersHud;
        private HudAPIv2.HUDMessage infoHud;
        private HudAPIv2.MenuKeybindInput nextRacerInput;
        private HudAPIv2.MenuKeybindInput prevRacerInput;
        private HudAPIv2.MenuCategoryBase menuRoot;
        private readonly StringBuilder activeRacersText = new StringBuilder("Initializing...");
        private readonly StringBuilder infoHudText = new StringBuilder();
        private string finalRacersString;

        const int numberWidth = 2; 
        const int distWidth = 10;
        const int nameWidth = 20;

        const double moveThreshold2 = 16; // Squared minimum velocity
        private const int rankUpTime = 90;
        private bool running = false;

        private Dictionary<ulong, StaticRacerInfo> staticRacerInfo = new Dictionary<ulong, StaticRacerInfo>();

        private Dictionary<ulong, RacerInfo> finishers = new Dictionary<ulong, RacerInfo>();
        private readonly HashSet<ulong> activePlayers = new HashSet<ulong>();
        private int numRacersPreviousTick = 0;

        readonly ConcurrentDictionary<ulong, HashSet<ulong>> nextPlayerRequests = new ConcurrentDictionary<ulong, HashSet<ulong>>();
        readonly ConcurrentDictionary<ulong, HashSet<ulong>> prevPlayerRequests = new ConcurrentDictionary<ulong, HashSet<ulong>>();

        SpecCamAPI Spec;

        private string hudHeader;

        private const string headerColor = "<color=127,127,127>";
        const string colorWhite = "<color=white>";
        const string colorStationary = "<color=255,124,124>";
        const string colorRankUp = "<color=124,255,154>";
        const string colorFinalist = "<color=140,255,255>";

        private readonly Color gateWaypointColor = new Color(0, 255, 255); // nodes and cleared checkpoints
        const string gateWaypointName = "Waypoint";
        const string gateWaypointDescription = "The next waypoint to guide you through the race.";
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
            textApi = new HudAPIv2(CreateHudItems);

            Spec = new SpecCamAPI();

            gateWaypointGps = MyAPIGateway.Session.GPS.Create(gateWaypointName, gateWaypointDescription, Vector3D.Zero, true).Hash;

            MyAPIGateway.Utilities.MessageEntered += MessageEntered;
            if (MyAPIGateway.Multiplayer.IsServer)
            {
                MyVisualScriptLogicProvider.RemoveGPSForAll(gateWaypointName);
                MyAPIGateway.Multiplayer.RegisterMessageHandler(packetCmd, ReceiveCmd);
                MyAPIGateway.Multiplayer.RegisterMessageHandler(packetSpecRequest, ReceiveSpecRequest);

            }
            else
            {
                MyAPIGateway.Session.GPS.RemoveLocalGps(gateWaypointGps);
                MyAPIGateway.Multiplayer.RegisterMessageHandler(packetMainId, ReceiveActiveRacers);
                MyAPIGateway.Multiplayer.RegisterMessageHandler(packetSpecResponse, ReceiveSpecResponse);
            }

            config = RacingPreferences.LoadXML();

            running = true;
        }

        public override void UpdateBeforeSimulation ()
        {
            if (MyAPIGateway.Session == null)
                return;

            try
            {
                if (!running)
                    Start();

                if (MyAPIGateway.Session?.Player != null && MyAPIGateway.Session.Config != null && activeRacersHud != null &&
                    MyAPIGateway.Input.GetGameControl(MyControlsSpace.TOGGLE_HUD).IsNewPressed())
                {
                    if (MyAPIGateway.Session.Config.HudState == 1)
                    {
                        if (activeRacersHud.Visible)
                        {
                            activeRacersHud.Visible = false;
                            infoHud.Visible = false;
                            MyVisualScriptLogicProvider.SetHudState(0);
                        }
                        else
                        {
                            activeRacersHud.Visible = true;
                            infoHud.Visible = true;
                        }
                    }
                    else
                    {
                        activeRacersHud.Visible = true;
                        infoHud.Visible = true;
                    }
                }
            }
            catch (Exception e)
            {
                ShowError(e, GetType());
            }
        }

        public override void UpdateAfterSimulation ()
        {
            if (!running || MyAPIGateway.Session == null)
                return;

            Runtime++;
            try
            {
                if (MyAPIGateway.Session.Player != null)
                    Spectator();

                if (MyAPIGateway.Session.IsServer)
                {
                    ProcessValues();
                    BroadcastData(activeRacersText, packetMainId);
                    DrawDebug();
                }
            }
            catch (Exception e)
            {
                ShowError(e, GetType());
            }
        }

        protected override void UnloadData ()
        {
            if (running)
            {
                MyAPIGateway.Utilities.MessageEntered -= MessageEntered;

                if (MyAPIGateway.Multiplayer.IsServer)
                {
                    MyAPIGateway.Multiplayer.UnregisterMessageHandler(packetMainId, ReceiveActiveRacers);
                }
            }

            textApi?.Unload();

            Instance = null;
        }

        public void UpdateHeader()
        {
            tempSb.Clear();
            tempSb.Append(headerColor).Append("#".PadRight(numberWidth + 1));
            tempSb.Append("Name".PadRight(nameWidth + 1));
            tempSb.Append("Position".PadRight(distWidth + 1));
            if (numLaps > 1)
                tempSb.Append("Lap");
            tempSb.AppendLine().Append(colorWhite);
            hudHeader = tempSb.ToString();
        }

        public void UpdateInfo()
        {
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
                    numLaps = Math.Max((int)node.NodeNumber, 1);
                    UpdateHeader();
                    nodes.Add(node);
                    finish = node;
                }
                else
                {
                    node.Beacon.CustomData = "";
                    return;
                }
            }
            else if (AddSorted(nodes, node))
            {
                if (finish != null && finish.Beacon == node.Beacon)
                {
                    numLaps = 1;
                    UpdateHeader();
                    finish = null;
                }
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
                numLaps = 1;
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
                IMyPlayer p = GetPlayer(cmd.steamId);
                bool temp = false;
                if (p != null)
                    ProcessCommand(p, cmd.cmd, ref temp);
            }
            catch (Exception e)
            {
                ShowError(e, GetType());
            }
        }

        IMyPlayer GetPlayer(ulong steamId)
        {
            playersTemp.Clear();
            MyAPIGateway.Players.GetPlayers(playersTemp);
            foreach (IMyPlayer p in playersTemp)
            {
                if (p.SteamUserId == steamId)
                    return p;
            }
            return null;
        }

        IMyPlayer GetPlayer(string name)
        {
            name = name.ToLower();
            playersTemp.Clear();
            MyAPIGateway.Players.GetPlayers(playersTemp);
            foreach (IMyPlayer p in playersTemp)
            {
                if (p.DisplayName.ToLower().Contains(name))
                    return p;
            }
            return null;
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
                ShowError(e, GetType());
            }
        }

        private void CreateHudItems ()
        {
            activeRacersHud = new HudAPIv2.HUDMessage(activeRacersText, activeHudPosition, HideHud: false, Font: "monospace", Blend: BlendTypeEnum.PostPP);
            infoHud = new HudAPIv2.HUDMessage(infoHudText, infoHudPosition, HideHud: false, Blend: BlendTypeEnum.PostPP);

            menuRoot = new HudAPIv2.MenuRootCategory("Racer Spectator", HudAPIv2.MenuRootCategory.MenuFlag.PlayerMenu, "Spectator Camera Options");
            nextRacerInput = new HudAPIv2.MenuKeybindInput("Next Racer - " + config.NextPlayer.ToString(), menuRoot, "Press any Key [Next Racer]", SetNextPlayerKey);
            prevRacerInput = new HudAPIv2.MenuKeybindInput("Previous Racer - " + config.PrevPlayer.ToString(), menuRoot, "Press any Key [Previous Racer]", SetPrevPlayerKey);
        }

        private void SetPrevPlayerKey (MyKeys key, bool arg1, bool arg2, bool arg3)
        {
            config.PrevPlayer = key;
            prevRacerInput.Text = "Previous Racer - " + config.PrevPlayer.ToString();
        }

        private void SetNextPlayerKey (MyKeys key, bool arg1, bool arg2, bool arg3)
        {
            config.NextPlayer = key;
            nextRacerInput.Text = "Next Racer - " + config.NextPlayer.ToString();
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
                ShowError(e, GetType());
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
                    if(activeRacersHud != null)
                        activeRacersHud.Visible = !activeRacersHud.Visible;
                        infoHud.Visible = !infoHud.Visible;
                    break;
                case "join":
                    if (MyAPIGateway.Session.IsServer)
                        JoinRace(p);
                    else
                        redirect = true;
                    break;
                case "leave":
                    if (MyAPIGateway.Session.IsServer)
                        LeaveRace(p);
                    else
                        redirect = true;
                    break;
                case "rejoin":
                    if (MyAPIGateway.Session.IsServer)
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
                    if (p.PromoteLevel != MyPromoteLevel.Owner && p.PromoteLevel != MyPromoteLevel.Admin)
                        return;

                    if (MyAPIGateway.Session.IsServer)
                    {
                        if(cmd.Length == 2)
                        {
                            ClearFinishers();
                        }
                        else if(cmd.Length == 3)
                        {
                            IMyPlayer result = GetPlayer(cmd [2]);
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
                        FinishesUpdated();
                        MyVisualScriptLogicProvider.SendChatMessage("Cleared all finishers.", "rcd", p.IdentityId, "Red");
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
                    if (p.PromoteLevel != MyPromoteLevel.Owner && p.PromoteLevel != MyPromoteLevel.Admin)
                        return;

                    if (MyAPIGateway.Session.IsServer)
                    {
                        timeMode = !timeMode;
                        if(timeMode)
                            MyVisualScriptLogicProvider.SendChatMessage("Mode is now timed.", "rcd", p.IdentityId, "Red");
                        else
                            MyVisualScriptLogicProvider.SendChatMessage("Mode is now normal.", "rcd", p.IdentityId, "Red");

                        FinishesUpdated();
                    }
                    else
                    {
                        redirect = true;
                    }
                    break;
                case "grant":
                    if (p.PromoteLevel != MyPromoteLevel.Owner && p.PromoteLevel != MyPromoteLevel.Admin)
                        return;

                    if (cmd.Length != 3)
                    {
                        MyVisualScriptLogicProvider.SendChatMessage("Usage:\n/rcd grant <name>: Fixes a 'missing' error on the ui.", "rcd", p.IdentityId, "Red");
                        return;
                    }

                    if (MyAPIGateway.Session.IsServer)
                    {
                        IMyPlayer result = GetPlayer(cmd [2]);
                        if (result == null)
                        {
                            MyVisualScriptLogicProvider.SendChatMessage($"No racer was found with a name containing '{cmd [2]}'.", "rcd", p.IdentityId, "Red");
                        }
                        else
                        {
                            MyVisualScriptLogicProvider.SendChatMessage($"Reset {result.DisplayName}'s missing status.", "rcd", p.IdentityId, "Red");
                            StaticRacerInfo info;
                            if (StaticInfo(p, out info))
                                info.NextNode = null;
                        }
                        return;
                    }
                    else
                    {
                        redirect = true;
                    }
                    break;
                case "kick":
                    if (p.PromoteLevel != MyPromoteLevel.Owner && p.PromoteLevel != MyPromoteLevel.Admin)
                        return;

                    if (cmd.Length != 3)
                    {
                        MyVisualScriptLogicProvider.SendChatMessage("Usage:\n/rcd kick <name>: Removes a player from the race.", "rcd", p.IdentityId, "Red");
                        return;
                    }

                    if (MyAPIGateway.Session.IsServer)
                    {
                        IMyPlayer result = GetPlayer(cmd [2]);
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
                                MyVisualScriptLogicProvider.ShowNotification("You have been kicked from the race.", defaultMsgMs, "White", result.IdentityId);
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
                MyAPIGateway.Multiplayer.SendMessageToServer(packetCmd, data);
            }
        }

        private bool StaticInfo (IMyPlayer p, out StaticRacerInfo info)
        {
            if (staticRacerInfo.TryGetValue(p.SteamUserId, out info))
                return true;
            info = null;
            return false;
        }

        private StaticRacerInfo StaticInfo(IMyPlayer p)
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
            if (p.PromoteLevel == MyPromoteLevel.Owner || p.PromoteLevel == MyPromoteLevel.Admin)
                s = "\nAdmin Commands:\n/rcd clear [name]: Removes finalist(s).\n" +
                    "/rcd grant <name>: Fixes a racer's 'missing' status.\n/rcd mode: Toggles timed mode.\n" +
                    "/rcd kick <name>: Removes a racer from the race." + s;
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
            if (activePlayers.Add(p.SteamUserId))
            {
                StaticRacerInfo info = StaticInfo(p);
                info.NextNode = null;
                info.InFinish = false;
                info.Laps = 0;
                info.Timer.Reset(true);
                info.PreviousTick = null;
                if (!timeMode && RemoveFinisher(p.SteamUserId))
                    FinishesUpdated();
                MyVisualScriptLogicProvider.ShowNotification("You have joined the race.", defaultMsgMs, "White", p.IdentityId);
                return true;
            }
            else
            {
                MyVisualScriptLogicProvider.ShowNotification("You are already in the race.", defaultMsgMs, "White", p.IdentityId);
                return false;
            }
        }

        bool RemoveFinisher(ulong id)
        {
            if(finishers.Remove(id))
            {
                StaticRacerInfo info;
                if (staticRacerInfo.TryGetValue(id, out info))
                    info.BestTime = new TimeSpan(0);
                return true;
            }
            return false;
        }

        void ClearFinishers()
        {
            foreach (ulong id in finishers.Keys)
            {
                StaticRacerInfo info;
                if (staticRacerInfo.TryGetValue(id, out info))
                    info.BestTime = new TimeSpan(0);
            }
            finishers.Clear();
        }

        bool LeaveRace(IMyPlayer p)
        {
            StaticRacerInfo info;
            if (StaticInfo(p, out info))
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
                MyVisualScriptLogicProvider.ShowNotification("You have left the race.", defaultMsgMs, "White", p.IdentityId);
                return true;
            }
            else
            {
                MyVisualScriptLogicProvider.ShowNotification("You are not in the race.", defaultMsgMs, "White", p.IdentityId);
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

            SortedSet<RacerInfo> ranking;
            if (timeMode)
                ranking = new SortedSet<RacerInfo>(new RacerTimeComparer());
            else
                ranking = new SortedSet<RacerInfo>(new RacerDistanceComparer());
            playersTemp.Clear();
            MyAPIGateway.Players.GetPlayers(playersTemp, IsPlayerActive);
            foreach (IMyPlayer p in playersTemp)
            {
                StaticRacerInfo info = StaticInfo(p);

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
                        if (info.Laps >= numLaps - 1)
                        {
                            // The racer has finished all laps
                            int rank = finishers.Count + 1;
                            RacerInfo finishInfo = new RacerInfo(p, 0, rank, missed);
                            finishers[p.SteamUserId] = finishInfo;
                            info.PreviousTick = finishInfo;
                            info.Finish();
                            FinishesUpdated();

                            if(timeMode)
                                MyVisualScriptLogicProvider.ShowNotificationToAll($"{p.DisplayName} just finished with time {info.Timer.GetTime(timerFormating)}", defaultMsgMs, "White");
                            else
                                MyVisualScriptLogicProvider.ShowNotificationToAll($"{p.DisplayName} just finished in position {rank}", defaultMsgMs, "White");
                            LeaveRace(p);
                        }
                        else
                        { 
                            // The racer needs more laps
                            if(!info.InFinish)
                            {
                                info.InFinish = true;
                                MyVisualScriptLogicProvider.ShowNotification($"Lap {info.Laps + 2} / {numLaps}", defaultMsgMs, "White", p.IdentityId);
                                info.Laps++;
                            }
                            info.NextNode = 0;
                            RacerInfo racer = new RacerInfo(p, dist, 0, missed)
                            {
                                Destination = destination
                            };
                            ranking.Add(racer);
                        }
                    }
                    else
                    {
                        RacerInfo racer = new RacerInfo(p, dist, 0, missed)
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
            if (info.PreviousTick.HasValue && GetCockpit(p) == null)
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
                if (numLaps > 1)
                    MyVisualScriptLogicProvider.ShowNotification($"Lap {info.Laps + 1} / {numLaps}", defaultMsgMs, "White", p.IdentityId);
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

        IMyEntity GetEntity(IMyPlayer p)
        {
            IMyEntity e = p.Controller?.ControlledEntity?.Entity;
            if (e is IMyCubeBlock)
                return ((IMyCubeBlock)e).CubeGrid;
            return e;
        }

        public static IMyCubeBlock GetCockpit (IMyPlayer p)
        {
            return p?.Controller?.ControlledEntity?.Entity as IMyCubeBlock;
        }

        private void RequestNextRacer (CurrentRacerInfo info)
        {
            if (MyAPIGateway.Session.IsServer)
            {
                AddSpecRequest(info);
            }
            else
            {
                byte [] data = MyAPIGateway.Utilities.SerializeToBinary(info);
                MyAPIGateway.Multiplayer.SendMessageToServer(packetSpecRequest, data);
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

        private void FinishesUpdated ()
        {
            // Build the final racer text
            tempSb.Clear();

            if (finishers.Count > 0)
            {
                tempSb.Append(colorFinalist);
                int i = 0;
                if(timeMode)
                {
                    SortedSet<StaticRacerInfo> sorted = new SortedSet<StaticRacerInfo>(new RacerBestTimeComparer());
                    foreach (ulong current in finishers.Keys)
                        sorted.Add(staticRacerInfo [current]);

                    foreach (StaticRacerInfo info in sorted)
                    {
                        i++;
                        tempSb.Append(SetLength(i, numberWidth)).Append(' ');
                        tempSb.Append(SetLength(info.Name, nameWidth)).Append(' ');
                        tempSb.Append(info.BestTime.ToString("mm\\:ss\\:ff"));
                        tempSb.AppendLine();
                    }
                }
                else
                {
                    foreach (RacerInfo current in finishers.Values)
                    {
                        i++;
                        tempSb.Append(SetLength(i, numberWidth)).Append(' ');
                        tempSb.Append(SetLength(current.Name, nameWidth));
                        tempSb.AppendLine();
                    }
                }

                tempSb.Length--;
                tempSb.Append(colorWhite).AppendLine();
            }
            finalRacersString = tempSb.ToString();
        }

        private void RemoveWaypoint (long identityId)
        {
            MyVisualScriptLogicProvider.RemoveGPS(gateWaypointName, identityId);
        }

        void DrawRemoveWaypoint(RacerInfo info)
        {
            RemoveWaypoint(info.Racer.IdentityId);

            RacingBeacon node = info.Destination;

            MyVisualScriptLogicProvider.AddGPSObjective(gateWaypointName, gateWaypointDescription, node.Coords, 
                gateWaypointColor, 0, info.Racer.IdentityId);
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

                double scaler = ScalarProjection(endToRacer, segment);
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
                numRacersPreviousTick = 0;
                //Dictionary<ulong, RacerInfo> newPrevRacerInfos = new Dictionary<ulong, RacerInfo>(ranking.Count);

                bool drawWhite = false;

                int i = finishers.Count + 1;
                double previousDist = nodeDistances[nodes.Count - 1] * numLaps;

                SendNextSpectatorResponse(ranking.Last().RacerId, ranking.First().Racer);
                SendPrevSpectatorResponse(ranking.First().RacerId, ranking.Last().Racer);

                ulong previousId = 0;
                IMyPlayer previousRacer = null;
                foreach (RacerInfo racer in ranking)
                {
                    StaticRacerInfo staticInfo = StaticInfo(racer.Racer);
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
                            drawnColor = colorStationary;
                        }
                        else if (numRacersPreviousTick == ranking.Count && current.Rank < previous.Rank)
                        {
                            // just ranked up
                            current.RankUpFrame = Runtime;
                            drawnColor = colorRankUp;
                        }
                        else if (previous.RankUpFrame > 0)
                        {
                            // recently ranked up
                            if (previous.RankUpFrame + rankUpTime > Runtime)
                                current.RankUpFrame = previous.RankUpFrame;
                            drawnColor = colorRankUp;
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
                        activeRacersText.Append(colorWhite);
                        drawWhite = false;
                    }

                    // <num>
                    activeRacersText.Append(SetLength(i, numberWidth)).Append(' ');

                    // <num> <name>
                    activeRacersText.Append(SetLength(current.Name, nameWidth)).Append(' ');

                    // <num> <name> <distance>
                    if(current.Missed)
                    {
                        activeRacersText.Append(SetLength("missed", distWidth));
                    }
                    else
                    {
                        if(timeMode)
                        {
                            activeRacersText.Append(SetLength(staticInfo.Timer.GetTime("mm\\:ss\\:ff"), distWidth));
                        }
                        else
                        {
                            activeRacersText.Append(SetLength((int)(previousDist - current.Distance), distWidth));
                            previousDist = current.Distance;
                        }
                    }

                    int lap = staticInfo.Laps;
                    if (numLaps > 1)
                        activeRacersText.Append(' ').Append(lap + 1);

                    activeRacersText.AppendLine();

                    numRacersPreviousTick++;
                    staticInfo.PreviousTick = current;

                    i++;
                    previousId = current.RacerId;
                    previousRacer = current.Racer;
                }
            }

            nextPlayerRequests.Clear();
            prevPlayerRequests.Clear();
        }

        bool Moved (RacerInfo value)
        {
            IMyCubeGrid grid = GetCockpit(value.Racer)?.CubeGrid;
            if (grid?.Physics == null)
                return false;
            return grid.Physics.LinearVelocity.LengthSquared() > moveThreshold2;
        }

        static string SetLength(object o, int length, int startIndex = 0)
        {
            string s = "";
            if(o != null)
                s = o.ToString();
            return s.PadRight(length + startIndex).Substring(startIndex, length);
        }

        /// <summary>
        /// Projects a value onto another vector.
        /// </summary>
        /// <param name="guide">Must be of length 1.</param>
        static double ScalarProjection (Vector3D value, Vector3D guide)
        {
            double returnValue = Vector3.Dot(value, guide);
            if (double.IsNaN(returnValue))
                return 0;
            return returnValue;
        }
        
        /// <summary>
        /// Adds an element to a list keeping the list sorted,
        /// or replaces the element if it already exists.
        /// </summary>
        /// <param name="list">List to operate on.</param>
        /// <param name="item">Item to add.</param>
        /// <typeparam name="T">Item type stored in the list.</typeparam>
        /// <returns>A bool indicating whether the item was added.</returns>
        public static bool AddSorted<T>(List<T> list, T item) where T: IComparable<T>
        {
            // add the element into the list at an index that keeps the list sorted
            int index = list.BinarySearch(item);
            if (index < 0)
                list.Insert(~index, item);
            return index < 0;
        }
        
        public static void ShowError(Exception e, Type type)
        {
            MyLog.Default.WriteLineAndConsole($"ERROR in {type.FullName}: {e.Message}\n{e.StackTrace}");
            if (MyAPIGateway.Session?.Player != null)
                MyAPIGateway.Utilities.ShowNotification($"[ ERROR: {type.FullName}: {e.Message} | Send SpaceEngineers.Log to mod author ]", 10000, MyFontEnum.Red);
        }

        class RacerDistanceComparer : IComparer<RacerInfo>
        {
            public int Compare (RacerInfo x, RacerInfo y)
            {
                int result = y.Distance.CompareTo(x.Distance);
                if (result == 0)
                    return 1;
                else
                    return result;
            }
        }

        class RacerTimeComparer : IComparer<RacerInfo>, IComparer<StaticRacerInfo>
        {
            public int Compare (RacerInfo x, RacerInfo y)
            {
                var info1 = Instance.StaticInfo(x.Racer);
                var info2 = Instance.StaticInfo(y.Racer);
                return Compare(info1, info2);
            }

            public int Compare (StaticRacerInfo x, StaticRacerInfo y)
            {
                int result = x.Timer.GetTime().CompareTo(y.Timer.GetTime());
                if (result == 0)
                    return 1;
                else
                    return result;
            }
        }

        class RacerBestTimeComparer : IComparer<StaticRacerInfo>
        {
            public int Compare (StaticRacerInfo x, StaticRacerInfo y)
            {
                int result = x.BestTime.CompareTo(y.BestTime);
                if (result == 0)
                    return 1;
                else
                    return result;
            }
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

        [ProtoContract]
        public class NextRacerInfo
        {
            [ProtoMember(1)]
            public ulong steamId;
            [ProtoMember(2)]
            public Vector3 position;

            public NextRacerInfo ()
            {
            }

            public NextRacerInfo (ulong steamId, Vector3 position)
            {
                this.steamId = steamId;
                this.position = position;
            }
        }

        [ProtoContract]
        public class CurrentRacerInfo
        {
            [ProtoMember(1)]
            public ulong requestor;
            [ProtoMember(2)]
            public ulong current;
            [ProtoMember(3)]
            public Vector3 camera;
            [ProtoMember(4)]
            public bool direction;

            public CurrentRacerInfo ()
            {
            }

            public CurrentRacerInfo (ulong requestor, ulong current, Vector3 camera, bool direction)
            {
                this.requestor = requestor;
                this.current = current;
                this.camera = camera;
                this.direction = direction;
            }
        }
    }
}
