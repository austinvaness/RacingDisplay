using Draygo.API;
using System;
using System.Text;
using VRageMath;
using BlendTypeEnum = VRageRender.MyBillboard.BlendTypeEnum;

namespace avaness.RacingPaths.Hud
{
    public class HudText
    {
        private readonly HudAPIv2.HUDMessage msg;

        public HudText(string text, Vector2D origin, Color color, string font = "white")
        {
            msg = new HudAPIv2.HUDMessage(GetInitialText(text, color), origin, HideHud: false, Blend: BlendTypeEnum.PostPP, Font: font);
        }

        private static StringBuilder GetInitialText(string text, Color color)
        {
            StringBuilder sb = new StringBuilder();
            if(color != Color.White)
            {
                sb.Append("<color=");
                sb.Append(color.R).Append(',');
                sb.Append(color.G).Append(',');
                sb.Append(color.B).Append('>');
            }
            sb.Append(text);
            return sb;
        }

        public Vector2D GetTextLength()
        {
            return msg.GetTextLength();
        }
    }
}
