using Myre.UI.InputDevices;
using System;
using System.Collections.Generic;

using GameTime = Microsoft.Xna.Framework.GameTime;
using IGameComponent = Microsoft.Xna.Framework.IGameComponent;
using IUpdateable = Microsoft.Xna.Framework.IUpdateable;

namespace Myre.UI
{
// ReSharper disable ClassWithVirtualMembersNeverInherited.Global
    public class InputActor
// ReSharper restore ClassWithVirtualMembersNeverInherited.Global
        : FocusChain, IGameComponent, IUpdateable, ICollection<IInputDevice>
    {
        private readonly IList<IInputDevice> _devices;
        private readonly int _id;
        private bool _enabled;
        private int _updateOrder;

        public IInputDevice this[int i]
        {
            get { return _devices[i]; }
            set
            { 
                _devices[i] = value;
                _devices[i].Owner = this;
            }
        }

        public int ID
        {
            get { return _id; }
        }

        #region IUpdateable Members

        public bool Enabled
        {
            get { return _enabled; }
            set
            {
                if (_enabled != value)
                {
                    _enabled = value;
                    OnEnabledChanged();
                }
            }
        }

        public int UpdateOrder
        {
            get { return _updateOrder; }
            set
            {
                if (_updateOrder != value)
                {
                    _updateOrder = value;
                    OnUpdateOrderChanged();
                }
            }
        }

#if XNA_3_1
        public event EventHandler UpdateOrderChanged;
        public event EventHandler EnabledChanged;
#else
        public event EventHandler<EventArgs> UpdateOrderChanged;
        public event EventHandler<EventArgs> EnabledChanged;
#endif

        protected virtual void OnEnabledChanged()
        {
            if (EnabledChanged != null)
                EnabledChanged(this, new EventArgs());
        }

        protected virtual void OnUpdateOrderChanged()
        {
            if (UpdateOrderChanged != null)
                UpdateOrderChanged(this, new EventArgs());
        }

        #endregion

        public InputActor(int id)
        {
            _id = id;
            _devices = new List<IInputDevice>();
            _enabled = true;
        }

        public InputActor(int id, params IInputDevice[] inputs)
            :this(id)
        {
            for (int i = 0; i < inputs.Length; i++)
                Add(inputs[i]);
        }

        public void Initialize()
        {
        }

        public void Update(GameTime gameTIme)
        {
            if (FocusedControl != null && FocusedControl.IsDisposed)
                RestorePrevious(null);

            foreach (var device in _devices)
                device.Update(gameTIme);
        }

        public T FindDevice<T>()
            where T : class, IInputDevice
        {
            foreach (var item in this)
            {
                if (item is T)
                    return item as T;
            }

            return null;
        }

        protected override void Focus(Control control, bool rememberPrevious)
        {
            if (control != null && !control.UserInterface.Actors.Contains(this))
                throw new InvalidOperationException("This actor does not belong to the specified UserInterface.");

            base.Focus(control, rememberPrevious);
        }

        protected override void AddFocus(Control control)
        {
            control.focusedBy.Add(new ActorFocus() { Actor = this, Record = PreviousFocus() });
            base.AddFocus(control);
        }

        protected override void RemoveFocus(Control control)
        {
            for (int i = control.focusedBy.Count - 1; i >= 0 ; i--)
            {
                if (control.focusedBy[i].Actor == this)
                    control.focusedBy.RemoveAt(i);
            }

            base.RemoveFocus(control);
        }

        internal void Evaluate(GameTime gameTime, UserInterface ui)
        {
            foreach (var device in _devices)
                device.Evaluate(gameTime, FocusRoot == ui.Root ? FocusedControl : null, ui);
        }

        #region ICollection<IInputDevice> Members

        public void Add(IInputDevice item)
        {
            _devices.Add(item);
            item.Owner = this;
        }

        public void Clear()
        {
            _devices.Clear();
        }

        public bool Contains(IInputDevice item)
        {
            return _devices.Contains(item);
        }

        public void CopyTo(IInputDevice[] array, int arrayIndex)
        {
            _devices.CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get { return _devices.Count; }
        }

        public bool IsReadOnly
        {
            get { return _devices.IsReadOnly; }
        }

        public bool Remove(IInputDevice item)
        {
            if (_devices.Remove(item))
            {
                item.Owner = null;
                return true;
            }
            else
            {
                return false;
            }            
        }

        #endregion

        #region IEnumerable<IInputDevice> Members

        public IEnumerator<IInputDevice> GetEnumerator()
        {
            return _devices.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }
}