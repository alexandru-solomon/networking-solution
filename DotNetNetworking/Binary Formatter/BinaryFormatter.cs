//Copyright 2022, Alexandru Solomon, All rights reserved.
//Contacts: Alexandru.Solomon.inbox@gmail.com

using System.Collections.Generic;
using System.Collections;
using System.Reflection;
using System.IO;
using System;

namespace CNet.Serialization
{
    public class BinaryFormatter
    {
        private readonly Dictionary<Type, IFormattable> formatters;
        private readonly MemoryStream stream;
        private readonly BinaryWriter writer;
        private readonly BinaryReader reader;
        private readonly Type attributeFlag;

        public BinaryFormatter(Assembly[] assemblies, Type attributeFlag)
        {
            this.attributeFlag = attributeFlag;
            formatters = new Dictionary<Type, IFormattable>();
            stream = new MemoryStream(1048576);
            reader = new BinaryReader(stream);
            writer = new BinaryWriter(stream);

            CreateAndAddPrimimitiveFormatters();
            CreateAssemblyFormatters(assemblies);
        }

        public bool Flagged(Type type, bool hierarchically)
        {
            if (type == null)
            {
                Error($"Attempt to check if null type is flagged with [{attributeFlag.Name}]");
                return false;
            }
            if (!hierarchically) return Flagged(type);
            else return DeclaringTypeFlagged(type);

            bool DeclaringTypeFlagged(Type type)
            {
                if (type == null) return false;
                if (Flagged(type)) return true;
                else return DeclaringTypeFlagged(type.DeclaringType);
            }
        }
        public bool Flagged(Type type)
        {
            if (type == null)
            {
                Error($"Attempt to check if null type is flagged with [{attributeFlag.Name}]");
                return false;
            }
            return type.GetCustomAttributes(attributeFlag, true).Length > 0;
        }
        public bool Formattable(Type type)
        {
            if (type == null)
            {
                Error("Attempt to check if null Type is Formattable.");
                return false;
            }
            else return formatters.ContainsKey(type);
        }

        public void Serialize(object obj)
        {
            if (obj == null)
            {
                Error("Attempt to serialize a null object without specifying a type. Use \"Serialize(Type type, object obj)\" instead.");
                return;
            }

            Serialize(obj.GetType(), obj);
        }
        public void Serialize(Type type, object obj)
        {
            formatters.TryGetValue(obj.GetType(), out IFormattable formatter);
            if (formatter == null)
            {
                Error($"Type {obj.GetType().FullName} cannot be serialized. Make sure the Type is supported and is flagged with the [{attributeFlag.Name}] attribute.");
                return;
            }
            formatter.Serialize(obj);
        }
        public void Serialize<Type>(object obj)
        {
            Serialize(typeof(Type), obj);
        }
        public Type Deserialize<Type>()
        {
            object obj = Deserialize(typeof(Type));
            if (obj == null) return default;
                        else return (Type)obj;
        }
        public object Deserialize(Type type)
        {
            if (type == null)
            {
                Error("Attempt to deserialize a object of null type. The deserialization process requires the type of the serialized object.");
                return null;
            }
            formatters.TryGetValue(type, out IFormattable formatter);
            if (formatter == null)
            {
                Error($"Type {type} cannot be deserialized. Make sure the Type is supported and is flagged with the [{attributeFlag.Name}] attribute.");
                return null;
            }
            else return formatter.Deserialize();
        }

        internal IFormattable GetFormatter(Type type)
        {
            if(type == null)
            {
                Error("Attempt to get a Formatter for a null Type. You shall check with \"Formattable(Type type)\" before getting a formatter.");
                return null;
            }

            if (Formattable(type)) return formatters[type];
            else
            {
                Error("Attempt to get a Formatter for a Type that doesn't hava a Formatter.");
                return null;
            }
          
        }

