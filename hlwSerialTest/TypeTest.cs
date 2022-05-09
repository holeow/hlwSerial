using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using hlwSerial;

namespace hlwSerialTest
{


    public class fooType : ISerializable
    {
        [Serialize]public int? integer { get; set; }
        [Serialize]public bool? boolean { get; set; }
        [Serialize]public float? floating { get; set; }
    }



    [TestClass]
    public class TypeTests
    {

        [TestMethod]
        public void TestWithGenericIncludingATypeFromAnotherAssembly()
        {
            var d = typeof(FooDict<Serializer,int>).GetShortTypeName();

            //var t = typeof(FooList<int>).AssemblyQualifiedName;
            //var t = Type.GetType(d);
            Trace.WriteLine(d);

            var x = typeof(FooDict<Serializer, int>).FullName; // With fullname of a generic type, we get the version number and the culture we don't really need (and could possibly lead to errors) in the genericTypeParameters.
            Trace.WriteLine(x);
            //Trace.WriteLine(t);

            // example of Type with modified versions. It seems to work anyway BUT ONLY AFTER CORE 5.0 !! That's the reason of the workaround !.
            var s =
                "hlwSerialTest.FooDict`2[[hlwSerial.Serializer, hlwSerial, Version=2.0.0.9, Culture=neutral, PublicKeyToken=null],[System.Int32, System.Private.CoreLib, Version=119.0.0.0, Culture=neutral, PublicKeyToken=7cec85d7bea7798e]], hlwSerialTest, Version=2.0.0.0, Culture=neutral, PublicKeyToken=null";

            Assert.AreEqual(typeof(FooDict<Serializer, int>), Type.GetType(d));

        }


    }
}
