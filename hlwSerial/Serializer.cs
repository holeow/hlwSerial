using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;

namespace hlwSerial
{
    public class Serializer : IDisposable
    {
        private readonly byte[] Size8 = new byte[8];
        private readonly byte[] Size4 = new byte[4];
        private readonly byte[] Size2 = new byte[2];
        private readonly byte[] Size1 = new byte[1];

        public Stream underlyingStream;

        public long Position
        {
            get
            {
                return underlyingStream.Position;
            }
            set
            {
                underlyingStream.Position = value;
            }
        }
        public Serializer(Stream stream)
        {
            this.underlyingStream = stream;
        }


        private static Dictionary<Type,PropertyInfo[]> infos = new Dictionary<Type,PropertyInfo[]>();

        private static PropertyInfo[] GetPropertiesWithAttribute(Type T)
        {
            if(infos.ContainsKey(T)) return infos[T];
            else
            {
                infos.Add(T, T.GetProperties().Where(a => a.IsDefined(typeof(SerializeAttribute))).ToArray());
                return infos[T];
            }
            
        }

        /// <summary>
        /// Writes the serializable object into the stream by writing each of its properties decorated with the SerializeAttribute attribute. 
        /// </summary>
        /// <param name="stream">The stream to write the bytes into</param>
        /// <param name="serializable">The object to serialize</param>
        public void Write(object serializable)
        {
            if (serializable is IPrepareSerialization s) s.PrepareSerialization();
            
            foreach (var propertyInfo in GetPropertiesWithAttribute(serializable.GetType()))
            {
                this.WriteProperty(propertyInfo.PropertyType,propertyInfo.GetValue(serializable));
            }
        }

