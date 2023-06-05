using avaness.RacingMod.Race.Finish;
using avaness.RacingMod.Racers;
using System;
using System.Collections.Generic;

namespace avaness.RacingMod.Race.Modes
{
    public class TrackModeDistance : TrackModeBase
    {
        public override bool EnableRankColors => true;
        public override bool EnableRankIndex => true;

        public TrackModeDistance() : base(TrackModeType.Distance)
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
            return ((int)distance).ToString();
        }
    }
}
