using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;

namespace hlwSerial
{
    public static class Serializer
    {
        private static readonly byte[] Size8 = new byte[8];
        private static readonly byte[] Size4 = new byte[4];
        private static readonly byte[] Size2 = new byte[2];
        private static readonly byte[] Size1 = new byte[1];



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
        public static void Write(this Stream stream,  object serializable)
        {
            if (serializable is Serializable s) s.PrepareSerialization();
            
            foreach (var propertyInfo in GetPropertiesWithAttribute(serializable.GetType()))
            {
                stream.Write(propertyInfo.PropertyType,propertyInfo.GetValue(serializable));
            }
        }

        private static void Write(this Stream stream, Type type, object value)
        {
            if (type == typeof(byte))
            {
                stream.WriteByte((byte)value);
            }
            else if (type == typeof(sbyte))
            {
                stream.WriteByte(unchecked((byte)(sbyte)value));
            }
            else if (type == typeof(short))
            {
                stream.Write(BitConverter.GetBytes((short)value), 0, 2);
            }
            else if (type == typeof(ushort))
            {
                stream.Write(BitConverter.GetBytes((ushort)value), 0, 2);
            }
            else if (type == typeof(int))
            {
                stream.Write(BitConverter.GetBytes((int)value), 0, 4);
            }
            else if (type == typeof(uint))
            {
                stream.Write(BitConverter.GetBytes((uint)value), 0, 4);
            }
            else if (type == typeof(long))
            {
                stream.Write(BitConverter.GetBytes((long)value), 0, 8);
            }
            else if (type == typeof(ulong))
            {
                stream.Write(BitConverter.GetBytes((ulong)value), 0, 8);
            }
            else if (type == typeof(float))
            {
                stream.Write(BitConverter.GetBytes((float)value), 0, 4);
            }
            else if (type == typeof(double))
            {
                stream.Write(BitConverter.GetBytes((double)value), 0, 8);
            }
            else if (type == typeof(bool))
            {
                stream.Write(BitConverter.GetBytes((bool)value), 0, 1);
            }
            else if (type == typeof(string))
            {
                var val = (string)value;
                stream.Write(BitConverter.GetBytes(val == null), 0, 1);
                if (val != null)
                {
                    var by = Encoding.UTF8.GetBytes(val);
                    stream.Write(BitConverter.GetBytes(by.Length), 0, 4);
                    stream.Write(by, 0, by.Length);
                }

            }
            else if (typeof(Serializable).IsAssignableFrom(type))
            {
                var val = value;
                stream.Write(BitConverter.GetBytes(val == null), 0, 1);
                if (val != null)
                {

                    stream.Write(val);

                }
            }
            else if (typeof(Array).IsAssignableFrom(type))
            {
                var val = value as Array;
                stream.Write(BitConverter.GetBytes(val == null), 0, 1);
                if (val != null)
                {
                    var ty = type.GetElementType();
                    if (ty != null)
                    {
                        stream.Write(typeof(int), val.Length);
                        foreach (var VARIABLE in val)
                        {
                            stream.Write(ty, VARIABLE);
                        }
                    }
                }
            }
        }


        public static object Read(this Stream stream, Type T)
        {
            var inst = Activator.CreateInstance(T);


            foreach (var propertyInfo in GetPropertiesWithAttribute(inst.GetType()))
            {

                propertyInfo.SetValue(inst,stream.ReadProperty(propertyInfo.PropertyType));

            }
            if (inst is Serializable i2)
                i2.AfterDeserialization();
            return inst;
        }

        public static T ReadProperty<T>(this Stream stream)
        {
            if (stream.ReadProperty(typeof(T)) is T val)
                return val;
            else return default(T);
            
        }

        public static object ReadProperty(this Stream stream, Type T)
        {
            if (T == typeof(byte))
            {
                var b = stream.ReadByte();
                if (b == -1) throw new IndexOutOfRangeException("Trying to read after the end of stream.");

                return(byte)b;
            }
            else if (T == typeof(sbyte))
            {
                var b = stream.ReadByte();
                if (b == -1) throw new IndexOutOfRangeException("Trying to read after the end of stream.");

                byte by = (byte)b;
                sbyte by2 = unchecked((sbyte)by);

                return by2;
            }
            else if (T == typeof(short))
            {
                stream.Read(Size2, 0, 2);
                return (short)BitConverter.ToInt16(Size2, 0);
            }
            else if (T == typeof(ushort))
            {
                stream.Read(Size2, 0, 2);
                return (ushort)BitConverter.ToUInt16(Size2, 0);
            }
            else if (T == typeof(int))
            {
                stream.Read(Size4, 0, 4);
                return (int)BitConverter.ToInt32(Size4, 0);
            }
            else if (T == typeof(uint))
            {
                stream.Read(Size4, 0, 4);
                return (uint)BitConverter.ToUInt32(Size4, 0);
            }
            else if (T == typeof(long))
            {
                stream.Read(Size8, 0, 8);
                return (long)BitConverter.ToInt64(Size8, 0);
            }
            else if (T == typeof(ulong))
            {
                stream.Read(Size8, 0, 8);
                return (ulong)BitConverter.ToUInt64(Size8, 0);
            }
            else if (T == typeof(float))
            {
                stream.Read(Size4, 0, 4);
                return (float)BitConverter.ToSingle(Size4, 0);
            }
            else if (T == typeof(double))
            {
                stream.Read(Size8, 0, 8);
                return (double)BitConverter.ToDouble(Size8, 0);
            }
            else if (T == typeof(bool))
            {
                stream.Read(Size1, 0, 1);
                return (bool)BitConverter.ToBoolean(Size1, 0);
            }
            else if (T == typeof(string))
            {
                stream.Read(Size1, 0, 1);
                var isNull = BitConverter.ToBoolean(Size1, 0);
                if (isNull) return null;
                else
                {
                    stream.Read(Size4, 0, 4);
                    var size = BitConverter.ToInt32(Size4, 0);
                    if (size <= 0) return "";
                    else
                    {
                        var by = new byte[size];
                        stream.Read(by, 0, size);
                        return Encoding.UTF8.GetString(by);
                    }
                }

            }
            else if (typeof(Serializable).IsAssignableFrom(T))
            {
                stream.Read(Size1, 0, 1);
                var isNull = BitConverter.ToBoolean(Size1, 0);
                if (isNull) return null;
                else
                {
                    return stream.Read(T);
                }
            }
            else if (typeof(Array).IsAssignableFrom(T))
            {
                
                var isNull = stream.ReadProperty<bool>();
                if (isNull) return null;
                else
                {
                    var ty = T.GetElementType();
                    if (ty == null) return null;

                    var array = Array.CreateInstance(T.GetElementType(), stream.ReadProperty<int>());
                    for (int i = 0; i < array.Length; i++)
                    {
                        array.SetValue(stream.ReadProperty(ty),i);
                    }
                    return array;
                }
            }
            else return null;
        }

        public static T Read<T>(this Stream stream) where T : class
        {
            var inst = stream.Read(typeof(T));
            


            
            return inst as T;
        }

        

    }
}
