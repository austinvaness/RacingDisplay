using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Draygo.API;
using Sandbox.Game;
using Sandbox.ModAPI;
using VRage.Game.Components;
using VRage.Game.ModAPI;
using VRage.Utils;
using VRageMath;
using System.Collections.Concurrent;
using KlimeDraygo.RelativeSpectator.API;
using VRage;
using SpaceEngineers.Game.ModAPI;
using avaness.RacingMod.Paths;

namespace avaness.RacingMod
{
    [MySessionComponentDescriptor(MyUpdateOrder.AfterSimulation)]
    public partial class RacingSession : MySessionComponentBase
    {
        public static RacingSession Instance;
        public int Runticks = 0;
        public DateTime Runtime = new DateTime (1);
        public readonly RacingMapSettings MapSettings = new RacingMapSettings();
        public readonly NodeManager Nodes;
        public readonly HashSet<IMyTimerBlock> StartTimers = new HashSet<IMyTimerBlock>();
        public bool Enabled { get; private set; } = true;

        public ClientRaceRecorder Recorder;


        private bool racersNeedUpdating = false;

        bool debug = false;

        private readonly StringBuilder activeRacersText = new StringBuilder("Initializing...");
        private readonly StringBuilder infoHudText = new StringBuilder();

        private bool initApi = false;
        private bool running = false;

        private readonly Dictionary<ulong, StaticRacerInfo> staticRacerInfo = new Dictionary<ulong, StaticRacerInfo>();
        private SortedSet<StaticRacerInfo> previousTick = new SortedSet<StaticRacerInfo>();

        private FinishList finishers;
        private readonly HashSet<ulong> activePlayers = new HashSet<ulong>();

        readonly ConcurrentDictionary<ulong, HashSet<ulong>> nextPlayerRequests = new ConcurrentDictionary<ulong, HashSet<ulong>>();
        readonly ConcurrentDictionary<ulong, HashSet<ulong>> prevPlayerRequests = new ConcurrentDictionary<ulong, HashSet<ulong>>();

        private SpecCamAPI Spec;

        private string hudHeader;

        private int gateWaypointGps;
        
        // Temporary
        private readonly List<IMyPlayer> playersTemp = new List<IMyPlayer>();
        private readonly StringBuilder tempSb = new StringBuilder();

        private RacingPreferences config = new RacingPreferences();

        public RacingSession ()
        {
            Instance = this;
            Nodes = new NodeManager(MapSettings);
            UpdateHeader(MapSettings.NumLaps);
            MapSettings.NumLapsChanged += UpdateHeader;
        }

