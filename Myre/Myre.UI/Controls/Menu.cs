using System;
using Microsoft.Xna.Framework.Input;
using Myre.UI.Gestures;
using Myre.UI.InputDevices;
using Myre.UI.Text;

using GameTime = Microsoft.Xna.Framework.GameTime;

namespace Myre.UI.Controls
{
    /// <summary>
    /// A control which represents a menu of options.
    /// </summary>
    public class Menu
        : Control
    {
        /// <summary>
        /// Occurs when selected changes.
        /// </summary>
        public event Action<Control> SelectedChanged;

        /// <summary>
        /// Gets the selected option.
        /// </summary>
        /// <value>The selected option.</value>
        public Button SelectedOption { get; private set; }

        private int SelectedOptionIndex
        {
            get;
            set;
            //{
            //    for (int i = 0; i < Children.Count; i++)
            //    {
            //        if (Children[i] is Button && Children[i].IsFocused)
            //            return i;
            //    }

            //    return 0;
            //}
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Menu"/> class.
        /// </summary>
        /// <param name="parent">This controls parent control.</param>
        public Menu(Control parent)
            : base(parent)
        {
            SelectedOptionIndex = -1;

            ChildAdded += c =>
            {
                if (IsFocused && SelectedOption == null)
                    NextOption();
            };

            ChildRemoved += c =>
            {
                if (IsFocused && SelectedOption == c)
                {
                    NextOption();
                    if (!Children.Contains(SelectedOption))
                    {
                        SelectedOption = null;
                    }
                }
            };

            FocusedChanged += sender => {
                if (IsFocused)
                {
                    if (SelectedOption == null)
                        NextOption();
                    else
                        UserInterface.Actors.AllFocus(SelectedOption);
                }
            };

            BindGestures();
        }

        /// <summary>
        /// Binds menu navigation to the up and down keys.
        /// Override this to implement different behaviour.
        /// </summary>
        private void BindGestures()
        {
            Gestures.Bind(PreviousOption,
                new ButtonPressed(Buttons.DPadUp),
                new ButtonPressed(Buttons.LeftThumbstickUp),
                new KeyPressed(Keys.Up));

            Gestures.Bind(NextOption,
                new ButtonPressed(Buttons.DPadDown),
                new ButtonPressed(Buttons.LeftThumbstickDown),
                new KeyPressed(Keys.Down));
        }

        /// <summary>
        /// <para>Arranges the menu options vertically in the order they were added.</para>
        /// <para>Also resizes this menu controls' height to contain its child options.</para>
        /// <para>Also resizes the width of each menu option.</para>
        /// </summary>
        public void Arrange(Justification justfication)
        {
            int height = 0;
            int width = 0;
            Control previous = null;
            for (int i = 0; i < Children.Count; i++)
            {
                var child = Children[i] as Button;
                if (child != null)
                {
                    if (previous == null)
                        child.SetPoint(Points.Top, 0, 0);
                    else
                        child.SetPoint(Points.Top, 0, 0, previous, Points.Bottom);
                    
                    height += child.Area.Height;
                    width = Math.Max(width, child.Area.Width);
                    child.SetPoint(Points.Left, 0, 0);
                    child.SetPoint(Points.Right, 0, 0);

                    previous = child;

                    child.Justification = justfication;
                }
            }

            SetSize(width, height);
        }

        /// <summary>
        /// Called when the current selection changes.
        /// </summary>
        protected void OnSelectedChanged()
        {
            if (SelectedChanged != null)
                SelectedChanged(this);
        }

        /// <summary>
        /// Moves focus to the next button.
        /// </summary>
        public void NextOption()
        {
            for (int i = SelectedOptionIndex + 1; i < Children.Count; i++)
            {
                if (Children[i] is Button)
                {
                    ChangeSelection(i);
                    break;
                }
            }
        }

        /// <summary>
        /// Moves focus to the previous button.
        /// </summary>
        public void PreviousOption()
        {
            for (int i = SelectedOptionIndex - 1; i >= 0; i--)
            {
                if (Children[i] is Button)
                {
                    ChangeSelection(i);
                    break;
                }
            }
        }

        private void NextOption(IGesture gesture, GameTime time, IInputDevice device)
        {
            NextOption();
        }

        private void PreviousOption(IGesture gesture, GameTime time, IInputDevice device)
        {
            PreviousOption();
        }

        private void ChangeSelection(int i)
        {
            SelectedOptionIndex = i;
            SelectedOption = Children[i] as Button;
            UserInterface.Actors.AllFocus(Children[i]);
            OnSelectedChanged();
        }
    }
}
