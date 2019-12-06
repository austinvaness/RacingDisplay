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
using VRage;

namespace RacingMod
{
    [MySessionComponentDescriptor(MyUpdateOrder.AfterSimulation)]
    public class RacingSession : MySessionComponentBase
    {
        int numLaps = 1;

        bool debug = false;

        const int defaultMsgMs = 4000;

        const ushort packetMainId = 1337;
        const ushort packetCmd = 1339;
        const ushort packetSpecRequest = 1357;
        const ushort packetSpecResponse = 1358;

        public static RacingSession Instance;
        
        private readonly List<RacingBeacon> nodes = new List<RacingBeacon>();
        private RacingBeacon finish;
        private double[] nodeDistances = { };

        private HudAPIv2 textApi;
        private Vector2D activeHudPosition = new Vector2D(-0.95, 0.90);
        private HudAPIv2.HUDMessage activeRacersHud;
        private readonly StringBuilder ActiveRacersText = new StringBuilder("Initializing...");
        private string finalRacersString;

        const int numberWidth = 2; 
        const int distWidth = 10;
        const int nameWidth = 20;

        const double moveThreshold2 = 16; // Squared minimum velocity
        private const int rankUpTime = 90;
        private bool running = false;

        private Dictionary<ulong, RacerInfo> previousRacerInfos = new Dictionary<ulong, RacerInfo>();
        private readonly Dictionary<ulong, RacerInfo> finishes = new Dictionary<ulong, RacerInfo>();
        readonly Dictionary<ulong, int> nextNode = new Dictionary<ulong, int>();
        readonly Dictionary<ulong, int> laps = new Dictionary<ulong, int>();
        readonly HashSet<ulong> inFinish = new HashSet<ulong>();
        private readonly HashSet<ulong> activePlayers = new HashSet<ulong>();

        readonly ConcurrentDictionary<ulong, HashSet<ulong>> nextPlayerRequests = new ConcurrentDictionary<ulong, HashSet<ulong>>();
        readonly ConcurrentDictionary<ulong, HashSet<ulong>> prevPlayerRequests = new ConcurrentDictionary<ulong, HashSet<ulong>>();

        private string hudHeader;

        private int frameCount = 0;
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

        IMyPlayer followedEntity;
        Vector3D followedPos;
        MySpectator SpecCam => MyAPIGateway.Session.CameraController as MySpectator;
        const double maxCamDistance = 100;
        bool specPositionLock = false;
        MatrixD specCamLocal;

        public RacingSession ()
        {
            UpdateHeader();
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


            nextNode.Clear();
            
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
            MyVisualScriptLogicProvider.PlayerDisconnected += PlayerDisconnected;
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

            running = true;
            Instance = this;
        }

        private void PlayerDisconnected (long playerId)
        {
            if (MyAPIGateway.Session?.Player != null && followedEntity != null && followedEntity.IdentityId == playerId)
                ClearFollow();
        }

        private void ReceiveSpecResponse (byte [] obj)
        {
            try
            {
                NextRacerInfo info = MyAPIGateway.Utilities.SerializeFromBinary<NextRacerInfo>(obj);
                SetFollow(info.steamId, info.position);
            }
            catch (Exception e)
            {
                ShowError(e, GetType());
            }
        }

        private void ReceiveSpecRequest(byte[] data)
        {
            if (activePlayers.Count != 0)
                try
                {
                    AddSpecRequest(MyAPIGateway.Utilities.SerializeFromBinary<CurrentRacerInfo>(data));
                }
                catch (Exception e)
                {
                    ShowError(e, GetType());
                }
        }

