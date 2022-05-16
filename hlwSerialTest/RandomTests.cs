using System;
using System.Collections.Generic;
using System.Diagnostics;
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
