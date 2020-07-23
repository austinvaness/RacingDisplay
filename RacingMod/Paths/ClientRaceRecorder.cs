using Sandbox.ModAPI;
using VRage.Game.ModAPI;

namespace avaness.RacingMod.Paths
{
    public class ClientRaceRecorder : IRaceRecorder
    {
        private PathRecorder recPlay;
        private bool play;
        private int runtime = 0;

        private PathRecorder recTemp = new PathRecorder();
        private bool rec;
        private int recLen = int.MaxValue;
        private int startRec = 0;

        public ClientRaceRecorder()
        {

        }

        public void Update()
        {
            if (rec)
                recTemp.Record(GetGrid());

            if (play)
                play = recPlay.Play();

            runtime++;
        }

        public void StartTrack()
        {
            if(recPlay != null)
                play = true;
            IMyCubeGrid grid = GetGrid();
            recTemp.ClearData();
            recTemp.Record(grid);
            startRec = runtime;
            rec = true;
        }

        public void EndTrack()
        {
            play = false;
            recPlay?.StopPlay();
            if(rec)
            {
                int len = runtime - startRec;
                if(len < recLen)
                {
                    recPlay?.ClearData();
                    recPlay = recTemp;
                    recTemp = new PathRecorder();
                    recLen = len;
                }
                else
                {
                    recTemp.ClearData();
                }
                rec = false;
            }
        }

        public void LeftTrack()
        {
            play = false;
            recPlay?.StopPlay();
            rec = false;
            recTemp.ClearData();
        }

        public void ClearData()
        {
            play = false;
            recPlay?.ClearData();
            rec = false;
            recLen = int.MaxValue;
            recTemp.ClearData();
        }

        private IMyCubeGrid GetGrid()
        {
            return RacingTools.GetCockpit(MyAPIGateway.Session.Player)?.CubeGrid;
        }
    }
}
