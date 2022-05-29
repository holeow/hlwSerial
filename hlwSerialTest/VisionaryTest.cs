using System;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using hlwSerial;

namespace hlwSerialTest
{
    //?? Visionary Proxy Design Pattern

    //guide Visionary proxy
    //this design pattern can be used when the implementation of the class you want to serialize can be subject to change.
    // Create other classes implementing the IVisionary<T> interface that will be used for serialization for each version of the implemention, that holds the serializable data and the method to deserialize it again to the main class.
    //Create them on the fly using a [Serialize(SerializeType=true)] property of the IVisionnary<T> type and implement it as below on the set method.
    //That way, you create a new visionary class every time the implementation of the main class changes, but old files created with older versions (and then olden visionaries) can still be read and you still can use it.

    public class Mainfoo : ISerializable
    {
        public int integer1;
        public int integer2;
        public string version;
        public IVisionary<Mainfoo> ForceVisionary;//debug Not needed in production. For test only.

        [Serialize(SerializeType = true)]
        public IVisionary<Mainfoo> Visionary
        {
            get  => ForceVisionary!= null ? ForceVisionary : new mc2(this);
            //debug (in production just return the last visionary version (here mc2))
            set => value.ApplyVision(this);
        }

    }

    public class mc1 : IVisionary<Mainfoo>
    {
        [Serialize]public int integer1{ get; set; }

        public mc1(Mainfoo entity)
        {
            this.integer1 = entity.integer1;// here we can see that in version 1 of Mainclass, the integer2 property didn't exist yet.
        }

        public mc1() { }//! Don't forget to have a parameterless constructor so the serializer can create it.

        public void ApplyVision(Mainfoo item)
        {
            item.integer1 = this.integer1;
            item.version = "V1";
        }
    }

    public class mc2 : IVisionary<Mainfoo>
    {
        [Serialize] public int integer1 { get; set; }
        [Serialize] public int integer2 { get; set; }

        public mc2(Mainfoo entity)
        {
            this.integer1 = entity.integer1;
            this.integer2 = entity.integer2;
        }
        public mc2(){ }//! Don't forget to have a parameterless constructor so the serializer can create it.

        public void ApplyVision(Mainfoo item)
        {
            item.integer1 = this.integer1;
            item.integer2 = this.integer2;
            item.version = "V2";
        }
    }

    [TestClass]
    public class VisionaryTests
    {
        [TestMethod]
        public void hlwSerializeVisionary_mc2()
        {
            Stream s = new MemoryStream();
            Serializer ser = new Serializer(s);

            Mainfoo mf = new Mainfoo() {integer1 = 18, integer2 = 125};

            ser.Write(mf);

            ser.Position = 0;

            var mf2 = ser.Read<Mainfoo>();

            Assert.AreEqual(true, mf2.version == "V2" && mf2.integer1 == 18 && mf2.integer2 == 125);
        }

        [TestMethod]
        public void hlwSerializeVisionary_mc1()
        {
            Stream s = new MemoryStream();
            Serializer ser = new Serializer(s);

            Mainfoo mf = new Mainfoo() { integer1 = 18, integer2 = 125 };

            mf.ForceVisionary = new mc1(mf); // Force the use of the version 1 visionary.
            ser.Write(mf);

            ser.Position = 0;

            var mf2 = ser.Read<Mainfoo>();

            Assert.AreEqual(true, 
                mf2.version == "V1" //here we can see it used the version 1 fallback
                    && mf2.integer1 == 18 
                    && mf2.integer2 == 0);// as the integer2 didn't exist in version 1, it didn't get serialized and stayed at it's default value.
        }
    }
}
