using System;
using System.IO;
using System.Reflection;

namespace hlwSerial
{
    public class CustomPropertyInfo
    {

        public PropertyInfo PropertyInfo { get; set; }
        public bool SerializeType { get; set; }
        public bool SerializeElementsType{ get; set; }

        public CustomPropertyInfo(PropertyInfo info, bool SerializeType, bool SerializeElementsType)
        {
            this.PropertyInfo = info;
            this.SerializeType = SerializeType;
            this.SerializeElementsType = SerializeElementsType;
        }

    }

}