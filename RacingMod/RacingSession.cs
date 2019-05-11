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

namespace RacingMod
{
    [MySessionComponentDescriptor(MyUpdateOrder.AfterSimulation)]
    public class RacingSession : MySessionComponentBase
    {
        const ushort packetMainId = 1337;
        const ushort packetFinalsId = 1338;

        public static RacingSession Instance;
        
        private readonly List<RacingBeacon> nodes = new List<RacingBeacon>();
        private readonly List<RacingBeacon> checkpointMapping = new List<RacingBeacon>(); // checkpointMapping[checkpointIndex] = node;
        private Vector3[] nodePositionCache = { };
        private float[] nodeDistanceCache = { };

        private HudAPIv2 textApi;
        private Vector2D activeHudPosition = new Vector2D(-0.95, 0.90);
        private HudAPIv2.HUDMessage activeRacersHud;
        private StringBuilder ActiveRacersText = new StringBuilder("Initializing...");
        private Vector2D finalHudPosition = new Vector2D(0.95, 0.9);
        private HudAPIv2.HUDMessage finalRacersHud;
        private StringBuilder FinalRacersText = new StringBuilder("Initializing...");

        private readonly Dictionary<long, IMyCubeGrid> grids = new Dictionary<long, IMyCubeGrid>();
        private readonly Dictionary<long, int> nextCheckpointIndices = new Dictionary<long, int>(); // key=GridId, value=index of checkpointMapping
        private readonly List<RacerInfo> finishes = new List<RacerInfo>();

        const int numberWidth = 2; 
        const int distWidth = 8;
        const int maxNameWidth = 20;
        const int minNameWidth = 4;
        private int nameWidth = maxNameWidth;

        const float moveThreshold2 = 4; // Squared minimum velocity
        private const int rankUpTime = 90;
        private bool running = false;
        Dictionary<long, RacerInfo> previousRacerInfos = new Dictionary<long, RacerInfo>();

        string hudHeader;
        string finalHudHeader;

        int frameCount = 0;
        private const string headerColor = "<color=127,127,127>";
        const string colorWhite = "<color=white>";
        const string colorStationary = "<color=255,124,124>";
        const string colorRankUp = "<color=124,255,154>";

        private readonly Color gateWaypointColor = new Color(0, 255, 255); // nodes and cleared checkpoints
        private readonly Color gateWaypointColorMandatory = new Color(255, 31, 0); // your next mandatory checkpoint
        const string gateWaypointName = "Waypoint";
        const string gateWaypointDescription = "The next waypoint to guide you through the race.";
        const string gateCheckpointName = "Checkpoint";
        const string gateCheckpointDescription = "The next mandatory checkpoint in the race.";
        int gateWaypointGps;

        public RacingSession ()
        {
            GenerateHeader();
        }

        private void GenerateHeader()
        {
            hudHeader = headerColor +
                        "#".PadRight(numberWidth + 1) +
                        "Name".PadRight(nameWidth + 1) +
                        "Distance".PadRight(distWidth + 1);
            finalHudHeader = headerColor + "#".PadRight(numberWidth + 1) + "Finalist".PadRight(maxNameWidth);
            if (checkpointMapping.Count > 0)
            {
                if (checkpointMapping.Count < 10)
                    hudHeader += "Ckpnt";
                else
                    hudHeader += "Chckpnt";
            }

            hudHeader += "\n" + colorWhite;
            finalHudHeader += "\n" + colorWhite;
        }

        public void RegisterNode(RacingBeacon node)
        {
            if (!node.Valid || node.Type == RacingBeacon.BeaconType.IGNORED)
                throw new ArgumentException("Trying to register an invalid or ignored node");

            if (!AddSorted(nodes, node))
            {
                // checkpoint with this id already existed
                node.AssignNewNumber(nodes.Last().NodeNumber + 1);
                return; // changing the node number triggers a re-register
            }
            
            // keep track of checkpoints separately for performance reasons
            if (node.Type == RacingBeacon.BeaconType.CHECKPOINT)
            {
                AddSorted(checkpointMapping, node);
                GenerateHeader();
            }
            
            RebuildNodeCaches();
        }

