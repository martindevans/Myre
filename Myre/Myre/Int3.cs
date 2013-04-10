
namespace Myre
{
    /// <summary>
    /// An integer location in 3space
    /// </summary>
    public struct Int3
    {
        /// <summary>
        /// X Position
        /// </summary>
        public readonly int X;

        /// <summary>
        /// Y Positions
        /// </summary>
        public readonly int Y;

        /// <summary>
        /// Z Position
        /// </summary>
        public readonly int Z;

        /// <summary>
        /// Constructs a new point in 3D integer space
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
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
