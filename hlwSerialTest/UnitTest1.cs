using System;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using hlwSerial;

namespace hlwSerialTest
{
    class foo : Serializable
    {
        [Serialize]public int lala { get; set; } = 32;

        [Serialize]public int lolo { get; set; } = 256;


        public void AfterDeserialization()
        {
            
        }

        public void PrepareSerialization()
        {
            
        }
    }

    class stringFoo : Serializable
    {
        [Serialize]public int random { get; set; }
        [Serialize]public string str { get; set; }
        [Serialize]public double d { get; set; }

        public void PrepareSerialization()
        {
            
        }

        public void AfterDeserialization()
        {
            
        }
    }
    class bar : Serializable
    {
        [Serialize] public sbyte s { get; set; } = -90;
        [Serialize] public sbyte s2 { get; set; } = 30;
        [Serialize] public sbyte s3 { get; set; } = sbyte.MaxValue;



        public void AfterDeserialization()
        {
        }

        public void PrepareSerialization()
        {
        }
    }

    [TestClass]
    public class propertyTester
    {
        [TestMethod]
        public void FooHasTwoPropertiesWithAttribute()
        {
            var cnt = typeof(foo).GetProperties().Where(a => a.IsDefined(typeof(SerializeAttribute), false))
                .ToArray().Count();
            Assert.AreEqual(2, cnt);
        }

        [TestMethod]
        public void FooHasTwoPropertiesIntegerWithAttribute()
        {
            var cnt = typeof(foo).GetProperties().Where(a => a.IsDefined(typeof(SerializeAttribute), false)).Where(q=> q.PropertyType == typeof(int)).ToArray().Count();
            Assert.AreEqual(2, cnt);
        }
    }
    


    [TestClass]
    public class SerializerTest
    {
        [TestMethod]
        public void TestSerializeIntegers()
        {
            MemoryStream stream = new MemoryStream(8);
            var f = new foo();
            f.lala = 33;
            stream.Write(f);

            using (var reader = new BinaryReader(stream))
            {
                reader.BaseStream.Position = 0;
                
                int one = reader.ReadInt32();
                int two = reader.ReadInt32();
                Assert.AreEqual(289, one+two);
            }

        }

        [TestMethod]
        public void TestDeserializeIntegers()
        {
            MemoryStream stream = new MemoryStream(8);
            var f = new foo();
            f.lala = 33;
            stream.Write(f);
            stream.Position = 0;

            var f2 = stream.Read<foo>();

            Assert.AreEqual(289, f2.lala + f2.lolo);
        }

        [TestMethod]
        public void TestDeserializeSbyte1()
        {
            MemoryStream stream = new MemoryStream(8);
            var f = new bar();
            f.s = -80;
            stream.Write(f);
            stream.Position = 0;

            var f2 = stream.Read<bar>();

            Assert.AreEqual(-80, f2.s);
        }
        [TestMethod]
        public void TestDeserializeSbyte2()
        {
            MemoryStream stream = new MemoryStream(8);
            var f = new bar();
            stream.Write(f);
            stream.Position = 0;

            var f2 = stream.Read<bar>();

            Assert.AreEqual(30, f2.s2);
        }

        [TestMethod]
        public void TestDeserializeSbyte3()
        {
            MemoryStream stream = new MemoryStream(8);
            var f = new bar();

            stream.Write(f);
            stream.Position = 0;

            var f2 = stream.Read<bar>();

            Assert.AreEqual(sbyte.MaxValue, f2.s3);
        }


        [TestMethod]
        public void TestMultiple()
        {
            MemoryStream stream = new MemoryStream();
            var s = new stringFoo();
            s.str = "hello ";
            stream.Write(s);
            var s2 = new stringFoo();
            s2.str = "world";
            stream.Write(s2);

            stream.Position = 0;
            var ns = stream.Read<stringFoo>();
            var ns2 = stream.Read<stringFoo>();

            Assert.AreEqual("hello world",ns.str+ns2.str);
        }
    }

    [TestClass]
    public class TestsWithString
    {
        [TestMethod]
        public void TestNullString()
        {
            MemoryStream stream = new MemoryStream();
            var s = new stringFoo();
            s.str = null;
            stream.Write(s);
            stream.Position = 0;

            stringFoo result = stream.Read<stringFoo>();
            Assert.AreEqual(null,result.str);
        }

        [TestMethod]
        public void TestEmptyString()
        {
            MemoryStream stream = new MemoryStream();
            var s = new stringFoo();
            s.str = "";
            stream.Write(s);
            stream.Position = 0;

            stringFoo result = stream.Read<stringFoo>();
            Assert.AreEqual("", result.str);
        }

        [TestMethod]
        public void TestFullString()
        {
            MemoryStream stream = new MemoryStream();
            var s = new stringFoo();
            s.str = "Hello world";
            stream.Write(s);
            stream.Position = 0;

            stringFoo result = stream.Read<stringFoo>();
            Assert.AreEqual("Hello world", result.str);
        }
    }
}