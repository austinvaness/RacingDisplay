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
using VRage.Game.ModAPI.Interfaces;

namespace RacingMod
{
    [MySessionComponentDescriptor(MyUpdateOrder.AfterSimulation)]
    public class RacingSession : MySessionComponentBase
    {
        // TODO:
        // Try to crash with many racers at once (formation?) 
        // Test end conditions
        // Test start = finish maps
        // Finish not a node?

        const int defaultMsgMs = 4000;

        const ushort packetMainId = 1337;
        const ushort packetFinalsId = 1338;
        const ushort serverCmdId = 1339;

        public static RacingSession Instance;
        
        private readonly List<RacingBeacon> nodes = new List<RacingBeacon>();
        readonly List<RacingBeacon> lapNodes = new List<RacingBeacon>();
        private RacingBeacon finish;
        private double[] nodeDistances = { };

        private HudAPIv2 textApi;
        private Vector2D activeHudPosition = new Vector2D(-0.95, 0.90);
        private HudAPIv2.HUDMessage activeRacersHud;
        private readonly StringBuilder ActiveRacersText = new StringBuilder("Initializing...");
        private Vector2D finalHudPosition = new Vector2D(0.95, 0.9);
        private HudAPIv2.HUDMessage finalRacersHud;
        private StringBuilder FinalRacersText = new StringBuilder("Initializing...");

        const int numberWidth = 2; 
        const int distWidth = 8;
        const int nameWidth = 20;

        const double moveThreshold2 = 16; // Squared minimum velocity
        private const int rankUpTime = 90;
        private bool running = false;

        Dictionary<ulong, RacerInfo> previousRacerInfos = new Dictionary<ulong, RacerInfo>();
        private readonly Dictionary<ulong, RacerInfo> finishes = new Dictionary<ulong, RacerInfo>();
        readonly Dictionary<ulong, int> reachedNode = new Dictionary<ulong, int>();
        readonly Dictionary<ulong, int> laps = new Dictionary<ulong, int>();

        readonly string hudHeader;
        readonly string finalHudHeader;

        int frameCount = 0;
        private const string headerColor = "<color=127,127,127>";
        const string colorWhite = "<color=white>";
        const string colorStationary = "<color=255,124,124>";
        const string colorRankUp = "<color=124,255,154>";

        private readonly Color gateWaypointColor = new Color(0, 255, 255); // nodes and cleared checkpoints
        const string gateWaypointName = "Waypoint";
        const string gateWaypointDescription = "The next waypoint to guide you through the race.";
        int gateWaypointGps;

        private readonly List<IMyPlayer> playersTemp = new List<IMyPlayer>(); // Temporary
        private readonly HashSet<ulong> activePlayers = new HashSet<ulong>();

        public RacingSession ()
        {
            hudHeader = headerColor +
                        "#".PadRight(numberWidth + 1) +
                        "Name".PadRight(nameWidth + 1) +
                        "Distance".PadRight(distWidth + 1);
            finalHudHeader = headerColor + "#".PadRight(numberWidth + 1) + "Finalist".PadRight(nameWidth);

            hudHeader += "\n" + colorWhite;
            finalHudHeader += "\n" + colorWhite;
        }

