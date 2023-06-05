using avaness.RacingMod.Race.Finish;
using avaness.RacingMod.Racers;
using System;
using System.Collections.Generic;

namespace avaness.RacingMod.Race.Modes
{
    public class TrackModeQualify : TrackModeBase
    {
        public override bool EnableRankColors => false;
        public override bool EnableRankIndex => false;

        public TrackModeQualify() : base(TrackModeType.Qualify)
        {
        }

        public override void OnRacerReset(Track race, StaticRacerInfo info)
        {
            if (info.MarkTime())
            {
                string time = RacingTools.Format(info.BestTime);
                RacingTools.ShowNotificationToAll($"{RacingTools.GetDisplayName(info.Racer)} just finished with time {time}", RacingConstants.defaultMsgMs, "White");
            }
        }

        public override void OnRacerFinished(Track race, StaticRacerInfo info)
        {
        }

        public override void OnRacerJoined(Track race, StaticRacerInfo info)
        {
        }

        public override IEnumerable<IFinisher> SortFinisherList(IEnumerable<IFinisher> finishers)
        {
            return new SortedSet<IFinisher>(finishers, new RacerBestTimeComparer());
        }

        public override string GetPosition(int rank, double distance, double? speed2, Timer timer)
        {
            return RacingTools.Format(timer.GetTime());
        }
    }
}