        private void Start()
        {
            if (RacingConstants.IsPlayer && MyAPIGateway.Session.Player == null)
                return;

            gateWaypointGps = MyAPIGateway.Session.GPS.Create(RacingConstants.gateWaypointName, RacingConstants.gateWaypointDescription, Vector3D.Zero, true).Hash;

            MyAPIGateway.Utilities.MessageEntered += MessageEntered;
            if (RacingConstants.IsServer)
            {
                MyVisualScriptLogicProvider.RemoveGPSForAll(RacingConstants.gateWaypointName);
                MyAPIGateway.Multiplayer.RegisterMessageHandler(RacingConstants.packetCmd, ReceiveCmd);
                MyAPIGateway.Multiplayer.RegisterMessageHandler(RacingConstants.packetSpecRequest, ReceiveSpecRequest);
                MapSettings.Copy(RacingMapSettings.LoadFile());
                finishers = new FinishList();
                MapSettings.NumLapsChanged += NumLapsChanged;
                MapSettings.LoopedChanged += LoopedChanged;
            }
            else
            {
                MyAPIGateway.Session.GPS.RemoveLocalGps(gateWaypointGps);
                MyAPIGateway.Multiplayer.RegisterMessageHandler(RacingConstants.packetMainId, ReceiveActiveRacers);
                MyAPIGateway.Multiplayer.RegisterMessageHandler(RacingConstants.packetSpecResponse, ReceiveSpecResponse);
                MyAPIGateway.Multiplayer.SendMessageToServer(RacingConstants.packetSettingsInit, BitConverter.GetBytes(MyAPIGateway.Session.Player.SteamUserId));
                MyAPIGateway.Multiplayer.RegisterMessageHandler(RacingConstants.packetRec, ServerRaceRecorder.Packet.Received);
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
            if (!Enabled)
                UpdatingStarted();
            Runticks++;
            Runtime += RacingConstants.oneTick;

            if (MyAPIGateway.Session == null)
                return;

            // Startup
            if (textApi == null)
                textApi = new HudAPIv2(CreateHudItems);

            if (Spec == null)
                Spec = new SpecCamAPI();

            if (!initApi && RacingConstants.IsServer && Runticks >= 300)
            {
                MyAPIGateway.Utilities.SendModMessage(RacingConstants.ModMessageId, 
                    new MyTuple<Func<ulong []>, Func<MyTuple<ulong, TimeSpan>[]>, Func<ulong []>, Func<MyTuple<ulong, double, TimeSpan, int>[]>, Func<double>, Func<int>>
                    (ApiGetFinishers, ApiGetFinisherInfo, ApiGetRacers, ApiGetRacerInfo, ApiGetTrackLength, ApiGetTrackLaps));
                initApi = true;
            }

            if (!running)
                Start();
            if (!running)
                return;
            // End startup

            try
            {
                if (RacingConstants.IsPlayer)
                {
                    Recorder?.Update();
                    Spectator();
                }

                if (RacingConstants.IsServer)
                {
                    ProcessValues();
                    BroadcastData(activeRacersText, RacingConstants.packetMainId);
                    if(debug)
                        Nodes.DrawDebug();
                }
            }
            catch (Exception e)
            {
                RacingTools.ShowError(e, GetType());
            }
        }

        private void UpdatingStarted ()
        {
            if(racersNeedUpdating)
                UpdateAllRacerNodes();
            Enabled = true;
        }

        public override void UpdatingStopped ()
        {
            Enabled = false;
        }

        #region API
        private ulong [] ApiGetFinishers ()
        {
            return finishers.Select(StaticInfoId).ToArray();
        }
        private MyTuple<ulong, TimeSpan> [] ApiGetFinisherInfo ()
        {
            return finishers.Select(StaticInfoFinishData).ToArray();
        }

        private ulong [] ApiGetRacers ()
        {
            return previousTick.Select(StaticInfoId).ToArray();
        }
        private MyTuple<ulong, double, TimeSpan, int> [] ApiGetRacerInfo ()
        {
            return previousTick.Select(StaticInfoRacerData).ToArray();
        }

        private double ApiGetTrackLength ()
        {
            return Nodes.TotalDistance;
        }

        private int ApiGetTrackLaps ()
        {
            return MapSettings.NumLaps;
        }

        private ulong StaticInfoId (StaticRacerInfo info)
        {
            return info.Id;
        }

        private MyTuple<ulong, TimeSpan> StaticInfoFinishData (StaticRacerInfo info)
        {
            return new MyTuple<ulong, TimeSpan>(info.Id, info.BestTime);
        }
        private MyTuple<ulong, double, TimeSpan, int> StaticInfoRacerData (StaticRacerInfo info)
        {
            return new MyTuple<ulong, double, TimeSpan, int>(info.Id, info.Distance, info.Timer.GetTime(), info.Laps);
        }
        #endregion

        protected override void UnloadData ()
        {
            Enabled = false;
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
                MyAPIGateway.Multiplayer.UnregisterMessageHandler(RacingConstants.packetRec, ServerRaceRecorder.Packet.Received);

            }
            MyAPIGateway.Multiplayer.UnregisterMessageHandler(RacingConstants.packetSettings, ReceiveSettings);
            MyAPIGateway.Multiplayer.UnregisterMessageHandler(RacingConstants.packetSettingsInit, ReceiveSettingsInit);

            textApi?.Unload();
            Spec?.Close();
            if(MapSettings != null)
            {
                MapSettings.NumLapsChanged -= UpdateUI_NumLaps;
                MapSettings.TimedModeChanged -= UpdateUI_TimedMode;
                MapSettings.StrictStartChanged -= UpdateUI_StrictStart;
                MapSettings.NumLapsChanged -= NumLapsChanged;
                MapSettings.NumLapsChanged -= UpdateHeader;
                MapSettings.LoopedChanged -= LoopedChanged;
                MapSettings.Unload();
                Nodes.Unload();
            }

            Instance = null;
        }

