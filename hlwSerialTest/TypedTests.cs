using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using hlwSerial;

namespace hlwSerialTest
{

    public class fooTyped : ISerializable
    {
        [Serialize(SerializeType = true)]
        public int integer { get; set; }
        [Serialize(SerializeType = true)]
        public foox foo { get; set; }
        [Serialize(SerializeElementsType = true)]
        public List<foox> list { get; set; }
    }

    public abstract class foox : ISerializable
    {
        public abstract int integer { get; set; }
        
    }

    public class fooOne : foox
    {
        [Serialize]public override int integer { get; set; }
        [Serialize(SerializeType = true)]public int integuru { get; set; }
        [Serialize]public Type ty { get; set; }
    }

    public class fooTwo : foox
    {
        [Serialize(SerializeType = true)]public override int integer { get; set; }
        [Serialize(SerializeType = true)]public int? nullable { get; set; }
    }




    [TestClass]
    public class TypedTests
    {

        [TestMethod]
        public void GivesCorrectType()
        {
            MemoryStream stream = new MemoryStream();

            fooOne one = new fooOne();
            one.integer = 4;
            one.integuru = 8;
            one.ty = typeof(int);
            fooTyped typed = new fooTyped();
            typed.foo = one;

            Serializer ser = new Serializer(stream);


            ser.Write(one,true);
            ser.Position = 0;

    


            foox x = ser.Read<foox>(true);
            Trace.WriteLine($"x is null ? {x==null}");
            Assert.AreEqual(typeof(fooOne),x.GetType());
        }



        [TestMethod]
        public void ElementsType1()
        {
            MemoryStream stream = new MemoryStream();

            fooOne one = new fooOne();
            one.integer = 4;
            one.integuru = 9;
            one.ty = typeof(foox);
            fooTyped typed = new fooTyped();
            typed.foo = one;
            typed.integer = 8;
            fooTwo two = new fooTwo() {integer = 130, nullable = 80};
            fooTwo three = new fooTwo() { integer = 130, nullable = 16 };
            fooOne four = new fooOne() { integer = 90,integuru=15,ty = typeof(int)};
            fooTwo five = new fooTwo() { integer = 99, nullable = 55 };
            fooOne six = new fooOne() { integer = 90, integuru = 15, ty = null };
            fooTwo seven = new fooTwo() { integer = 99, nullable = 112 };

            typed.list = new List<foox> { two, three, four, five, six, seven};


            Serializer ser = new Serializer(stream);

            ser.Write(typed, true);
            ser.Position = 0;

            
           /*
            //? fooTyped
            Type u = ser.Read<Type>();
            Console.WriteLine(u);
            int i = ser.Read<int>();
            Console.WriteLine(i);

            //? foo
            u = ser.Read<Type>();
            Console.WriteLine(u);
            i = ser.Read<int>();
            Console.WriteLine(i);

            i = ser.Read<int>();
            Console.WriteLine(i);


            u = ser.Read<Type>();
            Console.WriteLine(u);

            //? list

            var b = ser.Read<bool>();
            Console.WriteLine(b);
            i = ser.Read<int>();
            Console.WriteLine(i);

            Console.WriteLine("=====");
            u = ser.Read<Type>();
            Console.WriteLine(u);
            i = ser.Read<int>();
            Console.WriteLine(i);

            b = ser.Read<bool>();
            Console.WriteLine(b);
            i = ser.Read<int>();
            Console.WriteLine(i);

            */
            fooTyped typed2 = ser.Read<fooTyped>(true);
            Console.WriteLine(typed2 == null);
            Console.WriteLine(typed2.list == null);

            Assert.AreEqual(typeof(int), (typed2.list[2] as fooOne).ty);
        }

        [TestMethod]
        public void ElementsType2()
        {
            MemoryStream stream = new MemoryStream();

            fooOne one = new fooOne();
            one.integer = 4;
            one.integuru = 8;
            one.ty = typeof(foox);
            fooTyped typed = new fooTyped();
            typed.foo = one;
            typed.integer = 8;
            fooTwo two = new fooTwo() { integer = 130, nullable = 80 };
            fooTwo three = new fooTwo() { integer = 130, nullable = 0 };
            fooOne four = new fooOne() { integer = 90, integuru = 15, ty = typeof(int) };
            fooTwo five = new fooTwo() { integer = 99, nullable = 55 };
            fooOne six = new fooOne() { integer = 90, integuru = 15, ty = null };
            fooTwo seven = new fooTwo() { integer = 99, nullable = 112 };

            typed.list = new List<foox> { one, two, three, four, five, six, seven };


            Serializer ser = new Serializer(stream);

            ser.Write(typed, true);
            ser.Position = 0;


            fooTyped typed2 = ser.Read<fooTyped>(true);

            Assert.AreEqual(null, (typed2.list[5] as fooOne).ty);
        }

