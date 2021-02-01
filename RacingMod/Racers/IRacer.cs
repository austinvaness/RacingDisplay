using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace avaness.RacingMod.Racers
{
    public interface IRacer
    {
        bool OnTrack { get; set; }
        int Rank { get; set; }
        int RankUpFrame { get; set; }
        string Name { get; }
        bool Missed { get; set; }
        Timer Timer { get; }
        double Distance { get; set; }
        int Laps { get; set; }
    }
}
