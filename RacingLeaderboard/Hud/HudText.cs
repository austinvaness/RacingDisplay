using Draygo.API;
using System;
using System.Text;
using VRageMath;
using BlendTypeEnum = VRageRender.MyBillboard.BlendTypeEnum;

namespace avaness.RacingLeaderboard.Hud
{
    public class HudText
    {
        private readonly HudAPIv2.HUDMessage msg;
        private readonly string prefix;
        private string text;

        public HudText(string text, Vector2D origin, Color color, string font = "monospace")
        {
            StringBuilder sb = new StringBuilder();
            if (color != Color.White)
            {
                sb.Append("<color=");
                sb.Append(color.R).Append(',');
                sb.Append(color.G).Append(',');
                sb.Append(color.B).Append('>');
            }
            prefix = sb.ToString();
            sb.Append(text);
            this.text = text;

            msg = new HudAPIv2.HUDMessage(sb, origin, HideHud: false, Blend: BlendTypeEnum.PostPP, Font: font);
        }

        public bool Visible
        {
            get
            {
                return msg.Visible;
            }
            set
            {
                msg.Visible = value;
            }
        }

        public string Text
        {
            get
            {
                return text;
            }
            set
            {
                text = value;
                msg.Message.Clear().Append(prefix).Append(text);
            }
        }

        public Vector2D GetTextLength()
        {
            return msg.GetTextLength();
        }
    }
}
