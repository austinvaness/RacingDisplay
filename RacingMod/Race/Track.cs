﻿using avaness.RacingMod.Racers;
using Sandbox.Game;
using Sandbox.ModAPI;
using System.Collections.Generic;
using System.Text;
using VRage.Game.ModAPI;
using System.Linq;
using avaness.RacingMod.Race.Finish;
using VRageMath;
using avaness.RacingMod.Hud;
using System;

namespace avaness.RacingMod.Race
{
    public class Track
    {
        public NodeManager Nodes => RacingSession.Instance.Nodes;
        public FinishList Finishers { get; }
        public RacingMapSettings MapSettings { get; }
        public RacerStorage Racers { get; }
        public bool Debug => debug;

        private readonly StringBuilder activeRacersText = new StringBuilder();
        private readonly HashSet<ulong> activePlayers = new HashSet<ulong>();
        private SortedSet<StaticRacerInfo> previousTick = new SortedSet<StaticRacerInfo>();
        private string hudHeader = "";
        private bool debug;

        private HudText altHud;
        private StringBuilder altText;

        private readonly List<IMyPlayer> playersTemp = new List<IMyPlayer>();
        private readonly StringBuilder tempSb = new StringBuilder();

        public Track(RacingMapSettings mapSettings)
        {
            MapSettings = mapSettings;
            Racers = new RacerStorage();
            Finishers = new FinishList();

            NumLapsChanged(MapSettings.NumLaps);
            MapSettings.NumLapsChanged += NumLapsChanged;
            MapSettings.LoopedChanged += LoopedChanged;

        }

