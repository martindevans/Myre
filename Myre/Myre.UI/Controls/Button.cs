using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Myre.UI.Gestures;
using Myre.UI.InputDevices;
using Myre.UI.Text;

namespace Myre.UI.Controls
{
    /// <summary>
    /// Base class for a button.
    /// </summary>
    public abstract class Button
        : Control
    {
        /// <summary>
        /// An event envoked when this menu item is selected.
        /// </summary>
        public event Action Selected;

        /// <summary>
        /// Gets or sets the justfication.
        /// </summary>
        /// <value>The justfication.</value>
        public abstract Justification Justification { get; set; }

        /// <summary>
        /// Gets or sets a value which determines if this button is clicked on mouse up or down.
        /// </summary>
        public bool RespondOnMouseDown { get; set; }

        private bool _mouseDown;

        /// <summary>
        /// Initializes a new instance of the <see cref="Button"/> class.
        /// </summary>
        /// <param name="parent">The parent control.</param>
        protected Button(Control parent)
            : base(parent)
        {
            LikesHavingFocus = true;
            //WarmChanged += new ControlEventHandler(MenuOption_HotChanged);

            BindGestures();
        }

        //void MenuOption_HotChanged(Control sender)
        //{
        //    if (IsWarm && (Parent != null && Parent.IsFocused)) UserInterface.Actors.AllFocus(this);
        //}

        /// <summary>
        /// Binds the A and Enter buttons to select.
        /// </summary>
        private void BindGestures()
        {
            Gestures.Bind(Select,
                new ButtonPressed(Buttons.A),
                new KeyPressed(Keys.Enter));

            Gestures.Bind((GestureHandler<IGesture>) MouseDown,
                new MousePressed(MouseButtons.Left));

            Gestures.Bind((GestureHandler<IGesture>) MouseUp,
                new MousePressed(MouseButtons.Left));

            WarmChanged += c => { if (!IsWarm) _mouseDown = false; };
        }

        /// <summary>
        /// Called when this option is selected.
        /// </summary>
        protected virtual void OnSelected()
        {
            if (Selected != null)
                Selected();
        }

        private void Select(IGesture gesture, GameTime time, IInputDevice device)
        {
            OnSelected();
        }

        private void MouseDown(IGesture gesture, GameTime time, IInputDevice device)
        {
            if (RespondOnMouseDown)
                OnSelected();

            _mouseDown = true;
        }

        private void MouseUp(IGesture gesture, GameTime time, IInputDevice device)
        {
            if (!RespondOnMouseDown && _mouseDown)
                OnSelected();
        }
    }
}
