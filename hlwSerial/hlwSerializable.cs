using System;
using System.Collections.Generic;
using System.Text;

namespace hlwSerial
{
    public interface hlwSerializable
    {

        void PrepareSerialization();
        void AfterDeserialization();

    }
}