        public void UpdateHeader(int laps)
        {
            tempSb.Clear();
            tempSb.Append(RacingConstants.headerColor).Append("#".PadRight(RacingConstants.numberWidth + 1));
            tempSb.Append("Name".PadRight(RacingConstants.nameWidth + 1));
            tempSb.Append("Position".PadRight(RacingConstants.distWidth + 1));
            if (laps > 1)
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

        private void ToggleUI()
        {
            activeRacersHud.Visible = !activeRacersHud.Visible;
        }

        private bool GetStaticInfo (string name, out StaticRacerInfo info)
        {
            foreach(StaticRacerInfo i in staticRacerInfo.Values)
            {
                if (i.Name.ToLower().Contains(name))
                {
                    info = i;
                    return true;
                }
            }

            info = null;
            return false;
        }

        private bool GetStaticInfo (IMyPlayer p, out StaticRacerInfo info)
        {
            if (staticRacerInfo.TryGetValue(p.SteamUserId, out info))
            {
                info.Racer = p;
                return true;
            }
            return false;
        }

        private StaticRacerInfo GetStaticInfo(ulong id)
        {
            StaticRacerInfo info;
            if (staticRacerInfo.TryGetValue(id, out info))
                return info;
            info = new StaticRacerInfo(RacingTools.GetPlayer(id));
            staticRacerInfo.Add(id, info);
            return info;
        }

        private StaticRacerInfo GetStaticInfo (IMyPlayer p)
        {
            StaticRacerInfo info;
            if (staticRacerInfo.TryGetValue(p.SteamUserId, out info))
            {
                info.Racer = p;
                return info;
            }
            info = new StaticRacerInfo(p);
            staticRacerInfo.Add(p.SteamUserId, info);
            return info;
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

        bool IsPlayerActive (IMyPlayer p)
        {
            return activePlayers.Contains(p.SteamUserId) && p.Character?.Physics != null;
        }

        bool JoinRace (IMyPlayer p, bool force = false)
        {
            if (!force && MapSettings.StrictStart)
            {
                if (Nodes.OnTrack(p))
                {
                    MyVisualScriptLogicProvider.ShowNotification("Move behind the starting line before joining the race.", RacingConstants.defaultMsgMs, "White", p.IdentityId);
                    return false;
                }
            }

            if (activePlayers.Add(p.SteamUserId))
            {
                StaticRacerInfo info = GetStaticInfo(p);
                info.Reset();
                if (!MapSettings.TimedMode)
                    finishers.Remove(info);
                MyVisualScriptLogicProvider.ShowNotification("You have joined the race.", RacingConstants.defaultMsgMs, "White", p.IdentityId);
                Nodes.JoinedRace(info);
                return true;
            }
            else
            {
                MyVisualScriptLogicProvider.ShowNotification("You are already in the race.", RacingConstants.defaultMsgMs, "White", p.IdentityId);
                return false;
            }
        }

        bool LeaveRace (IMyPlayer p, bool alwaysMsg = true)
        {
            StaticRacerInfo info = GetStaticInfo(p);
            info.Reset();
            info.Recorder?.LeftTrack();

            info.AutoJoin = false;
            info.HideWaypoint();

            if (activePlayers.Remove(p.SteamUserId))
            {
                MyVisualScriptLogicProvider.ShowNotification("You have left the race.", RacingConstants.defaultMsgMs, "White", p.IdentityId);
                return true;
            }
            else
            {
                if(alwaysMsg)
                    MyVisualScriptLogicProvider.ShowNotification("You are not in the race.", RacingConstants.defaultMsgMs, "White", p.IdentityId);
                return false;
            }
        }

        void ProcessValues ()
        {
            if (Nodes.Count < 2)
            {
                activeRacersText.Clear();
                activeRacersText.Append("Waiting for (" + (2 - Nodes.Count) + ") beacon nodes...").AppendLine();
                return;
            }

            SortedSet<StaticRacerInfo> ranking = new SortedSet<StaticRacerInfo>(new RacerDistanceComparer());
            playersTemp.Clear();
            MyAPIGateway.Players.GetPlayers(playersTemp, IsPlayerActive);
            foreach (IMyPlayer p in playersTemp)
            {
                StaticRacerInfo info = GetStaticInfo(p);

                NodeManager.RacerState state = Nodes.Update(info);
                if(state != NodeManager.RacerState.On)
                {
                    finishers.Add(info);
                    if (!MapSettings.TimedMode)
                        MyVisualScriptLogicProvider.ShowNotificationToAll($"{p.DisplayName} just finished in position {finishers.Count}", RacingConstants.defaultMsgMs, "White");
                    if (state == NodeManager.RacerState.Finish)
                        LeaveRace(p);
                }
                else
                {
                    ranking.Add(info);
                }

            }

            BuildText(ranking);
        }

        void BuildText (SortedSet<StaticRacerInfo> ranking)
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
                activeRacersText.Append(finishers.ToString());

            if (ranking.Count > 0)
            {
                bool drawWhite = false;

                int i = finishers.Count + 1;
                double previousDist = Nodes.TotalDistance * MapSettings.NumLaps;

                SendNextSpectatorResponse(ranking.Last().Id, ranking.First().Racer);
                SendPrevSpectatorResponse(ranking.First().Id, ranking.Last().Racer);

                ulong previousId = 0;
                IMyPlayer previousRacer = null;
                foreach (StaticRacerInfo info in ranking)
                {
                    if (previousId != 0)
                    {
                        // Some spectators need this racer as their current racer.
                        SendNextSpectatorResponse(previousId, info.Racer);
                    }
                    if (previousRacer != null)
                    {
                        // Some spectators need this racer as their current racer.
                        SendPrevSpectatorResponse(info.Id, previousRacer);
                    }

                    string drawnColor = null;
                    if (info.OnTrack)
                    {
                        if (!Moved(info.Racer))
                        {
                            // stationary
                            drawnColor = RacingConstants.colorStationary;
                        }
                        else if(!MapSettings.TimedMode)
                        {
                            if (previousTick.Count == ranking.Count && i < info.Rank)
                            {
                                // just ranked up
                                info.RankUpFrame = Runticks;
                                drawnColor = RacingConstants.colorRankUp;
                            }
                            else if (info.RankUpFrame > 0)
                            {
                                // recently ranked up
                                if (info.RankUpFrame + RacingConstants.rankUpTime > Runticks)
                                    drawnColor = RacingConstants.colorRankUp;
                                else
                                    info.RankUpFrame = 0;
                            }
                        }
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
                    if (MapSettings.TimedMode)
                        activeRacersText.Append("   ");
                    else
                        activeRacersText.Append(RacingTools.SetLength(i, RacingConstants.numberWidth)).Append(' ');

                    // <num> <name>
                    activeRacersText.Append(RacingTools.SetLength(info.Name, RacingConstants.nameWidth)).Append(' ');

                    // <num> <name> <distance>
                    string dist;
                    if(!info.OnTrack)
                    {
                        dist = "---";
                    }
                    else if (info.Missed)
                    {
                        dist = "missed";
                    }
                    else
                    {
                        if (MapSettings.TimedMode)
                        {
                            dist = info.Timer.GetTime(RacingConstants.timerFormating);
                        }
                        else
                        {
                            dist = ((int)(previousDist - info.Distance)).ToString();
                            previousDist = info.Distance;
                        }
                    }
                    activeRacersText.Append(RacingTools.SetLength(dist, RacingConstants.distWidth));

                    int lap = info.Laps;
                    if (info.OnTrack && MapSettings.NumLaps > 1)
                        activeRacersText.Append(' ').Append(lap + 1);

                    activeRacersText.AppendLine();

                    info.Rank = i;
                    i++;
                    previousId = info.Id;
                    previousRacer = info.Racer;
                }
                previousTick = ranking;
            }

            foreach (KeyValuePair<ulong, HashSet<ulong>> kv in nextPlayerRequests)
            {
                foreach (ulong requestor in kv.Value)
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

        bool Moved (IMyPlayer racer)
        {
            IMyCubeGrid grid = RacingTools.GetCockpit(racer)?.CubeGrid;
            if (grid?.Physics == null)
                return false;
            return grid.Physics.LinearVelocity.LengthSquared() > RacingConstants.moveThreshold2;
        }

        public void InvalidateRacerNodes()
        {
            if (Enabled)
                UpdateAllRacerNodes();
            else
                racersNeedUpdating = true;
        }

        private void UpdateAllRacerNodes()
        {
            foreach (ulong id in activePlayers.ToArray())
            {
                StaticRacerInfo info = GetStaticInfo(id);
                if (Nodes.Count < 2 || !Nodes.ResetPosition(info))
                    LeaveRace(info.Racer);
            }
            ClearAllRecorders();
            racersNeedUpdating = false;
        }

        private void ClearAllRecorders()
        {
            foreach(StaticRacerInfo info in staticRacerInfo.Values)
                info.Recorder?.ClearData();
        }

        private void NumLapsChanged (int laps)
        {
            UpdateHeader(laps);
            RacingTools.SendAPIMessage(RacingConstants.apiLaps);
            foreach(ulong id in activePlayers)
            {
                StaticRacerInfo info = GetStaticInfo(id);
                info.Laps = Math.Min(info.Laps, laps - 1);
            }
            ClearAllRecorders();
        }

        private void LoopedChanged(bool looped)
        {
            ClearAllRecorders();
        }
    }
}
