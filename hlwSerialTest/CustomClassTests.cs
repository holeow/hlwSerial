using System;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using hlwSerial;

namespace hlwSerialTest
{
    public class fooBar : Serializable
    {
        [Serialize]public int n { get; set; }
        [Serialize]public float r { get; set; }


        public void PrepareSerialization()
        {
            
        }

        public void AfterDeserialization()
        {

        }

        public fooBar()
        {

        }
        public fooBar(int n, float r)
        {
            this.n = n;
            this.r = r;
        }
    }

    public class bigfoo : Serializable
    {
        [Serialize]public long l { get; set; }
        [Serialize]public fooBar f { get; set; }

        public void PrepareSerialization()
        {
            
        }

        public void AfterDeserialization()
        {

        }

        public bigfoo()
        {

        }

        public bigfoo(long l, fooBar f)
        {
            this.l = l;
            this.f = f;
        }
    }

    [TestClass]
    public class CustomClassTests
    {
        [TestMethod]
        public void hlwSerializableTest()
        {
            bigfoo b = new bigfoo(999, new fooBar(15, 33.3f));
            Stream s = new MemoryStream();

            s.Write(b);

            s.Position = 0;

            var b2 = s.Read<bigfoo>();

            Assert.AreEqual(33.3f,b2.f.r);

        }
    }
}
