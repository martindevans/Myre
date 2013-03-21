using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Myre.UI.InputDevices;

namespace Myre.UI.Gestures
{
    /// <summary>
    /// Encapsulates a method which handles input events.
    /// </summary>
    /// <typeparam name="G">The type of input gesture. This must inherit from IGesture.</typeparam>
    /// <param name="gesture">The gesture which has caused the input event.</param>
    /// <param name="gameTime">The GameTime for this frame.</param>
    /// <param name="device">The input device which caused the event.</param>
    public delegate void GestureHandler<in G>(G gesture, GameTime gameTime, IInputDevice device) 
        where G : IGesture;

    interface IGesturePair
    {
        bool Evaluated { get; set; }
        bool Evaluate(GameTime gameTime, IInputDevice device);
        void BlockInputs(IInputDevice device);
    }

    class GesturePair<G>
        : IGesturePair
        where G : IGesture
    {
        public G Gesture { get; set; }
        public GestureHandler<G> Handler { get; set; }
        public bool Evaluated { get; set; }

        private bool _matched;

        public bool Evaluate(GameTime gameTime, IInputDevice device)
        {
            Evaluated = true;

            if (device.IsBlocked(Gesture.BlockedInputs))
                return false;

            if (Gesture.Test(device))
            {
                Handler(Gesture, gameTime, device);
                _matched = true;
                return true;
            }

            return false;
        }

        public void BlockInputs(IInputDevice device)
        {
            if (_matched)
            {
                device.BlockInputs(Gesture.BlockedInputs);
                _matched = false;
            }
        }
    }


    public class GestureGroup
    {
        private readonly Dictionary<Type, List<IGesturePair>> _gesturePairs;
        private readonly UserInterface _ui;

        public List<Type> BlockedDevices { get; private set; }

        public GestureGroup(UserInterface ui)
        {
            _gesturePairs = new Dictionary<Type, List<IGesturePair>>();
            _ui = ui;

            BlockedDevices = new List<Type>();
        }

        public void Evaluate(GameTime gameTime, IInputDevice device)
        {
            Type type = device.GetType();
            if (_gesturePairs.ContainsKey(type))
            {
                List<IGesturePair> pairs = _gesturePairs[type];
                for (int i = 0; i < pairs.Count; i++)
                    pairs[i].Evaluate(gameTime, device);
                for (int i = 0; i < pairs.Count; i++)
                    pairs[i].BlockInputs(device);
            }
        }

        /// <summary>
        /// Binds the specified gesture to the control.
        /// </summary>
        /// <param name="handler">A delegate with signature: (IGesture gesture, GameTime gameTime, IInputDevice device)</param>
        /// <param name="gestures">The gestures to bind.</param>
        public void Bind(GestureHandler<IGesture> handler, params IGesture[] gestures)
        {
            Bind<IGesture>(handler, gestures);
        }

        /// <summary>
        /// Binds the specified gesture to the control.
        /// </summary>
        /// <param name="handler">A delegate with signature: (<typeparamref name="TGesture"/> gesture, GameTime gameTime, IInputDevice device)</param>
        /// <param name="gestures">The gestures to bind.</param>
        public void Bind<TGesture>(GestureHandler<TGesture> handler, params TGesture[] gestures)
            where TGesture : IGesture
        {
            if (handler == null)
                throw new ArgumentNullException("handler");

            foreach (var gesture in gestures)
            {
                AddGesture<TGesture>(_gesturePairs, handler, gesture);

                if (gesture.AlwaysEvaluate)
                    AddGesture<TGesture>(_ui.GlobalGestures, handler, gesture);
            }
        }

        private static void AddGesture<TGesture>(Dictionary<Type, List<IGesturePair>> pairs, GestureHandler<TGesture> handler, TGesture gesture)
            where TGesture : IGesture
        {
            Type type = gesture.DeviceType;
            List<IGesturePair> p;
            if (pairs.ContainsKey(type))
                p = pairs[type];
            else
            {
                p = new List<IGesturePair>();
                pairs.Add(type, p);
            }

            p.Add(new GesturePair<TGesture>()
            {
                Gesture = gesture,
                Handler = handler
            });
        }
    }
}