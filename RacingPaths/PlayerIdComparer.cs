using System.Collections.Generic;
using VRage.Game.ModAPI;

namespace avaness.RacingPaths
{
    public class PlayerIdComparer : IEqualityComparer<IMyPlayer>
    {
        public bool Equals(IMyPlayer x, IMyPlayer y)
        {
            return x.SteamUserId.Equals(y.SteamUserId);
        }

        public int GetHashCode(IMyPlayer p)
        {
            return p.SteamUserId.GetHashCode();
        }
    }
}
