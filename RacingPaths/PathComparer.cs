﻿using avaness.RacingPaths.Data;
using System.Collections.Generic;

namespace avaness.RacingPaths
{
    public class PathComparer : IComparer<Path>
    {
        public int Compare(Path x, Path y)
        {
            int result = x.Length.CompareTo(y);
            if (result == 0)
                return 1;
            return result;
        }
    }
}