        public void RemoveNode(RacingBeacon node)
        {
            int index = nodes.FindIndex(other => other.Entity.EntityId == node.Entity.EntityId); 
            if(index >= 0)
                nodes.RemoveAt(index);

            index = checkpointMapping.FindIndex(other => other.Entity.EntityId == node.Entity.EntityId);
            if (index >= 0)
            {
                checkpointMapping.RemoveAt(index);
                GenerateHeader();
            }
            
            RebuildNodeCaches();
        }

        private void RebuildNodeCaches()
        {
            // rebuild position cache
            nodePositionCache = nodes.Select(b => b.Coords).ToArray();
            
            // rebuild distance cache
            nodeDistanceCache = new float[nodePositionCache.Length];
            if (nodeDistanceCache.Length == 0)
                return;
            
            float cumulative = 0;
            nodeDistanceCache[0] = 0.0f;
            Vector3 prev = nodePositionCache[0];
            for (int i = 1; i < nodePositionCache.Length; i++)
            {
                Vector3 v = nodePositionCache[i];
                cumulative += Vector3.Distance(prev, v);
                nodeDistanceCache[i] = cumulative;
                prev = nodePositionCache[i];
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
                MyAPIGateway.Entities.OnEntityAdd += OnEntityAdd;
                MyAPIGateway.Entities.OnEntityRemove += OnEntityRemove;

                // grid scan
                HashSet<IMyEntity> temp = new HashSet<IMyEntity>();
                MyAPIGateway.Entities.GetEntities(temp);
                foreach (IMyEntity e in temp)
                    OnEntityAdd(e);
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
                MyLog.Default.WriteLineAndConsole($"{e.Message}\n{e.StackTrace}");

                if (MyAPIGateway.Session?.Player != null)
                    MyAPIGateway.Utilities.ShowNotification($"[ ERROR: {GetType().FullName}: {e.Message} | Send SpaceEngineers.Log to mod author ]", 10000, MyFontEnum.Red);
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
            if (messageText == "/rcd" && activeRacersHud != null)
            {
                activeRacersHud.Visible = !activeRacersHud.Visible;
                finalRacersHud.Visible = activeRacersHud.Visible;
                sendToOthers = false;
            }
        }
        
        protected override void UnloadData ()
        {
            if (running)
            {
                MyAPIGateway.Utilities.MessageEntered -= MessageEntered;
                MyAPIGateway.Entities.OnEntityAdd -= OnEntityAdd;
                MyAPIGateway.Entities.OnEntityRemove -= OnEntityRemove;

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
            if (!running || !MyAPIGateway.Multiplayer.IsServer)
                return;

            ProcessValues();
            BroadcastData(ActiveRacersText, packetMainId);
            frameCount++;
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

        private void OnEntityAdd (IMyEntity ent)
        {
            IMyCubeGrid myGrid = ent as IMyCubeGrid;
            if (myGrid != null && myGrid.GridSizeEnum == MyCubeSize.Small)
            {
                grids [myGrid.EntityId] = myGrid;
                nextCheckpointIndices[myGrid.EntityId] = 0;
            }
        }

        private void OnEntityRemove (IMyEntity ent)
        {
            IMyCubeGrid myGrid = ent as IMyCubeGrid;
            if (myGrid != null && myGrid.GridSizeEnum == MyCubeSize.Small)
            {
                grids.Remove(myGrid.EntityId);
                nextCheckpointIndices.Remove(myGrid.EntityId);
            }
        }

        void ProcessValues ()
        {
            if (nodes.Count < 2)
            {
                ActiveRacersText.Clear();
                ActiveRacersText.Append("Waiting for beacon nodes...").AppendLine();
                return;
            }

            int tempMaxNameWidth = 0;

            SortedDictionary<float, RacerInfo> ranking = new SortedDictionary<float, RacerInfo>(new DescendingComparer<float>());
            foreach (IMyCubeGrid g in grids.Values)
            {
                if (g.Physics == null)
                    continue;

                // check if this grid is a racer
                string name = g.CustomName;
                if (name.Length == 0 || name [0] != '#')
                    continue;
                
                // Find the closest node
                Vector3 pos = g.GetPosition();
                int closest = GetClosestIndex(pos);

                RacingBeacon beg = null;
                if (closest > 0)
                    beg = nodes[closest - 1];
                
                RacingBeacon end = null;
                if (closest < nodes.Count - 1)
                    end = nodes[closest + 1];

                RacingBeacon destination;
                float dist = GetDistance(pos, beg, closest, end, out destination);
                
                if (dist > 0)
                {
                    name = name.Substring(1).Trim();
                    
                    // get next checkpoint
                    int nextCheckpointIndex = nextCheckpointIndices[g.EntityId];
                    if (nextCheckpointIndex < checkpointMapping.Count)
                    {
                        // check if we reached it
                        RacingBeacon nextCheckpoint = checkpointMapping[nextCheckpointIndex];
                        if (g.WorldAABB.Intersects(nextCheckpoint.CollisionArea))
                        {
                            nextCheckpointIndices[g.EntityId] = ++nextCheckpointIndex;
                            
                            // check if it's a finish
                            if (checkpointMapping.Count > 0 && nextCheckpointIndex >= checkpointMapping.Count)
                            {
                                int lastCheckpointNodeId = nodes.BinarySearch(checkpointMapping.Last());
                                int rank = finishes.Count + 1;
                                RacerInfo finishInfo = new RacerInfo(g, nodeDistanceCache[lastCheckpointNodeId],
                                    rank, name);
                                
                                finishes.Add(finishInfo);
                                FinishesUpdated();
                                
                                // remove their gps waypoints
                                IMyPlayer p = MyAPIGateway.Players.GetPlayerControllingEntity(g);
                                if(p != null)
                                    RemoveWaypoint(p.IdentityId);
                                
                                MyAPIGateway.Utilities.ShowNotification($"{name} just finished in position {rank}");
                            }
                        }
                        
                        // make sure the destination is no further along the track than our next checkpoint
                        if (nextCheckpointIndex < checkpointMapping.Count) // re-check because it might have changed 
                        {
                            int nextCheckpointNodeId = nodes.BinarySearch(checkpointMapping[nextCheckpointIndex]);
                            if (nodes.BinarySearch(destination) > nextCheckpointNodeId)
                            {
                                destination = checkpointMapping[nextCheckpointIndex];
                                dist = nodeDistanceCache[nextCheckpointNodeId];
                            }
                        }
                    }

                    // only show racers who haven't finished yet
                    if (checkpointMapping.Count == 0 || nextCheckpointIndex < checkpointMapping.Count)
                    {
                        if (name.Length > tempMaxNameWidth)
                            tempMaxNameWidth = name.Length;

                        RacerInfo racer = new RacerInfo(g, dist, 0, name);
                        racer.Destination = destination;
                        IMyPlayer p = MyAPIGateway.Players.GetPlayerControllingEntity(g);
                        if(p != null)
                            racer.Controller = p.IdentityId;
                        ranking [dist] = racer;
                    }
                }
                else
                {
                    RacerInfo previous;
                    if (previousRacerInfos.TryGetValue(g.EntityId, out previous) && previous.Controller != 0)
                        RemoveWaypoint(previous.Controller);
                }

            }
            tempMaxNameWidth = MathHelper.Clamp(tempMaxNameWidth, minNameWidth, maxNameWidth);
            if (tempMaxNameWidth != nameWidth)
            {
                nameWidth = tempMaxNameWidth;
                GenerateHeader();
            }
            BuildText(ranking);
        }

        private void FinishesUpdated ()
        {
            // Build the final racer text
            FinalRacersText.Clear();
            if (finishes.Count > 0)
            {
                FinalRacersText.Append(finalHudHeader);
                for (int i = 0; i < finishes.Count; i++)
                {
                    RacerInfo current = finishes [i];
                    FinalRacersText.Append(SetLength(i + 1, numberWidth)).Append(' ');
                    FinalRacersText.Append(SetLength(current.Name, maxNameWidth));
                    FinalRacersText.AppendLine();
                }
            }

            // Send the data
            BroadcastData(FinalRacersText, packetFinalsId);
        }

        private void RemoveWaypoint (long identityId)
        {
            MyVisualScriptLogicProvider.RemoveGPS(gateWaypointName, identityId);
            MyVisualScriptLogicProvider.RemoveGPS(gateCheckpointName, identityId);
        }

        void DrawRemoveWaypoint(RacerInfo info)
        {
            RemoveWaypoint(info.Controller);

            RacingBeacon node = info.Destination;

            // figure out if it's a checkpoint we have yet to clear
            int myNextCheckpointIndex = nextCheckpointIndices[info.GridId];
            bool isCheckpoint = node.Type == RacingBeacon.BeaconType.CHECKPOINT;
            bool notCleared = isCheckpoint && myNextCheckpointIndex <= checkpointMapping.BinarySearch(node);
            bool mandatory = isCheckpoint && notCleared;

            if (mandatory)
            {
                // this node is a mandatory checkpoint
                MyVisualScriptLogicProvider.AddGPSObjective(gateCheckpointName, gateCheckpointDescription, node.Coords,
                    gateWaypointColorMandatory, 0, info.Controller);
            }
            else
            {
                // this node is just a waypoint
                MyVisualScriptLogicProvider.AddGPSObjective(gateWaypointName, gateWaypointDescription, node.Coords, 
                    gateWaypointColor, 0, info.Controller);
                
                // additionally show the next mandatory checkpoint so the racer knows where to go
                if (myNextCheckpointIndex < checkpointMapping.Count)
                {
                    RacingBeacon myNextCheckpoint = checkpointMapping[myNextCheckpointIndex];
                    MyVisualScriptLogicProvider.AddGPSObjective(gateCheckpointName, gateCheckpointDescription, 
                        myNextCheckpoint.Coords, gateWaypointColorMandatory, 0, info.Controller);
                }
            }
        }

        void RenderWaypoint(RacerInfo current, RacerInfo previous)
        {
            long playerIdentity = current.Controller;
            long previousId = previous.Controller;

            if(playerIdentity == 0) // No controller currently
            {
                if(previousId != 0) // Controller left
                    RemoveWaypoint(previousId);
            }
            else // Has controller
            {
                if(previousId != 0 && playerIdentity != previousId) // Controller switched
                    RemoveWaypoint(previousId);

                if(current.Destination == null) // Before start or after finish
                {
                    if (previous.Destination != null) // Player just left the track
                        RemoveWaypoint(playerIdentity);
                }
                else // Player is on the track
                {
                    // Player just took control, or Player just got on the track, or Player has crossed a node/checkpoint
                    if (previousId == 0 || playerIdentity != previousId ||
                        previous.Destination == null || current.Destination != previous.Destination) 
                        DrawRemoveWaypoint(current);
                }
            }
        }
        
        int GetClosestIndex(Vector3 racerPos)
        { 
            int closest = 0;
            float closestDist2 = float.MaxValue;
            for (int i = 0; i < nodePositionCache.Length; i++)
            {
                float dist2 = Vector3.DistanceSquared(nodePositionCache[i], racerPos);
                if (dist2 < closestDist2)
                {
                    closestDist2 = dist2;
                    closest = i;
                }
            }
            return closest;
        }

        float GetDistance (Vector3 gridPos, RacingBeacon beg, int midIndex, RacingBeacon end, out RacingBeacon destination)
        {
            RacingBeacon mid = nodes[midIndex];
            float midDistance = nodeDistanceCache[midIndex];

            if (beg == null && end == null)
            {
                destination = null;
                return -1;
            }

            Vector3 dir = gridPos - mid.Coords;

            float? before = null;
            float beforeLength2 = 0;
            if (beg != null)
            {
                Vector3 beforeSegment = mid.Coords - beg.Coords;
                beforeLength2 = beforeSegment.LengthSquared();
                before = ScalarProjection(dir, Vector3.Normalize(beforeSegment));
            }

            float? after = null;
            float afterLength2 = 0;
            if (end != null)
            {
                Vector3 afterSegment = end.Coords - mid.Coords;
                afterLength2 = afterSegment.LengthSquared();
                after = ScalarProjection(dir, Vector3.Normalize(afterSegment));
            }

            if (!after.HasValue)
            {
                if(before.Value < 0)
                {
                    destination = mid;
                    return midDistance + before.Value;
                }
                destination = null;
                return -1;
            }

            if (!before.HasValue)
            {
                if (after.Value > 0)
                {
                    destination = end;
                    return midDistance + after.Value;
                }
                destination = null;
                return -1;
            }

            if (after > 0)
            {
                destination = end;
                return midDistance + after.Value;
            }

            destination = mid;
            return midDistance + before.Value;
        }

        void BuildText (SortedDictionary<float, RacerInfo> ranking)
        {

            // Build the active racer text
            ActiveRacersText.Clear();
            if (ranking.Count == 0)
            {
                ActiveRacersText.Append("No racers in range.");
                previousRacerInfos.Clear();
            }
            else
            {
                ActiveRacersText.Append(hudHeader);
                bool drawWhite = false;

                Dictionary<long, RacerInfo> newPrevRacerInfos = new Dictionary<long, RacerInfo>(ranking.Count);
                int i = 1;
                foreach(RacerInfo racer in ranking.Values)
                {
                    RacerInfo current = racer;
                    current.Rank = i;

                    string drawnColor = null;
                    RacerInfo previous;
                    if (previousRacerInfos.TryGetValue(current.GridId, out previous))
                    {
                        RenderWaypoint(current, previous);

                        if (!Moved(current, previous))
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
                        if (current.Controller != 0 && current.Destination != null)
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
                    ActiveRacersText.Append(SetLength((int)current.Distance, distWidth)).Append(' ');
                    
                    // <num> <name> <distance> <checkpoint>
                    if (checkpointMapping.Count > 0)
                    {
                        int cpWidth = 2;
                        if (checkpointMapping.Count < 10)
                            cpWidth = 1;
                        ActiveRacersText.Append(SetLength(nextCheckpointIndices[current.GridId], cpWidth));
                        ActiveRacersText.Append(" / ");
                        ActiveRacersText.Append(SetLength(checkpointMapping.Count, cpWidth));
                    }

                    ActiveRacersText.AppendLine();

                    newPrevRacerInfos [current.GridId] = current;

                    i++;
                }
                previousRacerInfos = newPrevRacerInfos;
            }
        }

        bool Moved(RacerInfo value, RacerInfo previous)
        {
            if (value.Racer.Physics == null)
                return false;
            return value.Racer.Physics.LinearVelocity.LengthSquared() > moveThreshold2;
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
        static float ScalarProjection (Vector3 value, Vector3 guide)
        {
            float returnValue = Vector3.Dot(value, guide);
            if (float.IsNaN(returnValue))
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
        
        struct RacerInfo
        {
            public IMyCubeGrid Racer;
            public long GridId => Racer.EntityId;
            public float Distance;
            public int Rank;
            public string Name;
            public int RankUpFrame;
            public RacingBeacon Destination;
            public long Controller;

            public RacerInfo (IMyCubeGrid racer, float distance, int rank, string name)
            {
                Racer = racer;
                Distance = distance;
                Rank = rank;
                Name = name;
                RankUpFrame = 0;
                Destination = null;
                Controller = 0;
            }
        }

        class DescendingComparer<T> : IComparer<T> where T : IComparable<T>
        {
            public int Compare (T x, T y)
            {
                return y.CompareTo(x);
            }
        }
    }
}
