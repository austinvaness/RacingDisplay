using ProtoBuf;
using VRageMath;

namespace avaness.RacingMod.Ghost.Data
{
    [ProtoContract]
    public struct SerializableMatrix
    {
        [ProtoMember(1, IsPacked = true)]
        private float[] data;

        public SerializableMatrix(Matrix m)
        {
            data = new float[9];
            // Translation
            data[0] = m.M41;
            data[1] = m.M42;
            data[2] = m.M43;
            // Forward
            data[3] = -m.M31;
            data[4] = -m.M32;
            data[5] = -m.M33;
            // Up
            data[6] = m.M21;
            data[7] = m.M22;
            data[8] = m.M23;
        }

        public static explicit operator Matrix(SerializableMatrix d)
        {
            return Matrix.CreateWorld(
                new Vector3(d.data[0], d.data[1], d.data[2]),
                new Vector3(d.data[3], d.data[4], d.data[5]),
                new Vector3(d.data[6], d.data[7], d.data[8]));
        }

        public static explicit operator SerializableMatrix(Matrix m)
        {
            return new SerializableMatrix(m);
        }
    }
}