        private void CreateAssemblyFormatters(Assembly[] assemblies)
        {
            List<Type> types = new();

            if (assemblies == null) return;
            foreach (Assembly assembly in assemblies)
            {
                foreach (Type type in assembly.GetTypes())
                {
                    types.Add(type);
                }
            }
            foreach (Type type in types)
            {
                bool isDirectlyFlagged = Flagged(type);

                if (isDirectlyFlagged) AddTypeFormatter(type);
            }
        }
        private void CreateAndAddPrimimitiveFormatters()
        {
            Add<int>(obj => writer.Write((int)obj), () => reader.ReadInt32());
            Add<bool>(obj => writer.Write((bool)obj), () => reader.ReadBoolean());
            Add<byte>(obj => writer.Write((byte)obj), () => reader.ReadByte());
            Add<uint>(obj => writer.Write((uint)obj), () => reader.ReadUInt32());
            Add<char>(obj => writer.Write((char)obj), () => reader.ReadChar());
            Add<long>(obj => writer.Write((long)obj), () => reader.ReadInt64());
            Add<sbyte>(obj => writer.Write((sbyte)obj), () => reader.ReadSByte());
            Add<short>(obj => writer.Write((short)obj), () => reader.ReadInt16());
            Add<ulong>(obj => writer.Write((ulong)obj), () => reader.ReadUInt64());
            Add<float>(obj => writer.Write((float)obj), () => reader.ReadSingle());
            Add<ushort>(obj => writer.Write((ushort)obj), () => reader.ReadUInt16());
            Add<double>(obj => writer.Write((double)obj), () => reader.ReadDouble());
            Add<string>(obj => writer.Write((string)obj), () => reader.ReadString());

            void Add<T>(SerializerMethod serializer, DeserializerMethod deserializer)
            {
                AddPrimitiveFormatter(new(serializer, deserializer),typeof(T));
            }
        }

        private ContainerFormatter CreateContainerFormatter(Type containerType)
        {
            List<ContainerFormatter.Field> fieldFormatters = new();
            FieldInfo[] fieldInfos = containerType.GetFields();

            foreach (FieldInfo fieldInfo in fieldInfos)
            {
                Type fieldType = fieldInfo.FieldType;
                IFormattable fieldFormatter;
                if (Formattable(fieldType)) fieldFormatter = GetFormatter(fieldType);
                else fieldFormatter = AddTypeFormatter(fieldType);

                if (fieldFormatter == null)
                {
                    Error($"Field {fieldInfo.Name} from {containerType.FullName} will be ignored because its type {fieldInfo.FieldType} is unformattable.");
                    continue;
                }

                fieldFormatters.Add(new(fieldInfo, fieldFormatter));
            }
            return new(containerType, fieldFormatters.ToArray());
        }

