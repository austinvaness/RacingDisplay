using System.Collections.Generic;

namespace avaness.RacingMod
{
    public class RacerDistanceComparer : IComparer<StaticRacerInfo>
    {
        public int Compare (StaticRacerInfo x, StaticRacerInfo y)
        {
            if (x.Equals(y))
                return 0;
            int result = y.Distance.CompareTo(x.Distance);
            if (result == 0)
                return 1;
            else
                return result;
        }
    }

    public class RacerBestTimeComparer : IComparer<StaticRacerInfo>
    {
        public int Compare (StaticRacerInfo x, StaticRacerInfo y)
        {
            if (x.Equals(y))
                return 0;
            int result = x.BestTime.CompareTo(y.BestTime);
            if (result != 0)
                return result;
            return 1;
        }
    }
}
