using System;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using hlwSerial;

namespace hlwSerialTest
{
    class foo : hlwSerializable
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

    class bar : hlwSerializable
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
    }
}