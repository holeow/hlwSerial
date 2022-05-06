using System;
using System.Collections.Generic;
using System.Text;

namespace hlwSerial
{
    public interface Serializable
    {

        void PrepareSerialization();
        void AfterDeserialization();

    }
}
