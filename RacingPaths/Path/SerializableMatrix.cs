using ProtoBuf;
using VRage;
using VRageMath;

namespace avaness.RacingPaths.Path
{
    [ProtoContract]
    public struct SerializableMatrix
    {
        [ProtoMember(1)]
        private SerializableVector3 forward;
        [ProtoMember(2)]
        private SerializableVector3 up;
        [ProtoMember(3)]
        private SerializableVector3 translation;

        public SerializableMatrix(Matrix m)
        {
            forward = m.Forward;
            up = m.Up;
            translation = m.Translation;
        }

        public static explicit operator Matrix(SerializableMatrix d)
        {
            return Matrix.CreateWorld(d.translation, d.forward, d.up);
        }

        public static explicit operator SerializableMatrix(Matrix m)
        {
            return new SerializableMatrix(m);
        }
    }
}
