using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using hlwSerial;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace hlwSerialTest
{
    public class ExampleFoo : ISerializable
    {
        public int NonSerialized;

        [Serialize] public int serialized { get; set; }
        [Serialize] public string serializedToo { get; set; }

        public ExampleFoo(int s, string s2)
        {
            serialized = s;
            serializedToo = s2;
        }

        //ctor : Make a parameterless constructor so the serializer can create it
        public ExampleFoo()
        {

        }

    }

    public class BiggerFoo : ISerializable
    {
        [Serialize] public string infos { get; set; }

        [Serialize] public ExampleFoo myExample { get; set; }

        [Serialize(SerializeType =true)] public ISerializable duck { get; set; }

        [Serialize(SerializeElementsType =true)] public List<ISerializable> multiDuck{ get; set; }
    }


    [TestClass]
    public class MyMainClass
    {
        [TestMethod]
        public void MyMainMethod()
        {
            ExampleFoo foo = new ExampleFoo(42, "hello world!");

            //Writing into a file:
            using (var serializer = new Serializer(new FileStream("C:\\Users\\Admin\\source\\repos\\test.binary", FileMode.Create)))
            {
                serializer.Write(foo, true);
            }

            ExampleFoo foo2;
            //Read from file:
            using (var serializer = new Serializer(new FileStream("C:\\Users\\Admin\\source\\repos\\test.binary", FileMode.Open)))
            {
                foo2 = serializer.Read<ExampleFoo>(true);
            }

            Assert.AreEqual(foo.serializedToo, foo2.serializedToo);



        }
    }
}
