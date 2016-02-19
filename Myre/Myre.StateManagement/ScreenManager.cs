using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using GameTime = Microsoft.Xna.Framework.GameTime;
using MathHelper = Microsoft.Xna.Framework.MathHelper;

namespace Myre.StateManagement
{
    /// <summary>
    /// A class which manages screens.
    /// </summary>
    public class ScreenManager
    {
        private readonly Stack<Screen> _screenStack;
        private readonly List<Screen> _screens;

        private readonly IEnumerable<Screen> _transitioningOn;
        private readonly IEnumerable<Screen> _transitioningOff;
        private readonly IEnumerable<Screen> _visible;

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
        /// <returns>The screen which was just removed</returns>
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
            foreach (var screen in _transitioningOff)
            {
                UpdateTransitionProgress(screen, gameTime);

                if (screen.TransitionProgress <= 0)
                {
                    screen.TransitionState = TransitionState.Hidden;
                    if (!_screenStack.Contains(screen))
                        screen.Dispose();
                }
            }

            foreach (var screen in _transitioningOn)
            {
                UpdateTransitionProgress(screen, gameTime);

                if (screen.TransitionProgress >= 1)
                    screen.TransitionState = TransitionState.Shown;
            }
            
            for (var i = _screens.Count - 1; i >= 0; i--)
            {
                if (_screens[i].TransitionState == TransitionState.Hidden)
                    _screens.RemoveAt(i);
                else
                    _screens[i].Update(gameTime);
            }
        }

        private static void UpdateTransitionProgress(Screen screen, GameTime gameTime)
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
            foreach (var screen in _visible)
                screen.PrepareDraw();
        }

        /// <summary>
        /// Draws visible screens.
        /// </summary>
        /// <param name="gameTime">The game time.</param>
        public void Draw(GameTime gameTime)
        {
            foreach (var screen in _screens)
            {
                if (screen.TransitionState != TransitionState.Hidden)
                    screen.Draw(gameTime);
            }
        }
    }
}