        /// <summary>
        /// Called on server only.
        /// </summary>
        public void Init()
        {
            if(!RacingSession.Instance.HasTextHudAPI)
            {
                altText = new StringBuilder();
                altHud = new HudText(1708268562, altText, new Vector2D(-0.125, 0.05), 0.7, RacingConstants.fontId);
            }
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
            int runticks = RacingSession.Instance.Runticks;

            ProcessValues();
            if(runticks % 2 == 0)
            {
                BroadcastData(activeRacersText);

                if (altHud != null && runticks % 10 == 0)
                    altHud.Update();
            }


            if (debug && MyAPIGateway.Session.Player != null)
            {
                Nodes.DrawDebug();
                //RacingSession.Instance.DebugRecorder();
            }


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

        public void Bind(RacingHud hud)
        {
            if (!ReferenceEquals(activeRacersText, hud.Text))
                hud.Text = activeRacersText;
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

            tempSb.Clear();
            tempSb.Append("#".PadRight(RacingConstants.numberWidth + 1));
            tempSb.Append("Name".PadRight(RacingConstants.nameWidth + 1));
            tempSb.Append("Position".PadRight(RacingConstants.distWidth + 1));
            if (laps > 1)
                tempSb.Append("Lap");
            tempSb.AppendLine();
            hudHeader = tempSb.ToString();
        }

        bool IsPlayerActive(IMyPlayer p)
        {
            return activePlayers.Contains(p.SteamUserId) && p.Character?.Physics != null;
        }

        bool Moved(IMyPlayer racer)
        {
            IMyCubeGrid grid = RacingTools.GetCockpit(racer)?.CubeGrid;
            if (grid?.Physics == null)
                return false;
            return grid.Physics.LinearVelocity.LengthSquared() > RacingConstants.moveThreshold2;
        }


        public bool JoinRace(IMyPlayer p, bool force = false)
        {
            if (Nodes.Count < 2)
                return false;

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
                StaticRacerInfo info = Racers.GetStaticInfo(p);
                info.Reset();
                if (!MapSettings.TimedMode)
                    Finishers.Remove(info);
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

        public bool LeaveRace(IMyPlayer p, bool alwaysMsg = true)
        {
            StaticRacerInfo info = Racers.GetStaticInfo(p);
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
                if (alwaysMsg)
                    MyVisualScriptLogicProvider.ShowNotification("You are not in the race.", RacingConstants.defaultMsgMs, "White", p.IdentityId);
                return false;
            }
        }

        void ProcessValues()
        {
            NodeManager nodes = Nodes;

            if (nodes.Count < 2)
            {
                TextClear();
                TextAppend("Waiting for (");
                TextAppend(2 - nodes.Count);
                TextAppend(") beacon nodes...");
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
                    if (!MapSettings.TimedMode)
                        MyVisualScriptLogicProvider.ShowNotificationToAll($"{p.DisplayName} just finished in position {Finishers.Count}", RacingConstants.defaultMsgMs, "White");
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

            TextClear();

            if (ranking.Count == 0 && Finishers.Count == 0)
            {
                TextAppend("No racers in range.");
                previousTick.Clear();
                return;
            }

            activeRacersText.Append(RacingConstants.headerColor);
            TextAppend(hudHeader);
            if (Finishers.Count > 0)
            {
                activeRacersText.Append(RacingConstants.colorFinalist);
                TextAppend(Finishers.ToString());
            }
            activeRacersText.Append(RacingConstants.colorWhite);


            if (ranking.Count > 0)
            {
                bool drawWhite = false;

                int i = Finishers.Count + 1;
                double previousDist = Nodes.TotalDistance * MapSettings.NumLaps;

                foreach (StaticRacerInfo info in ranking)
                {
                    string drawnColor = null;
                    if (info.OnTrack)
                    {
                        if (!Moved(info.Racer))
                        {
                            // stationary
                            drawnColor = RacingConstants.colorStationary;
                        }
                        else if (!MapSettings.TimedMode)
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
                    {
                        TextAppend("   ");
                    }
                    else
                    {
                        TextAppend(RacingTools.SetLength(i, RacingConstants.numberWidth));
                        TextAppend(' ');
                    }

                    // <num> <name>
                    TextAppend(info.FormattedName);
                    TextAppend(' ');
                    
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
                        if (MapSettings.TimedMode)
                        {
                            dist = RacingTools.Format(info.Timer.GetTime());
                        }
                        else
                        {
                            dist = ((int)(previousDist - info.Distance)).ToString();
                            previousDist = info.Distance;
                        }
                    }

                    if(info.OnTrack && MapSettings.NumLaps > 1)
                    {
                        TextAppend(RacingTools.SetLength(dist, RacingConstants.distWidth));
                        TextAppend(' ');
                        TextAppend(info.Laps + 1);
                    }
                    else
                    {
                        TextAppend(dist);
                    }

                    TextAppendLine();

                    info.Rank = i;
                    i++;
                }

                previousTick = ranking;
            }
            else
            {
                previousTick.Clear();
            }

            //table.ClampRowCount(Finishers.Count + ranking.Count);
        }

        private void TextClear()
        {
            activeRacersText.Clear();
            altText?.Clear();
        }

        private void TextAppend(string s)
        {
            activeRacersText.Append(s);
            altText?.Append(s);
        }

        private void TextAppend(int n)
        {
            activeRacersText.Append(n);
            altText?.Append(n);
        }

        private void TextAppend(char ch)
        {
            activeRacersText.Append(ch);
            altText?.Append(ch);
        }

        private void TextAppendLine()
        {
            activeRacersText.AppendLine();
            altText?.AppendLine();
        }

        private void BroadcastData(StringBuilder sb)
        {
            Net.Network net = RacingSession.Instance.Net;
            byte[] data = net.Prep(RacingConstants.packetMainId, Encoding.UTF8.GetBytes(sb.ToString()));

            List<IMyPlayer> players = new List<IMyPlayer>();
            MyAPIGateway.Players.GetPlayers(players);
            foreach (IMyPlayer p in players)
            {
                if (p.SteamUserId == MyAPIGateway.Multiplayer.ServerId)
                    continue;

                net.SendTo(data, p.SteamUserId);
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