        [TestMethod]
        public void ElementsType3()
        {
            MemoryStream stream = new MemoryStream();

            fooOne one = new fooOne();
            one.integer = 4;
            one.integuru = 8;
            one.ty = typeof(foox);
            fooTyped typed = new fooTyped();
            typed.foo = one;
            typed.integer = 8;
            fooTwo two = new fooTwo() { integer = 130, nullable = 80 };
            fooTwo three = new fooTwo() { integer = 130, nullable = 0 };
            fooOne four = new fooOne() { integer = 90, integuru = 15, ty = typeof(int) };
            fooTwo five = new fooTwo() { integer = 99, nullable = 55 };
            fooOne six = new fooOne() { integer = 90, integuru = 15, ty = null };
            fooTwo seven = new fooTwo() { integer = 99, nullable = 112 };

            typed.list = new List<foox> { one, two, three, four, five, six, seven };


            Serializer ser = new Serializer(stream);

            ser.Write(typed, true);

            ser.Position = 0;


            fooTyped typed2 = ser.Read<fooTyped>(true);

            Assert.AreEqual(112, (typed2.list[6] as fooTwo).nullable);
        }

        [TestMethod]
        public void ElementsType4()
        {
            MemoryStream stream = new MemoryStream();

            fooOne one = new fooOne();
            one.integer = 4;
            one.integuru = 8;
            one.ty = typeof(foox);
            fooTyped typed = new fooTyped();
            typed.foo = one;
            typed.integer = 8;
            fooTwo two = new fooTwo { integer = 130, nullable = 80 };
            fooTwo three = new fooTwo { integer = 130, nullable = 0 };
            fooOne four = new fooOne { integer = 90, integuru = 15, ty = typeof(int) };
            fooTwo five = new fooTwo { integer = 99, nullable = 55 };
            fooOne six = new fooOne { integer = 90, integuru = 15, ty = null };
            fooTwo seven = new fooTwo { integer = 99, nullable = 112 };

            typed.list = new List<foox> { one, two, three, four, five, six, seven };


            Serializer ser = new Serializer(stream);

            ser.Write(typed, true);

            ser.Position = 0;


            fooTyped typed2 = ser.Read<fooTyped>(true);

            Assert.AreEqual(typeof(foox), (typed2.foo as fooOne).ty);
        }



        [TestMethod]
        public void ElementsType5()
        {
            MemoryStream stream = new MemoryStream();

            fooOne one = new fooOne();
            one.integer = 4;
            one.integuru = 8;
            one.ty = typeof(foox);
            fooTyped typed = new fooTyped();
            typed.foo = one;
            typed.integer = 8;
            fooTwo two = new fooTwo() { integer = 130, nullable = 80 };
            fooTwo three = new fooTwo() { integer = 130, nullable = 0 };
            fooOne four = new fooOne() { integer = 90, integuru = 15, ty = typeof(int) };
            fooTwo five = new fooTwo() { integer = 99, nullable = 55 };
            fooOne six = new fooOne() { integer = 90, integuru = 15, ty = null };
            fooTwo seven = new fooTwo() { integer = 99, nullable = 112 };

            typed.list = new List<foox> { one, two, three, four, null, five, six, seven };


            Serializer ser = new Serializer(stream);

            ser.Write(typed, true);

            ser.Position = 0;

            
            fooTyped typed2 = ser.Read<fooTyped>(true);

            Assert.AreEqual(null,typed2.list[4]);
        }

        [TestMethod]
        public void ElementsType6()
        {
            MemoryStream stream = new MemoryStream();

            fooTyped typed = null;
            


            Serializer ser = new Serializer(stream);

            ser.Write(typed, true);

            Console.WriteLine(String.Join(' ', stream.ToArray().Select(a => a.ToString())));

            ser.Position = 0;


            fooTyped typed2 = ser.Read<fooTyped>(true);

            Assert.AreEqual(null, typed2);
        }
    }

}
