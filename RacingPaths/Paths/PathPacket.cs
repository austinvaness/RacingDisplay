using ProtoBuf;
using Sandbox.ModAPI;

namespace avaness.RacingPaths.Paths
{

    [ProtoContract]
    public class PathPacket
    {
        [ProtoMember(1)]
        public byte methodId;
        [ProtoMember(2)]
        public ulong[] ghosts;

        public Method method => (Method)methodId;

        public enum Method : byte
        {
            Start,
            End,
            Left,
            Clear
        }

        public PathPacket()
        {

        }

        public PathPacket(Method methodId, ulong[] ghosts = null)
        {
            this.methodId = (byte)methodId;
            this.ghosts = ghosts ?? new ulong[0];
        }

        public void Send(ulong steamId)
        {
            if (MyAPIGateway.Session.Player?.SteamUserId == steamId)
            {
                RacingSession.Instance.Paths.PacketReceived(this);
            }
            else
            {
                RacingSession.Instance.Net.SendTo(RacingConstants.packetRecord, this, steamId);
            }
        }
    }
}
