using avaness.RacingMod.Net;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRage.Game.ModAPI;

namespace avaness.RacingMod.Ghost
{
    public class RecordingManager
    {
        private readonly PathRecorder record;
        private readonly PathPlayer play;
        private readonly IMyPlayer me;

        public RecordingManager()
        {
            me = MyAPIGateway.Session.Player;
            record = new PathRecorder(me);
            play = new PathPlayer();
        }

        public void Update()
        {
            play.Update();
            record.Update();
        }

        private void OnPlayerLeft()
        {
            record.Cancel();
            play.Clear();
        }

        private void OnPlayerFinished(IMyPlayer p)
        {
            record.Stop();
            play.Clear();
        }

        private void OnPlayerStarted(IMyPlayer p)
        {
            bool recording = false;
            if (recording)
                record.Start();

            bool playing = recording || false;
            if(record.Best != null && playing)
                play.Play(record.Best);
        }
    }
}
