using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using hlwSerial;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace hlwSerialTest
{
    public class ComplexTimeTestClass : ISerializable
    {
        [Serialize]public int integer{ get; set; }
        [Serialize] public string str { get; set; }

        [Serialize] public bool boolean { get; set; }

        [Serialize] public Type ty { get; set; }

        [Serialize] public int? nulint { get; set; }

        [Serialize] public underTestClass under { get; set; }


    }

    public class underTestClassTyped : ISerializable
    {
        [Serialize(SerializeType = true)]public bool? nulboolean { get; set; }
        [Serialize(SerializeType = true , SerializeElementsType = true)] public List<int> list { get; set; }
    }


    public class ComplexTimeTestClassTyped : ISerializable
    {
        [Serialize(SerializeType = true)] public int integer { get; set; }
        [Serialize(SerializeType = true)] public string str { get; set; }

        [Serialize(SerializeType = true)] public bool boolean { get; set; }

        [Serialize(SerializeType = true)] public Type ty { get; set; }

        [Serialize(SerializeType = true)] public int? nulint { get; set; }

        [Serialize(SerializeType = true)] public underTestClassTyped under { get; set; }


    }

    public class underTestClass : ISerializable
    {
        [Serialize] public bool? nulboolean { get; set; }
        [Serialize] public List<int> list { get; set; }
    }

    public static class ListGenerator
    {
        static Random rand = new Random();
        public static List<ComplexTimeTestClass> GenerateComplexList(int amount)
        {
            List<ComplexTimeTestClass> list = new List<ComplexTimeTestClass>();

            for (int i = 0; i < amount; i++)
            {
                list.Add(GenerateComplexOne());
            }
            return list;
        }

        public static ComplexTimeTestClass GenerateComplexOne()
        {
            var n = new ComplexTimeTestClass()
            {
                integer = rand.Next(int.MaxValue), boolean = rand.Next(100) > 50,
                nulint = rand.Next(100) > 90 ? null : rand.Next(int.MaxValue), ty = typeof(ComplexTimeTestClass),
                str = rand.NextDouble().ToString()
            };

            List<int> list = new List<int>();
            for (int i = 0; i < 50; i++)
            {
                list.Add(rand.Next(100));
            }

            n.under = new underTestClass() {nulboolean = rand.Next(50) > 25 ? null : true, list = list};
            return n;
        }


        public static List<ComplexTimeTestClassTyped> GenerateTypedComplexList(int amount)
        {
            List<ComplexTimeTestClassTyped> list = new List<ComplexTimeTestClassTyped>();

            for (int i = 0; i < amount; i++)
            {
                list.Add(GenerateTypedComplexOne());
            }
            return list;
        }
        public static ComplexTimeTestClassTyped GenerateTypedComplexOne()
        {
            var n = new ComplexTimeTestClassTyped()
            {
                integer = rand.Next(int.MaxValue),
                boolean = rand.Next(100) > 50,
                nulint = rand.Next(100) > 90 ? null : rand.Next(int.MaxValue),
                ty = typeof(ComplexTimeTestClass),
                str = rand.NextDouble().ToString()
            };

            List<int> list = new List<int>();
            for (int i = 0; i < 50; i++)
            {
                list.Add(rand.Next(100));
            }

            n.under = new underTestClassTyped() { nulboolean = rand.Next(50) > 25 ? null : true, list = list };
            return n;
        }
    }


    [TestClass]
    public class SpeedTestsWrite
    {
        [TestMethod]
        public void NewtonSoftTest5()
        {
            Stopwatch sw = new Stopwatch();
            var list = ListGenerator.GenerateComplexList(5);
            var one = ListGenerator.GenerateComplexOne();

            MemoryStream s1 = new MemoryStream();
            MemoryStream s2 = new MemoryStream();
            string serializedOne = JsonConvert.SerializeObject(one);
            using (var writer = new BinaryWriter(s1))
            {
                writer.Write(serializedOne);
            }


            using (var writer = new BinaryWriter(s2))
            {
                sw.Start();
                {
                    writer.Write(JsonConvert.SerializeObject(list));
                }
                sw.Stop();
                Console.WriteLine(sw.ElapsedTicks);

            }


            Assert.IsTrue(true);

        }

        


        [TestMethod]
        public void hlwSerialTest5()
        {
            Stopwatch sw = new Stopwatch();
            var list = ListGenerator.GenerateComplexList(5);
            var one = ListGenerator.GenerateComplexOne();

            MemoryStream s1 = new MemoryStream();
            MemoryStream s2 = new MemoryStream();

            Serializer ss1 = new Serializer(s1);
            Serializer ss2 = new Serializer(s2);

            ss1.Write(one);

            sw.Start();
            ss2.Write(list);
            sw.Stop();
            Console.Write(sw.ElapsedTicks);

            Assert.IsTrue(true);
        }

        [TestMethod]public void NewtonSoftTest100()
        {
            Stopwatch sw = new Stopwatch();
            var list = ListGenerator.GenerateComplexList(100);
            var one = ListGenerator.GenerateComplexOne();

            MemoryStream s1 = new MemoryStream();
            MemoryStream s2 = new MemoryStream();
            string serializedOne = JsonConvert.SerializeObject(one);
            using (var writer = new BinaryWriter(s1))
            {
                writer.Write(serializedOne);
            }


            using (var writer = new BinaryWriter(s2))
            {
                sw.Start();
                {
                    writer.Write(JsonConvert.SerializeObject(list));
                }
                sw.Stop();
                Console.WriteLine(sw.ElapsedTicks);

            }


            Assert.IsTrue(true);

        }


        [TestMethod]
        public void hlwSerialTest100()
        {
            Stopwatch sw = new Stopwatch();
            var list = ListGenerator.GenerateComplexList(100);
            var one = ListGenerator.GenerateComplexOne();

            MemoryStream s1 = new MemoryStream();
            MemoryStream s2 = new MemoryStream();

            Serializer ss1 = new Serializer(s1);
            Serializer ss2 = new Serializer(s2);

            ss1.Write(one);

            sw.Start();
            ss2.Write(list);
            sw.Stop();
            Console.Write(sw.ElapsedTicks);

            Assert.IsTrue(true);
        }




        [TestMethod]
        public void NewtonSoftTest5000()
        {
            Stopwatch sw = new Stopwatch();
            var list = ListGenerator.GenerateComplexList(5000);
            var one = ListGenerator.GenerateComplexOne();

            MemoryStream s1 = new MemoryStream();
            MemoryStream s2 = new MemoryStream();
            string serializedOne = JsonConvert.SerializeObject(one);
            using (var writer = new BinaryWriter(s1))
            {
                writer.Write(serializedOne);
            }


            using (var writer = new BinaryWriter(s2))
            {
                sw.Start();
                {
                    writer.Write(JsonConvert.SerializeObject(list));
                }
                sw.Stop();
                Console.WriteLine(sw.ElapsedTicks);

            }


            Assert.IsTrue(true);

        }


        [TestMethod]
        public void hlwSerialTest5000()
        {
            Stopwatch sw = new Stopwatch();
            var list = ListGenerator.GenerateComplexList(5000);
            var one = ListGenerator.GenerateComplexOne();

            MemoryStream s1 = new MemoryStream();
            MemoryStream s2 = new MemoryStream();

            Serializer ss1 = new Serializer(s1);
            Serializer ss2 = new Serializer(s2);

            ss1.Write(one);

            sw.Start();
            ss2.Write(list);
            sw.Stop();
            Console.Write(sw.ElapsedTicks);

            Assert.IsTrue(true);
        }


        [TestMethod]
        public void NewtonSoftTest50000()
        {
            Stopwatch sw = new Stopwatch();
            var list = ListGenerator.GenerateComplexList(50000);
            var one = ListGenerator.GenerateComplexOne();

            MemoryStream s1 = new MemoryStream();
            MemoryStream s2 = new MemoryStream();
            string serializedOne = JsonConvert.SerializeObject(one);
            using (var writer = new BinaryWriter(s1))
            {
                writer.Write(serializedOne);
            }


            using (var writer = new BinaryWriter(s2))
            {
                sw.Start();
                {
                    writer.Write(JsonConvert.SerializeObject(list));
                }
                sw.Stop();
                Console.WriteLine(sw.ElapsedTicks);

            }


            Assert.IsTrue(true);

        }


        [TestMethod]
        public void hlwSerialTest50000()
        {
            Stopwatch sw = new Stopwatch();
            var list = ListGenerator.GenerateComplexList(50000);
            var one = ListGenerator.GenerateComplexOne();

            MemoryStream s1 = new MemoryStream();
            MemoryStream s2 = new MemoryStream();

            Serializer ss1 = new Serializer(s1);
            Serializer ss2 = new Serializer(s2);

            ss1.Write(one);

            sw.Start();
            ss2.Write(list);
            sw.Stop();
            Console.Write(sw.ElapsedTicks);

            Assert.IsTrue(true);
        }



        [TestMethod]
        public void NewtonSoftTestTypedAll5()
        {
            Stopwatch sw = new Stopwatch();
            var list = ListGenerator.GenerateTypedComplexList(5);
            var one = ListGenerator.GenerateTypedComplexOne();

            MemoryStream s1 = new MemoryStream();
            MemoryStream s2 = new MemoryStream();


            JsonSerializerSettings serializerSettings = new JsonSerializerSettings();
            serializerSettings.TypeNameHandling = TypeNameHandling.All;
            
            string serializedOne = JsonConvert.SerializeObject(one,serializerSettings);
            using (var writer = new BinaryWriter(s1))
            {
                writer.Write(serializedOne);
            }


            using (var writer = new BinaryWriter(s2))
            {
                sw.Start();
                {
                    writer.Write(JsonConvert.SerializeObject(list,serializerSettings));
                }
                sw.Stop();
                Console.WriteLine(sw.ElapsedTicks);

            }


            Assert.IsTrue(true);

        }

        [TestMethod]
        public void hlwSerialTestTypedAll5()
        {
            Stopwatch sw = new Stopwatch();
            var list = ListGenerator.GenerateTypedComplexList(5);
            var one = ListGenerator.GenerateTypedComplexOne();

            MemoryStream s1 = new MemoryStream();
            MemoryStream s2 = new MemoryStream();

            Serializer ss1 = new Serializer(s1);
            Serializer ss2 = new Serializer(s2);

            ss1.Write(one,true);

            sw.Start();
            ss2.Write(list,true);
            sw.Stop();
            Console.Write(sw.ElapsedTicks);

            Assert.IsTrue(true);
        }







        [TestMethod]
        public void NewtonSoftTestTypedAll100()
        {
            Stopwatch sw = new Stopwatch();
            var list = ListGenerator.GenerateTypedComplexList(100);
            var one = ListGenerator.GenerateTypedComplexOne();

            MemoryStream s1 = new MemoryStream();
            MemoryStream s2 = new MemoryStream();


            JsonSerializerSettings serializerSettings = new JsonSerializerSettings();
            serializerSettings.TypeNameHandling = TypeNameHandling.All;

            string serializedOne = JsonConvert.SerializeObject(one, serializerSettings);
            using (var writer = new BinaryWriter(s1))
            {
                writer.Write(serializedOne);
            }


            using (var writer = new BinaryWriter(s2))
            {
                sw.Start();
                {
                    writer.Write(JsonConvert.SerializeObject(list, serializerSettings));
                }
                sw.Stop();
                Console.WriteLine(sw.ElapsedTicks);

            }


            Assert.IsTrue(true);

        }

        [TestMethod]
        public void hlwSerialTestTypedAll100()
        {
            Stopwatch sw = new Stopwatch();
            var list = ListGenerator.GenerateTypedComplexList(100);
            var one = ListGenerator.GenerateTypedComplexOne();

            MemoryStream s1 = new MemoryStream();
            MemoryStream s2 = new MemoryStream();

            Serializer ss1 = new Serializer(s1);
            Serializer ss2 = new Serializer(s2);

            ss1.Write(one, true);

            sw.Start();
            ss2.Write(list, true);
            sw.Stop();
            Console.Write(sw.ElapsedTicks);

            Assert.IsTrue(true);
        }


        
    }

    [TestClass]
    public class SpeedTestRead
    {
        [TestMethod]
        public void NewtonSoftRead5()
        {
            Stopwatch sw = new Stopwatch();
            var list = ListGenerator.GenerateComplexList(5);
            var one = ListGenerator.GenerateComplexOne();

            MemoryStream s1 = new MemoryStream();
            MemoryStream s2 = new MemoryStream();
            string serializedOne = JsonConvert.SerializeObject(one);
            using (var writer = new BinaryWriter(s1, Encoding.UTF8, true))
            {
                writer.Write(serializedOne);
            }


            using (var writer = new BinaryWriter(s2, Encoding.UTF8, true))
            {


                writer.Write(JsonConvert.SerializeObject(list));



            }
            using (var reader = new BinaryReader(s1))
            {
                reader.BaseStream.Position = 0;
                var one2 = JsonConvert.DeserializeObject<ComplexTimeTestClass>(reader.ReadString());
            }
            using (var reader = new BinaryReader(s2))
            {
                reader.BaseStream.Position = 0;
                sw.Start();
                var list2 = JsonConvert.DeserializeObject<List<ComplexTimeTestClass>>(reader.ReadString());
                sw.Stop();
                Console.WriteLine(sw.ElapsedTicks);
            }


            Assert.IsTrue(true);
        }

        [TestMethod]
        public void hlwSerialRead5()
        {
            Stopwatch sw = new Stopwatch();
            var list = ListGenerator.GenerateComplexList(5);
            var one = ListGenerator.GenerateComplexOne();

            MemoryStream s1 = new MemoryStream();
            MemoryStream s2 = new MemoryStream();

            Serializer ss1 = new Serializer(s1);
            Serializer ss2 = new Serializer(s2);

            ss1.Write(one);
            ss2.Write(list);

            ss1.Position = 0;
            var one2 = ss1.Read<ComplexTimeTestClass>();
            ss2.Position = 0;
            sw.Start();

            var list2 = ss2.Read<List<ComplexTimeTestClass>>();
            sw.Stop();
            Console.WriteLine(sw.ElapsedTicks);
            Assert.AreEqual(list[4].str, list2[4].str);
        }

        [TestMethod]
        public void NewtonSoftRead100()
        {
            Stopwatch sw = new Stopwatch();
            var list = ListGenerator.GenerateComplexList(100);
            var one = ListGenerator.GenerateComplexOne();

            MemoryStream s1 = new MemoryStream();
            MemoryStream s2 = new MemoryStream();
            string serializedOne = JsonConvert.SerializeObject(one);
            using (var writer = new BinaryWriter(s1, Encoding.UTF8, true))
            {
                writer.Write(serializedOne);
            }


            using (var writer = new BinaryWriter(s2, Encoding.UTF8, true))
            {


                writer.Write(JsonConvert.SerializeObject(list));



            }
            using (var reader = new BinaryReader(s1))
            {
                reader.BaseStream.Position = 0;
                var one2 = JsonConvert.DeserializeObject<ComplexTimeTestClass>(reader.ReadString());
            }
            using (var reader = new BinaryReader(s2))
            {
                reader.BaseStream.Position = 0;
                sw.Start();
                var list2 = JsonConvert.DeserializeObject<List<ComplexTimeTestClass>>(reader.ReadString());
                sw.Stop();
                Console.WriteLine(sw.ElapsedTicks);
            }


            Assert.IsTrue(true);
        }

        [TestMethod]
        public void hlwSerialRead100()
        {
            Stopwatch sw = new Stopwatch();
            var list = ListGenerator.GenerateComplexList(100);
            var one = ListGenerator.GenerateComplexOne();

            MemoryStream s1 = new MemoryStream();
            MemoryStream s2 = new MemoryStream();

            Serializer ss1 = new Serializer(s1);
            Serializer ss2 = new Serializer(s2);

            ss1.Write(one);
            ss2.Write(list);

            ss1.Position = 0;
            var one2 = ss1.Read<ComplexTimeTestClass>();
            ss2.Position = 0;
            sw.Start();

            var list2 = ss2.Read<List<ComplexTimeTestClass>>();
            sw.Stop();
            Console.WriteLine(sw.ElapsedTicks);
            Assert.AreEqual(list[4].str, list2[4].str);
        }




    }
}
