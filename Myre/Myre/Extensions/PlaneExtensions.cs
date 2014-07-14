using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Myre.Extensions
{
    /// <summary>
    /// Extensions to the XNA plane structure
    /// </summary>
    public static class PlaneExtensions
    {
        /// <summary>
        /// Projects the given point onto the plane
        /// </summary>
        /// <param name="plane"></param>
        /// <param name="point"></param>
        /// <returns></returns>
        public static Vector3 ClosestPoint(this Plane plane, Vector3 point)
        {
            var d = plane.Distance(point);

            return point - plane.Normal * d;
        }

        /// <summary>
        /// Gets the distance from the plane to the point.
        /// </summary>
        /// <param name="plane"></param>
        /// <param name="point"></param>
        /// <returns></returns>
        public static float Distance(this Plane plane, Vector3 point)
        {
            //http://stackoverflow.com/questions/4436160/distance-to-a-plane

            float dot = Vector3.Dot(plane.Normal, point);
            float value = dot + plane.D;
            return value;
        }
    }
}
