using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using hlwSerial;

namespace hlwSerialTest
{


    public class foonull : Serializable
    {
        [Serialize]public int? integer { get; set; }
        [Serialize]public bool? boolean { get; set; }
        [Serialize]public float? floating { get; set; }

        

        

        public void PrepareSerialization()
        {
            
        }

        public void AfterDeserialization()
        {
        }
    }
    

    [TestClass]
    public class NullableTests
    {

        [TestMethod]
        public void TestNullables()
        {
            MemoryStream ms = new MemoryStream();

            var foo = new foonull();
            foo.integer = 1;
            foo.boolean = null;
            foo.floating = 16.5f;

            ms.Write(foo);
            ms.Position = 0;
            var foo2 = ms.Read<foonull>();
            Assert.AreEqual(true, foo2.integer==1 && foo2.boolean==null && foo2.floating == 16.5f);
        }


    }
}
