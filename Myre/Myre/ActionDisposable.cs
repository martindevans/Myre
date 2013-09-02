﻿using System;
using System.Collections.Generic;

namespace Myre
{
    /// <summary>
    /// A disposable object which calls an action when it is disposed
    /// </summary>
    public class ActionDisposable
        :MarshalByRefObject, IDisposable, IDisposableObject
    {
        private readonly bool _allowRepeats;
        private readonly List<Action> _disposeActions = new List<Action>();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="allowRepeats"></param>
        public ActionDisposable(bool allowRepeats = false)
        {
            _allowRepeats = allowRepeats;
        }

        public void Dispose()
        {
            if ((IsDisposed && _allowRepeats) || !IsDisposed)
                ExecuteList();
            IsDisposed = true;
        }

        private void ExecuteList()
        {
            foreach (var disposeAction in _disposeActions)
                disposeAction();
        }

        public bool IsDisposed { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        public void AddAction(Action a)
        {
            _disposeActions.Add(a);
        }
    }
}
