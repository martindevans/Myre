using System.Numerics;
using Microsoft.Xna.Framework.Graphics;
using Myre.Extensions;

using Color = Microsoft.Xna.Framework.Color;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace Myre.Debugging.UI
{
    public class Graph
    {
        float _max;
        readonly float[] _data;
        readonly VertexPositionColor[] _transformedData;
        readonly VertexBuffer _vertices;
        bool _dirty;
        Color _colour;
        readonly BasicEffect _effect;
        float _screenWidth, _screenHeight;
        Rectangle _previousArea;

        public Color Colour
        {
            get { return _colour; }
            set
            {
                if (_colour != value)
                {
                    _colour = value;
                    _dirty = true;
                }
            }
        }

        public Graph(GraphicsDevice device, int resolution)
        {
            _vertices = new VertexBuffer(device, typeof(VertexPositionColor), resolution, BufferUsage.WriteOnly);
            _data = new float[resolution];
            _transformedData = new VertexPositionColor[resolution];
            _colour = Color.Red;
            //effect = Content.Load<Effect>(game, "Graph");

            //var pp = device.PresentationParameters;

            _effect = new BasicEffect(device)
            {
                LightingEnabled = false, 
                VertexColorEnabled = true,
                World = Matrix4x4.Identity.ToXNA(),
                View = Matrix4x4.Identity.ToXNA(),
                Projection = Matrix4x4.Identity.ToXNA()
            };

            _dirty = true;
        }

        public void Add(float value)
        {
            for (int i = 0; i < _data.Length - 1; i++)
                _data[i] = _data[i + 1];

            if (value > _max)
            {
                float scale = _max / value;
                for (int i = 0; i < _data.Length - 1; i++)
                    _data[i] = _data[i] * scale;
                _data[_data.Length - 1] = 1;
                _max = value;
            }
            else
                _data[_data.Length - 1] = value / _max;

            _dirty = true;
        }

        public void Draw(Rectangle area)
        {
            var device = _vertices.GraphicsDevice;
            var pp = device.PresentationParameters;

// ReSharper disable CompareOfFloatsByEqualityOperator
            if (_dirty || area != _previousArea || _screenHeight != pp.BackBufferHeight || _screenWidth != pp.BackBufferWidth)
// ReSharper restore CompareOfFloatsByEqualityOperator
            {
                _screenHeight = pp.BackBufferHeight;
                _screenWidth = pp.BackBufferWidth;

                float x = (area.X / _screenWidth) * 2 - 1;
                float y = -((area.Y / _screenHeight) * 2 - 1);
                float width = (area.Width / _screenWidth) * 2;
                float height = (area.Height / _screenHeight) * 2;

                for (int i = 0; i < _data.Length; i++)
                {
                    var position = new Vector3(
                            x + (i / (float)(_data.Length - 1)) * width,
                            (y - height) + _data[i] * height,
                            0);
                    _transformedData[i] = new VertexPositionColor(position.ToXNA(), _colour);
                }

                _vertices.SetData<VertexPositionColor>(_transformedData);

                _dirty = false;
                _previousArea = area;
            }

            _effect.Techniques[0].Passes[0].Apply();
            device.SetVertexBuffer(_vertices);
            device.DrawPrimitives(PrimitiveType.LineStrip, 0, _data.Length - 1);
        }
    }
}
