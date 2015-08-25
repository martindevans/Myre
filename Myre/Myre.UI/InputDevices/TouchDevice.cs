
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Input.Touch;
using Myre.Extensions;
using GameTime = Microsoft.Xna.Framework.GameTime;

namespace Myre.UI.InputDevices
{
    public class TouchDevice
        : IInputDevice
    {
        private TouchCollection _touches;
        private List<int> _blocked;
        private List<Control> _buffer;
        private List<Control> _current;
        private List<Control> _previous;
        private IEnumerable<Control> _warmed;
        private IEnumerable<Control> _cooled;

        public InputActor Owner { get; set; }

        public TouchLocation Current { get; private set; }

        public void Update(GameTime gameTime)
        {
            _touches = TouchPanel.GetState();
            _buffer = new List<Control>();
            _current = new List<Control>();
            _previous = new List<Control>();
            _blocked = new List<int>();

            _cooled = _previous.Except(_current).Distinct();
            _warmed = _current.Except(_previous).Distinct();
        }

        public void Evaluate(GameTime gameTime, Control focused, UserInterface ui)
        {
            var type = typeof(TouchDevice);

            for (int i = 0; i < _touches.Count; i++)
            {
                var t = _touches[i];
                Current = t;

                ui.FindControls(t.Position.FromXNA(), _buffer);
                _current.AddRange(_buffer);

                for (int j = 0; j < _buffer.Count; j++)
                {
                    _buffer[j].Gestures.Evaluate(gameTime, this);

                    if (_buffer[j].Gestures.BlockedDevices.Contains(type))
                        break;
                }

                ui.EvaluateGlobalGestures(gameTime, this);
                _blocked.Clear();
                _buffer.Clear();
            }

            foreach (var item in _cooled)
                item.HeatCount--;
            foreach (var item in _warmed)
                item.HeatCount++;

            _previous.Clear();
            _previous.AddRange(_current);
            _current.Clear();
        }

        public void BlockInputs(IEnumerable<int> inputs)
        {
            _blocked.AddRange(inputs);
        }

        public bool IsBlocked(IEnumerable<int> inputs)
        {
            foreach (var item in inputs)
            {
                if (_blocked.Contains(item))
                    return true;
            }

            return false;
        }
    }
}