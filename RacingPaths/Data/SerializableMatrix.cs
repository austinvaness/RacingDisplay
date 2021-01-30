using ProtoBuf;
using VRage;
using VRageMath;

namespace avaness.RacingPaths.Data
{
    [ProtoContract]
    public struct SerializableMatrix
    {
        [ProtoMember(1)]
        private float forward_x;
        [ProtoMember(2)]
        private float forward_y;
        [ProtoMember(3)]
        private float forward_z;
        [ProtoMember(4)]
        private float up_x;
        [ProtoMember(5)]
        private float up_y;
        [ProtoMember(6)]
        private float up_z;
        [ProtoMember(7)]
        private float translation_x;
        [ProtoMember(8)]
        private float translation_y;
        [ProtoMember(9)]
        private float translation_z;

        public SerializableMatrix(Matrix m)
        {
            forward_x = -m.M31;
            forward_y = -m.M32;
            forward_z = -m.M33;
            up_x = m.M21;
            up_y = m.M22;
            up_z = m.M23;
            translation_x = m.M41;
            translation_y = m.M42;
            translation_z = m.M43;
        }

        public static explicit operator Matrix(SerializableMatrix d)
        {
            return Matrix.CreateWorld(
                new Vector3(d.translation_x, d.translation_y, d.translation_z), 
                new Vector3(d.forward_x, d.forward_y, d.forward_z), 
                new Vector3(d.up_x, d.up_y, d.up_z));
        }

        public static explicit operator SerializableMatrix(Matrix m)
        {
            return new SerializableMatrix(m);
        }
    }
}
