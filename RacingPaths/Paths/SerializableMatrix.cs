using ProtoBuf;
using VRage;
using VRageMath;

namespace avaness.RacingMod.Paths
{
    [ProtoContract]
    public struct SerializableMatrix
    {
        [ProtoMember(1)]
        private SerializableVector3 forward;
        [ProtoMember(2)]
        private SerializableVector3 up;
        [ProtoMember(3)]
        private SerializableVector3 position;

        public SerializableMatrix(Matrix m)
        {
            forward = m.Forward;
            up = m.Up;
            position = m.Translation;
        }

        public static explicit operator Matrix(SerializableMatrix d)
        {
            return Matrix.CreateWorld(d.position, d.forward, d.up);
        }

        public static explicit operator SerializableMatrix(Matrix m)
        {
            return new SerializableMatrix(m);
        }
    }
}
