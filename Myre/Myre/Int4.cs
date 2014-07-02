
using System;
using System.Diagnostics.Contracts;
using Microsoft.Xna.Framework;

namespace Myre
{
    /// <summary>
    /// An integer location in 4space
    /// </summary>
    public struct Int4
        : IEquatable<Int4>
    {
        /// <summary>
        /// W Position
        /// </summary>
        public readonly int W;

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
        /// <param name="w"></param>
        public Int4(int x, int y, int z, int w)
        {
            X = x;
            Y = y;
            Z = z;
            W = w;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [Pure]
        public Vector4 ToVector4()
        {
            return new Vector4(X, Y, Z, W);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return X * 37 + Y * 13 + Z * 43 + W * 71;
            }
        }

        public override bool Equals(object obj)
        {
            if (obj is Int4)
            {
                return Equals((Int4)obj);
            }

            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equals(Int4 other)
        {
            return X == other.X
                && Y == other.Y
                && Z == other.Z
                && W == other.W;
        }

        public override string ToString()
        {
            return string.Format("{0},{1},{2},{3}", X, Y, Z, W);
        }

        /// <summary>
        /// Add together the members of two Int3s
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static Int4 operator +(Int4 a, Int4 b)
        {
            return new Int4(a.X + b.X, a.Y + b.Y, a.Z + b.Z, a.W + b.W);
        }

        /// <summary>
        /// Subtract the members of one Int3 off another
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static Int4 operator -(Int4 a, Int4 b)
        {
            return new Int4(a.X - b.X, a.Y - b.Y, a.Z - b.Z, a.W - b.W);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool operator ==(Int4 a, Int4 b)
        {
            return a.Equals(b);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool operator !=(Int4 a, Int4 b)
        {
            return !(a == b);
        }
    }
}
