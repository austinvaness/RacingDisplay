using avaness.RacingMod.Paths;
using avaness.RacingMod.Race.Finish;
using Sandbox.Game;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using VRage.Game.ModAPI;

namespace avaness.RacingMod.Racers
{
    public class StaticRacerInfo : IFinisher, IEquatable<StaticRacerInfo>
    {
        public ulong Id { get; }
        public IMyPlayer Racer { get; set; }
        public string Name { get; }
        public Timer Timer;
        public bool InStart;
        public bool OnTrack;
        public int Laps = 0;
        public long Time => BestTime.Ticks;
        public TimeSpan BestTime { get; private set; } = new TimeSpan(0);
        public int RankUpFrame = 0;
        public int Rank = 0;
        /// <summary>
        /// Distance from start
        /// </summary>
        public double Distance = 0;
        public bool Missed = false;
        public bool AutoJoin = false;
        public IRaceRecorder Recorder { get; private set; }

        private int nextNode;
        public int NextNode
        {
            get
            {
                return nextNode;
            }
            set
            {
                SetNextNode(value);
            }
        }

        public StaticRacerInfo(IMyPlayer p)
        {
            Id = p.SteamUserId;
            Name = RacingTools.SetLength(RacingTools.GetDisplayName(p), RacingConstants.nameWidth);
            Racer = p;
            Timer = new Timer(true);
        }

        public void UpdateTime(SerializableFinisher finisher)
        {
            if (BestTime.Ticks == 0 || BestTime > finisher.BestTime)
                BestTime = finisher.BestTime;
        }

        public void SetNextNode (int value, bool force = false)
        {
            if (nextNode != value || force)
            {
                nextNode = value;
                RacingSession.Instance.Gps.CreateGps(RacingSession.Instance.CurrentNodes.GetCoords(value), Racer.IdentityId);
            }
        }

        public void HideWaypoint()
        {
            RacingSession.Instance?.Gps.RemoveGps(Racer.IdentityId);
        }


        /// <summary>
        /// Returns true if the new time was used.
        /// </summary>
        public bool MarkTime()
        {
            TimeSpan span = Timer.GetTime();
            Recorder?.EndTrack();
            if (BestTime.Ticks == 0 || span < BestTime)
            {
                BestTime = span;
                return true;
            }
            return false;
        }

        public void RemoveFinish()
        {
            BestTime = new TimeSpan(0);
        }

        public void Reset()
        {
            NextNode = 0;
            InStart = false;
            OnTrack = false;
            Laps = 0;
            Timer.Reset(true);
            Rank = 0;
            Distance = 0;
            Missed = false;
            AutoJoin = false;
        }

        public void CreateRecorder()
        {
            if (Recorder == null)
            {
                if (Id == MyAPIGateway.Session.Player?.SteamUserId)
                    Recorder = RacingSession.Instance.Recorder = new ClientRaceRecorder();
                else
                    Recorder = new ServerRaceRecorder(Racer);
            }
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as StaticRacerInfo);
        }

        public bool Equals(StaticRacerInfo other)
        {
            return other != null &&
                   Id == other.Id;
        }

        public override int GetHashCode()
        {
            return 2108858624 + Id.GetHashCode();
        }

        public static bool operator ==(StaticRacerInfo left, StaticRacerInfo right)
        {
            return EqualityComparer<StaticRacerInfo>.Default.Equals(left, right);
        }

        public static bool operator !=(StaticRacerInfo left, StaticRacerInfo right)
        {
            return !(left == right);
        }
    }
}
