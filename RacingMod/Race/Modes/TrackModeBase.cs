using avaness.RacingMod.Race.Finish;
using avaness.RacingMod.Racers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace avaness.RacingMod.Race.Modes
{
    public abstract class TrackModeBase
    {
        public TrackModeType TypeEnum { get; private set; }

        public abstract bool EnableRankColors { get; }
        public abstract bool EnableRankIndex { get; }

        protected TrackModeBase(TrackModeType modeNum)
        {
            TypeEnum = modeNum;
        }

        public static TrackModeBase Create(TrackModeType mode)
        {
            switch (mode)
            {
                case TrackModeType.Interval:
                    return new TrackModeInterval();
                case TrackModeType.Qualify:
                    return new TrackModeQualify();
                default:
                    return new TrackModeDistance();
            }
        }

        public static bool TryParseType(string text, out byte typeRaw)
        {
            TrackModeType mode;
            if(Enum.TryParse(text.Trim(), true, out mode))
            {
                typeRaw = (byte)mode;
                return true;
            }
            typeRaw = 0;
            return false;
        }

        public override string ToString()
        {
            return TypeEnum.ToString();
        }

        public abstract IEnumerable<IFinisher> SortFinisherList(IEnumerable<IFinisher> finishers);

        /// <summary>
        /// Called when the racer finished or is reset via autojoin
        /// </summary>
        public abstract void OnRacerReset(Track race, StaticRacerInfo info);

        /// <summary>
        /// Called when the racer finishes and leaves the race
        /// </summary>
        public abstract void OnRacerFinished(Track race, StaticRacerInfo info);
        public abstract void OnRacerJoined(Track race, StaticRacerInfo info);
        public abstract string GetPosition(int rank, double distance, double? speed2, Timer timer);
    }
}
