using Microsoft.Xna.Framework.Input;
using Myre.Extensions;
using System;
using System.Collections.Generic;
using System.Numerics;

using GameTime = Microsoft.Xna.Framework.GameTime;
using MathHelper = Microsoft.Xna.Framework.MathHelper;
using PlayerIndex = Microsoft.Xna.Framework.PlayerIndex;

namespace Myre.UI.InputDevices
{
    /// <summary>
    /// The left or right.
    /// </summary>
    public enum Side
    {
        /// <summary>
        /// The left.
        /// </summary>
        Left,

        /// <summary>
        /// The right.
        /// </summary>
        Right
    }

    public class GamepadDevice
        : IInputDevice
    {
        public PlayerIndex Player { get; private set; }
        private GamePadState _currentState;
        private GamePadState _previousState;
        private const float MIN_DEAD_ZONE = 0.25f;
        private readonly List<int> _blocked;

        public InputActor Owner { get; set; }

        public Vector2 LeftThumbstick
        {
            get { return _currentState.ThumbSticks.Left.FromXNA(); }
        }

        public Vector2 LeftThumbstickMovement
        {
            get { return _currentState.ThumbSticks.Left.FromXNA() - _previousState.ThumbSticks.Left.FromXNA(); }
        }

        public Vector2 RightThumbstick
        {
            get { return _currentState.ThumbSticks.Right.FromXNA(); }
        }

        public Vector2 RightThumbstickMovement
        {
            get { return _currentState.ThumbSticks.Right.FromXNA() - _previousState.ThumbSticks.Right.FromXNA(); }
        }

        public float LeftTrigger
        {
            get { return _currentState.Triggers.Left; }
        }

        public float LeftTriggerMovement
        {
            get { return _currentState.Triggers.Left - _previousState.Triggers.Left; }
        }

        public float RightTrigger
        {
            get { return _currentState.Triggers.Right; }
        }

        public float RightTriggerMovement
        {
            get { return _currentState.Triggers.Right - _previousState.Triggers.Right; }
        }

        public GamepadDevice(PlayerIndex player)
        {
            Player = player;
            _previousState = _currentState = GamePad.GetState(player);
            _blocked = new List<int>();
        }

        public void Update(GameTime gameTime)
        {
            _previousState = _currentState;
            _currentState = GamePad.GetState(Player, GamePadDeadZone.IndependentAxes);
        }

        public void Evaluate(GameTime gameTime, Control focused, UserInterface ui)
        {
            if (!_currentState.IsConnected)
                return;

            var type = typeof(GamepadDevice);
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

        public Vector2 ApplyDeadZone(Vector2 direction, GamePadDeadZone deadZone, float power)
        {
            switch (deadZone)
            {
                case GamePadDeadZone.Circular:
                    return RescaleMagnitude(direction, power);
                case GamePadDeadZone.IndependentAxes:
                    float x = RescaleAxis(direction.X, power);
                    float y = RescaleAxis(direction.Y, power);
                    return new Vector2(x, y);
                default:
                    return direction;
            }
        }

        private static Vector2 RescaleMagnitude(Vector2 direction, float power)
        {
            float magnitude = direction.Length();

            if (Math.Abs(magnitude - 0) < float.Epsilon)
                return Vector2.Zero;

            float targetMagnitude = Rescale(magnitude, MIN_DEAD_ZONE, 1f);
            targetMagnitude = (float)Math.Pow(targetMagnitude, power);
            return direction * (targetMagnitude / magnitude);
        }

        private float RescaleAxis(float value, float power)
        {
            if (value > 0)
                value = Rescale(value, MIN_DEAD_ZONE, 1f);
            else
                value = -Rescale(-value, MIN_DEAD_ZONE, 1f);

            return (float)Math.Pow(value, power);
        }

        private static float Rescale(float value, float min, float max)
        {
            var range = max - min;
            var alpha = (value - min) / max;
            return MathHelper.Clamp(alpha * range, min, max);
        }

        public bool IsButtonDown(Buttons button)
        {
            return _currentState.IsButtonDown(button);
        }

        public bool IsButtonUp(Buttons button)
        {
            return _currentState.IsButtonUp(button);
        }

        public bool WasButtonDown(Buttons button)
        {
            return _previousState.IsButtonDown(button);
        }

        public bool WasButtonUp(Buttons button)
        {
            return _previousState.IsButtonUp(button);
        }

        public bool IsButtonNewlyDown(Buttons button)
        {
            return IsButtonDown(button) && WasButtonUp(button);
        }

        public bool IsButtonNewlyUp(Buttons button)
        {
            return IsButtonUp(button) && WasButtonDown(button);
        }
    }
}
