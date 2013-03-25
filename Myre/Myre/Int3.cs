
namespace Myre
{
    /// <summary>
    /// An integer location in 3space
    /// </summary>
    public struct Int3
    {
        public readonly int X;
        public readonly int Y;
        public readonly int Z;

        public Int3(int x, int y, int z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return X * 37 + Y * 13 + Z * 43;
            }
        }

        public override bool Equals(object obj)
        {
            if (obj is Int3)
            {
                return Equals((Int3)obj);
            }

            return false;
        }

        public bool Equals(Int3 other)
        {
            return X == other.X
                && Y == other.Y
                && Z == other.Z;
        }

        public override string ToString()
        {
            return X + "," + Y + "," + Z;
        }
    }
}
