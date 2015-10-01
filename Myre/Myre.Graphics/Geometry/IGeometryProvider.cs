using System;
using Myre.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Myre.Graphics.Geometry
{
    public interface IGeometryProvider
    {
        void Query(string phase, NamedBoxCollection metadata, ICollection<IGeometry> result);
    }

    public class GeometryRenderer
    {
        private readonly ReadOnlyCollection<IGeometryProvider> _geometryProviders;

        private readonly List<IGeometry> _geometry = new List<IGeometry>();

        public bool BackToFront { get; set; }

        public GeometryRenderer(ReadOnlyCollection<IGeometryProvider> geometryProviders)
        {
            _geometryProviders = geometryProviders;
        }

        public void Draw(string phase, Renderer renderer)
        {
            //Find geometry to draw in this phase
            _geometry.Clear();
            foreach (var geometryProvider in _geometryProviders)
                geometryProvider.Query(phase, renderer.Data, _geometry);

            //Draw the geometry
            Draw(_geometry, BackToFront, phase, renderer);
        }

        public static void Draw(List<IGeometry> geometry, bool backToFront, string phase, Renderer renderer)
        {
            //Depth sort geometry
            DepthSortGeometry(geometry);

            //Draw geometry
            if (backToFront)
            {
                for (int i = geometry.Count - 1; i >= 0; i--)
                    geometry[i].Draw(phase, renderer);
            }
            else
            {
                foreach (IGeometry g in geometry)
                    g.Draw(phase, renderer);
            }
        }

        public static void DepthSortGeometry(List<IGeometry> meshes)
        {
            meshes.Sort(RenderDataComparator);
        }

        private static int RenderDataComparator(IGeometry a, IGeometry b)
        {
            //Negated, because XNA uses a negative Z space
            return -a.WorldView.Translation.Z.CompareTo(b.WorldView.Translation.Z);
        }
    }
}
