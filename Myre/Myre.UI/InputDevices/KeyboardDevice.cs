using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Xna.Framework.Input;
using Myre.Extensions;

using GameTime = Microsoft.Xna.Framework.GameTime;
using PlayerIndex = Microsoft.Xna.Framework.PlayerIndex;

namespace Myre.UI.InputDevices
{
    public class KeyboardDevice
        : IInputDevice
    {
        private readonly PlayerIndex _player;
        private KeyboardState _previousState;
        private KeyboardState _currentState;
        private readonly List<char> _newCharacters;
        private readonly List<char> _characters;
        private readonly List<int> _blocked;
        private bool _charactersBlocked;

        /// <summary>
        /// Gets the owner of this keyboard device.
        /// </summary>
        public InputActor Owner { get; set; }

        /// <summary>
        /// Gets a collection of chars which were entered in the previous frame.
        /// This only works on Windows. Use Guide.BeginShowKeyboardInput on Xbox or Windows Phone.
        /// </summary>
        public ReadOnlyCollection<char> Characters { get; private set; }

        public KeyboardDevice(PlayerIndex player)
            : this(player, IntPtr.Zero)
        {
        }

        public KeyboardDevice(PlayerIndex player, IntPtr windowHandle)
        {
            _player = player;
            _currentState = Keyboard.GetState(player);
            _previousState = _currentState;
            _blocked = new List<int>();
            _newCharacters = new List<char>();
            _characters = new List<char>();
            Characters = new ReadOnlyCollection<char>(_characters);

#if WINDOWS
            if (player == PlayerIndex.One)
            {
                if (windowHandle != IntPtr.Zero)
                    TextInput.Initialize(windowHandle);

                TextInput.CharEntered += (sender, e) =>
                    {
                        if (char.IsControl(e.Character))
                            return;

                        lock (_newCharacters)
                            _newCharacters.Add(e.Character);
                    };
            }
#endif
        }

        public void Update(GameTime gameTime)
        {
            _previousState = _currentState;
            _currentState = Keyboard.GetState(_player);

            lock (_newCharacters)
            {
                _characters.Clear();
                _characters.AddRange(_newCharacters);
                _newCharacters.Clear();
            }
        }

        public void Evaluate(GameTime gameTime, Control focused, UserInterface ui)
        {
            var type = typeof(KeyboardDevice);
            _charactersBlocked = false;

            for (var control = focused; control != null; control = control.Parent)
            {
                control.Gestures.Evaluate(gameTime, this);

                if (control.Gestures.BlockedDevices.Contains(type))
                    break;
            }

            ui.EvaluateGlobalGestures(gameTime, this);

            _blocked.Clear();
        }

        public void BlockInputs(IEnumerable<int> inputs)
        {
            _blocked.AddRange(inputs);
            if (!_charactersBlocked && inputs.Contains(-1))
                _charactersBlocked = true;
        }

        public bool IsBlocked(IEnumerable<int> inputs)
        {
            foreach (var item in inputs)
            {
                if (_charactersBlocked && (item == -1 || ((Keys)item).IsCharacterKey()))
                    return true;

                if (_blocked.Contains(item))
                    return true;
            }

            return false;
        }

        public bool IsKeyDown(Keys key)
        {
            return _currentState.IsKeyDown(key);
        }

        public bool IsKeyUp(Keys key)
        {
            return _currentState.IsKeyUp(key);
        }

        public bool WasKeyDown(Keys key)
        {
            return _previousState.IsKeyDown(key);
        }

        public bool WasKeyUp(Keys key)
        {
            return _previousState.IsKeyUp(key);
        }

        public bool IsKeyNewlyDown(Keys key)
        {
            return IsKeyDown(key) && WasKeyUp(key);
        }

        public bool IsKeyNewlyUp(Keys key)
        {
            return IsKeyUp(key) && WasKeyDown(key);
        }
    }
}
