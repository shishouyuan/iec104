using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouyuan.IEC104;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shouyuan.IEC104.Tests
{
    [TestClass()]
    public class ExtensionsTests
    {
        [TestMethod()]
        public void BitTest()
        {
            byte l = 0;
            Assert.AreEqual(l.Bit(0), false);
        }

        [TestMethod()]
        public void SetBitTest()
        {
            byte l = 0;
            Assert.AreEqual(l.SetBit(1),2);
        }
    }
}