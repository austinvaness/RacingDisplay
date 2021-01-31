using Draygo.API;
using System.Text;
using VRage.Game;
using VRageMath;

namespace avaness.RacingPaths.Hud
{
    public class GhostHud
    {
        private readonly HudAPIv2 hud;
        private HudAPIv2.HUDMessage board;
        private readonly StringBuilder boardText = new StringBuilder();


        public GhostHud()
        {
            hud = new HudAPIv2(OnHudReady);
        }

        public void Unload()
        {
            hud.Unload();
        }

        private void OnHudReady()
        {
            board = new HudAPIv2.HUDMessage(boardText, new Vector2D(0.95, 0.9));
        }

        private void Realign()
        {
            Vector2D size = board.GetTextLength();
            board.Offset = new Vector2D(-size.X, 0);
        }
    }
}
