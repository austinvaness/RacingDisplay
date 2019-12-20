using System;
using VRage.Game.ModAPI;

namespace RacingMod
{
    public class StaticRacerInfo
    {
        public RacerInfo? PreviousTick;
        public ulong Id;
        public Timer Timer;
        public int? NextNode;
        public bool InFinish;
        public int Laps = 0;
        public TimeSpan BestTime = new TimeSpan(0);
        public string Name;

        public StaticRacerInfo(IMyPlayer p)
        {
            Id = p.SteamUserId;
            Name = p.DisplayName;
            Timer = new Timer(true);
        }

        public void Finish()
        {
            TimeSpan span = Timer.GetTime();
            if (BestTime.Ticks == 0 || span < BestTime)
                BestTime = span;
        }
    }

    public class Timer
    {
        DateTime start;
        DateTime? paused;

        public Timer(bool paused = false)
        {
            start = DateTime.Now;
            if (paused)
                this.paused = start;
            else
                this.paused = null;
        }

        public bool Started => !paused.HasValue;

        public TimeSpan GetTime()
        {
            if (paused.HasValue)
                return paused.Value - start;
            return DateTime.Now - start;
        }
        public string GetTime (string format)
        {
            return GetTime().ToString(format);
        }

        public void Start()
        {
            if(paused.HasValue)
            {
                start += DateTime.Now - paused.Value;
                paused = null;
            }
        }

        public void Stop()
        {
            if(!paused.HasValue)
                paused = DateTime.Now;
        }

        public void Reset (bool paused = false)
        {
            start = DateTime.Now;
            if (paused)
                this.paused = start;
            else
                this.paused = null;
        }
    }

    public struct RacerInfo
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
            if (obj is RacerInfo)
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

}
