using System;
using System.Collections.Generic;
using System.Text;

namespace hlwSerial
{
    public class SerializationTooMuchRecursivityException : Exception
    {
        public SerializationTooMuchRecursivityException(string message): base(message)
        {

        }
    }
}
