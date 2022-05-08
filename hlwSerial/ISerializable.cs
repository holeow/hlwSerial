using System;
using System.Collections.Generic;
using System.Text;

namespace hlwSerial
{
    public interface ISerializable
    {

    }

    public interface IPrepareSerialization : ISerializable
    {
        void PrepareSerialization();
    }

    public interface IAfterDeserialization : ISerializable
    {
        void AfterDeserialization();
    }
}
