using Sandbox.ModAPI;
using VRage.Game.ModAPI;

namespace avaness.RacingPaths.Paths
{
    /// <summary>
    /// Keeps the best time of racer.
    /// </summary>
    public class PathRecorder : IRaceRecorder
    {
        public Path Best { get; private set; }

        private readonly IMyPlayer p;
        private bool rec;
        private Path temp;

        public PathRecorder(IMyPlayer p, Path best = null)
        {
            this.p = p;
            Best = best;
            temp = new Path("BestTime" + p.SteamUserId, p.DisplayName, "The best time of " + p.DisplayName);
        }

        public void Update()
        {
            if(rec)
                RecordTick();
            MyAPIGateway.Utilities.ShowNotification($"Recording: {rec}", 16);
        }

        public void StartTrack()
        {
            temp.ClearData();
            RecordTick();
            rec = true;
        }

        public void EndTrack()
        {
            if(rec)
            {
                if (Best == null || temp.SmallerThan(Best))
                {
                    Path newEmpty = temp.EmptyCopy();
                    Best = temp;
                    temp = newEmpty;
                }
                else
                {
                    temp.ClearData();
                }
                rec = false;
            }
        }

        public void ClearData()
        {
            Best.ClearData();
            Best = null;
            temp.ClearData();
            rec = false;
        }

        public void LeftTrack()
        {
            rec = false;
            temp.ClearData();
        }

        private void RecordTick()
        {
            temp.Record(RacingTools.GetCockpit(p)?.CubeGrid);
        }
    }
}
