using ProtoBuf;

namespace avaness.RacingLeaderboard.Net
{
    [ProtoContract]
    public class PacketStartRecording
    {
        public bool recording;

        private PacketStartRecording()
        {

        }

        public PacketStartRecording(bool recording)
        {
            this.recording = recording;
        }
    }
}
