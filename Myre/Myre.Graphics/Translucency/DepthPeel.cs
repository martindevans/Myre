using Myre.Graphics.Geometry;
using System;
using System.Collections.Generic;

namespace Myre.Graphics.Translucency
{
    internal class DepthPeel
    {
        public void Peel(List<IGeometry> geometry, List<IGeometry>[] layers)
        {
            layers[0].AddRange(geometry);
            //throw new NotImplementedException();
        }
    }
}
