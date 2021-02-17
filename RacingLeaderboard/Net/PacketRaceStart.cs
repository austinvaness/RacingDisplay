using ProtoBuf;
using Sandbox.ModAPI;

namespace avaness.RacingLeaderboard.Net
{
    [ProtoContract]
    public class PacketRaceStart
    {
        [ProtoMember(1)]
        public bool recording;

        private PacketRaceStart()
        {

        }

        public PacketRaceStart(bool recording)
        {
            this.recording = recording;
        }

    }
}
