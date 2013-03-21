using System;
using System.Collections.Generic;
using System.Linq;

namespace Myre.UI
{
    public class FocusChain
    {
        //private static Pool<WeakReference> weakReferencePool = new Pool<WeakReference>();
        private static readonly Predicate<FocusRecord> _disposedRecords = delegate(FocusRecord record) { var c = record.Control; return c == null || c.IsDisposed; };
        private static readonly Func<Control, int> _byFocusPriority = control => control.FocusPriority;

        private int _idCounter;
        private readonly List<FocusRecord> _previous;
        private Control _focused;
        private Control _root;
        private readonly List<Control> _unfocused;
        private readonly List<Control> _newFocused;

        public Control FocusedControl
        {
            get { return _focused; }
        }

        public Control FocusRoot
        {
            get { return _root; }
        }

        public FocusChain()
        {
            _previous = new List<FocusRecord>();
            _unfocused = new List<Control>();
            _newFocused = new List<Control>();
        }

        public FocusRecord? Focus(Control control)
        {
            Focus(control, true);
            return PreviousFocus();
        }

        public void RestorePrevious(FocusRecord? hint)
        {
            FocusRecord record;
            Control control;
            do
            {
                if (_previous.Count == 0)
                {
                    control = null;
                    break;
                }

                record = _previous[_previous.Count - 1];
                _previous.RemoveAt(_previous.Count - 1);
                control = record.Control;
            } while ((control == null || control.IsDisposed)
                  || (hint != null && hint.Value.ID < record.ID));

            Focus(control, false);
        }

        protected FocusRecord? PreviousFocus()
        {
            if (_previous.Count > 0)
                return _previous[_previous.Count - 1];
            else
                return null;
        }

        protected virtual void AddFocus(Control control) { }

        protected virtual void RemoveFocus(Control control) { }

        protected virtual void Focus(Control control, bool rememberPrevious)
        {
            if (control == _focused)
                return;

            // clear buffers
            _unfocused.Clear();
            _newFocused.Clear();

            if (control != null && !control.LikesHavingFocus)
            {
                foreach (var item in control.Children.OrderBy(_byFocusPriority))
                {
                    if (item.LikesHavingFocus)
                    {
                        control = item;
                        break;
                    }
                }
            }

            // find all old controls being unfocused
            for (var c = _focused; c != null; c = c.Parent)
                _unfocused.Add(c);

            // find all new controls being focused
            for (var c = control; c != null; c = c.Parent)
                _newFocused.Add(c);

            var newRoot = _newFocused.Count > 0 ? _newFocused[_newFocused.Count - 1] : null;

            // remove all the common controls
            // walk both lists backwards (from the shared root) until the trees diverge
            for (int i = _newFocused.Count - 1; i >= 0; i--)
            {
                if (_unfocused.Count > 0 && _newFocused[i] == _unfocused[_unfocused.Count - 1])
                {
                    _newFocused.RemoveAt(i);
                    _unfocused.RemoveAt(_unfocused.Count - 1);
                }
                else
                {
                    break;
                }
            }

            // walk through the old controls, from leaf towards root, unfocusing them
            for (int i = 0; i < _unfocused.Count; i++)
            {
                var u = _unfocused[i];
                _focused = u;

                RemoveFocus(u);
                u.FocusedCount--;

                // quit if FocusChanged focused another control
                if (_focused != u)
                    return;
            }

            // walk through the new controls, from root to leaf, focusing them
            for (int i = _newFocused.Count - 1; i >= 0; i--)
            {
                var f = _newFocused[i];
                _focused = f;

                AddFocus(_newFocused[i]);
                _newFocused[i].FocusedCount++;

                // quit if FocusChanged focused another control
                if (_focused != f)
                    return;
            }

            // push old control onto stack
            if (rememberPrevious && _focused != null)
            {
                unchecked
                {
                    _idCounter++;
                }

                var record = new FocusRecord() { ID = _idCounter, Control = _focused };
                _previous.Add(record);
            }

            // remember new control
            //focused = control;
            _root = newRoot;

            CompactPreviousList();
        }

        private void CompactPreviousList()
        {
            for (int i = _previous.Count - 1; i >= 0; i--)
            {
                if (_disposedRecords(_previous[i]))
                    _previous.RemoveAt(i);
            }
            
            //previous.RemoveAll(disposedRecords);
        }

        public struct FocusRecord
        {
            private WeakReference _reference;
            public int ID;
            public Control Control
            {
                get { return _reference.Target as Control; }
                set
                {
                    _reference = new WeakReference(value);
                    //reference = weakReferencePool.Get();
                    //reference.Target = value;
                }
            }
        }
    }
}
