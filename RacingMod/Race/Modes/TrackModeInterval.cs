using avaness.RacingMod.Race.Finish;
using avaness.RacingMod.Racers;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace avaness.RacingMod.Race.Modes
{
    public class TrackModeInterval : TrackModeBase
    {
        public override bool EnableRankColors => true;
        public override bool EnableRankIndex => true;

        public TrackModeInterval() : base(TrackModeType.Interval)
        {
        }

        public override void OnRacerReset(Track race, StaticRacerInfo info)
        {
            info.MarkTime();
        }

        public override void OnRacerFinished(Track race, StaticRacerInfo info)
        {
            RacingTools.ShowNotificationToAll($"{RacingTools.GetDisplayName(info.Racer)} just finished in position {race.Finishers.Count}", RacingConstants.defaultMsgMs, "White");
        }

        public override void OnRacerJoined(Track race, StaticRacerInfo info)
        {
            race.Finishers.Remove(info);
        }

        public override IEnumerable<IFinisher> SortFinisherList(IEnumerable<IFinisher> finishers)
        {
            return finishers;
        }

        public override string GetPosition(int rank, double distance, double? speed2, Timer timer)
        {
            if (!speed2.HasValue || speed2.Value < RacingConstants.moveThreshold2)
                return " ";
            if (rank <= 1)
                return RacingTools.Format(timer.GetTime());
            double seconds = distance / Math.Sqrt(speed2.Value);
            TimeSpan interval = TimeSpan.FromSeconds(seconds);
            if(interval.TotalMinutes > 0)
                return RacingTools.Format(interval);
            return interval.TotalSeconds.ToString("0.###", CultureInfo.InvariantCulture);
        }
    }
}
