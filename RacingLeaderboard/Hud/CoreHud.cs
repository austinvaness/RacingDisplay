using avaness.RacingLeaderboard.Data;
using avaness.RacingLeaderboard.Recording;
using avaness.RacingLeaderboard.Storage;
using Draygo.API;
using Sandbox.ModAPI;
using System.Text;
using VRageMath;

namespace avaness.RacingLeaderboard.Hud
{
    public class CoreHud
    {
        private readonly HudAPIv2 hud;
        private HudAPIv2.HUDMessage recording;
        private PathStorage paths;
        private PlaybackManager play;
        private LeaderboardHud leaderboard;
        private Cursor cursor;

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

        public CoreHud(PathStorage paths)
        {
            this.paths = paths;
            hud = new HudAPIv2(CreateHud);
        }


        public void Unload()
        {
            hud.Unload();
            cursor?.Unload();
            leaderboard?.Unload();
        }

        private void CreateHud()
        {
            cursor = new Cursor();
            
            recording = new HudAPIv2.HUDMessage(new StringBuilder("Recording"), new Vector2D(-1, 1), Scale: 0.75)
            {
                Visible = false
            };
            leaderboard = new LeaderboardHud(play, cursor, Vector2D.Zero, 0.5, "# ", "Name                ", "Time      ");

            cursor.Create(); // Create last to show on top
        }

        public void Draw()
        {
            if(cursor != null)
            {
                if (MyAPIGateway.Gui.ChatEntryVisible && !cursor.Visible)
                    cursor.Visible = true;
                cursor.Draw();
            }
        }
    }
}
