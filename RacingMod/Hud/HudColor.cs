using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace avaness.RacingMod.Hud
{
    public struct HudColor : IEquatable<HudColor>
    {
        public byte R;
        public byte G;
        public byte B;
        public string AltFont;

        public HudColor(byte red, byte green, byte blue, string altFont)
        {
            R = red;
            G = green;
            B = blue;
            AltFont = altFont;
        }

        public override bool Equals(object obj)
        {
            return obj is HudColor && Equals((HudColor)obj);
        }

        public bool Equals(HudColor other)
        {
            return R == other.R &&
                   G == other.G &&
                   B == other.B;
        }

        public override int GetHashCode()
        {
            int hashCode = -1520100960;
            hashCode = hashCode * -1521134295 + R.GetHashCode();
            hashCode = hashCode * -1521134295 + G.GetHashCode();
            hashCode = hashCode * -1521134295 + B.GetHashCode();
            return hashCode;
        }

        public static bool operator ==(HudColor left, HudColor right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(HudColor left, HudColor right)
        {
            return !(left == right);
        }
    }
}
