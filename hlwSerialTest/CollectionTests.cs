using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using hlwSerial;

namespace hlwSerialTest
{
    public class fooArray : ISerializable
    {
        [Serialize]public int n { get; set; }
        [Serialize]public float r { get; set; }

        [Serialize]public int[] array { get; set; }

        public void PrepareSerialization()
        {
            
        }

        public void AfterDeserialization()
        {

        }

        
    }

    public class FooList<T> : ISerializable
    {
        [Serialize] public int n { get; set; }
        [Serialize] public float r { get; set; }

        [Serialize] public List<T> list { get; set; }

        public void PrepareSerialization()
        {
            
        }

        public void AfterDeserialization()
        {
            
        }
    }


    public class FooDict<T, Y> : ISerializable
    {
        [Serialize] public int n { get; set; }
        [Serialize] public float r { get; set; }

        [Serialize] public Dictionary<T,Y> list { get; set; }

        public void PrepareSerialization()
        {

        }

        public void AfterDeserialization()
        {

        }
    }
    

    [TestClass]
    public class CollectionTests
    {
        [TestMethod]
        public void ListIsAssignable()
        {
            

            Assert.IsTrue(typeof(List<>) == typeof(List<int>).GetGenericTypeDefinition());

            
        }

        [TestMethod]
        public void intArrayTest()
        {
            fooArray c = new fooArray();
            c.array = new int[] { 1, 2, 3 };
            Stream s = new MemoryStream();
            Serializer serializer = new Serializer(s);

            serializer.Write(c);
            
            s.Position = 0;

            var b2 = serializer.Read<fooArray>();

            Assert.AreEqual(true, c.array[0] == 1 && c.array[1] == 2 && c.array[2] == 3);
        }

        [TestMethod]
        public void intListTest()
        {
            FooList<int> c = new FooList<int>();
            c.list = new List<int>() { 1, 2, 3 };
            Stream s = new MemoryStream();
            Serializer serializer = new Serializer(s);
            serializer.Write(c);

            serializer.Position = 0;

            var b2 = serializer.Read<FooList<int>>();

            Assert.AreEqual(true, b2.list[0] == 1 && b2.list[1] == 2 && b2.list[2] == 3);
        }

        [TestMethod]
        public void DictionnaryTest()
        {
            FooDict<int,string> c = new FooDict<int,string>();
            c.list = new Dictionary<int, string>();
            c.list.Add(21, "hello ");
            c.list.Add(55, "happiness");
            c.list.Add(99, "world");

            Stream s = new MemoryStream();
            Serializer serializer = new Serializer(s);
            serializer.Write(c);

            serializer.Position = 0;

            var b2 = serializer.Read<FooDict<int,string>>();

            Assert.AreEqual("hello world", b2.list[21]+b2.list[99]);
        }

    }
}
