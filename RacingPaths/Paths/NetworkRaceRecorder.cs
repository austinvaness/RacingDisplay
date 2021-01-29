using ProtoBuf;
using Sandbox.Graphics;
using Sandbox.ModAPI;
using System;
using VRage.Game.ModAPI;

namespace avaness.RacingPaths.Paths
{
    public class NetworkRaceRecorder : IRaceRecorder
    {
        private readonly IMyPlayer p;
        private readonly PathRecorder bestTime;
        private readonly bool local;

        public NetworkRaceRecorder(IMyPlayer p)
        {
            this.p = p;
            bestTime = RacingSession.Instance.Paths.GetRecorder(p);
            local = bestTime == RacingSession.Instance.Paths.LocalRecorder;
        }

        public void Update()
        {
            if(!local)
                bestTime.Update();
        }

        public void StartTrack()
        {
            if (!local)
                bestTime.StartTrack();
            new PathPacket(PathPacket.Method.Start, info.GetSelectedGhosts()).Send(info.Id);
        }

        public void EndTrack()
        {
            if (!local)
                bestTime.EndTrack();
            SendPacket(PathPacket.Method.End);
        }

        public void LeftTrack()
        {
            if (!local)
                bestTime.LeftTrack();
            SendPacket(PathPacket.Method.Left);
        }

        public void ClearData()
        {
            if (!local)
                bestTime.ClearData();
            SendPacket(PathPacket.Method.Clear);
        }

        private void SendPacket(PathPacket.Method method) 
        {
            new PathPacket(method).Send(info.Id);
        }
    }
}
