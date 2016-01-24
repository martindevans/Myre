using System;
using Myre.Collections;
using System.Collections.Generic;

namespace Myre.Graphics.Geometry
{
    public interface IGeometryProvider
    {
        void Query(string phase, NamedBoxCollection metadata, ICollection<IGeometry> result);
    }

    public class GeometryRenderer
    {
        private readonly IReadOnlyList<IGeometryProvider> _geometryProviders;

        private readonly List<IGeometry> _geometry = new List<IGeometry>();

        public bool BackToFront { get; set; }

        public GeometryRenderer(IReadOnlyList<IGeometryProvider> geometryProviders)
        {
            _geometryProviders = geometryProviders;
        }

        public List<IGeometry> Query(string phase, Renderer renderer)
        {
            //Find geometry to draw in this phase
            _geometry.Clear();
            foreach (var geometryProvider in _geometryProviders)
                geometryProvider.Query(phase, renderer.Data, _geometry);

            //Return the raw list. Risky, as it could be externally mutated, but we don't want to incur a copy
            return _geometry;
        }

        public void Draw(string phase, Renderer renderer)
        {
            //Draw the geometry
            Draw(Query(phase, renderer), DepthSort.FrontToBack, phase, renderer);
        }

        public static void Draw(List<IGeometry> geometry, DepthSort sort, string phase, Renderer renderer)
        {
            //Depth sort geometry (always sort front-to-back, we'll render in reverse order for back-to-front)
            if (sort != DepthSort.None)
                DepthSortGeometryFrontToBack(geometry);

            //Draw geometry
            switch (sort)
            {
                case DepthSort.BackToFront: {
                    for (int i = geometry.Count - 1; i >= 0; i--)
                        geometry[i].Draw(phase, renderer);
                    break;
                }
                case DepthSort.None:
                case DepthSort.FrontToBack: {
                    foreach (var g in geometry)
                        g.Draw(phase, renderer);

                    break;
                }
                default:
                    throw new ArgumentOutOfRangeException("sort");
            }
        }

        public static void DepthSortGeometryFrontToBack(List<IGeometry> meshes)
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
