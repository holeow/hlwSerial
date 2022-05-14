using System;
using System.Collections.Generic;
using System.Text;

namespace hlwSerial
{

    public class SerializationException : Exception
    {
        public SerializationException(string message) : base(message)
        {

        }
    }

    public class SerializationTooMuchRecursivityException : SerializationException
    {
        public SerializationTooMuchRecursivityException(string message): base(message)
        {

        }
    }

    public class SerializationContainsSameObjectTwiceInTheStack : SerializationException
    {
        public SerializationContainsSameObjectTwiceInTheStack(string message) : base(message)
        {

        }
    }
}
