using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using hlwSerial;

namespace hlwSerialTest
{
    public class TestThings
    {
        //?? This is a chapter: //??
        //? this is a zone //?
        //todo this is a todo //todo
        //! this is a warning //!
        //!! this is a super //!!
        //bug this is a bug //bug
        //ctor this is a constructor //ctor
        //dispose dispose is using ctor theme. //dispose
        //url this is an url //url
        //guide this is a guide //guide
        //src this is a source //src
        //debug this is a debug //debug

        public void Write(object obj)
        {

        }

        public void Write(int obj)
        {

        }
    }


    [TestClass]
    public class RandomTests
    {

        [TestMethod]
        public void TestArrayTypes()
        {
            var simpleArr = new int[10];
            var biArr = new int[10, 15];
            var jaggArr = new int[5][];
            Console.WriteLine(simpleArr.GetType());
            Console.WriteLine(biArr.GetType());
            Console.WriteLine(biArr.Length);
            Console.WriteLine(biArr.Rank);
            Console.WriteLine(biArr.GetLength(0));
            Console.WriteLine(biArr.GetLength(1));
            //!Exception: Console.WriteLine(biArr.GetLength(2));

            Console.WriteLine("===================");
            var biArr2 = Array.CreateInstance(typeof(int), new int[] {10, 15});

            Console.WriteLine(biArr2.GetType());
            Console.WriteLine(biArr2.Length);
            Console.WriteLine(biArr2.Rank);
            Console.WriteLine(biArr2.GetLength(0));
            Console.WriteLine(biArr2.GetLength(1));

            Console.WriteLine("===================");

            Console.WriteLine(jaggArr.GetType());
            Console.WriteLine(jaggArr.Length);
            Console.WriteLine(jaggArr.Rank);
            Console.WriteLine(jaggArr.GetLength(0));
            Console.WriteLine("===================");

            var jaggArr2 = Array.CreateInstance(typeof(int[]), 5);
            Console.WriteLine(jaggArr2.GetType());
        }

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


        public void BufferKnowledge()
        {
            var t = new TestThings();
            t.Write(16);
        }

        [TestMethod]
        public void TestBufferCopy5()
        {
            Random rand = new Random();
            List<int> hello = new List<int>();
            for (int i = 0; i < 5; i++)
            {
                hello.Add(rand.Next(int.MaxValue));
            }

            MemoryStream stream = new MemoryStream();
            Serializer ser = new Serializer(stream);
            Stopwatch sw = new Stopwatch();
            sw.Start();
            byte[] result = new byte[hello.Count * sizeof(int)];
            Buffer.BlockCopy(hello.ToArray(), 0, result, 0, result.Length);
            ser.Write(5);
            ser.underlyingStream.Write(result, 0, result.Length);
            sw.Stop();
            Console.WriteLine(sw.ElapsedTicks);
        }
        [TestMethod]
        public void TestBufferCopyRead5()
        {
            Random rand = new Random();
            List<int> hello = new List<int>();
            for (int i = 0; i < 5; i++)
            {
                hello.Add(rand.Next(int.MaxValue));
            }

            MemoryStream stream = new MemoryStream();
            Serializer ser = new Serializer(stream);
            Stopwatch sw = new Stopwatch();
            byte[] result = new byte[hello.Count * sizeof(int)];
            Buffer.BlockCopy(hello.ToArray(), 0, result, 0, result.Length);
            ser.Write(5);
            ser.underlyingStream.Write(result, 0, result.Length);

            ser.Position = 0;

            sw.Start();
            var amount = ser.Read<int>();
            var buffer = new byte[amount * 4];
            int[] resultint = new int[amount];
            ser.underlyingStream.Read(buffer, 0, buffer.Length);
            Buffer.BlockCopy(buffer, 0, resultint, 0, result.Length);
            var L = resultint.ToList();
            sw.Stop();
            Console.WriteLine(sw.ElapsedTicks);

            Assert.AreEqual(hello[0],resultint[0]);
        }








        [TestMethod]
        public void TestBufferCopy100()
        {
            Random rand = new Random();
            List<int> hello = new List<int>();
            for (int i = 0; i < 100; i++)
            {
                hello.Add(rand.Next(int.MaxValue));
            }

            MemoryStream stream = new MemoryStream();
            Serializer ser = new Serializer(stream);
            Stopwatch sw = new Stopwatch();
            sw.Start();
            byte[] result = new byte[hello.Count * sizeof(int)];
            Buffer.BlockCopy(hello.ToArray(), 0, result, 0, result.Length);
            ser.Write(100);
            ser.underlyingStream.Write(result, 0, result.Length);
            sw.Stop();
            Console.WriteLine(sw.ElapsedTicks);
        }

