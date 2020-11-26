using Draygo.API;
using System.Text;
using VRageMath;
using BlendTypeEnum = VRageRender.MyBillboard.BlendTypeEnum;

namespace avaness.RacingMod.Hud
{
    public class InfoHud
    {
        private readonly HudAPIv2.HUDMessage infoHud;
        private StringBuilder text;
        private Vector2D pos = new Vector2D(1, 1);

        public InfoHud(StringBuilder text)
        {
            this.text = text;
            infoHud = new HudAPIv2.HUDMessage(text, pos, HideHud: true, Blend: BlendTypeEnum.PostPP);
        }

        public bool Visible
        {
            get
            {
                return infoHud.Visible;
            }
            set
            {
                infoHud.Visible = value;
            }
        }

        public void Set(string s)
        {
            text.Clear().Append(s);
            UpdatePosition();
        }

        public void UpdatePosition()
        {
            if (infoHud == null)
                return;
            Vector2D v = infoHud.GetTextLength();
            v.Y = 0;
            v.X *= -1;
            infoHud.Offset = v;
        }
    }
}
