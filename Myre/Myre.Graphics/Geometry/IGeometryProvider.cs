using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Myre.Collections;
using Microsoft.Xna.Framework;
using Myre.Entities;

namespace Myre.Graphics.Geometry
{
    public interface IGeometryProvider
    {
        void Draw(string phase, BoxedValueStore<string> metadata);
    }
}
