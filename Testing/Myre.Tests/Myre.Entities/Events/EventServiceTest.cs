using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Myre.Entities.Events;
using Check = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace Myre.Tests.Myre.Entities.Events
{
    [TestClass]
    public class EventServiceTest
    {
        readonly EventService _events = new EventService();

        [TestMethod]
        public void EventService_GetEventInGlobalScope_IsNotNull()
        {
            var evt = _events.GetEvent<int>();

            Check.IsNotNull(evt);
        }

        [TestMethod]
        public void EventService_GetEventInLocalScope_IsNotNull()
        {
            var obj = new Object();
            var evt = _events.GetEvent<int>(obj);

            Check.IsNotNull(evt);
        }

        [TestMethod]
        public void EventService_LocalScopeAndGlobalScope_AreNotEqual()
        {
            var obj = new Object();
            var local = _events.GetEvent<int>(obj);

            var global = _events.GetEvent<int>();

            Check.AreNotEqual(local, global);
        }

        [TestMethod]
        public void EventService_SendMessageGlobalEvent_DoesNotFail()
        {
            var evt = _events.GetEvent<int>();

            evt.Send(1);
        }

        [TestMethod]
        public void EventService_SendMessageLocalEvent_DoesNotFail()
        {
            var evt = _events.GetEvent<int>(new object());

            evt.Send(1);
        }

        [TestMethod]
        public void EventService_Update_SendsMessages()
        {
            var evt = _events.GetEvent<int>();

            var l = new ListListener<int>();
            evt.AddListener(l);

            //Adding listener does not send a message
            Check.AreEqual(0, l.Received.Count);

            evt.Send(13);

            //Sending message does not yet send message!
            Check.AreEqual(0, l.Received.Count);

            _events.Update(1);

            //Update should have sent the message
            Check.AreEqual(1, l.Received.Count);
            Check.AreEqual(13, l.Received[0].Item1);
            Check.AreEqual(null, l.Received[0].Item2);
        }

        private class ListListener<T>
            : IEventListener<T>
        {
            public readonly List<Tuple<T, object>> Received = new List<Tuple<T, object>>();

            public T HandleEvent(T data, object scope)
            {
                Received.Add(new Tuple<T, object>(data, scope));
                return data;
            }
        }
    }
}
