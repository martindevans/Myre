using System.Linq;
using System.Numerics;
using Myre.Extensions;
using Myre.Graphics.Geometry;
using SwizzleMyVectors;
using SwizzleMyVectors.Geometry;
using System;
using System.Collections.Generic;

namespace Myre.Graphics.Translucency
{
    internal class DepthPeel
    {
        private readonly List<Layer> _layers = new List<Layer>();
        private readonly List<BoundedGeometry> _geomtryBounds = new List<BoundedGeometry>(); 

        private readonly Vector3[] _corners = new Vector3[8];
        private readonly Vector2[] _corners2D = new Vector2[8];

        public void Peel(List<IGeometry> geometry, IReadOnlyList<List<IGeometry>> batches, View view)
        {
            //Sanity check, we can't batch items into *zero* batches!
            if (batches.Count == 0)
                throw new ArgumentException("Must be at least one layer for depth peeling", "batches");

            //Sort the geometry
            GeometryRenderer.DepthSortGeometryFrontToBack(geometry);

            //Create a layer object for each batch
            CreateLayers(batches);

            //Calculate screen space bounds for each piece of geometry
            _geomtryBounds.Clear();
            for (int i = 0; i < geometry.Count; i++)
            {
                _geomtryBounds.Add(new BoundedGeometry {
                    Geometry = geometry[i],
                    Bounds = CalculateScreenSpaceBounds(geometry[i], view),
                });
            }

            //Cascade down layers, adding each item to the highest index layer possible
            //This ensures that all layers (except 0) have no screen space error
            for (int g = 0; g < _geomtryBounds.Count; g++)
            {
                //Find the highest index layer we overlap, then add to the previous layer
                for (int l = 0; l < _layers.Count; l++)
                {
                    var overlaps = _layers[l].Overlaps(_geomtryBounds[g].Bounds).Any();
                    if (overlaps)
                    {
                        if (l == 0)
                            _layers[0].Add(_geomtryBounds[g]);
                        else
                            _layers[l - 1].Add(_geomtryBounds[g]);
                        break;
                    }

                    //If we get to the last layer with no overlaps, join the last layer
                    if (l == _layers.Count - 1)
                        _layers[_layers.Count - 1].Add(_geomtryBounds[g]);
                }
            }
        }

        private void CreateLayers(IReadOnlyList<List<IGeometry>> batches)
        {
            //Make sure we have the same number of layers as specified in the parameter
            while (_layers.Count < batches.Count)
                _layers.Add(new Layer());
            while (_layers.Count > batches.Count)
                _layers.RemoveAt(_layers.Count - 1);

            //Initialize the layer objects with a list
            for (int i = 0; i < batches.Count; i++)
                _layers[i].Initialize(batches[i]);
        }

        private BoundingRectangle CalculateScreenSpaceBounds(IGeometry item, View view)
        {
            //Create a bounding box around this geometry
            var box = new BoundingBox(item.BoundingSphere);
            box.GetCorners(_corners);

            //Multiply box corners by WVP matrix to move into screen space
            for (int i = 0; i < _corners.Length; i++)
            {
                //Why is world matrix Identity?
                //THe bounding sphere is already in world space, we don't need to apply it again!
                _corners[i] = view.Viewport.Project(_corners[i].ToXNA(), view.Camera.Projection.ToXNA(), view.Camera.View.ToXNA(), Microsoft.Xna.Framework.Matrix.Identity).FromXNA();
                _corners2D[i] = _corners[i].XY();
            }

            //Find a rectangle around this box
            var rect = BoundingRectangle.CreateFromPoints(_corners2D);
            return rect;
        }

        private static float EstimateError(Layer batch, IGeometry geometry, BoundingRectangle screenSpaceBounds)
        {
            float totalError = 0;
            foreach (var bound in batch.Overlaps(screenSpaceBounds))
            {
                var overlap = bound.Intersection(screenSpaceBounds);

                if (!overlap.HasValue)
                    continue;

                var size = overlap.Value.Max - overlap.Value.Min;
                totalError += size.X * size.Y;
            }

            return totalError;
        }

        private struct BoundedGeometry
        {
            public BoundingRectangle Bounds;
            public IGeometry Geometry;
        }

        private class Layer
        {
            public int Count
            {
                get { return _geometryList.Count; }
            }

            private List<IGeometry> _geometryList;
            private readonly List<BoundingRectangle> _screenSpaceBounds = new List<BoundingRectangle>();

            public void Initialize(List<IGeometry> geometry)
            {
                _geometryList = geometry;
                _geometryList.Clear();

                _screenSpaceBounds.Clear();
            }

            public void Add(BoundedGeometry geometry)
            {
                _geometryList.Add(geometry.Geometry);
                _screenSpaceBounds.Add(geometry.Bounds);
            }

            public IEnumerable<BoundingRectangle> Overlaps(BoundingRectangle query)
            {
                //todo: this could be optimised to some kind of interval tree to more efficiently find overlaps
                return _screenSpaceBounds.Where(bound => bound.Intersects(query));
            }
        }
    }
}
