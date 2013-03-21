using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Myre.UI.InputDevices
{
    public class MouseDevice
        : IInputDevice
    {
        private MouseState _previousState;
        private MouseState _currentState;
        private readonly List<Control> _controls;
        private readonly List<Control> _previous;
        private readonly IEnumerable<Control> _cooled;
        private readonly IEnumerable<Control> _warmed;
        private readonly List<int> _blocked;

        public InputActor Owner { get; set; }

        public Vector2 Position
        {
            get { return new Vector2(_currentState.X, _currentState.Y); }
            set { Mouse.SetPosition((int)value.X, (int)value.Y); }
        }

        public Vector2 PositionMovement
        {
            get { return new Vector2(_currentState.X - _previousState.X, _currentState.Y - _previousState.Y); }
        }

        public float Wheel
        {
            get { return _currentState.ScrollWheelValue; }
        }

        public float WheelMovement
        {
            get { return _currentState.ScrollWheelValue - _previousState.ScrollWheelValue; }
        }

        public MouseDevice()
        {
            _previousState = _currentState = Mouse.GetState();
            _controls = new List<Control>();
            _blocked = new List<int>();
            _previous = new List<Control>();

            _cooled = _previous.Except(_controls);
            _warmed = _controls.Except(_previous);
        }

        public void Update(GameTime gameTime)
        {
            _previousState = _currentState;
            _currentState = Mouse.GetState();
        }

        public void Evaluate(GameTime gameTime, Control focused, UserInterface ui)
        {
            ui.FindControls(Position, _controls);

            foreach (var item in _cooled)
                item.HeatCount--;
            foreach (var item in _warmed)
                item.HeatCount++;

            var type = typeof(MouseDevice);

            for (int i = 0; i < _controls.Count; i++)
            {
                _controls[i].Gestures.Evaluate(gameTime, this);

                if (_controls[i].Gestures.BlockedDevices.Contains(type))
                    break;
            }

            ui.EvaluateGlobalGestures(gameTime, this);

            _previous.Clear();
            _previous.AddRange(_controls);
            _blocked.Clear();
            _controls.Clear();
        }

        public void BlockInputs(IEnumerable<int> inputs)
        {
            _blocked.AddRange(inputs);
        }

        public bool IsBlocked(IEnumerable<int> inputs)
        {
// ReSharper disable LoopCanBeConvertedToQuery
            foreach (var item in inputs)
// ReSharper restore LoopCanBeConvertedToQuery
            {
                if (_blocked.Contains(item))
                    return true;
            }

            return false;
        }

        public bool IsButtonDown(MouseButtons button)
        {
            return _currentState.IsButtonDown(button);
        }

        public bool IsButtonUp(MouseButtons button)
        {
            return _currentState.IsButtonUp(button);
        }

        public bool WasButtonDown(MouseButtons button)
        {
            return _previousState.IsButtonDown(button);
        }

        public bool WasButtonUp(MouseButtons button)
        {
            return _previousState.IsButtonUp(button);
        }

        public bool IsButtonNewlyDown(MouseButtons button)
        {
            return IsButtonDown(button) && WasButtonUp(button);
        }

        public bool IsButtonNewlyUp(MouseButtons button)
        {
            return IsButtonUp(button) && WasButtonDown(button);
        }

        ~MouseDevice()
        {
            foreach (var item in _previous)
            {
                if (!item.IsDisposed)
                    item.HeatCount--;
            }
        }
    }
}
