using System;
using System.IO;
using System.Reflection;

namespace hlwSerial
{
    public class CustomPropertyInfo
    {
        //? properties
        public PropertyInfo PropertyInfo { get; set; }
        public bool SerializeType { get; set; }
        public bool SerializeElementsType{ get; set; }
        public bool Nullable { get; set; }

        //ctor 
        public CustomPropertyInfo(PropertyInfo info, bool SerializeType, bool SerializeElementsType,bool nullable)
        {
            this.PropertyInfo = info;
            this.SerializeType = SerializeType;
            this.SerializeElementsType = SerializeElementsType;
            this.Nullable = nullable;
        }

    }

}