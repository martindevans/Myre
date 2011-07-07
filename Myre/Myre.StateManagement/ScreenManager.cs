using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Myre.StateManagement
{
    /// <summary>
    /// A class which manages screens.
    /// </summary>
    public class ScreenManager
    {
        Stack<Screen> screenStack;
        List<Screen> screens;
        List<Screen> oldScreens;

        IEnumerable<Screen> transitioningOn;
        IEnumerable<Screen> transitioningOff;
        IEnumerable<Screen> visible;

        public TransitionType TransitionType { get; set; }

        public int StackCount
        {
            get
            {
                return screenStack.Count;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ScreenManager"/> class.
        /// </summary>
        public ScreenManager()
        {
            screenStack = new Stack<Screen>();
            screens = new List<Screen>();

            transitioningOn = from s in screens
                              where s.TransitionState == TransitionState.On
                              select s;

            transitioningOff = from s in screens
                               where s.TransitionState == TransitionState.Off
                               select s;

            visible = from s in screens
                      where s.TransitionState != TransitionState.Hidden
                      select s;
        }

        /// <summary>
        /// Pushes the specified screen.
        /// </summary>
        /// <param name="screen">The screen.</param>
        public void Push(Screen screen)
        {
            foreach (var s in screenStack)
            {
                if (s.TransitionState == TransitionState.On || s.TransitionState == TransitionState.Shown)
                    s.TransitionState = TransitionState.Off;
            }

            screens.Add(screen);
            screenStack.Push(screen);
            screen.TransitionState = TransitionState.On;
            screen.Manager = this;
        }

        /// <summary>
        /// Pops this instance.
        /// </summary>
        /// <returns></returns>
        public Screen Pop()
        {
            var oldScreen = screenStack.Pop();
            oldScreen.TransitionState = TransitionState.Off;

            if (screenStack.Count > 0)
            {
                var newScreen = screenStack.Peek();
                newScreen.TransitionState = TransitionState.On;
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
            foreach (var screen in transitioningOff)
            {
                UpdateTransition(screen, gameTime);

                if (screen.TransitionProgress == 0)
                    screen.TransitionState = TransitionState.Hidden;
                else
                    screensAreTransitioningOff = true;
            }

            foreach (var screen in transitioningOn)
            {
                if (TransitionType == TransitionType.CrossFade || !screensAreTransitioningOff)
                    UpdateTransition(screen, gameTime);

                if (screen.TransitionProgress == 1)
                    screen.TransitionState = TransitionState.Shown;
            }
            
            for (int i = screens.Count - 1; i >= 0; i--)
            {
                if (screens[i].TransitionState == TransitionState.Hidden)
                    screens.RemoveAt(i);
                else
                    screens[i].Update(gameTime);
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

            foreach (var screen in visible)
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

            foreach (var screen in screens)
            {
                if (screen.TransitionState != TransitionState.Hidden)
                    screen.Draw(gameTime);
            }

            //screens.Clear();
        }
    }
}