        public void RegisterNode(RacingBeacon node)
        {
            if (!node.Valid || node.Type == RacingBeacon.BeaconType.IGNORED)
                return;

            if(node.Type == RacingBeacon.BeaconType.LAP)
            {
                lapNodes.Add(node);
            }
            else if (AddSorted(nodes, node))
            {
                if (node.Type == RacingBeacon.BeaconType.FINISH)
                    finish = node;
                else if (finish == node)
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

            int lapIndex = lapNodes.FindIndex(other => other.Beacon.EntityId == node.Beacon.EntityId);
            if (lapIndex >= 0)
                lapNodes.RemoveAtFast(lapIndex);

            if (finish == node)
                finish = null;
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

            reachedNode.Clear();
            
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
        
        public override void BeforeStart ()
        {
            textApi = new HudAPIv2(CreateHudItems);

            gateWaypointGps = MyAPIGateway.Session.GPS.Create(gateWaypointName, gateWaypointDescription, Vector3D.Zero, true).Hash;

            MyAPIGateway.Utilities.MessageEntered += MessageEntered;
            if(MyAPIGateway.Multiplayer.IsServer)
            {
                MyVisualScriptLogicProvider.RemoveGPSForAll(gateWaypointName);
                MyAPIGateway.Multiplayer.RegisterMessageHandler(serverCmdId, ReceiveCmd);
            }
            else
            {
                MyAPIGateway.Session.GPS.RemoveLocalGps(gateWaypointGps);
                MyAPIGateway.Multiplayer.RegisterMessageHandler(packetMainId, ReceiveActiveRacers);
                MyAPIGateway.Multiplayer.RegisterMessageHandler(packetFinalsId, ReceiveFinalRacers);
            }

            running = true;
            Instance = this;
        }

        private void ReceiveCmd (byte [] obj)
        {
            try
            {
                CommandInfo cmd = MyAPIGateway.Utilities.SerializeFromBinary<CommandInfo>(obj);
                playersTemp.Clear();
                MyAPIGateway.Players.GetPlayers(playersTemp);
                foreach (IMyPlayer p in playersTemp)
                {
                    if (p.SteamUserId == cmd.steamId)
                    {
                        bool temp = false;
                        ProcessCommand(p, cmd.cmd, ref temp);
                        return;
                    }
                }
            }
            catch (Exception e)
            {
                ShowError(e, GetType());
            }
        }

        private void ReceiveActiveRacers(byte[] obj)
        {
            DecodeString(obj, ActiveRacersText);
        }
        private void ReceiveFinalRacers (byte [] obj)
        {
            DecodeString(obj, FinalRacersText);
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
            activeRacersHud = new HudAPIv2.HUDMessage(ActiveRacersText, activeHudPosition, HideHud: false, Font: "monospace");
            FinalRacersText = new StringBuilder(finalHudHeader);
            finalRacersHud = new HudAPIv2.HUDMessage(FinalRacersText, finalHudPosition, HideHud: false, Font: "monospace");
            // Align the text to the right of the window
            Vector2D len = finalRacersHud.GetTextLength();
            len.Y = 0;
            finalRacersHud.Offset = -len;
            FinalRacersText.Clear();
        }

        private void MessageEntered(string messageText, ref bool sendToOthers)
        {
            IMyPlayer p = MyAPIGateway.Session?.Player;
            if (p == null || string.IsNullOrEmpty(messageText) || !messageText.StartsWith("/rcd "))
                return;
            ProcessCommand(p, messageText, ref sendToOthers);
        }
        
        void ProcessCommand(IMyPlayer p, string cmd, ref bool sendToOthers)
        {
            bool redirect = false;
            switch (cmd)
            {
                case "/rcd toggle":
                    if(activeRacersHud != null)
                    {
                        activeRacersHud.Visible = !activeRacersHud.Visible;
                        finalRacersHud.Visible = activeRacersHud.Visible;
                        sendToOthers = false;
                    }
                    break;
                case "/rcd join":
                    sendToOthers = false;
                    if (MyAPIGateway.Session.IsServer)
                    {
                        if(activePlayers.Add(p.SteamUserId))
                        {
                            reachedNode.Remove(p.SteamUserId);
                            if(finishes.Remove(p.SteamUserId))
                                FinishesUpdated();
                            MyVisualScriptLogicProvider.ShowNotification("You have joined the race.", defaultMsgMs, "White", p.IdentityId);
                        }
                        else
                        {
                            MyVisualScriptLogicProvider.ShowNotification("You are already in the race.", defaultMsgMs, "White", p.IdentityId);
                        }
                    }
                    else
                    {
                        redirect = true;
                    }
                    break;
                case "/rcd leave":
                    sendToOthers = false;
                    if (MyAPIGateway.Session.IsServer)
                        LeaveRace(p);
                    else
                        redirect = true;
                    break;
                case "/rcd clear":
                    if (p.PromoteLevel != MyPromoteLevel.Owner && p.PromoteLevel != MyPromoteLevel.Admin)
                        return;

                    sendToOthers = false;
                    if (MyAPIGateway.Session.IsServer)
                    {
                        finishes.Clear();
                        FinishesUpdated();
                    }
                    else
                    {
                        redirect = true;
                    }
                    MyAPIGateway.Utilities.ShowNotification("Cleared all finishers.", defaultMsgMs);
                    break;
                default:
                    sendToOthers = true;
                    return;
            }

            if (redirect)
            {
                byte [] data = MyAPIGateway.Utilities.SerializeToBinary(new CommandInfo(cmd, p.SteamUserId));
                MyAPIGateway.Multiplayer.SendMessageToServer(serverCmdId, data);
            }
        }

        protected override void UnloadData ()
        {
            if (running)
            {
                MyAPIGateway.Utilities.MessageEntered -= MessageEntered;

                if(MyAPIGateway.Multiplayer.IsServer)
                {
                    MyAPIGateway.Multiplayer.UnregisterMessageHandler(packetMainId, ReceiveActiveRacers);
                    MyAPIGateway.Multiplayer.UnregisterMessageHandler(packetFinalsId, ReceiveFinalRacers);
                }
            }

            textApi?.Unload();

            Instance = null;
        }


        public override void UpdateAfterSimulation ()
        {
            if (!running || !MyAPIGateway.Multiplayer?.IsServer == true)
                return;

            try
            {
                ProcessValues();
                BroadcastData(ActiveRacersText, packetMainId);
                frameCount++;
            }
            catch (Exception e)
            {
                ShowError(e, GetType());
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

        bool LeaveRace(IMyPlayer p)
        {
            RemoveWaypoint(p.IdentityId);
            reachedNode.Remove(p.SteamUserId);
            laps.Remove(p.SteamUserId);
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
            ActiveRacersText.Clear();
            if (nodes.Count < 2)
            {
                ActiveRacersText.Clear();
                ActiveRacersText.Append("Waiting for beacon nodes...").AppendLine();
                return;
            }

            SortedDictionary<double, RacerInfo> ranking = new SortedDictionary<double, RacerInfo>(new DescendingComparer<double>());
            playersTemp.Clear();
            MyAPIGateway.Players.GetPlayers(playersTemp, IsPlayerActive);
            foreach (IMyPlayer p in playersTemp)
            {
                IMyCharacter c = p.Character;
                string name = p.DisplayName;

                RacerInfo previous;
                bool hasPrevious = previousRacerInfos.TryGetValue(p.SteamUserId, out previous);

                // Find the closest node
                Vector3D pos = c.GetPosition();
                double dist;
                int start;
                int end;
                double partial;
                if (GetClosestSegment(pos, out start, out end, out partial))
                {
                    int reachedNode;
                    if (!this.reachedNode.TryGetValue(p.SteamUserId, out reachedNode))
                    {
                        this.reachedNode [p.SteamUserId] = start;
                        reachedNode = start;
                    }
                    ActiveRacersText.Append(p.DisplayName).Append(": ").Append(reachedNode).Append(", ").Append(start).AppendLine();
                    if (start == reachedNode)
                    {
                        // Racer is in expected position.
                        dist = nodeDistances [start] + partial;
                    }
                    else if (start == reachedNode + 1 && NodeCleared(p, reachedNode + 1))
                    {
                        // Racer has moved past one node.
                        this.reachedNode [p.SteamUserId] = start;
                        dist = nodeDistances [start] + partial;
                    }
                    else// if (start > reachedNode + 1 || start < reachedNode)
                    {
                        // Racer has moved past multiple nodes, clamp them.
                        start = reachedNode;
                        end = reachedNode + 1;

                        if (hasPrevious)
                            dist = previous.Distance;
                        else
                            dist = nodeDistances[start];
                    }
                }
                else
                {
                    // Outside of the track, quit the race
                    LeaveRace(p);
                    continue;
                }

                RacingBeacon destination = nodes[end];
                if(hasPrevious && !MyVisualScriptLogicProvider.IsPlayerInCockpit(p.IdentityId))
                {
                    destination = previous.Destination;
                    dist = previous.Distance;
                }

                int currLaps;
                if (!laps.TryGetValue(p.SteamUserId, out currLaps))
                {
                    laps [p.SteamUserId] = 0;
                    currLaps = 0;
                }

                if (Lapped(p))
                    laps [p.SteamUserId] = currLaps + 1;

                dist += currLaps * nodeDistances [nodeDistances.Length - 1];

                // Check if racer is on the track
                if (dist >= 0)
                {
                    // Check if it's a finish
                    if(end == nodes.Count - 1 && finish != null && finish.Contains(p))
                    {
                        // If the closest node is the last node, a finish exists, and the grid is intersecting with the finish
                        int rank = finishes.Count + 1;
                        RacerInfo finishInfo = new RacerInfo(p, 0, rank, name);

                        finishes.Add(p.SteamUserId, finishInfo);
                        FinishesUpdated();

                        // Remove their gps waypoints
                        RemoveWaypoint(p.IdentityId);

                        MyVisualScriptLogicProvider.ShowNotificationToAll($"{name} just finished in position {rank}", defaultMsgMs, "White");
                        LeaveRace(p);
                    }
                    else
                    {
                        RacerInfo racer = new RacerInfo(p, dist, 0, name)
                        {
                            Destination = destination
                        };
                        ranking [dist] = racer;
                    }

                }
                else
                {
                    RemoveWaypoint(p.IdentityId);
                }

            }
            BuildText(ranking);
        }

        private bool Lapped (IMyPlayer p)
        {
            foreach(RacingBeacon lapNode in lapNodes)
            {
                if (lapNode.Contains(p))
                    return true;
            }
            return false;
        }

        bool NodeCleared (IMyPlayer p, int reachedNode)
        {
            RacingBeacon node = nodes [reachedNode];
            return node.Type != RacingBeacon.BeaconType.CHECKPOINT || node.Contains(p);
        }

        private void FinishesUpdated ()
        {
            // Build the final racer text
            FinalRacersText.Clear();
            if (finishes.Count > 0)
            {
                int i = 0;
                FinalRacersText.Append(finalHudHeader);
                foreach(RacerInfo current in finishes.Values)
                {
                    i++;
                    FinalRacersText.Append(SetLength(i, numberWidth)).Append(' ');
                    FinalRacersText.Append(SetLength(current.Name, nameWidth));
                    FinalRacersText.AppendLine();
                }
            }

            // Send the data
            BroadcastData(FinalRacersText, packetFinalsId);
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
        
        bool GetClosestSegment(Vector3D racerPos, out int start, out int end, out double partialDist)
        {
            start = 0;
            end = -1;
            partialDist = 0;
            double minLinear2 = double.PositiveInfinity;

            if (nodes.Count < 2)
                return false;

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
                if (dist2 < minDistance2 && beaconGrid != minDistanceGrid)
                {
                    minDistanceGrid = beaconGrid;
                    minDistance2 = dist2;
                    closest = i;
                }

                double scaler = ScalarProjection(endToRacer, segment);
                ActiveRacersText.Append(i).Append(": ").Append((int)scaler).Append(' ').Append((int)dist2);
                if (scaler <= 0 || scaler > segmentLen)
                {
                    ActiveRacersText.AppendLine();
                    continue;
                }
                double linear2 = (endToRacer - (scaler * segment)).LengthSquared();
                ActiveRacersText.Append(' ').Append((int)linear2).AppendLine();

                if(linear2 < minLinear2)
                {
                    start = i - 1;
                    end = i;
                    partialDist = segmentLen - scaler;
                    minLinear2 = linear2;
                }
            }
            if(end < 0 || minDistance2 < minLinear2)
            {
                if (closest >= nodes.Count - 1)
                    return false; // Racer is past the last node
                start = closest;
                end = closest + 1;
                partialDist = 0;
            }
            return true;
        }

        void BuildText (SortedDictionary<double, RacerInfo> ranking)
        {
            // Build the active racer text
            //ActiveRacersText.Clear();
            for(int i = 0; i < nodes.Count; i++)
            {
                ActiveRacersText.Append(i).Append(": ").Append(nodeDistances [i]);
                if (nodes [i].Type == RacingBeacon.BeaconType.CHECKPOINT)
                    ActiveRacersText.Append(" Contains ").Append(nodes [i].Contains(MyAPIGateway.Session.Player));
                ActiveRacersText.AppendLine();
            }
            if (ranking.Count == 0)
            {
                ActiveRacersText.Append("No racers in range.");
                previousRacerInfos.Clear();
            }
            else
            {
                ActiveRacersText.Append(hudHeader);
                bool drawWhite = false;

                Dictionary<ulong, RacerInfo> newPrevRacerInfos = new Dictionary<ulong, RacerInfo>(ranking.Count);
                int i = finishes.Count + 1;
                foreach(RacerInfo racer in ranking.Values)
                {
                    RacerInfo current = racer;
                    current.Rank = i;

                    string drawnColor = null;
                    RacerInfo previous;
                    if (previousRacerInfos.TryGetValue(current.RacerId, out previous))
                    {
                        RenderWaypoint(current, previous);

                        if (!Moved(current))
                        {
                            // stationary
                            drawnColor = colorStationary;
                        }
                        else if (previousRacerInfos.Count == ranking.Count && current.Rank < previous.Rank)
                        {
                            // just ranked up
                            current.RankUpFrame = frameCount;
                            drawnColor = colorRankUp;
                        }
                        else if (previous.RankUpFrame > 0)
                        {
                            // recently ranked up
                            if (previous.RankUpFrame + rankUpTime > frameCount)
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
                        ActiveRacersText.Append(drawnColor);
                        drawWhite = true;
                    }
                    else if (drawWhite)
                    {
                        ActiveRacersText.Append(colorWhite);
                        drawWhite = false;
                    }

                    // <num>
                    ActiveRacersText.Append(SetLength(i, numberWidth)).Append(' ');

                    // <num> <name>
                    ActiveRacersText.Append(SetLength(current.Name, nameWidth)).Append(' ');

                    // <num> <name> <distance>
                    ActiveRacersText.Append(SetLength((int)current.Distance, distWidth));
                    
                    ActiveRacersText.AppendLine();

                    newPrevRacerInfos [current.RacerId] = current;

                    i++;
                }
                previousRacerInfos = newPrevRacerInfos;
            }
        }

        bool Moved (RacerInfo value)
        {
            if (value.Car?.Physics == null)
                return false;
            return value.Car.Physics.LinearVelocity.LengthSquared() > moveThreshold2;
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

        struct RacerInfo
        {
            public IMyPlayer Racer;
            public ulong RacerId => Racer.SteamUserId;
            public double Distance;
            public int Rank;
            public string Name;
            public int RankUpFrame;
            public RacingBeacon Destination;
            public IMyCubeGrid Car => (Racer.Controller?.ControlledEntity as IMyCockpit)?.CubeGrid;

            public RacerInfo (IMyPlayer racer, double distance, int rank, string name)
            {
                Racer = racer;
                Distance = distance;
                Rank = rank;
                Name = name;
                RankUpFrame = 0;
                Destination = null;
            }

            public override bool Equals (object obj)
            {
                if(obj is RacerInfo)
                {
                    RacerInfo info = (RacerInfo)obj;
                    return RacerId == info.RacerId;
                }
                return false;
            }

            public override int GetHashCode ()
            {
                return -913653116 + RacerId.GetHashCode();
            }
        }

        class DescendingComparer<T> : IComparer<T> where T : IComparable<T>
        {
            public int Compare (T x, T y)
            {
                int result = y.CompareTo(x);
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

        
    }
}
