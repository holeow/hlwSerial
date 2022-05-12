using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using hlwSerial;

namespace hlwSerialTest
{


    public class foonull : ISerializable
    {
        [Serialize]public int? integer { get; set; }
        [Serialize]public bool? boolean { get; set; }
        [Serialize]public float? floating { get; set; }
    }



    [TestClass]
    public class NullableTests
    {
        [TestMethod]
        public void NullableGivesNull()
        {
            int? integer = null;
            object val = integer;

            Assert.IsTrue(val == null);
        }

        [TestMethod]
        public void TestNonNullNullables()
        {
            MemoryStream ms = new MemoryStream();

            Serializer serializer = new Serializer(ms);

            var foo = new foonull();
            foo.integer = 3;
            foo.boolean = true;
            foo.floating = 16.5f;

            serializer.Write(foo);

            byte[] bytes = ms.ToArray();

            Trace.WriteLine(String.Join(" ",bytes.Select(a=> a.ToString())));
            serializer.Position = 0;
            var foo2 = serializer.Read<foonull>();
            Assert.AreEqual(true, foo2.integer == 3 && foo2.boolean == true && foo2.floating == 16.5f);
        }

        [TestMethod]
        public void TestNullNullables()
        {
            MemoryStream ms = new MemoryStream();

            Serializer serializer = new Serializer(ms);

            var foo = new foonull();
            foo.integer = null;
            foo.boolean = null;
            foo.floating = null;

            serializer.Write(foo);
            serializer.Position = 0;
            var foo2 = serializer.Read<foonull>();
            Assert.AreEqual(true, foo2.integer == null && foo2.boolean == null && foo2.floating == null);
        }

        [TestMethod]
        public void TestNullables()
        {
            MemoryStream ms = new MemoryStream();

            Serializer serializer = new Serializer(ms);
            
            var foo = new foonull();
            foo.integer = 1;
            foo.boolean = null;
            foo.floating = 16.5f;

            serializer.Write(foo);
            serializer.Position = 0;
            var foo2 = serializer.Read<foonull>();
            Assert.AreEqual(true, foo2.integer==1 && foo2.boolean==null && foo2.floating == 16.5f);
        }


    }
}