        private void WriteProperty(Type type, object value)
        {
            if (type == typeof(byte))
            {
                underlyingStream.WriteByte((byte)value);
            }
            else if (type == typeof(sbyte))
            {
                underlyingStream.WriteByte(unchecked((byte)(sbyte)value));
            }
            else if (type == typeof(short))
            {
                underlyingStream.Write(BitConverter.GetBytes((short)value), 0, 2);
            }
            else if (type == typeof(ushort))
            {
                underlyingStream.Write(BitConverter.GetBytes((ushort)value), 0, 2);
            }
            else if (type == typeof(int))
            {
                underlyingStream.Write(BitConverter.GetBytes((int)value), 0, 4);
            }
            else if (type == typeof(uint))
            {
                underlyingStream.Write(BitConverter.GetBytes((uint)value), 0, 4);
            }
            else if (type == typeof(long))
            {
                underlyingStream.Write(BitConverter.GetBytes((long)value), 0, 8);
            }
            else if (type == typeof(ulong))
            {
                underlyingStream.Write(BitConverter.GetBytes((ulong)value), 0, 8);
            }
            else if (type == typeof(float))
            {
                underlyingStream.Write(BitConverter.GetBytes((float)value), 0, 4);
            }
            else if (type == typeof(double))
            {
                underlyingStream.Write(BitConverter.GetBytes((double)value), 0, 8);
            }
            else if (type == typeof(bool))
            {
                underlyingStream.Write(BitConverter.GetBytes((bool)value), 0, 1);
            }
            else if (type == typeof(string))
            {
                var val = (string)value;
                underlyingStream.Write(BitConverter.GetBytes(val == null), 0, 1);
                if (val != null)
                {
                    var by = Encoding.UTF8.GetBytes(val);
                    underlyingStream.Write(BitConverter.GetBytes(by.Length), 0, 4);
                    underlyingStream.Write(by, 0, by.Length);
                }

            }
            else if (type == typeof(Type))
            {
                var val = (Type)value;
                underlyingStream.Write(BitConverter.GetBytes(val == null), 0, 1);
                if (val != null)
                {
                    var str = val.GetShortTypeName();

                    var by = Encoding.UTF8.GetBytes(str);
                    underlyingStream.Write(BitConverter.GetBytes(by.Length), 0, 4);
                    underlyingStream.Write(by, 0, by.Length);
                }
                
                
            }
            else if (typeof(ISerializable).IsAssignableFrom(type))
            {
                var val = value;
                underlyingStream.Write(BitConverter.GetBytes(val == null), 0, 1);
                if (val != null)
                {

                    this.Write(val);

                }
            }
            else if (type.IsGenericType && typeof(Nullable<>) == type.GetGenericTypeDefinition())
            {
                var TT = Nullable.GetUnderlyingType(type);
                if (TT == typeof(byte))
                {
                    var val = (byte?)value;
                    this.WriteProperty(typeof(bool), !val.HasValue);
                    if(val.HasValue)
                        this.WriteProperty(TT, val.Value);
                }
                else if (TT == typeof(sbyte))
                {
                    var val = (sbyte?)value;
                    this.WriteProperty(typeof(bool), !val.HasValue);
                    if (val.HasValue)
                        this.WriteProperty(TT, val.Value);
                }
                else if (TT == typeof(short))
                {
                    var val = (short?)value;
                    this.WriteProperty(typeof(bool), !val.HasValue);
                    if (val.HasValue)
                        this.WriteProperty(TT, val.Value);
                }
                else if (TT == typeof(ushort))
                {
                    var val = (ushort?)value;
                    this.WriteProperty(typeof(bool), !val.HasValue);
                    if (val.HasValue)
                        this.WriteProperty(TT, val.Value);
                }
                else if (TT == typeof(int))
                {
                    var val = (int?)value;
                    this.WriteProperty(typeof(bool), !val.HasValue);
                    if (val.HasValue)
                        this.WriteProperty(TT, val.Value);
                }
                else if (TT == typeof(uint))
                {
                    var val = (uint?)value;
                    this.WriteProperty(typeof(bool), !val.HasValue);
                    if (val.HasValue)
                        this.WriteProperty(TT, val.Value);
                }
                else if (TT == typeof(long))
                {
                    var val = (long?)value;
                    this.WriteProperty(typeof(bool), !val.HasValue);
                    if (val.HasValue)
                        this.WriteProperty(TT, val.Value);
                }
                else if (TT == typeof(ulong))
                {
                    var val = (ulong?)value;
                    this.WriteProperty(typeof(bool), !val.HasValue);
                    if (val.HasValue)
                        this.WriteProperty(TT, val.Value);
                }
                else if (TT == typeof(float))
                {
                    var val = (float?)value;
                    this.WriteProperty(typeof(bool), !val.HasValue);
                    if (val.HasValue)
                        this.WriteProperty(TT, val.Value);
                }
                else if (TT == typeof(double))
                {
                    var val = (double?)value;
                    this.WriteProperty(typeof(bool), !val.HasValue);
                    if (val.HasValue)
                        this.WriteProperty(TT, val.Value);
                }
                else if (TT == typeof(bool))
                {
                    var val = (bool?)value;
                    this.WriteProperty(typeof(bool), !val.HasValue);
                    if (val.HasValue)
                        this.WriteProperty(TT, val.Value);
                }
            }
            else if (typeof(Array).IsAssignableFrom(type))
            {
                var val = value as Array;
                underlyingStream.Write(BitConverter.GetBytes(val == null), 0, 1);
                if (val != null)
                {
                    var ty = type.GetElementType();
                    if (ty != null)
                    {
                        this.WriteProperty(typeof(int), val.Length);
                        foreach (var VARIABLE in val)
                        {
                            this.WriteProperty(ty, VARIABLE);
                        }
                    }
                }
            }
            else if (type.IsGenericType && typeof(List<>) == type.GetGenericTypeDefinition())
            {
                var val = value as IList;

                this.WriteProperty(typeof(bool), val == null);
                if (val != null)
                {

                    this.WriteProperty(typeof(int), val.Count);
                    var ty = type.GetGenericArguments()[0];
                    if (ty != null)
                    {
                        foreach (var VARIABLE in val)
                        {
                            this.WriteProperty(ty, VARIABLE);
                        }
                    }
                }

                
            }
            else if (type.IsGenericType && typeof(Dictionary<,>) == type.GetGenericTypeDefinition())
            {
                var val = value as IDictionary;
                this.WriteProperty(typeof(bool), val == null);
                if (val != null)
                {

                    this.WriteProperty(typeof(int), val.Count);
                    var ty1 = type.GetGenericArguments()[0];
                    var ty2 = type.GetGenericArguments()[1];


                        foreach (var VARIABLE in val.Keys)
                        {
                            this.WriteProperty(ty1, VARIABLE);
                        }
                        foreach (var VARIABLE in val.Values)
                        {
                            this.WriteProperty(ty2, VARIABLE);
                        }
                }
            }
        }


