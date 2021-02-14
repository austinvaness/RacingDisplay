using Sandbox.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRageMath;

namespace avaness.RacingMod.Hud
{
    public class HudText
    {
        private readonly long id;
        private readonly Vector2 pos;
        private readonly StringBuilder text;
        private readonly float fontSize;
        private readonly string font;

        public HudText(long id, StringBuilder text, Vector2D pos, double fontSize = 1, string font = "White")
        {
            this.id = id;
            this.text = text;
            this.pos = new Vector2((float)pos.X, (float)pos.Y);
            this.fontSize = (float)fontSize;
            this.font = font;
        }

        public void Update()
        {
            MyVisualScriptLogicProvider.CreateUIString(id, text.ToString(), pos.X, pos.Y, fontSize, font);
        }

        public void Delete()
        {
            MyVisualScriptLogicProvider.RemoveUIString(id);
        }
    }
}
