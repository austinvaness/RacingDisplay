﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace avaness.RacingLeaderboard
{
    public static class Tools
    {
        public static string SetLength(string s, int length)
        {
            return s.PadRight(length).Substring(0, length);
        }
    }
}
