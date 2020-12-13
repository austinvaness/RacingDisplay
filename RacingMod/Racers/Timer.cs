using System;

namespace avaness.RacingMod.Racers
{
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
