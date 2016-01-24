using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using Myre.UI.Gestures;

using GameTime = Microsoft.Xna.Framework.GameTime;

namespace Myre.UI
{
    /// <summary>
    /// Encapsulated a method with no return type and a single parameter of type <see cref="Control"/>.
    /// </summary>
    /// <param name="sender">The control which sent the event.</param>
    public delegate void ControlEventHandler(Control sender);

    public struct ActorFocus
    {
        public InputActor Actor;
        public FocusChain.FocusRecord? Record;

        public void Restore()
        {
            Actor.RestorePrevious(Record);
        }
    }

    /// <summary>
    /// Represents a game control.
    /// </summary>
    public class Control
        : Frame, IDisposable
    {
        private List<Control> _children;
        internal List<ActorFocus> focusedBy;
        private int _focusCount;
        private int _heatCount;
        private bool _isFocused;
        private bool _isWarm;
        private bool _isVisible;
        private bool _isLoaded;
        private int _strataOffsetCount;

        /// <summary>
        /// Gets a value indicating if this control has been disposed.
        /// </summary>
        public bool IsDisposed { get; private set; }

        /// <summary>
        /// Gets the user interface this control is a member of.
        /// </summary>
        public UserInterface UserInterface { get; private set; }

        /// <summary>
        /// Gets this controls' parent.
        /// </summary>
        public new Control Parent { get { return base.Parent as Control; } }

        /// <summary>
        /// Gets this controls' children.
        /// </summary>
        public IReadOnlyList<Control> Children { get { return _children; } }

        public IReadOnlyList<ActorFocus> FocusedBy { get { return focusedBy; } }

        /// <summary>
        /// Gets this controls' gesture collection.
        /// </summary>
        public GestureGroup Gestures { get; private set; }

        /// <summary>
        /// Gets a value indicating if the control is in focus.
        /// </summary>
        public bool IsFocused
        {
            get { return _isFocused; }
            private set
            {
                if (_isFocused != value)
                {
                    _isFocused = value;
                    OnFocusChanged();
                    if (_isFocused)
                        IsVisible = true;
                }
            }
        }

        /// <summary>
        /// Gets a value indicating if a pointing device is looking at this control.
        /// </summary>
        public bool IsWarm
        {
            get { return _isWarm; }
            private set
            {
                if (_isWarm != value)
                {
                    _isWarm = value;
                    OnWarmChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets a value which determines if this control is visible.
        /// </summary>
        public bool IsVisible
        {
            get { return (Parent == null || Parent.IsVisible) && _isVisible; }
            set
            {
                if (_isVisible != value)
                {
                    if (!value && IsFocused)
                        throw new InvalidOperationException("A focused control cannot be hidden. Focus a new control first.");

                    if (value && Parent != null)
                        Parent.IsVisible = true;


                    _isVisible = value;
                    OnVisibleChanged();
                }
            }
        }

        /// <summary>
        /// Gets a value indicating if the control has its' content loaded.
        /// </summary>
        public bool IsLoaded
        {
            get { return _isLoaded; }
            private set
            {
                if (_isLoaded != value)
                {
                    _isLoaded = value;
                    OnLoadedChanged();
                }
            }
        }

        internal int FocusedCount
        {
            get { return _focusCount; }
            set
            {
                if (_focusCount != value)
                {
                    _focusCount = value;
                    IsFocused = _focusCount > 0;
                }
            }
        }

        internal int HeatCount
        {
            get { return _heatCount; }
            set
            {
                if (_heatCount != value)
                {
                    _heatCount = value;
                    _isWarm = _heatCount > 0;
                }
            }
        }

        /// <summary>
        /// Gets or sets the strata.
        /// Strata defines the depth which this control lies at. i.e. which controls are drawn ontop and below this control.
        /// </summary>
        /// <value>The strata.</value>
        public ControlStrata Strata { get; set; }

        /// <summary>
        /// Called when IsVisible changes.
        /// </summary>
        public event ControlEventHandler VisibleChanged;

        /// <summary>
        /// Called when IsFocused changes.
        /// </summary>
        public event ControlEventHandler FocusedChanged;

        /// <summary>
        /// Called when IsWarm changes.
        /// </summary>
        public event ControlEventHandler WarmChanged;

        /// <summary>
        /// Called when IsLoaded changes.
        /// </summary>
        public event ControlEventHandler LoadedChanged;

        /// <summary>
        /// Called when a child is added to this control.
        /// </summary>
        public event ControlEventHandler ChildAdded;

        /// <summary>
        /// Called when a child is removed from this control.
        /// </summary>
        public event ControlEventHandler ChildRemoved;

        /// <summary>
        /// Gets or sets a value which determines if the control will automatically
        /// try to pass focus on to a child when it is focused.
        /// </summary>
        public bool LikesHavingFocus { get; set; }

        /// <summary>
        /// Gets or sets the focus priority of this control.
        /// A higher priority increases the chances that this control
        /// will be given focus if its' parent has <c>LikesBeingFocused</c> as <c>false</c>.
        /// </summary>
        public int FocusPriority { get; set; }

        /// <summary>
        /// Creates a new instance of the <see cref="Control"/> class.
        /// </summary>
        /// <param name="parent">The parent control.</param>
// ReSharper disable MemberCanBeProtected.Global
        public Control(Control parent)
// ReSharper restore MemberCanBeProtected.Global
            : base(parent.Device, parent)
        {
            if (parent == null)
                throw new ArgumentNullException("parent");

            UserInterface = parent.UserInterface;
            Initialise();
        }

        /// <summary>
        /// Creates a new instance of the <see cref="Control"/> class.
        /// </summary>
        /// <param name="parent">The user interface to add this control to.</param>
        public Control(UserInterface parent)
            : base(parent.Device, parent.Root)
        {
            if (parent == null)
                throw new ArgumentNullException("parent");

            UserInterface = parent;
            Initialise();
        }

        private void Initialise()
        {
            _children = new List<Control>();
            focusedBy = new List<ActorFocus>();
            Gestures = new GestureGroup(UserInterface);
            _isVisible = true;
            LikesHavingFocus = true;
            FocusPriority = 100;

            if (Parent != null)
            {
                Parent._strataOffsetCount++;
                Strata = new ControlStrata() { Layer = Layer.Parent, Offset = Parent.Strata.Offset + Parent._strataOffsetCount };

                Parent.AddChild(this);
            }
            else
            {
                Strata = new ControlStrata() { Layer = Layer.Medium, Offset = 0 };
            }
        }

        private void AddChild(Control child)
        {
            _children.Add(child);
            OnChildAdded();
        }

        private void RemoveChild(Control child)
        {
            if (_children.Remove(child))
                OnChildRemoved();
        }

        /// <summary>
        /// Causes all input actors in this controls user interface to focus this control.
        /// Shortcut method for this.UserInterface.Actors.AllFocus(this);
        /// </summary>
// ReSharper disable MemberCanBeProtected.Global
        public void Focus()
// ReSharper restore MemberCanBeProtected.Global
        {
            UserInterface.Actors.AllFocus(this);
        }

        ///// <summary>
        ///// Called when the control should bind its gestres.
        ///// </summary>
        //protected internal virtual void BindGestures()
        //{
        //}

        //private bool TryToRidFocus()
        //{
        //    Control newFocus = null;
        //    for (int i = 0; i < Children.Count; i++)
        //    {
        //        var c = Children[i];
        //        if (!c.IsVisible || !c.FocusScope.Includes(FocusScope)) 
        //            continue;
        //        if (newFocus == null 
        //            || newFocus.FocusPriority > c.FocusPriority 
        //            || (newFocus.FocusPriority == c.FocusPriority && !newFocus.LikesHavingFocus && c.LikesHavingFocus))
        //            newFocus = c;
        //    }

        //    if (newFocus == null)
        //        return false;
        //    else
        //    {
        //        newFocus.Focus();
        //        return true;
        //    }
        //}

        /// <summary>
        /// Determins if this control is a parent of the specified control.
        /// This searches past the controls direct parent.
        /// </summary>
        /// <param name="control"></param>
        /// <returns>true if this control is a parent of the specified control; else false.</returns>
        public bool IsParentOf(Control control)
        {
            if (control == null)
                throw new ArgumentNullException("control");

            for (var c = control.Parent; c != null; c = c.Parent)
            {
                if (c == this)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Updates the control and its' children.
        /// </summary>
        /// <param name="gameTime">The current game time.</param>
// ReSharper disable UnusedParameter.Global
        public virtual void Update(GameTime gameTime)
// ReSharper restore UnusedParameter.Global
        {
        }

        /// <summary>
        /// Draws the control.
        /// </summary>
        /// <param name="batch">An spritebactch already started for alpha blending with deferred sort mode.</param>
        public virtual void Draw(SpriteBatch batch)
        {
        }

        /// <summary>
        /// Loads the content.
        /// </summary>
        public virtual void LoadContent()
        {
            IsLoaded = true;
        }

        /// <summary>
        /// Unloads the content.
        /// </summary>
        public virtual void UnloadContent()
        {
            IsLoaded = false;
        }

        /// <summary>
        /// Called when the IsFocused changes.
        /// </summary>
        private void OnFocusChanged()
        {
            if (FocusedChanged != null)
                FocusedChanged(this);
        }

        /// <summary>
        /// Called when the IsWarm changes.
        /// </summary>
        private void OnWarmChanged()
        {
            if (WarmChanged != null)
                WarmChanged(this);
        }

        /// <summary>
        /// Called when IsVisible changes.
        /// </summary>
        private void OnVisibleChanged()
        {
            if (VisibleChanged != null)
                VisibleChanged(this);
        }

        /// <summary>
        /// Called when IsLoaded changes.
        /// </summary>
        private void OnLoadedChanged()
        {
            if (LoadedChanged != null)
                LoadedChanged(this);
        }

        /// <summary>
        /// Called when a child is added.
        /// </summary>
        private void OnChildAdded()
        {
            if (ChildAdded != null)
                ChildAdded(this);
        }

        /// <summary>
        /// Called when a child is removed.
        /// </summary>
        private void OnChildRemoved()
        {
            if (ChildRemoved != null)
                ChildRemoved(this);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="automatic"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
// ReSharper disable UnusedParameter.Global
        protected virtual void Dispose(bool automatic)
// ReSharper restore UnusedParameter.Global
        {                
            if (_isLoaded)
                UnloadContent();

            if (Parent != null)
                Parent.RemoveChild(this);

            IsDisposed = true;

            for (int i = 0; i < _children.Count; i++)
                _children[i].Dispose();
        }

        /// <summary>
        /// Releases unmanaged resources and performs other cleanup operations before the
        /// <see cref="Control"/> is reclaimed by garbage collection.
        /// </summary>
        ~Control()
        {
            Dispose(false);
        }
    }
}
