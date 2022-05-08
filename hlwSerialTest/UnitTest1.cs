using System;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using hlwSerial;

namespace hlwSerialTest
{
    class foo : ISerializable
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

    class stringFoo : ISerializable
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
    class bar : ISerializable
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


    public struct baz : ISerializable
    {
        [Serialize] public int integer { get; set; }
        [Serialize] public int blob { get; set; }

    }


    [TestClass]
    public class propertyTester
    {

        [TestMethod]
        public void WorksWithStruct()
        {
            MemoryStream stream = new MemoryStream();
            Serializer serializer = new Serializer(stream);
            var b = new baz();
            b.blob = 12;
            b.integer = 16;

            serializer.Write(b);

            stream.Position = 0;

            var c = serializer.Read<baz>();
            Assert.AreEqual(true, c.integer == 16 && c.blob == 12);
        }

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
            Serializer serializer = new Serializer(stream);
            var f = new foo();
            f.lala = 33;
            serializer.Write(f);

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
            Serializer ser = new Serializer(stream);
            var f = new foo();
            f.lala = 33;
            ser.Write(f);
            stream.Position = 0;

            var f2 = ser.Read<foo>();

            Assert.AreEqual(289, f2.lala + f2.lolo);
        }

        [TestMethod]
        public void TestDeserializeSbyte1()
        {
            MemoryStream stream = new MemoryStream(8);
            Serializer serializer = new Serializer(stream);
            var f = new bar();
            f.s = -80;
            serializer.Write(f);
            serializer.Position = 0;

            var f2 = serializer.Read<bar>();

            Assert.AreEqual(-80, f2.s);
        }
        [TestMethod]
        public void TestDeserializeSbyte2()
        {
            MemoryStream stream = new MemoryStream(8);
            Serializer serializer = new Serializer(stream);
            var f = new bar();
            serializer.Write(f);
            serializer.Position = 0;

            var f2 = serializer.Read<bar>();

            Assert.AreEqual(30, f2.s2);
        }

        [TestMethod]
        public void TestDeserializeSbyte3()
        {
            MemoryStream stream = new MemoryStream(8);
            Serializer ser = new Serializer(stream);
            var f = new bar();

            ser.Write(f);
            stream.Position = 0;

            var f2 = ser.Read<bar>();

            Assert.AreEqual(sbyte.MaxValue, f2.s3);
        }


        [TestMethod]
        public void TestMultiple()
        {
            MemoryStream stream = new MemoryStream();

            Serializer ser = new Serializer(stream);
            var s = new stringFoo();
            s.str = "hello ";
            ser.Write(s);
            var s2 = new stringFoo();
            s2.str = "world";
            ser.Write(s2);

            stream.Position = 0;
            var ns = ser.Read<stringFoo>();
            var ns2 = ser.Read<stringFoo>();

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
            Serializer serializer = new Serializer(stream);
            var s = new stringFoo();
            s.str = null;
            serializer.Write(s);
            stream.Position = 0;

            stringFoo result = serializer.Read<stringFoo>();
            Assert.AreEqual(null,result.str);
        }

        [TestMethod]
        public void TestEmptyString()
        {
            MemoryStream stream = new MemoryStream();
            Serializer serializer = new Serializer(stream);
            var s = new stringFoo();
            s.str = "";
            serializer.Write(s);
            stream.Position = 0;

            stringFoo result = serializer.Read<stringFoo>();
            Assert.AreEqual("", result.str);
        }

        [TestMethod]
        public void TestFullString()
        {
            MemoryStream stream = new MemoryStream();
            Serializer ser = new Serializer(stream);
            var s = new stringFoo();
            s.str = "Hello world";
            ser.Write(s);
            stream.Position = 0;

            stringFoo result = ser.Read<stringFoo>();
            Assert.AreEqual("Hello world", result.str);
        }
    }
}