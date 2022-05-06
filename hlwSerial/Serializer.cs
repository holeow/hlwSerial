using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
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
        /// 
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="serializable"></param>
        public static void Write(this Stream stream,  object serializable)
        {
            if (serializable is hlwSerializable s) s.PrepareSerialization();
            



            foreach (var propertyInfo in GetPropertiesWithAttribute(serializable.GetType()))
            {



                if (propertyInfo.PropertyType == typeof(byte))
                {
                    stream.WriteByte((byte) propertyInfo.GetValue(serializable));
                }
                else if (propertyInfo.PropertyType == typeof(sbyte))
                {
                    stream.WriteByte(unchecked((byte)(sbyte)propertyInfo.GetValue(serializable)));
                }
                else if (propertyInfo.PropertyType == typeof(short))
                {
                    stream.Write(BitConverter.GetBytes((short)propertyInfo.GetValue(serializable)), 0, 2);
                }
                else if (propertyInfo.PropertyType == typeof(ushort))
                {
                    stream.Write(BitConverter.GetBytes((ushort)propertyInfo.GetValue(serializable)), 0, 2);
                }
                else if (propertyInfo.PropertyType == typeof(int))
                {
                    stream.Write(BitConverter.GetBytes((int) propertyInfo.GetValue(serializable)), 0, 4);
                }
                else if (propertyInfo.PropertyType == typeof(uint))
                {
                    stream.Write(BitConverter.GetBytes((uint)propertyInfo.GetValue(serializable)), 0, 4);
                }
                else if (propertyInfo.PropertyType == typeof(long))
                {
                    stream.Write(BitConverter.GetBytes((long)propertyInfo.GetValue(serializable)), 0, 8);
                }
                else if (propertyInfo.PropertyType == typeof(ulong))
                {
                    stream.Write(BitConverter.GetBytes((ulong)propertyInfo.GetValue(serializable)), 0, 8);
                }
                else if (propertyInfo.PropertyType == typeof(float))
                {
                    stream.Write(BitConverter.GetBytes((float)propertyInfo.GetValue(serializable)), 0, 4);
                }
                else if (propertyInfo.PropertyType == typeof(double))
                {
                    stream.Write(BitConverter.GetBytes((double)propertyInfo.GetValue(serializable)), 0, 8);
                }
                else if (propertyInfo.PropertyType == typeof(bool))
                {
                    stream.Write(BitConverter.GetBytes((bool)propertyInfo.GetValue(serializable)), 0, 1);
                }
                else if (propertyInfo.PropertyType == typeof(string))
                {
                    var value = (string)propertyInfo.GetValue(serializable);
                    stream.Write(BitConverter.GetBytes(value == null), 0, 1);
                    if (value != null)
                    {
                        var by = Encoding.UTF8.GetBytes(value);
                        stream.Write(BitConverter.GetBytes(by.Length), 0, 4);
                        stream.Write(by, 0, by.Length);
                    }

                }
                else if (typeof(hlwSerializable).IsAssignableFrom(propertyInfo.PropertyType))
                {
                    var value = propertyInfo.GetValue(serializable);
                    stream.Write(BitConverter.GetBytes(value == null), 0, 1);
                    if (value != null)
                    {
                        
                        stream.Write(value);
                        
                    }
                }

            }



        }

        public static object Read(this Stream stream, Type T)
        {
            var inst = Activator.CreateInstance(T);



            foreach (var propertyInfo in GetPropertiesWithAttribute(inst.GetType()))
            {

                if (propertyInfo.PropertyType == typeof(byte))
                {
                    var b = stream.ReadByte();
                    if (b == -1) throw new IndexOutOfRangeException("Trying to read after the end of stream.");

                    propertyInfo.SetValue(inst, (byte)b);
                }
                else if (propertyInfo.PropertyType == typeof(sbyte))
                {
                    var b = stream.ReadByte();
                    if (b == -1) throw new IndexOutOfRangeException("Trying to read after the end of stream.");

                    byte by = (byte)b;
                    sbyte by2 = unchecked((sbyte)by);

                    propertyInfo.SetValue(inst, by2);
                }
                else if (propertyInfo.PropertyType == typeof(short))
                {
                    stream.Read(Size2, 0, 2);
                    propertyInfo.SetValue(inst, (short)BitConverter.ToInt16(Size2, 0));
                }
                else if (propertyInfo.PropertyType == typeof(ushort))
                {
                    stream.Read(Size2, 0, 2);
                    propertyInfo.SetValue(inst, (ushort)BitConverter.ToUInt16(Size2, 0));
                }
                else if (propertyInfo.PropertyType == typeof(int))
                {
                    stream.Read(Size4, 0, 4);
                    propertyInfo.SetValue(inst, (int)BitConverter.ToInt32(Size4, 0));
                }
                else if (propertyInfo.PropertyType == typeof(uint))
                {
                    stream.Read(Size4, 0, 4);
                    propertyInfo.SetValue(inst, (uint)BitConverter.ToUInt32(Size4, 0));
                }
                else if (propertyInfo.PropertyType == typeof(long))
                {
                    stream.Read(Size8, 0, 8);
                    propertyInfo.SetValue(inst, (long)BitConverter.ToInt64(Size8, 0));
                }
                else if (propertyInfo.PropertyType == typeof(ulong))
                {
                    stream.Read(Size8, 0, 8);
                    propertyInfo.SetValue(inst, (ulong)BitConverter.ToUInt64(Size8, 0));
                }



                else if (propertyInfo.PropertyType == typeof(float))
                {
                    stream.Read(Size4, 0, 4);
                    propertyInfo.SetValue(inst, (float)BitConverter.ToSingle(Size4, 0));
                }
                else if (propertyInfo.PropertyType == typeof(double))
                {
                    stream.Read(Size8, 0, 8);
                    propertyInfo.SetValue(inst, (double)BitConverter.ToDouble(Size8, 0));
                }
                else if (propertyInfo.PropertyType == typeof(bool))
                {
                    stream.Read(Size1, 0, 1);
                    propertyInfo.SetValue(inst, (bool)BitConverter.ToBoolean(Size1, 0));
                }
                else if (propertyInfo.PropertyType == typeof(string))
                {
                    stream.Read(Size1, 0, 1);
                    var isNull = BitConverter.ToBoolean(Size1, 0);
                    if(isNull)propertyInfo.SetValue(inst,null);
                    else
                    {
                        stream.Read(Size4, 0, 4);
                        var size = BitConverter.ToInt32(Size4, 0);
                        if(size<=0)propertyInfo.SetValue(inst,"");
                        else
                        {
                            var by = new byte[size];
                            stream.Read(by, 0, size);
                            propertyInfo.SetValue(inst,Encoding.UTF8.GetString(by));
                        }
                    }

                }
                else if (typeof(hlwSerializable).IsAssignableFrom(propertyInfo.PropertyType))
                {
                    stream.Read(Size1, 0, 1);
                    var isNull = BitConverter.ToBoolean(Size1, 0);
                    if (isNull) propertyInfo.SetValue(inst, null);
                    else
                    {
                        propertyInfo.SetValue(inst,stream.Read(propertyInfo.GetType()));
                    }
                }

            }
            if (inst is hlwSerializable i2)
                i2.AfterDeserialization();
            return inst;
        }

        public static T Read<T>(this Stream stream) where T : class
        {
            var inst = stream.Read(typeof(T));
            


            
            return inst as T;
        }

        

    }
}
