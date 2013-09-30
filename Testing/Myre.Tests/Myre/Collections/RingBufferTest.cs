using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Myre.Collections;

namespace Myre.Tests.Myre.Collections
{
    [TestClass]
    public class RingBufferTest
    {
        [TestMethod]
        public void Construction()
        {
            RingBuffer<int> r = new RingBuffer<int>(5);
        }

        [TestMethod]
        public void AddingToARingBufferWorks()
        {
            RingBuffer<int> r = new RingBuffer<int>(5);
            r.Add(1);
            r.Add(2);
            r.Add(3);
            r.Add(4);
            r.Add(5);
            r.Add(6);
        }

        [TestMethod]
        public void ReadingFromARingBuffer()
        {
            RingBuffer<int> r = new RingBuffer<int>(3);
            r.Add(1);
            r.Add(2);
            r.Add(3);
            r.Add(4);

            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.AreEqual(4, r[0]);
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.AreEqual(2, r[1]);
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.AreEqual(3, r[2]);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void ReadingBeyondRingBufferRangeIsOutOfRange()
        {
            RingBuffer<int> i = new RingBuffer<int>(3);
            var a = i[3];
        }
    }
}
