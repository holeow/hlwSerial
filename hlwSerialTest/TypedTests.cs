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

            var properties = Serializer.GetPropertiesWithAttribute(typeof(fooTyped));
            foreach (var customPropertyInfo in properties)
            {
                Trace.WriteLine($"{customPropertyInfo.PropertyInfo.Name}:{customPropertyInfo.SerializeType}");
            }

            ser.Write(one,true);
            ser.Position = 0;
            byte[] bytes = stream.ToArray();

            Trace.WriteLine(String.Join(" ", bytes.Select(a => a.ToString())));
            foox x = ser.Read<foox>(true);
            Trace.WriteLine($"x is null ? {x==null}");
            Assert.AreEqual(typeof(fooOne),x.GetType());
        }
        

    }
}
