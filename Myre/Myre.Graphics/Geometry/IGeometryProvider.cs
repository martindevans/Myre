using Myre.Collections;

namespace Myre.Graphics.Geometry
{
    public interface IGeometryProvider
    {
        void Draw(string phase, NamedBoxCollection metadata);
    }
}
