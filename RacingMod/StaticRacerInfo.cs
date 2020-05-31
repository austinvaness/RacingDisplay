using Sandbox.Game;
using System;
using VRage;
using VRage.Game.ModAPI;

namespace RacingMod
{
    public class StaticRacerInfo : IEquatable<StaticRacerInfo>
    {
        public ulong Id;
        public IMyPlayer Racer;
        public string Name;
        public Timer Timer;
        public bool InStart;
        public bool OnTrack;
        public int Laps = 0;
        public TimeSpan BestTime = new TimeSpan(0);
        public DateTime FinishTime = new DateTime();
        public int RankUpFrame = 0;
        public int Rank = 0;
        /// <summary>
        /// Distance from start
        /// </summary>
        public double Distance = 0;
        public bool Missed = false;
        public bool AutoJoin = false;

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

        public void SetNextNode (int value, bool force = false)
        {
            if (nextNode != value || force)
            {
                nextNode = value;
                HideWaypoint();
                MyVisualScriptLogicProvider.AddGPSObjective(RacingConstants.gateWaypointName, RacingConstants.gateWaypointDescription, RacingSession.Instance.Nodes.GetCoords(value),
                    RacingConstants.gateWaypointColor, 0, Racer.IdentityId);
            }
        }

        public void HideWaypoint()
        {
            MyVisualScriptLogicProvider.RemoveGPS(RacingConstants.gateWaypointName, Racer.IdentityId);
        }

        public StaticRacerInfo(IMyPlayer p)
        {
            Id = p.SteamUserId;
            Name = p.DisplayName;
            Racer = p;
            Timer = new Timer(true);
        }

        public void Finish()
        {
            TimeSpan span = Timer.GetTime();
            if (BestTime.Ticks == 0 || span < BestTime)
                BestTime = span;
            FinishTime = DateTime.Now;
        }

        public void RemoveFinish()
        {
            BestTime = new TimeSpan(0);
            FinishTime = new DateTime();
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

        public override bool Equals (object obj)
        {
            StaticRacerInfo info = obj as StaticRacerInfo;
            return info != null && Id == info.Id;
        }

        public override int GetHashCode ()
        {
            return 2108858624 + Id.GetHashCode();
        }

        public bool Equals (StaticRacerInfo other)
        {
            return other != null && Id == other.Id;
        }
    }

    public class Timer
    {
        private DateTime started;
        private DateTime? paused;

        private DateTime Now => RacingSession.Instance.Runtime;

        public Timer(bool paused = false)
        {
            started = Now;
            if (paused)
                this.paused = started;
            else
                this.paused = null;
        }

        public TimeSpan GetTime()
        {
            if (paused.HasValue)
                return paused.Value - started;
            return Now - started;
        }
        public string GetTime (string format)
        {
            return GetTime().ToString(format);
        }

        public void Start()
        {
            if(paused.HasValue)
            {
                started += Now - paused.Value;
                paused = null;
            }
        }

        public void Stop()
        {
            if(!paused.HasValue)
                paused = Now;
        }

        public void Reset (bool paused = false)
        {
            started = Now;
            if (paused)
                this.paused = started;
            else
                this.paused = null;
        }
    }
}
