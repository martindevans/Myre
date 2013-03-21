using Microsoft.Xna.Framework;

namespace Myre.UI
{
    /// <summary>
    /// Defines an vector with 2 integer components.
    /// </summary>
    public struct Int2D
    {
        private static readonly Int2D _zero = new Int2D(0);
        private static readonly Int2D _one = new Int2D(1);

        /// <summary>
        /// An Int2D with X and Y components of 0.
        /// </summary>
        public static Int2D Zero { get { return _zero; } }

        /// <summary>
        /// An Int2D with X and Y components of 1.
        /// </summary>
        public static Int2D One { get { return _one; } }

        /// <summary>
        /// The x component of this vector.
        /// </summary>
        public readonly int X;

        /// <summary>
        /// The y component of this vector.
        /// </summary>
        public readonly int Y;

        /// <summary>
        /// Initializes a new instance of the <see cref="Int2D"/> struct.
        /// </summary>
        public Int2D(int value)
        {
            X = value;
            Y = value;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Int2D"/> struct.
        /// </summary>
        public Int2D(int x, int y)
        {
            X = x;
            Y = y;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Int2D"/> struct.
        /// </summary>
        public Int2D(Vector2 v)
        {
            X = (int)v.X;
            Y = (int)v.Y;
        }

        /// <summary>
        /// Returns a string representation of the Int2D.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.String"/> representing the Int2D.
        /// </returns>
        public override string ToString()
        {
            return "{" + X + ", " + Y + "}";
        }

        /// <summary>
        /// Indicates whether this instance and a specified Int2D are equal.
        /// </summary>
        /// <param name="value">The value to compare to.</param>
        /// <returns>true is <paramref name="value"/> and this instance are equal; else false.</returns>
        public bool Equals(Int2D value)
        {
            return X == value.X && Y == value.Y;
        }

        /// <summary>
        /// Indicates whether this instance and a specified object are equal.
        /// </summary>
        /// <param name="obj">Another object to compare to.</param>
        /// <returns>
        /// true if <paramref name="obj"/> and this instance are the same type and represent the same value; otherwise, false.
        /// </returns>
        public override bool Equals(object obj)
        {
            if (obj is Int2D)
                return Equals((Int2D)obj);
            return base.Equals(obj);
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode()
        {
            return X ^ Y;
        }

        #region Operators
        /// <summary>
        /// Implements the operator -.
        /// </summary>
        /// <param name="a">A.</param>
        /// <returns>A new Int2D with negated X and Y fields.</returns>
        public static Int2D operator -(Int2D a)
        {
            return new Int2D(-a.X, -a.Y);
        }

        /// <summary>
        /// Implements the operator +.
        /// </summary>
        /// <param name="a">A.</param>
        /// <param name="b">The b.</param>
        /// <returns>A new Int2D, where newValue.X = a.X + b.X, and newValue.Y = a.Y + b.Y.</returns>
        public static Int2D operator +(Int2D a, Int2D b)
        {
            return new Int2D(a.X + b.X, a.Y + b.Y);
        }

        /// <summary>
        /// Implements the operator -.
        /// </summary>
        /// <param name="a">A.</param>
        /// <param name="b">The b.</param>
        /// <returns>A new Int2D, where newValue.X = a.X - b.X, and newValue.Y = a.Y - b.Y.</returns>
        public static Int2D operator -(Int2D a, Int2D b)
        {
            return new Int2D(a.X - b.X, a.Y - b.Y);
        }

        /// <summary>
        /// Implements the operator *.
        /// </summary>
        /// <param name="a">A.</param>
        /// <param name="b">The b.</param>
        /// <returns>A new Int2D, where newValue.X = a.X * b.X, and newValue.Y = a.Y * b.Y.</returns>
        public static Int2D operator *(Int2D a, Int2D b)
        {
            return new Int2D(a.X * b.X, a.Y * b.Y);
        }

        /// <summary>
        /// Implements the operator /.
        /// </summary>
        /// <param name="a">A.</param>
        /// <param name="b">The b.</param>
        /// <returns>A new Int2D, where newValue.X = a.X / b.X, and newValue.Y = a.Y / b.Y.</returns>
        public static Int2D operator /(Int2D a, Int2D b)
        {
            return new Int2D(a.X / b.X, a.Y / b.Y);
        }

        /// <summary>
        /// Implements the operator *.
        /// </summary>
        /// <param name="a">A.</param>
        /// <param name="b">The b.</param>
        /// <returns>A new Int2D, where newValue.X = a.X * b, and newValue.Y = a.Y * b.</returns>
        public static Int2D operator *(Int2D a, int b)
        {
            return new Int2D(a.X * b, a.Y * b);
        }

        /// <summary>
        /// Implements the operator *.
        /// </summary>
        /// <param name="a">A.</param>
        /// <param name="b">The b.</param>
        /// <returns>A new Int2D, where newValue.X = a * b.X, and newValue.Y = a * b.Y.</returns>
        public static Int2D operator *(int a, Int2D b)
        {
            return b * a;
        }

        /// <summary>
        /// Implements the operator /.
        /// </summary>
        /// <param name="a">A.</param>
        /// <param name="b">The b.</param>
        /// <returns>A new Int2D, where newValue.X = a.X / b, and newValue.Y = a.Y / b.</returns>
        public static Int2D operator /(Int2D a, int b)
        {
            return new Int2D(a.X / b, a.Y / b);
        }

        /// <summary>
        /// Implements the operator /.
        /// </summary>
        /// <param name="a">A.</param>
        /// <param name="b">The b.</param>
        /// <returns>A new Int2D, where newValue.X = a / b.X, and newValue.Y = a / b.Y.</returns>
        public static Int2D operator /(int a, Int2D b)
        {
            return b / a;
        }

        /// <summary>
        /// Implements the operator ==.
        /// </summary>
        /// <param name="a">A.</param>
        /// <param name="b">The b.</param>
        /// <returns><c>true</c> if a.X == b.X and a.Y == b.Y; else <c>false</c>.</returns>
        public static bool operator ==(Int2D a, Int2D b)
        {
            return a.Equals(b);
        }

        /// <summary>
        /// Implements the operator !=.
        /// </summary>
        /// <param name="a">A.</param>
        /// <param name="b">The b.</param>
        /// <returns><c>true</c> if a.X != b.X or a.Y != b.Y; else <c>false</c>.</returns>
        public static bool operator !=(Int2D a, Int2D b)
        {
            return !a.Equals(b);
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="Myre.UI.Int2D"/> to <see cref="Microsoft.Xna.Framework.Point"/>.
        /// </summary>
        /// <param name="a">A.</param>
        /// <returns>The result of the conversion.</returns>
        public static implicit operator Point(Int2D a)
        {
            return new Point(a.X, a.Y);
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="Microsoft.Xna.Framework.Point"/> to <see cref="Myre.UI.Int2D"/>.
        /// </summary>
        /// <param name="a">A.</param>
        /// <returns>The result of the conversion.</returns>
        public static implicit operator Int2D(Point a)
        {
            return new Int2D(a.X, a.Y);
        }

        /// <summary>
        /// Performs an explicit conversion from <see cref="Microsoft.Xna.Framework.Vector2"/> to <see cref="Myre.UI.Int2D"/>.
        /// </summary>
        /// <param name="a">A.</param>
        /// <returns>The result of the conversion.</returns>
        public static explicit operator Int2D(Vector2 a)
        {
            return new Int2D((int)a.X, (int)a.Y);
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="Myre.UI.Int2D"/> to <see cref="Microsoft.Xna.Framework.Vector2"/>.
        /// </summary>
        /// <param name="a">A.</param>
        /// <returns>The result of the conversion.</returns>
        public static implicit operator Vector2(Int2D a)
        {
            return new Vector2(a.X, a.Y);
        }
        #endregion
    }
}