        void AddSpecRequest(CurrentRacerInfo info)
        {
            if(info.current == 0 || GetPlayer(info.current) == null)
            {
                IMyPlayer p;
                if(GetClosestRacer(info.camera, out p))
                {
                    if (MyAPIGateway.Session.Player?.SteamUserId == info.requestor)
                    {
                        SetFollow(p);
                    }
                    else
                    {
                        byte [] data = MyAPIGateway.Utilities.SerializeToBinary(new NextRacerInfo(p.SteamUserId, p.GetPosition()));
                        MyAPIGateway.Multiplayer.SendMessageTo(packetSpecResponse, data, info.requestor);
                    }
                }
            }
            else if(info.direction)
            {
                HashSet<ulong> list;
                if (!nextPlayerRequests.TryGetValue(info.current, out list))
                    list = new HashSet<ulong>();
                list.Add(info.requestor);
                nextPlayerRequests [info.current] = list;
            }
            else
            {
                HashSet<ulong> list;
                if (!prevPlayerRequests.TryGetValue(info.current, out list))
                    list = new HashSet<ulong>();
                list.Add(info.requestor);
                prevPlayerRequests [info.current] = list;
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

        private void ReceiveActiveRacers(byte[] obj)
        {
            DecodeString(obj, ActiveRacersText);
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
        
        void ProcessCommand(IMyPlayer p, string cmd, ref bool sendToOthers)
        {
            bool redirect = false;
            cmd = cmd.ToLower();
            if (!cmd.StartsWith("/rcd"))
                return;
            sendToOthers = false;
            switch (cmd)
            {
                case "/rcd ui":
                    if(activeRacersHud != null)
                        activeRacersHud.Visible = !activeRacersHud.Visible;
                    break;
                case "/rcd join":
                    if (MyAPIGateway.Session.IsServer)
                        JoinRace(p);
                    else
                        redirect = true;
                    break;
                case "/rcd leave":
                    if (MyAPIGateway.Session.IsServer)
                        LeaveRace(p);
                    else
                        redirect = true;
                    break;
                case "/rcd rejoin":
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
                case "/rcd clear":
                    if (p.PromoteLevel != MyPromoteLevel.Owner && p.PromoteLevel != MyPromoteLevel.Admin)
                        return;

                    if (MyAPIGateway.Session.IsServer)
                    {
                        finishes.Clear();
                        FinishesUpdated();
                        MyVisualScriptLogicProvider.ShowNotification("Cleared all finishers.", defaultMsgMs, "White", p.IdentityId);
                    }
                    else
                    {
                        redirect = true;
                    }
                    break;
                case "/rcd debug":
                    debug = !debug;
                    break;
                default:
                    MyVisualScriptLogicProvider.SendChatMessage("Usage:\n/rcd join: Joins the race.\n/rcd leave: Leaves the race.\n" +
                        "/rcd rejoin: Shortcut to leave and join the race.\n/rcd ui: Toggles the on screen UIs.\n/rcd clear: (Admin only) Removes all finalists.", "rcd", p.IdentityId);
                    return;
            }

            if (redirect)
            {
                byte [] data = MyAPIGateway.Utilities.SerializeToBinary(new CommandInfo(cmd, p.SteamUserId));
                MyAPIGateway.Multiplayer.SendMessageToServer(packetCmd, data);
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
                }
            }

            textApi?.Unload();

            Instance = null;
        }


        public override void UpdateAfterSimulation ()
        {
            if (!running || MyAPIGateway.Session == null)
                return;

            try
            {
                if (MyAPIGateway.Session.Player != null)
                    Spectator();

                if (MyAPIGateway.Session.IsServer)
                {
                    ProcessValues();
                    BroadcastData(ActiveRacersText, packetMainId);
                    DrawDebug();
                    frameCount++;
                }
            }
            catch (Exception e)
            {
                ShowError(e, GetType());
            }
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
                nextNode.Remove(p.SteamUserId);
                inFinish.Remove(p.SteamUserId);
                laps.Remove(p.SteamUserId);
                if (finishes.Remove(p.SteamUserId))
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

        bool LeaveRace(IMyPlayer p)
        {
            previousRacerInfos.Remove(p.SteamUserId);
            RemoveWaypoint(p.IdentityId);
            nextNode.Remove(p.SteamUserId);
            inFinish.Remove(p.SteamUserId);
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

                // Find the closest node
                int currLaps = GetNumLaps(p);
                bool missed;
                double dist;
                int? nextNode = GetNextNode(p, currLaps, out missed, out dist);

                if (!nextNode.HasValue)
                    continue;
                
                //MyAPIGateway.Utilities.ShowNotification($"{nextNode.Value}", 16);

                RacingBeacon destination = nodes[nextNode.Value];

                if (inFinish.Contains(p.SteamUserId))
                {
                    missed = false; // When racer has crossed the finish, the racer will be ahead of the first node
                    if(finish == null || !finish.Contains(p))
                        inFinish.Remove(p.SteamUserId);
                }

                // Check if racer is on the track
                if (dist >= 0)
                {
                    // Check if it's a finish
                    if(nextNode == nodes.Count - 1 && finish != null && finish.Contains(p))
                    {
                        // If the closest node is the last node, a finish exists, and the grid is intersecting with the finish
                        if (currLaps >= numLaps - 1)
                        {
                            // The racer has finished all laps
                            int rank = finishes.Count + 1;
                            RacerInfo finishInfo = new RacerInfo(p, 0, rank, missed);

                            finishes.Add(p.SteamUserId, finishInfo);
                            FinishesUpdated();

                            MyVisualScriptLogicProvider.ShowNotificationToAll($"{p.DisplayName} just finished in position {rank}", defaultMsgMs, "White");
                            LeaveRace(p);
                        }
                        else
                        { 
                            // The racer needs more laps
                            if(inFinish.Add(p.SteamUserId))
                            {
                                MyVisualScriptLogicProvider.ShowNotification($"Lap {currLaps + 2} / {numLaps}", defaultMsgMs, "White", p.IdentityId);
                                laps [p.SteamUserId] = currLaps + 1;
                            }
                            this.nextNode [p.SteamUserId] = 0;
                            RacerInfo racer = new RacerInfo(p, dist, 0, missed)
                            {
                                Destination = destination
                            };
                            ranking [dist] = racer;
                        }
                    }
                    else
                    {
                        RacerInfo racer = new RacerInfo(p, dist, 0, missed)
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

        int? GetNextNode(IMyPlayer p, int laps, out bool missed, out double dist)
        {
            RacerInfo? previous = GetPreviousInfo(p);

            if (previous.HasValue && GetCockpit(p) == null)
            {
                missed = previous.Value.Missed;
                dist = previous.Value.Distance;
                return previous.Value.Destination.Index;
            }
            
            missed = false;
            dist = laps * nodeDistances [nodes.Count - 1];

            int nextNode;
            bool isNew = !this.nextNode.TryGetValue(p.SteamUserId, out nextNode);
            if (isNew)
            {
                this.nextNode [p.SteamUserId] = 0;
                nextNode = 0;
                if (numLaps > 1)
                    MyVisualScriptLogicProvider.ShowNotification($"Lap {laps + 1} / {numLaps}", defaultMsgMs, "White", p.IdentityId);
            }

            int start;
            int end;
            double partial;
            GetClosestSegment(p.GetPosition(), out start, out end, out partial);
            //MyAPIGateway.Utilities.ShowNotification($"{start} {end}", 16);

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
                            dist += nodeDistances [nextNode];
                            return nextNode;
                        }
                    }
                    else
                    {

                        if (nextNode >= nodes.Count - 1)
                        {
                            // The previous node is the last node.
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
                    //end = 0;
                    //destination = nodes [0];
                }
            }
            else
            {
                // On the track
                if (isNew)
                {
                    this.nextNode [p.SteamUserId] = end;
                    nextNode = end;
                    //MyAPIGateway.Utilities.ShowNotification($"nextNode is now {end}");
                }

                if (end <= nextNode)
                {
                    // Racer is in expected position.
                    dist += nodeDistances [start] + partial;
                    return nextNode;
                }
                else if (start == nextNode && NodeCleared(p, nextNode))
                {
                    // Racer has moved past one node.
                    this.nextNode [p.SteamUserId] = end;
                    //nextNode = end;
                    dist += nodeDistances [start] + partial;
                    return end;
                }
                else
                {
                    // Racer has moved past multiple nodes, clamp them.
                    missed = start >= nextNode;
                    //start = nextNode - 1;
                    //end = nextNode;

                    if (previous.HasValue)
                        dist = previous.Value.Distance;
                    else
                        dist += nodeDistances [nextNode];
                    return nextNode;
                }
            }

        }

        RacerInfo? GetPreviousInfo(IMyPlayer p)
        {
            RacerInfo temp;
            if (previousRacerInfos.TryGetValue(p.SteamUserId, out temp))
                return temp;
            return null;
        }

        int GetNumLaps(IMyPlayer p)
        {
            int laps;
            if (this.laps.TryGetValue(p.SteamUserId, out laps))
            {
                if (laps >= numLaps)
                    laps = numLaps - 1;
                return laps;
            }
            else
            {
                this.laps [p.SteamUserId] = 0;
                return 0;
            }
        }

        void Spectator()
        {
            if (SpecCam != null && !MyAPIGateway.Session.IsCameraControlledObject && !MyAPIGateway.Gui.ChatEntryVisible 
                && !MyAPIGateway.Gui.IsCursorVisible && MyAPIGateway.Gui.GetCurrentScreen == MyTerminalPageEnum.None)
            {
                if (MyAPIGateway.Input.IsNewKeyPressed(VRage.Input.MyKeys.T))
                {
                    // Disable following, key used by something else
                    ClearFollow();
                }
                else if(MyAPIGateway.Input.IsNewKeyPressed(VRage.Input.MyKeys.OemCloseBrackets))
                {
                    // Next
                    ulong currentId = 0;
                    if (followedEntity != null)
                        currentId = followedEntity.SteamUserId;
                    RequestNextRacer(new CurrentRacerInfo(MyAPIGateway.Session.Player.SteamUserId, currentId, SpecCam.Position, true));
                }
                else if (MyAPIGateway.Input.IsNewKeyPressed(VRage.Input.MyKeys.OemOpenBrackets))
                {
                    // Previous
                    ulong currentId = 0;
                    if (followedEntity != null)
                        currentId = followedEntity.SteamUserId;
                    RequestNextRacer(new CurrentRacerInfo(MyAPIGateway.Session.Player.SteamUserId, currentId, SpecCam.Position, false));
                }
                else if (MyAPIGateway.Input.IsNewKeyPressed(VRage.Input.MyKeys.OemQuotes))
                {
                    FaceFollowed();
                }
                else if (MyAPIGateway.Input.IsNewKeyPressed(VRage.Input.MyKeys.OemPlus))
                {
                    if(followedEntity == null)
                    {
                        specPositionLock = false;
                    }
                    else
                    {
                        specPositionLock = !specPositionLock;
                        if (specPositionLock)
                        {
                            IMyEntity e = GetCockpit(followedEntity)?.CubeGrid;
                            if (e == null)
                                e = followedEntity.Character;

                            if (e == null)
                            {
                                specPositionLock = false;
                            }
                            else
                            {
                                UpdateSpectatorLock(e);
                            }

                        }
                        MyAPIGateway.Utilities.ShowNotification($"Spectator lock: {specPositionLock}");
                    }
                }
            }

            if(followedEntity != null && SpecCam != null && !MyAPIGateway.Session.IsCameraControlledObject)
            {
                IMyEntity e = GetCockpit(followedEntity)?.CubeGrid;
                if(e == null)
                    e = followedEntity.Character;

                if(e == null)
                {
                    SpecCam.Position = followedPos + Vector3D.One;
                }
                else 
                {
                    if (followedPos != Vector3D.Zero)
                    {
                        followedPos = Vector3D.Zero;
                        FaceFollowed();
                    }

                    if (specPositionLock)
                    {
                        Matrix m = specCamLocal * e.WorldMatrix;
                        SpecCam.Position = m.Translation;
                        SpecCam.SetTarget(m.Translation + m.Forward * 5, m.Up);
                    }
                    else
                    {
                        Vector3D velocity = Vector3D.Zero;
                        if (e.Physics != null)
                            velocity = e.Physics.LinearVelocity;
                        SpecCam.Position += velocity * (1f / 60);
                    }
                }

            }

        }

        private void FaceFollowed ()
        {
            if(followedEntity != null && SpecCam != null)
            {
                // Clamp
                Vector3D ePos = followedEntity.GetPosition();

                Vector3D diff = SpecCam.Position - ePos;
                double len2 = diff.LengthSquared();
                if (len2 > (maxCamDistance * maxCamDistance))
                    SpecCam.Position = ePos + ((diff / Math.Sqrt(len2)) * maxCamDistance);

                Vector3D? up = null;
                IMyEntity e = followedEntity.Controller?.ControlledEntity?.Entity;
                if(e != null)
                {
                    IMyCubeBlock block = e as IMyCubeBlock;
                    if(block?.CubeGrid != null)
                        e = block.CubeGrid;

                    Vector3D grav = e.Physics?.Gravity ?? Vector3D.Zero;

                    if (grav == Vector3D.Zero)
                        up = SpecCam.Orientation.Up;
                    else
                        up = -Vector3.Normalize(grav);
                }

                SpecCam.SetTarget(ePos, up);

                if (specPositionLock && e != null)
                    UpdateSpectatorLock(e);
            }
        }

        private void UpdateSpectatorLock(IMyEntity e)
        {
            MatrixD specCamMatrix = SpecCam.Orientation;
            specCamMatrix.Translation = SpecCam.Position;
            specCamLocal = specCamMatrix * e.WorldMatrixNormalizedInv;
        }

        /// <summary>
        /// Called only on server!!!
        /// </summary>
        private void SetFollow (IMyPlayer entity)
        {
            if(entity == null)
            {
                ClearFollow();
            }
            else
            {
                followedEntity = entity;
                followedPos = entity.GetPosition(); // Probably not necessary since this method will only be called by server, which has all entities loaded
                MyAPIGateway.Utilities.ShowNotification($"Following {followedEntity.DisplayName}.");

                FaceFollowed();
            }
        }

        private void SetFollow (ulong id, Vector3D pos)
        {
            IMyPlayer entity = GetPlayer(id);
            if(entity == null)
            {
                ClearFollow();
            }
            else
            {
                followedEntity = entity;
                followedPos = pos;
                MyAPIGateway.Utilities.ShowNotification($"Following {followedEntity.DisplayName}.");

                FaceFollowed();
            }

        }

        private void ClearFollow()
        {
            if (followedEntity != null)
            {
                followedEntity = null;
                MyAPIGateway.Utilities.ShowNotification("Cleared spectator follow.");
            }
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
        

        public static IMyCubeBlock GetCockpit(IMyPlayer p)
        {
            return p?.Controller?.ControlledEntity?.Entity as IMyCubeBlock;
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
            if (finishes.Count > 0)
            {
                tempSb.Append(colorFinalist);
                int i = 0;
                foreach(RacerInfo current in finishes.Values)
                {
                    i++;
                    tempSb.Append(SetLength(i, numberWidth)).Append(' ');
                    tempSb.Append(SetLength(current.Name, nameWidth)).AppendLine();
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

        void BuildText (SortedDictionary<double, RacerInfo> ranking)
        {
            // Build the active racer text
            ActiveRacersText.Clear();

            if (ranking.Count == 0 && finishes.Count == 0)
            {
                ActiveRacersText.Append("No racers in range.");
                previousRacerInfos.Clear();
                return;
            }


            ActiveRacersText.Append(hudHeader);
            if (finishes.Count > 0)
                ActiveRacersText.Append(finalRacersString);
            

            if (ranking.Count > 0)
            {
                Dictionary<ulong, RacerInfo> newPrevRacerInfos = new Dictionary<ulong, RacerInfo>(ranking.Count);

                bool drawWhite = false;

                int i = finishes.Count + 1;
                double previousDist = nodeDistances[nodes.Count - 1] * numLaps;

                SendNextSpectatorResponse(ranking.Values.Last().RacerId, ranking.Values.First().Racer);
                SendPrevSpectatorResponse(ranking.Values.First().RacerId, ranking.Values.Last().Racer);

                ulong previousId = 0;
                IMyPlayer previousRacer = null;
                foreach (RacerInfo racer in ranking.Values)
                {
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
                    if(current.Missed)
                    {
                        ActiveRacersText.Append(SetLength("missed", distWidth));
                    }
                    else
                    {
                        ActiveRacersText.Append(SetLength((int)(previousDist - current.Distance), distWidth));
                        previousDist = current.Distance;
                    }

                    int lap;
                    if (numLaps > 1 && laps.TryGetValue(current.RacerId, out lap))
                        ActiveRacersText.Append(' ').Append(lap + 1);

                    ActiveRacersText.AppendLine();

                    newPrevRacerInfos [current.RacerId] = current;

                    i++;
                    previousId = current.RacerId;
                    previousRacer = current.Racer;
                }

                previousRacerInfos = newPrevRacerInfos;

            }
            else
            {
                previousRacerInfos.Clear();
            }

            nextPlayerRequests.Clear();
            prevPlayerRequests.Clear();


        }

        private void SendNextSpectatorResponse (ulong oldRacer, IMyPlayer newRacer)
        {
            HashSet<ulong> requestors;
            if (nextPlayerRequests.TryRemove(oldRacer, out requestors) && requestors.Count > 0)
            {
                SendSpectatorResponse(requestors, newRacer);
                requestors.Clear();
                nextPlayerRequests [oldRacer] = requestors;
            }
        }

        private void SendPrevSpectatorResponse (ulong oldRacer, IMyPlayer newRacer)
        {
            HashSet<ulong> requestors;
            if (prevPlayerRequests.TryRemove(oldRacer, out requestors) && requestors.Count > 0)
            {
                SendSpectatorResponse(requestors, newRacer);
                requestors.Clear();
                prevPlayerRequests [oldRacer] = requestors;
            }
        }

        private void SendSpectatorResponse(IEnumerable<ulong> requestors, IMyPlayer newRacer)
        {
            Vector3D racerPos = newRacer.GetPosition();
            foreach (ulong id in requestors)
            {
                if (MyAPIGateway.Session.Player?.SteamUserId == id)
                {
                    SetFollow(newRacer);
                }
                else
                {
                    byte [] data = MyAPIGateway.Utilities.SerializeToBinary(new NextRacerInfo(newRacer.SteamUserId, racerPos));
                    MyAPIGateway.Multiplayer.SendMessageTo(packetSpecResponse, data, id);
                }
            }
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

        struct RacerInfo
        {
            public IMyPlayer Racer;
            public ulong RacerId => Racer.SteamUserId;
            public double Distance;
            public int Rank;
            public string Name => Racer.DisplayName;
            public int RankUpFrame;
            public RacingBeacon Destination;
            public bool Missed;

            public RacerInfo (IMyPlayer racer, double distance, int rank, bool missed)
            {
                Racer = racer;
                Distance = distance;
                Rank = rank;
                RankUpFrame = 0;
                Destination = null;
                Missed = missed;
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
