using Myre.Extensions;
using System.Numerics;

namespace Myre
{
    public struct Ray
    {
        internal readonly Microsoft.Xna.Framework.Ray XnaRay;

        public Ray(Vector3 position, Vector3 direction)
        {
            XnaRay = new Microsoft.Xna.Framework.Ray(position.ToXNA(), direction.ToXNA());
        }

        public Vector3 Position
        {
            get { return XnaRay.Position.FromXNA(); }
        }

        public Vector3 Direction
        {
            get { return XnaRay.Direction.FromXNA(); }
        }
    }
}
