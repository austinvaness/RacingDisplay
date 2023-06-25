using avaness.RacingMod.Racers;
using Sandbox.Game;
using Sandbox.ModAPI;
using System.Collections.Generic;
using System.Text;
using VRage.Game.ModAPI;
using System.Linq;
using avaness.RacingMod.Race.Finish;
using System;
using avaness.RacingMod.Hud;
using avaness.RacingMod.Race.Modes;

namespace avaness.RacingMod.Race
{
    public class Track
    {
        public NodeManager Nodes => RacingSession.Instance.CurrentNodes;
        public FinishList Finishers { get; }
        public RacingMapSettings MapSettings { get; }
        public RacerStorage Racers { get; }

        private RacingHud hud = new NullRacingHud();

        private readonly HashSet<ulong> activePlayers = new HashSet<ulong>();
        private SortedSet<StaticRacerInfo> previousTick = new SortedSet<StaticRacerInfo>();
        private bool debug;

        private readonly List<IMyPlayer> playersTemp = new List<IMyPlayer>();

        public Track(RacingMapSettings mapSettings)
        {
            MapSettings = mapSettings;
            Racers = new RacerStorage();
            Finishers = new FinishList();

            NumLapsChanged(MapSettings.NumLaps);
            MapSettings.NumLapsChanged += NumLapsChanged;
            MapSettings.LoopedChanged += LoopedChanged;

        }

        public void LoadServer()
        {
            RacingSession.Instance.Net.Register(RacingConstants.packetAutoRec, EnableRecording);
            Finishers.LoadFile(Racers);
        }


        public void Unload()
        {
            MapSettings.NumLapsChanged -= NumLapsChanged;
            MapSettings.LoopedChanged -= LoopedChanged;
            Finishers.Unload();
        }

        public void Update()
        {
            ProcessValues();
            hud.Broadcast();
        }

        public void SaveData()
        {
            Finishers.SaveFile();
        }

        public void EnableRecording(ulong sender)
        {
            if (sender == 0)
                return;
            Racers.GetStaticInfo(sender).CreateRecorder();
        }

        public void SetOutputHud(RacingHud hud)
        {
            this.hud = hud;
        }


        private void LoopedChanged(bool looped)
        {
            Racers.ClearRecorders();
        }


        public void ToggleDebug()
        {
            debug = !debug;
        }

        public bool Contains(ulong id)
        {
            return activePlayers.Contains(id);
        }

        private void NumLapsChanged(int laps)
        {
            foreach (ulong id in activePlayers)
                Racers.SetMaxLaps(id, laps);
            Racers.ClearRecorders();
        }

        bool IsPlayerActive(IMyPlayer p)
        {
            return activePlayers.Contains(p.SteamUserId) && p.Character?.Physics != null;
        }

        double? GetSpeed(IMyPlayer racer)
        {
            IMyCubeGrid grid = RacingTools.GetCockpit(racer)?.CubeGrid;
            if (grid?.Physics == null)
                return null;
            return grid.Physics.LinearVelocity.LengthSquared();
        }


        public bool JoinRace(IMyPlayer p, bool force = false)
        {
            if (Nodes.Count < 2)
                return false;

            if (!force && MapSettings.StrictStart)
            {
                if (Nodes.OnTrack(p))
                {
                    RacingTools.ShowNotification("Move behind the starting line before joining the race.", RacingConstants.defaultMsgMs, "White", p.IdentityId);
                    return false;
                }
            }

            if (activePlayers.Add(p.SteamUserId))
            {
                StaticRacerInfo info = Racers.GetStaticInfo(p);
                info.Reset();
                MapSettings.Mode.OnRacerJoined(this, info);
                RacingTools.ShowNotification("You have joined the race.", RacingConstants.defaultMsgMs, "White", p.IdentityId);
                Nodes.JoinedRace(info);
                return true;
            }
            else
            {
                RacingTools.ShowNotification("You are already in the race.", RacingConstants.defaultMsgMs, "White", p.IdentityId);
                return false;
            }
        }

        public bool LeaveRace(IMyPlayer p, bool alwaysMsg = true)
        {
            StaticRacerInfo info = Racers.GetStaticInfo(p);
            return LeaveRace(info, alwaysMsg);
        }

        private bool LeaveRace(StaticRacerInfo info, bool alwaysMsg)
        {
            IMyPlayer p = info.Racer;
            info.Reset();
            info.Recorder?.LeftTrack();

            info.AutoJoin = false;
            info.HideWaypoint();

            if (activePlayers.Remove(p.SteamUserId))
            {
                RacingTools.ShowNotification("You have left the race.", RacingConstants.defaultMsgMs, "White", p.IdentityId);
                return true;
            }
            else
            {
                if (alwaysMsg)
                    RacingTools.ShowNotification("You are not in the race.", RacingConstants.defaultMsgMs, "White", p.IdentityId);
                return false;
            }
        }

        public void Reset()
        {
            foreach (StaticRacerInfo info in Racers)
            {
                info.Recorder?.ClearData();
                LeaveRace(info, false);
            }
            Finishers.Clear();
        }


