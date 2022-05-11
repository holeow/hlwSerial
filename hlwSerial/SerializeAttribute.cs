using System;

namespace hlwSerial
{
    [AttributeUsage(AttributeTargets.Property ,AllowMultiple = false,Inherited = true)]
    public class SerializeAttribute : Attribute
    {
        public bool SerializeType = false;
    }
}
