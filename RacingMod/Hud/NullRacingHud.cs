using Sandbox.Game;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRage.Game.ModAPI;
using VRageMath;

namespace avaness.RacingMod.Hud
{
    public class NullRacingHud : RacingHud
    {
        public override RacingHud Append(string value)
        {
            return this;
        }

        public override RacingHud Append(char value)
        {
            return this;
        }

        public override RacingHud Append(int value)
        {
            return this;
        }

        public override RacingHud Append(HudColor color)
        {
            return this;
        }

        public override RacingHud AppendLine()
        {
            return this;
        }

        public override void Broadcast()
        {

        }

        public override RacingHud Clear()
        {
            return this;
        }

        public override void ToggleUI()
        {

        }

        public override void Unload()
        {

        }
    }
}