        [TestMethod]
        public void TestBufferCopyRead100()
        {
            Random rand = new Random();
            List<int> hello = new List<int>();
            for (int i = 0; i < 100; i++)
            {
                hello.Add(rand.Next(int.MaxValue));
            }

            MemoryStream stream = new MemoryStream();
            Serializer ser = new Serializer(stream);
            Stopwatch sw = new Stopwatch();
            byte[] result = new byte[hello.Count * sizeof(int)];
            Buffer.BlockCopy(hello.ToArray(), 0, result, 0, result.Length);
            ser.Write(100);
            ser.underlyingStream.Write(result, 0, result.Length);

            ser.Position = 0;

            sw.Start();
            var amount = ser.Read<int>();
            var buffer = new byte[amount * 4];
            int[] resultint = new int[amount];
            ser.underlyingStream.Read(buffer, 0, buffer.Length);
            Buffer.BlockCopy(buffer, 0, resultint, 0, result.Length);
            var L = resultint.ToList();
            sw.Stop();
            Console.WriteLine(sw.ElapsedTicks);

            Assert.AreEqual(hello[0], resultint[0]);
        }
        [TestMethod]
        public void TestBufferCopyRead5000()
        {
            Random rand = new Random();
            List<int> hello = new List<int>();
            for (int i = 0; i < 5000; i++)
            {
                hello.Add(rand.Next(int.MaxValue));
            }

            MemoryStream stream = new MemoryStream();
            Serializer ser = new Serializer(stream);
            Stopwatch sw = new Stopwatch();
            byte[] result = new byte[hello.Count * sizeof(int)];
            Buffer.BlockCopy(hello.ToArray(), 0, result, 0, result.Length);
            ser.Write(5000);
            ser.underlyingStream.Write(result, 0, result.Length);

            ser.Position = 0;

            sw.Start();
            var amount = ser.Read<int>();
            var buffer = new byte[amount * 4];
            int[] resultint = new int[amount];
            ser.underlyingStream.Read(buffer, 0, buffer.Length);
            Buffer.BlockCopy(buffer, 0, resultint, 0, result.Length);
            var L = resultint.ToList();
            sw.Stop();
            Console.WriteLine(sw.ElapsedTicks);

            Assert.AreEqual(hello[0], resultint[0]);
        }


        [TestMethod]
        public void TestWriteCopy5()
        {
            Random rand = new Random();
            List<int> hello = new List<int>();
            for (int i = 0; i < 5; i++)
            {
                hello.Add(rand.Next(int.MaxValue));
            }

            MemoryStream stream = new MemoryStream();
            Serializer ser = new Serializer(stream);
            Stopwatch sw = new Stopwatch();
            sw.Start();
            ser.Write(hello);
            sw.Stop();
            Console.WriteLine(sw.ElapsedTicks);
        }

        [TestMethod]
        public void TestReadCopy5()
        {
            Random rand = new Random();
            List<int> hello = new List<int>();
            for (int i = 0; i < 5; i++)
            {
                hello.Add(rand.Next(int.MaxValue));
            }

            MemoryStream stream = new MemoryStream();
            Serializer ser = new Serializer(stream);
            ser.Write(hello);
            ser.Position = 0;
            Stopwatch sw = new Stopwatch();
            sw.Start();
            var L = ser.Read <List<int>>();
            sw.Stop();
            Console.WriteLine(sw.ElapsedTicks);
        }
        [TestMethod]
        public void TestWriteCopy100()
        {
            Random rand = new Random();
            List<int> hello = new List<int>();
            for (int i = 0; i < 100; i++)
            {
                hello.Add(rand.Next(int.MaxValue));
            }

            MemoryStream stream = new MemoryStream();
            Serializer ser = new Serializer(stream);
            Stopwatch sw = new Stopwatch();
            sw.Start();
            ser.Write(hello);
            sw.Stop();
            Console.WriteLine(sw.ElapsedTicks);
        }
        [TestMethod]
        public void TestReadCopy100()
        {
            Random rand = new Random();
            List<int> hello = new List<int>();
            for (int i = 0; i < 100; i++)
            {
                hello.Add(rand.Next(int.MaxValue));
            }

            MemoryStream stream = new MemoryStream();
            Serializer ser = new Serializer(stream);
            ser.Write(hello);
            ser.Position = 0;
            Stopwatch sw = new Stopwatch();
            sw.Start();
            var L = ser.Read<List<int>>();
            sw.Stop();
            Console.WriteLine(sw.ElapsedTicks);
        }


        [TestMethod]
        public void TestReadCopy5000()
        {
            Random rand = new Random();
            List<int> hello = new List<int>();
            for (int i = 0; i < 5000; i++)
            {
                hello.Add(rand.Next(int.MaxValue));
            }

            MemoryStream stream = new MemoryStream();
            Serializer ser = new Serializer(stream);
            ser.Write(hello);
            ser.Position = 0;
            Stopwatch sw = new Stopwatch();
            sw.Start();
            var L = ser.Read<List<int>>();
            sw.Stop();
            Console.WriteLine(sw.ElapsedTicks);
        }

        [TestMethod]
        public void TestBufferCopy5000()
        {
            Random rand = new Random();
            List<int> hello = new List<int>();
            for (int i = 0; i < 5000; i++)
            {
                hello.Add(rand.Next(int.MaxValue));
            }

            MemoryStream stream = new MemoryStream();
            Serializer ser = new Serializer(stream);
            Stopwatch sw = new Stopwatch();
            sw.Start();
            byte[] result = new byte[hello.Count * sizeof(int)];
            Buffer.BlockCopy(hello.ToArray(), 0, result, 0, result.Length);
            ser.Write(5000);
            ser.underlyingStream.Write(result, 0, result.Length);
            sw.Stop();
            Console.WriteLine(sw.ElapsedTicks);
        }

        [TestMethod]
        public void TestWriteCopy5000()
        {
            Random rand = new Random();
            List<int> hello = new List<int>();
            for (int i = 0; i < 5000; i++)
            {
                hello.Add(rand.Next(int.MaxValue));
            }

            MemoryStream stream = new MemoryStream();
            Serializer ser = new Serializer(stream);
            Stopwatch sw = new Stopwatch();
            sw.Start();
            ser.Write(hello);
            sw.Stop();
            Console.WriteLine(sw.ElapsedTicks);
        }
    }
}