        public object Read( Type T)
        {
            var inst = Activator.CreateInstance(T);


            foreach (var propertyInfo in GetPropertiesWithAttribute(inst.GetType()))
            {

                propertyInfo.SetValue(inst,this.ReadProperty(propertyInfo.PropertyType));

            }
            if (inst is IAfterDeserialization i2)
                i2.AfterDeserialization();
            return inst;
        }

        public T ReadProperty<T>()
        {
            if (this.ReadProperty(typeof(T)) is T val)
                return val;
            else return default(T);
            
        }

        public object ReadProperty(Type T)
        {
            if (T == typeof(byte))
            {
                var b = underlyingStream.ReadByte();
                if (b == -1) throw new IndexOutOfRangeException("Trying to read after the end of stream.");

                return(byte)b;
            }
            else if (T == typeof(sbyte))
            {
                var b = underlyingStream.ReadByte();
                if (b == -1) throw new IndexOutOfRangeException("Trying to read after the end of stream.");

                byte by = (byte)b;
                sbyte by2 = unchecked((sbyte)by);

                return by2;
            }
            else if (T == typeof(short))
            {
                underlyingStream.Read(Size2, 0, 2);
                return (short)BitConverter.ToInt16(Size2, 0);
            }
            else if (T == typeof(ushort))
            {
                underlyingStream.Read(Size2, 0, 2);
                return (ushort)BitConverter.ToUInt16(Size2, 0);
            }
            else if (T == typeof(int))
            {
                underlyingStream.Read(Size4, 0, 4);
                return (int)BitConverter.ToInt32(Size4, 0);
            }
            else if (T == typeof(uint))
            {
                underlyingStream.Read(Size4, 0, 4);
                return (uint)BitConverter.ToUInt32(Size4, 0);
            }
            else if (T == typeof(long))
            {
                underlyingStream.Read(Size8, 0, 8);
                return (long)BitConverter.ToInt64(Size8, 0);
            }
            else if (T == typeof(ulong))
            {
                underlyingStream.Read(Size8, 0, 8);
                return (ulong)BitConverter.ToUInt64(Size8, 0);
            }
            else if (T == typeof(float))
            {
                underlyingStream.Read(Size4, 0, 4);
                return (float)BitConverter.ToSingle(Size4, 0);
            }
            else if (T == typeof(double))
            {
                underlyingStream.Read(Size8, 0, 8);
                return (double)BitConverter.ToDouble(Size8, 0);
            }
            else if (T == typeof(bool))
            {
                underlyingStream.Read(Size1, 0, 1);
                return (bool)BitConverter.ToBoolean(Size1, 0);
            }
            else if (T == typeof(string))
            {
                underlyingStream.Read(Size1, 0, 1);
                var isNull = BitConverter.ToBoolean(Size1, 0);
                if (isNull) return null;
                else
                {
                    underlyingStream.Read(Size4, 0, 4);
                    var size = BitConverter.ToInt32(Size4, 0);
                    if (size <= 0) return "";
                    else
                    {
                        var by = new byte[size];
                        underlyingStream.Read(by, 0, size);
                        return Encoding.UTF8.GetString(by);
                    }
                }

            }
            else if (T == typeof(Type))
            {
                underlyingStream.Read(Size1, 0, 1);
                var isNull = BitConverter.ToBoolean(Size1, 0);
                if (isNull) return null;
                else
                {
                    underlyingStream.Read(Size4, 0, 4);
                    var size = BitConverter.ToInt32(Size4, 0);
                    if (size <= 0) return null;
                    else
                    {
                        var by = new byte[size];
                        underlyingStream.Read(by, 0, size);
                        return Type.GetType( Encoding.UTF8.GetString(by));
                    }
                }
            }
            else if (typeof(ISerializable).IsAssignableFrom(T))
            {
                underlyingStream.Read(Size1, 0, 1);
                var isNull = BitConverter.ToBoolean(Size1, 0);
                if (isNull) return null;
                else
                {
                    return this.Read(T);
                }
            }
            else if (T.IsGenericType && typeof(Nullable<>) == T.GetGenericTypeDefinition())
            {
                var isNull = this.ReadProperty<bool>();
                if (isNull) return null;
                else
                {
                    var TT = Nullable.GetUnderlyingType(T);
                    if (TT == typeof(byte))
                    {
                        return (byte?)this.ReadProperty<byte>();
                    }
                    else if (TT == typeof(sbyte))
                    {
                        return (sbyte?)this.ReadProperty<sbyte>();
                    }
                    else if (TT == typeof(short))
                    {
                        return (short?)this.ReadProperty<short>();
                    }
                    else if (TT == typeof(ushort))
                    {
                        return (ushort?)this.ReadProperty<ushort>();
                    }
                    else if (TT == typeof(int))
                    {
                        return (int?)this.ReadProperty<int>();
                    }
                    else if (TT == typeof(uint))
                    {
                        return (uint?)this.ReadProperty<uint>();
                    }
                    else if (TT == typeof(long))
                    {
                        return (long?)this.ReadProperty<long>();
                    }
                    else if (TT == typeof(ulong))
                    {
                        return (ulong?)this.ReadProperty<ulong>();
                    }
                    else if (TT == typeof(float))
                    {
                        return (float?)this.ReadProperty<float>();
                    }
                    else if (TT == typeof(double))
                    {
                        return (double?)this.ReadProperty<double>();
                    }
                    else if (TT == typeof(bool))
                    {
                        return (bool?)this.ReadProperty<bool>();
                    }
                    else return null;
                }
            }
            else if (typeof(Array).IsAssignableFrom(T))
            {
                
                var isNull = this.ReadProperty<bool>();
                if (isNull) return null;
                else
                {
                    var ty = T.GetElementType();
                    if (ty == null) return null;

                    var array = Array.CreateInstance(T.GetElementType(), this.ReadProperty<int>());
                    for (int i = 0; i < array.Length; i++)
                    {
                        array.SetValue(this.ReadProperty(ty),i);
                    }
                    return array;
                }
            }
            else if (T.IsGenericType && typeof(List<>) == T.GetGenericTypeDefinition())
            {
                var isNull = this.ReadProperty<bool>();
                if (isNull) return null;
                else
                {
                    var ty = T.GetGenericArguments()[0];
                    var size = this.ReadProperty<int>();

                    var List = Activator.CreateInstance(T) as IList;
                    for (int i = 0; i < size; i++)
                    {
                        List.Add(this.ReadProperty(ty));
                    }
                    return List;
                }
            }
            else if (T.IsGenericType && typeof(Dictionary<,>) == T.GetGenericTypeDefinition())
            {
                var isNull = this.ReadProperty<bool>();
                if (isNull) return null;
                else
                {
                    var ty1 = T.GetGenericArguments()[0];
                    var ty2 = T.GetGenericArguments()[1];

                    var Dict = Activator.CreateInstance(T) as IDictionary;
                    List<object> keys = new List<object>();
                    List<object> values = new List<object>();
                    var size = this.ReadProperty<int>();
                    for (int i = 0; i < size; i++)
                    {
                        keys.Add(this.ReadProperty(ty1));
                    }

                    for (int i = 0; i < size; i++)
                    {
                        values.Add(this.ReadProperty(ty2));
                    }
                    for (int i = 0; i < size; i++)
                    {
                        Dict.Add(keys[i], values[i]);
                    }

                    return Dict;
                }
            }
            else return null;
        }

        public T Read<T>()
        {
            var inst = this.Read(typeof(T));


            if (inst is T tt) return tt;
            else return default;
            
            
        }


        public void Dispose()
        {
            underlyingStream?.Dispose();
        }
    }
}
