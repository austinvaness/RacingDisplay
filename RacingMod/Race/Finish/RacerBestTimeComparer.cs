using System.Collections.Generic;

namespace avaness.RacingMod.Race.Finish
{
    public class RacerBestTimeComparer : IComparer<IFinisher>
    {
        public int Compare (IFinisher x, IFinisher y)
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
