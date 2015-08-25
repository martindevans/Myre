using System;
using System.Collections.Generic;
using System.Linq;

using GameTime = Microsoft.Xna.Framework.GameTime;
using MathHelper = Microsoft.Xna.Framework.MathHelper;

namespace Myre.StateManagement
{
    /// <summary>
    /// A class which manages screens.
    /// </summary>
    public class ScreenManager
    {
        readonly Stack<Screen> _screenStack;
        readonly List<Screen> _screens;

        readonly IEnumerable<Screen> _transitioningOn;
        readonly IEnumerable<Screen> _transitioningOff;
        readonly IEnumerable<Screen> _visible;

        public TransitionType TransitionType { get; private set; }

        public int StackCount
        {
            get
            {
                return _screenStack.Count;
            }
        }

        public IEnumerable<Screen> Screens
        {
            get { return _screenStack; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ScreenManager"/> class.
        /// </summary>
        public ScreenManager()
        {
            _screenStack = new Stack<Screen>();
            _screens = new List<Screen>();

            _transitioningOn = from s in _screens
                              where s.TransitionState == TransitionState.On
                              select s;

            _transitioningOff = from s in _screens
                               where s.TransitionState == TransitionState.Off
                               select s;

            _visible = from s in _screens
                      where s.TransitionState != TransitionState.Hidden
                      select s;
        }

        /// <summary>
        /// Pushes the specified screen.
        /// </summary>
        /// <param name="screen">The screen.</param>
        public void Push(Screen screen)
        {
            foreach (var s in _screenStack)
            {
                if (s.TransitionState == TransitionState.On || s.TransitionState == TransitionState.Shown)
                    s.TransitionState = TransitionState.Off;
            }

            _screens.Add(screen);
            _screenStack.Push(screen);
            screen.TransitionState = TransitionState.On;
            screen.Manager = this;
        }

        /// <summary>
        /// Pops this instance.
        /// </summary>
        /// <returns></returns>
        public Screen Pop()
        {
            var oldScreen = _screenStack.Pop();
            oldScreen.TransitionState = TransitionState.Off;

            if (_screenStack.Count > 0)
            {
                var newScreen = _screenStack.Peek();
                newScreen.TransitionState = TransitionState.On;
                if (!_screens.Contains(newScreen))
                    _screens.Add(newScreen);
            }
            
            return oldScreen;
        }

        /// <summary>
        /// Updates visible screens.
        /// </summary>
        /// <param name="gameTime">The game time.</param>
        public void Update(GameTime gameTime)
        {
            //screens.AddRange(screenStack);

            bool screensAreTransitioningOff = false;
            foreach (var screen in _transitioningOff)
            {
                UpdateTransition(screen, gameTime);

// ReSharper disable CompareOfFloatsByEqualityOperator
                if (screen.TransitionProgress == 0)
// ReSharper restore CompareOfFloatsByEqualityOperator
                {
                    screen.TransitionState = TransitionState.Hidden;
                    if (!_screenStack.Contains(screen))
                        screen.Dispose();
                }
                else
                    screensAreTransitioningOff = true;
            }

            foreach (var screen in _transitioningOn)
            {
                if (TransitionType == TransitionType.CrossFade || !screensAreTransitioningOff)
                    UpdateTransition(screen, gameTime);

// ReSharper disable CompareOfFloatsByEqualityOperator
                if (screen.TransitionProgress == 1)
// ReSharper restore CompareOfFloatsByEqualityOperator
                    screen.TransitionState = TransitionState.Shown;
            }
            
            for (int i = _screens.Count - 1; i >= 0; i--)
            {
                if (_screens[i].TransitionState == TransitionState.Hidden)
                    _screens.RemoveAt(i);
                else
                    _screens[i].Update(gameTime);
            }

            //screens.Clear();
        }

        private void UpdateTransition(Screen screen, GameTime gameTime)
        {
            if (screen.TransitionState == TransitionState.On)
            {
                if (screen.TransitionOn == TimeSpan.Zero)
                    screen.TransitionProgress = 1;
                else
                    screen.TransitionProgress = MathHelper.Clamp(screen.TransitionProgress + (float)(gameTime.ElapsedGameTime.TotalSeconds / screen.TransitionOn.TotalSeconds), 0, 1);
            }
            else
            {
                if (screen.TransitionOn == TimeSpan.Zero)
                    screen.TransitionProgress = 0;
                else
                    screen.TransitionProgress = MathHelper.Clamp(screen.TransitionProgress - (float)(gameTime.ElapsedGameTime.TotalSeconds / screen.TransitionOff.TotalSeconds), 0, 1);
            }
        }

        /// <summary>
        /// Prepares visible screens for drawing.
        /// </summary>
        public void PrepareDraw()
        {
            //screens.AddRange(screenStack);

            foreach (var screen in _visible)
            {
                screen.PrepareDraw();
            }

            //screens.Clear();
        }

        /// <summary>
        /// Draws visible screens.
        /// </summary>
        /// <param name="gameTime">The game time.</param>
        public void Draw(GameTime gameTime)
        {
            //screens.AddRange(screenStack);

            foreach (var screen in _screens)
            {
                if (screen.TransitionState != TransitionState.Hidden)
                    screen.Draw(gameTime);
            }

            //screens.Clear();
        }
    }
}
