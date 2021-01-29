using ProtoBuf;
using VRage;

namespace avaness.RacingPaths.Paths
{
    [ProtoContract]
    public class SerializablePathInfo
    {
        [ProtoMember(1)]
        public ulong PlayerId { get; private set; }

        [ProtoMember(2)]
        public Path Data { get; set; }

        /// <summary>
        /// Used for serialization only.
        /// </summary>
        private SerializablePathInfo()
        {

        }

        public SerializablePathInfo(ulong pid, Path data)
        {
            PlayerId = pid;
            Data = data;
        }
    }
}
