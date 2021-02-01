using Draygo.API;
using System.Text;
using VRageMath;

namespace avaness.RacingPaths.Hud
{
    public class GhostHud
    {
        private readonly HudAPIv2 hud;
        private HudAPIv2.HUDMessage recording;

        public bool RecordingIndicator
        {
            get
            {
                if (recording == null)
                    return false;
                return recording.Visible;
            }
            set
            {
                if(recording != null)
                    recording.Visible = value;
            }
        }

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
            recording = new HudAPIv2.HUDMessage(new StringBuilder("Recording"), new Vector2D(-1, 1), Scale: 0.75)
            {
                Visible = false
            };
        }
    }
}
