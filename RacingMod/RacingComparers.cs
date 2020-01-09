using System.Collections.Generic;

namespace RacingMod
{

    class RacerDistanceComparer : IComparer<RacerInfo>
    {
        public int Compare (RacerInfo x, RacerInfo y)
        {
            int result = y.Distance.CompareTo(x.Distance);
            if (result == 0)
                return 1;
            else
                return result;
        }
    }

    class RacerRankComparer : IComparer<StaticRacerInfo>
    {
        public int Compare (StaticRacerInfo x, StaticRacerInfo y)
        {
            int result = x.FinishTime.CompareTo(y.FinishTime);
            if (result == 0)
                return 1;
            else
                return result;
        }
    }

    class RacerBestTimeComparer : IComparer<StaticRacerInfo>
    {
        public int Compare (StaticRacerInfo x, StaticRacerInfo y)
        {
            int result = x.BestTime.CompareTo(y.BestTime);
            if (result == 0)
                return 1;
            else
                return result;
        }
    }
}
