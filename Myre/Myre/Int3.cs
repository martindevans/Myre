
using System;
using System.Diagnostics.Contracts;
using Microsoft.Xna.Framework;

namespace Myre
{
    /// <summary>
    /// An integer location in 3space
    /// </summary>
    public struct Int3
        : IEquatable<Int3>
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

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [Pure]
        public Vector3 ToVector3()
        {
            return new Vector3(X, Y, Z);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            unchecked
            {
                return X * 37 + Y * 13 + Z * 43;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
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

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("{0},{1},{2}", X, Y, Z);
        }

        /// <summary>
        /// Add together the members of two Int3s
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static Int3 operator +(Int3 a, Int3 b)
        {
            return new Int3(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
        }

        /// <summary>
        /// Subtract the members of one Int3 off another
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static Int3 operator -(Int3 a, Int3 b)
        {
            return new Int3(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool operator ==(Int3 a, Int3 b)
        {
            return a.Equals(b);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool operator !=(Int3 a, Int3 b)
        {
            return !(a == b);
        }
    }
}