        private IFormattable AddRepresentedType(Type representantType)
        {
            var representantInterface = representantType.GetInterfaces()[0];
            if(representantInterface.GetGenericTypeDefinition() != typeof(IRepresentant<>))
            {
                Error($"The first interface of the representant Type {representantType} should be IRepresentant<\"representedTypeName\">");
                return null;
            }

            Type representedType = representantInterface.GetGenericArguments()[0];

            if(representantType == representedType)
            {
                Error($"Representant {representantType} cannot represent itself. Change the generic value of the IRepesentant<> interface into a valid one.");
                return null;
            }

            ContainerFormatter representantFormatter = CreateContainerFormatter(representantType);
            RepresentedFormatter representedFormatter = new(representantFormatter, representantType);

            formatters.Add(representedType, representedFormatter);
            return null;
        }
        private IFormattable AddPrimitiveFormatter(PrimitiveFormatter formatter, Type type)
        {
            formatters.Add(type, formatter);
            return formatter;
        }
        private IFormattable AddCollectionFormatter(Type collectionType)
        {
            Type[] genericArguments = collectionType.GetGenericArguments();
            if (genericArguments == null)
            {
                Error($"Collection {collectionType} is unformattable because does not have generic arguments.");
                return null;
            }
            if (genericArguments.Length != 1)
            {
                Error($"Collection {collectionType} is unformattable because the number of generic arguments is different from 1.");
                return null;
            }

            Type elementType = genericArguments[0];

            IFormattable elementFormatter;
            if (Formattable(elementType)) elementFormatter = GetFormatter(elementType);
            else elementFormatter = AddTypeFormatter(elementType);

            if(elementFormatter == null)
            {
                Error($"Collection {collectionType} is unformattable because its element type {elementType} is unformattable.");
                return null;
            }
            
            CollectionFormatter collectionFormatter = new(this, elementFormatter, elementType, collectionType);
            return new NullPrefixFormatter(this,collectionFormatter);
        }
        private IFormattable AddNullableFormatter(Type nullableType)
        {
            Type underlyingType = Nullable.GetUnderlyingType(nullableType);

            IFormattable underlyingTypeFormatter;
            if (Formattable(underlyingType)) underlyingTypeFormatter = GetFormatter(underlyingType);
            else underlyingTypeFormatter = AddTypeFormatter(underlyingType);

            if (underlyingTypeFormatter == null)
            {
                Error($"Nullable {nullableType.FullName} is unformattable because its underlying type {underlyingType.FullName} is unformattable.");
                return null;
            }

            NullPrefixFormatter nullPrefixFormatter = new(this, underlyingTypeFormatter);

            formatters.Add(nullableType, nullPrefixFormatter);

            return nullPrefixFormatter;
        }
        private IFormattable AddStructFormatter(Type structType)
        {
            if (!Flagged(structType, true))
            {
                Error($"Struct {structType.FullName} is unformattable because is not flagged with [{attributeFlag.Name}].");
                return null;
            }
            ContainerFormatter containerFormatter = CreateContainerFormatter(structType);
            formatters.Add(structType, containerFormatter);
            return containerFormatter;
        }
        private IFormattable AddClassFormatter(Type classType)
        {
            if (!Flagged(classType, true))
            {
                Error($"Class {classType.FullName} is unformattable because is not flagged with [{attributeFlag.Name}].");
                return null;
            }
            ContainerFormatter containerFormatter = CreateContainerFormatter(classType);
            NullPrefixFormatter nullPrefixFormatter = new(this, containerFormatter);
            formatters.Add(classType, nullPrefixFormatter);
            return nullPrefixFormatter;
        }
        private IFormattable AddArrayFormatter(Type arrayType)
        {
            Type elementType = arrayType.GetElementType();

            IFormattable elementFormatter;
            if (Formattable(elementType)) elementFormatter = GetFormatter(elementType);
            else elementFormatter = AddTypeFormatter(elementType);

            if (elementFormatter == null)
            {
                Error($"Array {arrayType} is unformattable because its element type {elementType} is unformattable.");
                return null;
            }

            CollectionFormatter collectionFormatter = new(this, elementFormatter, elementType, arrayType);
            return new NullPrefixFormatter(this, collectionFormatter);
        }
        private IFormattable AddEnumFormatter(Type enumType)
        {
            EnumFormatter enumFormatter = new EnumFormatter(this, enumType);
            formatters.Add(enumType, enumFormatter);

            return enumFormatter;
        }
        private IFormattable AddTypeFormatter(Type type)
        {
            if (type == null)
            {
                Error("Attempt to create Formatter for null Type.");
                return null;
            }
            if (Formattable(type))
            {
                Warn("Attempt to create Formatter for Type that already has a formatter.");
                return GetFormatter(type);
            }
            if (type.IsInterface)
            {
                Error($"Attempt to create a Formatter for a interface type ({type.FullName}). Interfaces are not supported.");
                return null;
            }
            if (typeof(IRepresentant<>).IsAssignableFrom(type)) return AddRepresentedType(type);
            if (type.IsClass)
            {
                if (type.IsSubclassOf(typeof(Delegate)))
                {
                    Error($"Attempt to create a Formatter for a delegate type ({type.FullName}). Delegates are not supported.");
                    return null;
                }
                if (type.IsArray) return AddArrayFormatter(type);
                if (typeof(ICollection).IsAssignableFrom(type))
                {
                    return AddCollectionFormatter(type);
                }

                return AddClassFormatter(type);
            }

            if (type.IsValueType)
            {
                if (type.IsPrimitive) return null;
                if (type.IsEnum) return AddEnumFormatter(type);
                if (Nullable.GetUnderlyingType(type) != null) return AddNullableFormatter(type);
                if (!type.FullName.StartsWith("System.")) return AddStructFormatter(type);
            }

            Error($"Attempt to create formatter for Type {type.FullName} failed. (Unsupported or Unimplemented)");
            return null;
        }


        public Action<object,BinaryFormatter> infoEvent, errorEvent, warningEvent;
        protected void Info(object message) => infoEvent?.Invoke(message, this);
        protected void Warn(object message) => warningEvent?.Invoke(message,this);
        protected void Error(object message) => errorEvent?.Invoke(message, this);
    }
}