        public void ToggleAutoJoin(IMyPlayer p)
        {
            if (MapSettings.Looped)
            {
                if (!Contains(p.SteamUserId) && !JoinRace(p))
                    return;

                StaticRacerInfo info = Racers.GetStaticInfo(p);
                if (info.AutoJoin)
                    RacingTools.ShowNotification("You will leave the race after finishing.", playerId: p.IdentityId);
                else
                    RacingTools.ShowNotification("Your timer will reset after finishing.", playerId: p.IdentityId);
                info.AutoJoin = !info.AutoJoin;
            }
            else
            {
                RacingTools.ShowNotification("Auto join only works for looped races.", playerId: p.IdentityId);
            }
        }

        void ProcessValues()
        {
            NodeManager nodes = Nodes;

            if (nodes.Count < 2)
            {
                hud.Clear();
                hud.Append("Racing Display").AppendLine();
                hud.Append("Current track '").Append(nodes.Id).Append("' is missing nodes.");
                return;
            }

            SortedSet<StaticRacerInfo> ranking = new SortedSet<StaticRacerInfo>(new RacerDistanceComparer());
            playersTemp.Clear();
            MyAPIGateway.Players.GetPlayers(playersTemp, IsPlayerActive);
            foreach (IMyPlayer p in playersTemp)
            {
                StaticRacerInfo info = Racers.GetStaticInfo(p);

                NodeManager.RacerState state = nodes.Update(info);
                if (state != NodeManager.RacerState.On)
                {
                    Finishers.Add(info);
                    MapSettings.Mode.OnRacerFinished(this, info);
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

        void BuildText(SortedSet<StaticRacerInfo> ranking)
        {
            int runticks = RacingSession.Instance.Runticks;

            // Build the active racer text
            hud.Clear();
            if (ranking.Count == 0 && Finishers.Count == 0)
            {
                hud.Append("Racing Display");
                previousTick.Clear();
                return;
            }

            // Header
            hud.Append(RacingConstants.headerColor).Append("#".PadRight(RacingConstants.numberWidth + 1));
            hud.Append("Name".PadRight(RacingConstants.nameWidth + 1));
            hud.Append("Position".PadRight(RacingConstants.distWidth + 1));
            if (MapSettings.NumLaps > 1)
                hud.Append("Lap");
            hud.AppendLine().Append(RacingConstants.colorWhite);

            // Finishers
            if (Finishers.Count > 0)
                Finishers.BuildText(hud);

            TrackModeBase mode = MapSettings.Mode;
            if (ranking.Count > 0)
            {
                bool drawWhite = false;

                int i = Finishers.Count + 1;
                double previousDist = Nodes.TotalDistance * MapSettings.NumLaps;

                foreach (StaticRacerInfo info in ranking)
                {
                    double? speed = GetSpeed(info.Racer);
                    HudColor? drawnColor = null;
                    if (info.OnTrack)
                    {
                        if (!speed.HasValue || speed.Value < RacingConstants.moveThreshold2)
                        {
                            // stationary
                            drawnColor = RacingConstants.colorStationary;
                        }
                        else if (mode.EnableRankColors)
                        {
                            if (previousTick.Count == ranking.Count && i < info.Rank)
                            {
                                // just ranked up
                                info.RankUpFrame = runticks;
                                drawnColor = RacingConstants.colorRankUp;
                            }
                            else if (info.RankUpFrame > 0)
                            {
                                // recently ranked up
                                if (info.RankUpFrame + RacingConstants.rankUpTime > runticks)
                                    drawnColor = RacingConstants.colorRankUp;
                                else
                                    info.RankUpFrame = 0;
                            }
                        }
                    }

                    if (drawnColor.HasValue)
                    {
                        hud.Append(drawnColor.Value);
                        drawWhite = true;
                    }
                    else if (drawWhite)
                    {
                        hud.Append(RacingConstants.colorWhite);
                        drawWhite = false;
                    }

                    // <num>
                    if (mode.EnableRankIndex)
                        hud.Append(RacingTools.SetLength(i, RacingConstants.numberWidth)).Append(' ');
                    else
                        hud.Append("   ");

                    // <num> <name>
                    hud.Append(info.Name).Append(' ');

                    // <num> <name> <distance>
                    string dist;
                    if (!info.OnTrack)
                    {
                        dist = "---";
                    }
                    else if (info.Missed)
                    {
                        dist = "missed";
                    }
                    else
                    {
                        dist = mode.GetPosition(i, previousDist - info.Distance, speed, info.Timer);
                        previousDist = info.Distance;
                    }

                    if(info.OnTrack && MapSettings.NumLaps > 1)
                    {
                        hud.Append(RacingTools.SetLength(dist, RacingConstants.distWidth));
                        int lap = info.Laps;
                        hud.Append(' ').Append(lap + 1);
                    }
                    else
                    {
                        hud.Append(dist);
                    }

                    hud.AppendLine();

                    info.Rank = i;
                    i++;
                }
                previousTick = ranking;
            }
            else
            {
                previousTick.Clear();
            }
        }

        public void UpdateAllRacerNodes()
        {
            NodeManager nodes = Nodes;
            foreach (ulong id in activePlayers.ToArray())
            {
                StaticRacerInfo info = Racers.GetStaticInfo(id);
                if (nodes.Count < 2 || !nodes.ResetPosition(info))
                    LeaveRace(info.Racer);
            }
            Racers.ClearRecorders();
        }

        public IEnumerable<StaticRacerInfo> GetListedRacers()
        {
            return previousTick;
        }
    }
}
