using System;
using System.IO;
using System.Reflection;

namespace hlwSerial
{
    public class CustomPropertyInfo
    {

        public PropertyInfo PropertyInfo { get; set; }
        public bool SerializeType { get; set; }

        public CustomPropertyInfo(PropertyInfo info, bool SerializeType)
        {
            this.PropertyInfo = info;
            this.SerializeType = SerializeType;
        }
    }
}