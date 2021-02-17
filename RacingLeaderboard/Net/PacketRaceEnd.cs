using ProtoBuf;

namespace avaness.RacingLeaderboard.Net
{
    [ProtoContract]
    public class PacketRaceEnd
    {
        [ProtoMember(1)]
        public bool finish;

        private PacketRaceEnd()
        {

        }

        public PacketRaceEnd(bool finish)
        {
            this.finish = finish;
        }
    }
}
