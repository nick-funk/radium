namespace Radium.Rendering
{
    using System;

    public struct Vector3
    {
        public Vector3(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public float X { get; set; }

        public float Y { get; set; }

        public float Z { get; set; }

        public override bool Equals(object obj)
        {
            if (!(obj is Vector3))
            {
                return false;
            }

            var other = (Vector3)obj;

            return Math.Abs(other.X - X) < float.Epsilon
                   && Math.Abs(other.Y - Y) < float.Epsilon
                   && Math.Abs(other.Z - Z) < float.Epsilon;
        }

        public override int GetHashCode()
        {
            var hashCode = -307843816;
            hashCode = hashCode * -1521134295 + base.GetHashCode();
            hashCode = hashCode * -1521134295 + X.GetHashCode();
            hashCode = hashCode * -1521134295 + Y.GetHashCode();
            hashCode = hashCode * -1521134295 + Z.GetHashCode();
            return hashCode;
        }

        public float[] ToArray()
        {
            return new[] { X, Y, Z };
        }

        public float[] ToArray(float scale)
        {
            return new[] { X * scale, Y * scale, Z * scale };
        }
    }
}