using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
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
        
        /// <summary>
        /// Gets or set the position of the underlying stream.
        /// </summary>
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


        private static Dictionary<Type,CustomPropertyInfo[]> infos = new Dictionary<Type,CustomPropertyInfo[]>();

        public static CustomPropertyInfo[] GetPropertiesWithAttribute(Type T)
        {
            if(infos.ContainsKey(T)) return infos[T];
            else
            {

                infos.Add(T, T.GetProperties().Where(a => a.IsDefined(typeof(SerializeAttribute))).Select(a=>
                {
                    SerializeAttribute attr = a.GetCustomAttribute<SerializeAttribute>();
                    bool isnullable = a.PropertyType.IsGenericType &&
                                      typeof(Nullable<>) == a.PropertyType.GetGenericTypeDefinition();
                    return new CustomPropertyInfo(a, attr.SerializeType,attr.SerializeElementsType,isnullable);
                }).ToArray());
                return infos[T];
            }
            
        }

        /// <summary>
        /// Writes the serializable object into the stream by writing each of its properties decorated with the SerializeAttribute attribute. If the object is a implemented type, it will be serialized as well.
        /// </summary>
        /// <param name="stream">The stream to write the bytes into</param>
        /// <param name="serializable">The object to serialize</param>
        public void Write(object serializable, bool SerializeType = false, bool SerializeElementsType = false, bool nullable = false)
        {
            if (serializable is ISerializable ss)
            {
                if (serializable is IPrepareSerialization s) s.PrepareSerialization();

                if (SerializeType)
                {
                    WriteProperty(serializable.GetType());
                }

                foreach (var customPropertyInfo in GetPropertiesWithAttribute(serializable.GetType()))
                {
                    this.WriteProperty(customPropertyInfo.PropertyInfo.GetValue(serializable),customPropertyInfo.SerializeType,customPropertyInfo.SerializeElementsType,customPropertyInfo.Nullable);
                }
            }
            else
            {
                WriteProperty(serializable,SerializeType,SerializeElementsType, nullable);
            }

            
        }


        private void WriteProperty(object value, bool SerializeType = false, bool SerializeElementsType = false,bool nullable = false)
        {
            //? Type writing if serializeType
            #region type writing
            var type = value == null ? null : value.GetType();
            if (SerializeType)
            {
                //todo rewrite this mess
                if(type!=null)
                    WriteProperty(type);
                if(type==null)WriteProperty(true);
                if (type == null) return;
            }
            else
            {

                if (value == null)
                {
                    WriteProperty(true);
                    return;
                }
                else if (value != null && nullable)
                {
                    WriteProperty(false);
                }
            }
            #endregion

            //? TYPE CHECKS AND WRITE
            //todo Remake this with a more "object oriented approach and less if else if

            //? Primary Types
            #region
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
            #endregion
            //? STRING
            #region
            else if (type == typeof(string))
            {
                var val = (string)value;
                if (!SerializeType && !nullable)
                    underlyingStream.Write(BitConverter.GetBytes(val == null), 0, 1);
                if (val != null)
                {
                    var by = Encoding.UTF8.GetBytes(val);
                    underlyingStream.Write(BitConverter.GetBytes(by.Length), 0, 4);
                    underlyingStream.Write(by, 0, by.Length);
                }

            }
            #endregion
            //? REFERENCE TYPES
            #region
            else if (typeof(Type).IsAssignableFrom(type))
            {
                var val = (Type)value;
                

                if (!SerializeType && !nullable)
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
                if (!SerializeType && !nullable)
                    underlyingStream.Write(BitConverter.GetBytes(val == null), 0, 1);
                if (val != null)
                {

                    this.Write(val,SerializeType);

                }
            }
            //?Check for nullable
            //x  Could be deleted?
            #region nullable
            /*
            else if (type.IsGenericType && typeof(Nullable<>) == type.GetGenericTypeDefinition())
            {
                

                var TT = Nullable.GetUnderlyingType(type);
                if (TT == typeof(byte))
                {
                    var val = (byte?)value;
                    if (!SerializeType && !nullable)
                        this.WriteProperty(!val.HasValue);
                    if(val.HasValue)
                        this.WriteProperty(val.Value);
                }
                else if (TT == typeof(sbyte))
                {
                    var val = (sbyte?)value;
                    if (!SerializeType && !nullable)
                        this.WriteProperty(!val.HasValue);
                    if (val.HasValue)
                        this.WriteProperty(val.Value);
                }
                else if (TT == typeof(short))
                {
                    var val = (short?)value;
                    if (!SerializeType && !nullable)
                        this.WriteProperty(!val.HasValue);
                    if (val.HasValue)
                        this.WriteProperty( val.Value);
                }
                else if (TT == typeof(ushort))
                {
                    var val = (ushort?)value;
                    if (!SerializeType && !nullable)
                        this.WriteProperty( !val.HasValue);
                    if (val.HasValue)
                        this.WriteProperty( val.Value);
                }
                else if (TT == typeof(int))
                {
                    var val = (int?)value;
                    if (!SerializeType && !nullable)
                        this.WriteProperty( !val.HasValue);
                    if (val.HasValue)
                        this.WriteProperty(val.Value);
                }
                else if (TT == typeof(uint))
                {
                    var val = (uint?)value;
                    if (!SerializeType && !nullable)
                        this.WriteProperty(!val.HasValue);
                    if (val.HasValue)
                        this.WriteProperty(val.Value);
                }
                else if (TT == typeof(long))
                {
                    var val = (long?)value;
                    if (!SerializeType && !nullable)
                        this.WriteProperty(!val.HasValue);
                    if (val.HasValue)
                        this.WriteProperty(val.Value);
                }
                else if (TT == typeof(ulong))
                {
                    var val = (ulong?)value;
                    if (!SerializeType && !nullable)
                        this.WriteProperty(!val.HasValue);
                    if (val.HasValue)
                        this.WriteProperty( val.Value);
                }
                else if (TT == typeof(float))
                {
                    var val = (float?)value;
                    if (!SerializeType && !nullable)
                        this.WriteProperty(!val.HasValue);
                    if (val.HasValue)
                        this.WriteProperty(val.Value);
                }
                else if (TT == typeof(double))
                {
                    var val = (double?)value;
                    if (!SerializeType && !nullable)
                        this.WriteProperty(!val.HasValue);
                    if (val.HasValue)
                        this.WriteProperty(val.Value);
                }
                else if (TT == typeof(bool))
                {
                    var val = (bool?)value;
                    if (!SerializeType && !nullable)
                        this.WriteProperty(!val.HasValue);
                    if (val.HasValue)
                        this.WriteProperty(val.Value);
                }
            }*/
            #endregion nullable
            //? COLLECTIONS
            #region collections
            else if (typeof(Array).IsAssignableFrom(type))
            {
                var val = value as Array;
                if (!SerializeType && !nullable)
                    underlyingStream.Write(BitConverter.GetBytes(val == null), 0, 1);
                if (val != null)
                {
                    var ty = type.GetElementType();
                    if (ty != null)
                    {
                        this.WriteProperty(val.Length);
                        foreach (var VARIABLE in val)
                        {
                            this.WriteProperty(VARIABLE,SerializeElementsType);
                        }
                    }
                }
            }
            else if (type.IsGenericType && typeof(List<>) == type.GetGenericTypeDefinition())
            {
                var val = value as IList;
                if (!SerializeType && !nullable)
                    this.WriteProperty(val == null);
                if (val != null)
                {

                    this.WriteProperty(val.Count);
                    var ty = type.GetGenericArguments()[0];
                    if (ty != null)
                    {
                        foreach (var VARIABLE in val)
                        {
                            this.WriteProperty(VARIABLE,SerializeElementsType);
                        }
                    }
                }

                
            }
            else if (type.IsGenericType && typeof(Dictionary<,>) == type.GetGenericTypeDefinition())
            {
                var val = value as IDictionary;
                if (!SerializeType && !nullable)
                    this.WriteProperty(val == null);
                if (val != null)
                {

                    this.WriteProperty(val.Count);
                    var ty1 = type.GetGenericArguments()[0];
                    var ty2 = type.GetGenericArguments()[1];


                        foreach (var VARIABLE in val.Keys)
                        {
                            this.WriteProperty(VARIABLE,SerializeElementsType);
                        }
                        foreach (var VARIABLE in val.Values)
                        {
                            this.WriteProperty(VARIABLE,SerializeElementsType);
                        }
                }
            }
#endregion collections
            #endregion reference types
        }


        public object Read( Type T, bool deserializeType = false, bool deserializeElementsType = false)
        {
            


            if (typeof(ISerializable).IsAssignableFrom(T))
            {
                object inst;
                if (deserializeType)
                {
                    var newType = (Type)this.ReadProperty(typeof(Type));
                    if (newType == null || !T.IsAssignableFrom(newType)) return null;
                    else
                    {
                        inst = Activator.CreateInstance(newType);
                    }
                }
                else
                {
                    inst = Activator.CreateInstance(T);
                }
                foreach (var customPropertyInfo in GetPropertiesWithAttribute(inst.GetType()))
                {

                    customPropertyInfo.PropertyInfo.SetValue(inst, this.ReadProperty(customPropertyInfo.PropertyInfo.PropertyType, customPropertyInfo.SerializeType, customPropertyInfo.SerializeElementsType));

                }
                if (inst is IAfterDeserialization i2)
                    i2.AfterDeserialization();
                return inst;
            }
            else
            {
                return ReadProperty(T,deserializeType,deserializeElementsType);
            }
            
            
        }

        public T ReadProperty<T>(bool DeserializeType = false, bool DeserializeElementType = false)
        {
            if (this.ReadProperty(typeof(T),DeserializeType,DeserializeElementType) is T val)
                return val;
            else return default(T);
            
        }

        public object ReadProperty(Type T, bool DeserializeType = false, bool DeserializeElementsType = false)
        {
            if (DeserializeType)
            {
                var newType = ReadProperty<Type>();
                if (newType == null || !T.IsAssignableFrom(newType)) return null;
                else T = newType;
            }
            //?PRIMARY TYPE
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
                bool isNull;
                if (!DeserializeType)
                {
                    underlyingStream.Read(Size1, 0, 1);
                    isNull = BitConverter.ToBoolean(Size1, 0);
                }
                else
                {
                    isNull = false;
                }

                
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
                bool isNull;
                if (!DeserializeType)
                {
                    underlyingStream.Read(Size1, 0, 1);
                    isNull = BitConverter.ToBoolean(Size1, 0);

                }
                else
                {
                    isNull = false;
                }
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
                bool isNull;
                if (!DeserializeType)
                {
                    underlyingStream.Read(Size1, 0, 1);
                    isNull = BitConverter.ToBoolean(Size1, 0);
                }
                else
                {
                    isNull = false;
                }
                if (isNull) return null;
                else
                {
                    return this.Read(T,DeserializeType);
                }
            }
            else if (T.IsGenericType && typeof(Nullable<>) == T.GetGenericTypeDefinition())
            {
                bool isNull;
                if (!DeserializeType)
                {
                    underlyingStream.Read(Size1, 0, 1);
                    isNull = BitConverter.ToBoolean(Size1, 0);
                }
                else
                {
                    isNull = false;
                }
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

                bool isNull;
                if (!DeserializeType)
                {
                    underlyingStream.Read(Size1, 0, 1);
                    isNull = BitConverter.ToBoolean(Size1, 0);
                }
                else
                {
                    isNull = false;
                }
                if (isNull) return null;
                else
                {
                    var ty = T.GetElementType();
                    if (ty == null) return null;

                    var array = Array.CreateInstance(T.GetElementType(), this.ReadProperty<int>());
                    for (int i = 0; i < array.Length; i++)
                    {
                        array.SetValue(this.ReadProperty(ty,DeserializeElementsType),i);
                    }
                    return array;
                }
            }
            else if (T.IsGenericType && typeof(List<>) == T.GetGenericTypeDefinition())
            {
                bool isNull;
                if (!DeserializeType)
                {
                    underlyingStream.Read(Size1, 0, 1);
                    isNull = BitConverter.ToBoolean(Size1, 0);
                }
                else
                {
                    isNull = false;
                }
                if (isNull) return null;
                else
                {
                    var ty = T.GetGenericArguments()[0];
                    var size = this.ReadProperty<int>();

                    var List = Activator.CreateInstance(T) as IList;
                    for (int i = 0; i < size; i++)
                    {
                        List.Add(this.ReadProperty(ty,DeserializeElementsType));
                    }
                    return List;
                }
            }
            else if (T.IsGenericType && typeof(Dictionary<,>) == T.GetGenericTypeDefinition())
            {
                bool isNull;
                if (!DeserializeType)
                {
                    underlyingStream.Read(Size1, 0, 1);
                    isNull = BitConverter.ToBoolean(Size1, 0);
                }
                else
                {
                    isNull = false;
                }
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
                        keys.Add(this.ReadProperty(ty1,DeserializeElementsType));
                    }

                    for (int i = 0; i < size; i++)
                    {
                        values.Add(this.ReadProperty(ty2,DeserializeElementsType));
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

        public T Read<T>(bool DeserializeType = false, bool DeserializeElementsType = false)
        {
            var inst = this.Read(typeof(T),DeserializeType,DeserializeElementsType);

            

            if (inst is T tt) return tt;
            else return default;
            
            
        }


        public void Dispose()
        {
            underlyingStream?.Dispose();
        }
    }
}
