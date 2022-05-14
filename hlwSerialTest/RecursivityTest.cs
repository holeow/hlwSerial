using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using hlwSerial;

namespace hlwSerialTest
{

    internal class RecursiveFoo : ISerializable
    {
        [Serialize]public int amount{ get; set; }
        [Serialize]public RecursiveFoo foo{ get; set; }
        [Serialize] public List<bar> bar{ get; set; }
    }

    [TestClass]
    public class RecursivityTests
    {
        [TestMethod]
        public void AutorecursiveTest()
        {
            RecursiveFoo foo = new RecursiveFoo {amount = 100};
            foo.foo = foo;
            Stream s = new MemoryStream();
            Serializer ser = new Serializer(s);
            ser.MaxRecursivity = 10;
            
            Assert.ThrowsException<SerializationContainsSameObjectTwiceInTheStack>(() => ser.Write(foo));
        }

        [TestMethod]
        public void NotAutorecursiveTest()
        {
            List<RecursiveFoo> foos = new List<RecursiveFoo>();

            RecursiveFoo foo = new RecursiveFoo { amount = 100 };
            foo.foo = null;
            for (int i = 0; i < 10; i++)
            {
                foos.Add(foo);
            }
            Stream s = new MemoryStream();
            Serializer ser = new Serializer(s);
            ser.MaxRecursivity = 10;

            ser.Write(foos);

            ser.Position = 0;

            var list = ser.Read<List<RecursiveFoo>>();

            Assert.AreEqual(list[4].amount,list[9].amount);
        }

        [TestMethod]
        public void BigListTest()
        {
            RecursiveFoo foo = new RecursiveFoo { amount = 100 };
            foo.bar = new List<bar>();

            bar b = new bar { s=5,s2 = 10 ,s3=52};

            for (int i = 0; i < 500; i++)
            {
                foo.bar.Add(b);
            }
            Stream s = new MemoryStream();
            Serializer ser = new Serializer(s);
            ser.MaxRecursivity = 10;

            ser.Write(foo);

            ser.Position = 0;

            RecursiveFoo recursiveFoo = ser.Read<RecursiveFoo>();

            Assert.AreEqual(5, recursiveFoo.bar[95].s);
        }
    }

}
