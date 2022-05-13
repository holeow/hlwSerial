using System;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using hlwSerial;

namespace hlwSerialTest
{
   

    [TestClass]
    public class RandomTests
    {
        [TestMethod]
        public void TestReference()
        {
            int lel = 0;
            RefPlusOne(ref lel);

            Assert.AreEqual(lel, 1);
        }


        public void RefPlusOne(ref int toChange)
        {
            toChange++;
        }
    }
}
