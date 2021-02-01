using ProtoBuf;
using Sandbox.ModAPI;

namespace avaness.RacingPaths.Net
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
