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
        //Todo Add support for endianness. (In progress)
        //Todo Make able to switch between a "CPU Cycle" or a "Memory" Serializer.
        public Serializer(Stream stream,  Endianness endianess = Endianness.Auto)
        {
            this.underlyingStream = stream;
            this.Endianess = endianess;
            this.ReversedEndianess = (BitConverter.IsLittleEndian && endianess == Endianness.BigEndian) ||
                                     (!BitConverter.IsLittleEndian && endianess == Endianness.LittleEndian);
        }

        //? Work byte arrays
        #region
        private readonly byte[] Size8 = new byte[8];
        private readonly byte[] Size4 = new byte[4];
        private readonly byte[] Size2 = new byte[2];
        private readonly byte[] Size1 = new byte[1];

        
        #endregion

        //?=================
        //?Properties and fields
        #region
        /// <summary>
        /// The stream used by the serializer.
        /// </summary>
        public readonly Stream underlyingStream;
        
        /// <summary>
        /// Gets or set the position in the underlying stream.
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
        
        private Stack<object> _stack = new Stack<object>();

        public int MaxRecursivity = 100;

        public readonly Endianness Endianess;
        public readonly bool ReversedEndianess;
        #endregion


        //? PropertyInfos and type handling.
        #region
        private static Dictionary<Type,CustomPropertyInfo[]> infos = new Dictionary<Type,CustomPropertyInfo[]>();

        private static Dictionary<Type, byte[]> typeStrings = new Dictionary<Type, byte[]>();

        private static byte[] GetTypeString(Type ty)
        {
            if(typeStrings.ContainsKey(ty)) return typeStrings[ty];
            else
            {
                var str = ty.GetShortTypeName();
                var strby = Encoding.UTF8.GetBytes(str);

                typeStrings.Add(ty, strby);
                return strby;
            }
        }

        /// <summary>
        /// Return the properties of asked type that are decorated with the [SerializeAttribute]
        /// </summary>
        /// <param name="T">The type from which you want the decorated properties.</param>
        /// <returns></returns>
        private static CustomPropertyInfo[] GetPropertiesWithAttribute(Type T)
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
        #endregion


        //?===============
        //?WRITING
        //todo add Datetime
        #region


        /// <summary>
        /// Writes the serializable object into the underlying stream by writing each of its properties decorated with the SerializeAttribute attribute. If the object is an implemented type, it will be serialized as is.
        /// </summary>
        /// <param name="serializable">The entity to serialize</param>
        /// <param name="SerializeType">Should you serialize the entity's type and support polymorphism? If used, DeserializeType must be used when deserializing the object</param>
        /// <param name="SerializeElementsType">If the serialized type is an array or collection, do you want to serialize the type of the elements it contains and support polymorphism? Will be ignored if elements type is a sealed primary type.</param>
        /// <param name="nullable">Has to be true if the type serialized is a nullable<T> like an int? is.</param>
        public void Write(object serializable, bool SerializeType = false, bool SerializeElementsType = false, bool nullable = false)
        {
            int RC = 0;
            _stack.Clear();
            WriteObject(serializable,ref RC,SerializeType,SerializeElementsType,nullable);
        }



        /// <summary>
        /// Writes the serializable object into the stream by writing each of its properties decorated with the SerializeAttribute attribute. If the object is an implemented type, it will be serialized as well.
        /// </summary>
        /// <param name="serializable">The object to serialize</param>
        private void WriteObject(object serializable,ref int RecursivityCount, bool SerializeType = false, bool SerializeElementsType = false, bool nullable = false)
        {
            unchecked { RecursivityCount++; }
            if (RecursivityCount > MaxRecursivity)
            {
                throw new SerializationTooMuchRecursivityException(
                    $"Max Recursivity in Writing Object have been reached.Actual recursivity{RecursivityCount}. You change change MaxRecursivity inside the serializer class.");
            }

            if (serializable is ISerializable ss)//!Only ISerializable properties will have their decorated properties serialized
            {
                _stack.Push(ss);

                if (serializable is IPrepareSerialization s) s.PrepareSerialization();

                if (SerializeType)
                {
                    WriteProperty(serializable.GetType(), ref RecursivityCount);
                }

                foreach (var customPropertyInfo in GetPropertiesWithAttribute(serializable.GetType()))
                {
        
                    this.WriteProperty(customPropertyInfo.PropertyInfo.GetValue(serializable), ref RecursivityCount, customPropertyInfo.SerializeType,customPropertyInfo.SerializeElementsType,customPropertyInfo.Nullable);
                }
                _stack.Pop();
            }
            else
            {//if it's not an ISerializable, serialize it as a property.
                WriteProperty(serializable, ref RecursivityCount, SerializeType,SerializeElementsType, nullable);
            }

           unchecked { RecursivityCount--; }
            
        }


        private void WriteProperty(object value,ref int RecursivityCount, bool SerializeType = false, bool SerializeElementsType = false,bool nullable = false)
        {
            //? Type writing if serializeType
            #region type writing
            var type = value == null ? null : value.GetType();
            if (SerializeType)
            {
                //todo rewrite this mess
                if(type!=null)
                    WriteProperty(type, ref RecursivityCount);
                if(type==null)WriteProperty(true, ref RecursivityCount);
                if (type == null) return;
            }
            else
            {

                if (value == null)
                {
                    WriteProperty(true, ref RecursivityCount);
                    return;
                }
                else if (value != null && nullable)
                {
                    WriteProperty(false, ref RecursivityCount);
                }
            }
            #endregion

            
            //todo Remake this with a more "object oriented" approach and less if else if

            //? Primary Types
            #region
            if (type == typeof(byte))
            {
                WriteProperty((byte)value, ref RecursivityCount,SerializeType,SerializeElementsType,nullable);
            }
            else if (type == typeof(sbyte))
            {
                WriteProperty((sbyte)value, ref RecursivityCount, SerializeType, SerializeElementsType, nullable);
            }
            else if (type == typeof(short))
            {
                WriteProperty((short)value, ref RecursivityCount, SerializeType, SerializeElementsType, nullable);
            }
            else if (type == typeof(ushort))
            {
                WriteProperty((ushort)value, ref RecursivityCount, SerializeType, SerializeElementsType, nullable);
            }
            else if (type == typeof(int))
            {
                WriteProperty((int)value, ref RecursivityCount, SerializeType, SerializeElementsType, nullable);
            }
            else if (type == typeof(uint))
            {
                WriteProperty((uint)value, ref RecursivityCount, SerializeType, SerializeElementsType, nullable);
            }
            else if (type == typeof(long))
            {
                WriteProperty((long)value, ref RecursivityCount, SerializeType, SerializeElementsType, nullable);
            }
            else if (type == typeof(ulong))
            {
                WriteProperty((ulong)value, ref RecursivityCount, SerializeType, SerializeElementsType, nullable);
            }
            else if (type == typeof(float))
            {
                WriteProperty((float)value, ref RecursivityCount, SerializeType, SerializeElementsType, nullable);
            }
            else if (type == typeof(double))
            {
                WriteProperty((double)value, ref RecursivityCount, SerializeType, SerializeElementsType, nullable);
            }
            else if (type == typeof(bool))
            {
                WriteProperty((bool)value, ref RecursivityCount, SerializeType, SerializeElementsType, nullable);
            }
            #endregion
            //? STRING
            #region
            else if (type == typeof(string))
            {
                WriteProperty((string)value, ref RecursivityCount, SerializeType, SerializeElementsType, nullable);

            }
            #endregion
            //? Type and ISerializable
            #region
            else if (typeof(Type).IsAssignableFrom(type))
            {

                WriteProperty((Type)value, ref RecursivityCount, SerializeType, SerializeElementsType, nullable);


            }
            else if (typeof(ISerializable).IsAssignableFrom(type))
            {
                WriteProperty((ISerializable)value, ref RecursivityCount, SerializeType, SerializeElementsType, nullable);
            }
            #endregion reference types
            //?Check for nullable
            //x  Could be deleted?
            #region nullable
            
            else if (type.IsGenericType && typeof(Nullable<>) == type.GetGenericTypeDefinition())
            {

                //!Usually : int? hello = 5 // or int? hello = null // Won't be typeof(int?), they will either be int or null types. 
                //!But we didn't check for int?[] and List<int?> serialization
                //todo Make a lot of tests to make sure there is never a serialization that asks for nullable<T> type.
                throw new NotImplementedException($"Trying to serialize a nullable<T> object !! type = {type.GetShortTypeName(true)}");
                /*
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
                */
            }
            #endregion nullable
            //? COLLECTIONS
            #region collections
            else if (typeof(Array).IsAssignableFrom(type))
            {
                WriteProperty((Array)value, ref RecursivityCount, SerializeType, SerializeElementsType, nullable);
            }
            else if (type.IsGenericType && typeof(List<>) == type.GetGenericTypeDefinition())
            {
                WriteProperty((IList)value, ref RecursivityCount, SerializeType, SerializeElementsType, nullable);

            }
            else if (type.IsGenericType && typeof(Dictionary<,>) == type.GetGenericTypeDefinition())
            {
                WriteProperty((IDictionary)value, ref RecursivityCount, SerializeType, SerializeElementsType, nullable);
            }
#endregion collections
            
        }


        //? Each of writing Methods

        #region
        //?byte
        private void WriteProperty(byte value, ref int RecursivityCount, bool SerializeType = false,
            bool SerializeElementsType = false, bool nullable = false)
        {
            underlyingStream.WriteByte(value);
        }
        private void WriteProperty(sbyte value, ref int RecursivityCount, bool SerializeType = false,
            bool SerializeElementsType = false, bool nullable = false)
        {
            underlyingStream.WriteByte(unchecked((byte)value));
        }
        //?short
        private void WriteProperty(short value, ref int RecursivityCount, bool SerializeType = false,
            bool SerializeElementsType = false, bool nullable = false)
        {
            if (ReversedEndianess)
            {
                var bytes = BitConverter.GetBytes((short) value);
                Size2[0] = bytes[1];
                Size2[1] = bytes[0];
                underlyingStream.Write(Size2, 0, 2);
            }
            else
            {
                underlyingStream.Write(BitConverter.GetBytes((short)value), 0, 2);
            }
        }
        private void WriteProperty(ushort value, ref int RecursivityCount, bool SerializeType = false,
            bool SerializeElementsType = false, bool nullable = false)
        {
            if (ReversedEndianess)
            {
                var bytes = BitConverter.GetBytes((ushort)value);
                Size2[0] = bytes[1];
                Size2[1] = bytes[0];
                underlyingStream.Write(Size2, 0, 2);
            }
            else
            {
                underlyingStream.Write(BitConverter.GetBytes((ushort)value), 0, 2);
            }
        }
        //?int
        private void WriteProperty(int value, ref int RecursivityCount, bool SerializeType = false,
            bool SerializeElementsType = false, bool nullable = false)
        {
            if (ReversedEndianess)
            {
                var bytes = BitConverter.GetBytes((int)value);
                Size4[0] = bytes[3];
                Size4[1] = bytes[2];
                Size4[2] = bytes[1];
                Size4[3] = bytes[0];
                underlyingStream.Write(Size4, 0, 4);
            }
            else
            {
                underlyingStream.Write(BitConverter.GetBytes((int)value), 0, 4);
            }
        }
        private void WriteProperty(uint value, ref int RecursivityCount, bool SerializeType = false,
            bool SerializeElementsType = false, bool nullable = false)
        {
            if (ReversedEndianess)
            {
                var bytes = BitConverter.GetBytes((uint)value);
                Size4[0] = bytes[3];
                Size4[1] = bytes[2];
                Size4[2] = bytes[1];
                Size4[3] = bytes[0];
                underlyingStream.Write(Size4, 0, 4);
            }
            else
            {
                underlyingStream.Write(BitConverter.GetBytes((uint)value), 0, 4);
            }
        }
        //?long
        private void WriteProperty(long value, ref int RecursivityCount, bool SerializeType = false,
            bool SerializeElementsType = false, bool nullable = false)
        {
            if (ReversedEndianess)
            {
                var bytes = BitConverter.GetBytes((long)value);
                Size8[0] = bytes[7];
                Size8[1] = bytes[6];
                Size8[2] = bytes[5];
                Size8[3] = bytes[4];
                Size8[4] = bytes[3];
                Size8[5] = bytes[2];
                Size8[6] = bytes[1];
                Size8[7] = bytes[0];
                underlyingStream.Write(Size8, 0, 8);
            }
            else
            {
                underlyingStream.Write(BitConverter.GetBytes((long)value), 0, 8);
            }
        }
        private void WriteProperty(ulong value, ref int RecursivityCount, bool SerializeType = false,
            bool SerializeElementsType = false, bool nullable = false)
        {
            if (ReversedEndianess)
            {
                var bytes = BitConverter.GetBytes((ulong)value);
                Size8[0] = bytes[7];
                Size8[1] = bytes[6];
                Size8[2] = bytes[5];
                Size8[3] = bytes[4];
                Size8[4] = bytes[3];
                Size8[5] = bytes[2];
                Size8[6] = bytes[1];
                Size8[7] = bytes[0];
                underlyingStream.Write(Size8, 0, 8);
            }
            else
            {
                underlyingStream.Write(BitConverter.GetBytes((ulong)value), 0, 8);
            }
        }
        //?float
        private void WriteProperty(float value, ref int RecursivityCount, bool SerializeType = false,
            bool SerializeElementsType = false, bool nullable = false)
        {
            if (ReversedEndianess)
            {
                var bytes = BitConverter.GetBytes((float)value);
                Size4[0] = bytes[3];
                Size4[1] = bytes[2];
                Size4[2] = bytes[1];
                Size4[3] = bytes[0];
                underlyingStream.Write(Size4, 0, 4);
            }
            else
            {
                underlyingStream.Write(BitConverter.GetBytes((float)value), 0, 4);
            }
        }
        private void WriteProperty(double value, ref int RecursivityCount, bool SerializeType = false,
            bool SerializeElementsType = false, bool nullable = false)
        {
            if (ReversedEndianess)
            {
                var bytes = BitConverter.GetBytes((double)value);
                Size8[0] = bytes[7];
                Size8[1] = bytes[6];
                Size8[2] = bytes[5];
                Size8[3] = bytes[4];
                Size8[4] = bytes[3];
                Size8[5] = bytes[2];
                Size8[6] = bytes[1];
                Size8[7] = bytes[0];
                underlyingStream.Write(Size8, 0, 8);
            }
            else
            {
                underlyingStream.Write(BitConverter.GetBytes((double)value), 0, 8);
            }
        }
        //?bool
        private void WriteProperty(bool value, ref int RecursivityCount, bool SerializeType = false,
            bool SerializeElementsType = false, bool nullable = false)
        {
            underlyingStream.WriteByte(BitConverter.GetBytes(value)[0]);
        }
        //?string
        private void WriteProperty(string value, ref int RecursivityCount, bool SerializeType = false,
            bool SerializeElementsType = false, bool nullable = false)
        {
            if (!SerializeType && !nullable)
                WriteProperty(value == null, ref RecursivityCount);
            if (value != null)
            {
                var by = Encoding.UTF8.GetBytes(value);
                WriteProperty(by.Length, ref RecursivityCount);
                underlyingStream.Write(by, 0, by.Length);
            }
        }
        //?Type
        private void WriteProperty(Type value, ref int RecursivityCount, bool SerializeType = false,
            bool SerializeElementsType = false, bool nullable = false)
        {
            if (!SerializeType && !nullable)
                WriteProperty(value == null, ref RecursivityCount);
            if (value != null)
            {
                var by = GetTypeString(value);
                WriteProperty(by.Length, ref RecursivityCount);
                underlyingStream.Write(by, 0, by.Length);
            }
        }
        //?ISerializable
        private void WriteProperty(ISerializable value, ref int RecursivityCount, bool SerializeType = false,
            bool SerializeElementsType = false, bool nullable = false)
        {
            var val = value;
            if (!SerializeType && !nullable)
                underlyingStream.Write(BitConverter.GetBytes(val == null), 0, 1);
            if (val != null)
            {
                if (_stack.Contains(val))
                {
                    throw new SerializationContainsSameObjectTwiceInTheStack(
                        $"Object is serialized inside itself or inside one object it contains.");
                }
                this.WriteObject(val, ref RecursivityCount, SerializeType);

            }
        }
        //?Arrays
        private void WriteProperty(Array value, ref int RecursivityCount, bool SerializeType = false,
            bool SerializeElementsType = false, bool nullable = false)
        {
            if (!SerializeType && !nullable)
                underlyingStream.Write(BitConverter.GetBytes(value == null), 0, 1);

            if (value != null)
            {
                var type = value.GetType();
                if (_stack.Contains(value))
                {
                    throw new SerializationContainsSameObjectTwiceInTheStack(
                        $"Object type {type} is serialized inside itself or inside one object it contains.");
                }
                _stack.Push(value);
                var ty = type.GetElementType();
                if (ty != null)
                {
                    this.WriteProperty((byte)value.Rank,ref RecursivityCount);//Handle multiDimensionnalArray
                    for (int i = 0; i < value.Rank; i++)//!Writing each length of each rank.
                    {
                        WriteProperty(value.GetLength(i), ref RecursivityCount);
                    }

                    if (typeof(Array).IsAssignableFrom(ty))//Handle JaggedArray 
                    {//x Potential loss of speed as jagged arrays aren't that common and will be tested at first every time
                        foreach (Array variable in value)
                        {
                            this.WriteProperty(variable, ref RecursivityCount, false, SerializeElementsType);
                        }
                    }
                    else if (ty == typeof(byte))
                    {
                        if(value.Rank==1) underlyingStream.Write(value as byte[], 0, value.Length);
                        else
                        {
                            var val = GetArrayAsBytes(value, 1);
                            underlyingStream.Write(val, 0, val.Length);
                        }
                    }
                    else if ( ty == typeof(sbyte) || ty == typeof(bool))
                    {
                        var val = GetArrayAsBytes(value, 1);
                        underlyingStream.Write(val, 0, val.Length);
                    }
                    else if (ty == typeof(short) || ty == typeof(ushort))
                    {
                        var val = GetArrayAsBytes(value, 2);
                        underlyingStream.Write(val, 0, val.Length);
                    }
                    else if (ty == typeof(int) || ty == typeof(uint) || ty == typeof(float))
                    {
                        var val = GetArrayAsBytes(value, 4);
                        underlyingStream.Write(val, 0, val.Length);
                    }
                    else if (ty == typeof(long) || ty == typeof(ulong) || ty == typeof(double))
                    {
                        var val = GetArrayAsBytes(value, 8);
                        underlyingStream.Write(val, 0, val.Length);
                    }
                    else if (ty == typeof(string))
                    {
                        foreach (string str in value)
                        {
                            this.WriteProperty(str, ref RecursivityCount);
                        }
                    }
                    else if (typeof(Type).IsAssignableFrom(ty))
                    {
                        foreach (Type t in value)
                        {
                            this.WriteProperty(t,ref RecursivityCount);
                        }
                    }
                    else if (typeof(ISerializable).IsAssignableFrom(ty))
                    {
                        foreach (ISerializable ser in value)
                        {
                            this.WriteProperty(ser,ref RecursivityCount,SerializeElementsType);
                        }
                    }
                    else if (ty.IsGenericType && typeof(Nullable<>) == ty.GetGenericTypeDefinition())
                    {
                        foreach (object obj in value)
                        {
                            this.WriteProperty(obj, ref RecursivityCount, SerializeElementsType, false, true);
                        }
                    }
                    //todo Add direct calls to collection writeProperty
                    else foreach (object VARIABLE in value)
                    {
                        this.WriteProperty(VARIABLE, ref RecursivityCount, SerializeElementsType);
                    }
                }

                _stack.Pop();
            }
        }

        //?Lists
        //!IList must in fact be a generic List<T> !!
        private void WriteProperty(IList value, ref int RecursivityCount, bool SerializeType = false,
            bool SerializeElementsType = false, bool nullable = false)
        {
            if (!SerializeType && !nullable)
                this.WriteProperty(value == null, ref RecursivityCount);
            if (value != null)
            {
                if (_stack.Contains(value))
                {
                    throw new SerializationContainsSameObjectTwiceInTheStack(
                        $"Object type {value.GetType()} is serialized inside itself or inside one object it contains.");
                }
                _stack.Push(value);
                this.WriteProperty(value.Count, ref RecursivityCount);
                var ty = value.GetType().GetGenericArguments()[0];//!Will fail if the list isn't generic
                if (ty != null)
                {

                    if (typeof(Array).IsAssignableFrom(ty))
                    {//x Potential loss of speed as arrays aren't that common and will be tested at first every time
                        foreach (Array variable in value)
                        {
                            this.WriteProperty(variable, ref RecursivityCount, false, SerializeElementsType);
                        }
                    }
                    else if (ty == typeof(byte))
                    {
                        underlyingStream.Write((value as List<byte>).ToArray(), 0, value.Count);
                        
                    }
                    else if (ty == typeof(sbyte))
                    {
                        var val = GetArrayAsBytes((value as List<sbyte>).ToArray(), 1);
                        underlyingStream.Write(val, 0, val.Length);
                    }
                    else if (ty == typeof(bool))
                    {
                        var val = GetArrayAsBytes((value as List<bool>).ToArray(), 1);
                        underlyingStream.Write(val, 0, val.Length);
                    }
                    else if (ty == typeof(short))
                    {
                        var val = GetArrayAsBytes((value as List<short>).ToArray(), 2);
                        underlyingStream.Write(val, 0, val.Length);
                    }
                    else if (ty == typeof(ushort))
                    {
                        var val = GetArrayAsBytes((value as List<ushort>).ToArray(), 2);
                        underlyingStream.Write(val, 0, val.Length);
                    }
                    else if (ty == typeof(int) )
                    {
                        var val = GetArrayAsBytes((value as List<int>).ToArray(), 4);
                        underlyingStream.Write(val, 0, val.Length);
                    }
                    else if (ty == typeof(uint))
                    {
                        var val = GetArrayAsBytes((value as List<uint>).ToArray(), 4);
                        underlyingStream.Write(val, 0, val.Length);
                    }
                    else if ( ty == typeof(float))
                    {
                        var val = GetArrayAsBytes((value as List<float>).ToArray(), 4);
                        underlyingStream.Write(val, 0, val.Length);
                    }
                    else if (ty == typeof(long))
                    {
                        var val = GetArrayAsBytes((value as List<long>).ToArray(), 8);
                        underlyingStream.Write(val, 0, val.Length);
                    }
                    else if (ty == typeof(ulong))
                    {
                        var val = GetArrayAsBytes((value as List<ulong>).ToArray(), 8);
                        underlyingStream.Write(val, 0, val.Length);
                    }
                    else if ( ty == typeof(double))
                    {
                        var val = GetArrayAsBytes((value as List<double>).ToArray(), 8);
                        underlyingStream.Write(val, 0, val.Length);
                    }
                    else if (ty == typeof(string))
                    {
                        foreach (string str in value)
                        {
                            this.WriteProperty(str, ref RecursivityCount);
                        }
                    }
                    else if (typeof(Type).IsAssignableFrom(ty))
                    {
                        foreach (Type t in value)
                        {
                            this.WriteProperty(t, ref RecursivityCount);
                        }
                    }
                    else if (typeof(ISerializable).IsAssignableFrom(ty))
                    {
                        foreach (ISerializable ser in value)
                        {
                            this.WriteProperty(ser, ref RecursivityCount, SerializeElementsType);
                        }
                    }
                    else if (ty.IsGenericType && typeof(Nullable<>) == ty.GetGenericTypeDefinition())
                    {
                        foreach (object obj in value)
                        {
                            this.WriteProperty(obj, ref RecursivityCount, SerializeElementsType, false, true);
                        }
                    }
                    //todo Add direct calls to collection writeProperty
                    else
                        foreach (object VARIABLE in value)
                        {
                            this.WriteProperty(VARIABLE, ref RecursivityCount, SerializeElementsType);
                        }

                }

                _stack.Pop();
            }

        }

        //?Dictionarys
        //!IDictionary must in fact be a generic Dictionary<TKey,TValue> !!
        //Todo Add direct calls to overloaded and array copy methods.
        private void WriteProperty(IDictionary value, ref int RecursivityCount, bool SerializeType = false,
            bool SerializeElementsType = false, bool nullable = false)
        {
            if (!SerializeType && !nullable)
                this.WriteProperty(value == null, ref RecursivityCount);
            if (value != null)
            {
                if (_stack.Contains(value))
                {
                    throw new SerializationContainsSameObjectTwiceInTheStack(
                        $"Object type {value.GetType()} is serialized inside itself or inside one object it contains.");
                }

                _stack.Push(value);
                this.WriteProperty(value.Count, ref RecursivityCount);
                var type = value.GetType();
                var ty1 = type.GetGenericArguments()[0];//!Will fail if IDictionary isn't typeof(Dictionary<TKey,TValue>) !!
                var ty2 = type.GetGenericArguments()[1];


                var nl1 = ty1.IsGenericType && typeof(Nullable<>) == ty1.GetGenericTypeDefinition();
                var nl2 = ty2.IsGenericType && typeof(Nullable<>) == ty2.GetGenericTypeDefinition();
                foreach (var VARIABLE in value.Keys)
                {
                    
                    this.WriteProperty(VARIABLE, ref RecursivityCount, SerializeElementsType,SerializeElementsType,nl1);
                }
                foreach (var VARIABLE in value.Values)
                {
                    this.WriteProperty(VARIABLE, ref RecursivityCount, SerializeElementsType, SerializeElementsType, nl1);
                }

                _stack.Pop();
            }
        }

        //todo propose to use permutation to reverse endianness instead of creating a new array (more CPU cycles, less memory).
        private byte[] GetArrayAsBytes(Array a, int sizeOfElement)
        {
            byte[] result = new byte[a.Length * sizeOfElement];
            Buffer.BlockCopy(a, 0, result, 0, result.Length);
            if (ReversedEndianess && sizeOfElement > 1)
            {
                byte[] reversed = new byte[result.Length];

                for (int i = 0; i < result.Length / sizeOfElement; i++)
                {
                    for (int j = 0; j < sizeOfElement; j++)
                    {
                        reversed[(i * sizeOfElement) + j] = result[(i * sizeOfElement) + (sizeOfElement - (j + 1))];
                    }
                }

                return reversed;
            }
            return result;
        }
        

        #endregion

        #endregion

        //?=============
        //?READING
        //todo add Datetime
        #region
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
            //?Deserialize type
            #region 
            if (DeserializeType)
            {
                var newType = ReadProperty<Type>();
                if (newType == null || !T.IsAssignableFrom(newType)) return null;
                else T = newType;
            }
            #endregion
            //?PRIMARY TYPE
            #region
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
            #endregion
            //?String
            #region
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
            #endregion
            //?Type and ISerializable
            #region
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
            #endregion
            //?Nullable
            #region
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
            #endregion
            //?Collections
            #region
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

                    var Rank = this.ReadProperty<byte>();
                    int[] levels = new int[Rank];
                    for (int i = 0; i < Rank; i++)
                    {
                        levels[i] = this.ReadProperty<int>();
                    }
                    var array = Array.CreateInstance(T.GetElementType(),levels);


                    //TODO IMPLEMENT FOR MULTIDIMENSIONNAL ARRAYS
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
            #endregion
            else return null;
        }

        public T Read<T>(bool DeserializeType = false, bool DeserializeElementsType = false)
        {
            var inst = this.Read(typeof(T),DeserializeType,DeserializeElementsType);

            

            if (inst is T tt) return tt;
            else return default;
            
            
        }

        #endregion




        public void Dispose()
        {
            underlyingStream?.Dispose();
        }
    }


    public enum Endianness
    {
        /// <summary>
        /// Will use the endianness of the system
        /// </summary>
        Auto,
        /// <summary>
        /// Will use big endianness and will potentially have to revert byte arrays for that.
        /// </summary>
        BigEndian,
        /// <summary>
        /// Will use little endianness and will potentially have to revert byte arrays for that.
        /// </summary>
        LittleEndian
    }
}
