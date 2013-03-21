using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Myre.Extensions;
using Myre.UI.Gestures;
using Myre.UI.InputDevices;

namespace Myre.UI
{
    public sealed class UserInterface
        : IGameComponent, IDrawable, IUpdateable
    {
        private readonly List<Control> _buffer;
        private readonly SpriteBatch _spriteBatch;
        private readonly InputActorCollection _actors;
        private int _drawOrder;
        private bool _visible;
        private int _updateOrder;
        private bool _enabled;
        private readonly Dictionary<Type, List<IGesturePair>> _globalGestures;

        public Control Root
        {
            get;
            private set;
        }

        internal Dictionary<Type, List<IGesturePair>> GlobalGestures
        {
            get { return _globalGestures; }
        }

        public InputActorCollection Actors
        {
            get { return _actors; }
        }

        public bool EnableInput { get; set; }

        public GraphicsDevice Device
        {
            get;
            private set;
        }

        #region IUpdateable Members

        public bool Enabled
        {
            get { return _enabled; }
            set
            {
                if (_enabled != value)
                {
                    _enabled = value;
                    OnEnabledChanged();
                }
            }
        }

        public int UpdateOrder
        {
            get { return _updateOrder; }
            set
            {
                if (_updateOrder != value)
                {
                    _updateOrder = value;
                    OnUpdateOrderChanged();
                }
            }
        }

#if XNA_3_1
        public event EventHandler UpdateOrderChanged;
        public event EventHandler EnabledChanged;
#else
        public event EventHandler<EventArgs> UpdateOrderChanged;
        public event EventHandler<EventArgs> EnabledChanged;
#endif

        #endregion

        #region IDrawable Members
        public int DrawOrder
        {
            get { return _drawOrder; }
            set
            {
                if (_drawOrder != value)
                {
                    _drawOrder = value;
                    OnDrawOrderChanged();
                }
            }
        }

        public bool Visible
        {
            get { return _visible; }
            set
            {
                if (_visible != value)
                {
                    _visible = value;
                    OnVisiblChanged();
                }
            }
        }

#if XNA_3_1
        public event EventHandler DrawOrderChanged;
        public event EventHandler VisibleChanged;
#else
        public event EventHandler<EventArgs> DrawOrderChanged;
        public event EventHandler<EventArgs> VisibleChanged;
#endif
        #endregion

        public UserInterface(GraphicsDevice graphics)
        {
            Device = graphics;
            _spriteBatch = new SpriteBatch(Device);
            _buffer = new List<Control>();
            _actors = new InputActorCollection();
            _globalGestures = new Dictionary<Type, List<IGesturePair>>();
            EnableInput = true;
            _visible = true;
            _drawOrder = 100;
            _enabled = true;
            _updateOrder = 0;

            Root = new Control(this);
            Root.SetPoint(Points.TopLeft, 0, 0);
            Root.SetPoint(Points.BottomRight, 0, 0);
        }

        public void Initialize()
        {
        }

        public void Update(GameTime gameTime)
        {
            if (EnableInput && Visible
#if WINDOWS
                )
#else
                && !Guide.IsVisible)
#endif
            {
                foreach (var actor in _actors)
                    actor.Evaluate(gameTime, this);
            }

            _buffer.Clear();
            AddControlsToBuffer(Root);
            _buffer.InsertionSort(ControlStrataComparer.BottomToTop);

            for (int i = 0; i < _buffer.Count; i++)
                _buffer[i].Update(gameTime);
        }

        public void Draw(GameTime gameTime)
        {
#if XNA_3_1
            spriteBatch.Begin(SpriteBlendMode.AlphaBlend);
#else
            _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
#endif
            for (int i = 0; i < _buffer.Count; i++)
            {
                if (_buffer[i].IsVisible)
                    _buffer[i].Draw(_spriteBatch);
            }
            _spriteBatch.End();
        }

        public void EvaluateGlobalGestures(GameTime gameTime, IInputDevice device)
        {
            var deviceType = device.GetType();
            if (_globalGestures.ContainsKey(deviceType))
            {
                var gestures = _globalGestures[deviceType];
                foreach (var gesture in gestures)
                {
                    if (!gesture.Evaluated)
                        gesture.Evaluate(gameTime, device);

                    gesture.Evaluated = false;
                }
            }
        }

        public void FindControls(Vector2 point, ICollection<Control> results)
        {
            for (int i = _buffer.Count - 1; i >= 0; i--)
            {
                var control = _buffer[i];
                if (control.IsVisible && control.Contains(point))
                    results.Add(control);
            }
        }

        private void OnDrawOrderChanged()
        {
            if (DrawOrderChanged != null)
                DrawOrderChanged(this, new EventArgs());
        }

        private void OnVisiblChanged()
        {
            if (VisibleChanged != null)
                VisibleChanged(this, new EventArgs());
        }

        private void OnEnabledChanged()
        {
            if (EnabledChanged != null)
                EnabledChanged(this, new EventArgs());
        }

        private void OnUpdateOrderChanged()
        {
            if (UpdateOrderChanged != null)
                UpdateOrderChanged(this, new EventArgs());
        }

        private void AddControlsToBuffer(Control control)
        {
            _buffer.Add(control);
            foreach (var child in control.Children)
                AddControlsToBuffer(child);
        }
    }
}
