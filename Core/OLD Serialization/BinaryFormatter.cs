//Copyright 2022, Alexandru Solomon, All rights reserved.
//Contacts: Alexandru.Solomon.inbox@gmail.com

namespace Networking.Serialization
{
    using System.Collections.Generic;
    using Networking.Logging;
    using System.Collections;
    using System.Reflection;
    using System.IO;
    using System;

    public class BinaryFormatter
    {
        public readonly Dictionary<Type, IFormattable> formatters;
        private readonly Dictionary<Type, Type> representedTypes;

        public readonly Assembly[] Assemblies;
        public readonly Type AttributeFlag;

        public MemoryStream Stream;
        public BinaryWriter Writer;
        public BinaryReader Reader;

        public readonly Log Log;

        public BinaryFormatter(Assembly[] assemblies, Type attributeFlag)
        {
            AttributeFlag = attributeFlag;
            Assemblies = assemblies;

            formatters = new Dictionary<Type, IFormattable>();
            representedTypes = new Dictionary<Type, Type>();

            Stream = new MemoryStream();
            Reader = new BinaryReader(Stream);
            Writer = new BinaryWriter(Stream);
            
            Log = new Log(this);
        }
        public void Initialize()
        {
            Log.Info("Binary Formatter Initialization Event");
            CreatePrimitiveFormatters();
            CreateFormattersForAssemblies(Assemblies);
            Log.Info("Binary Formatter Initialized.");
        }

        public bool Flagged(Type type, bool hierarchically)
        {
            if (Flagged(type)) return true;
            if (!hierarchically) return CheckIfFlaggedRecursively(type.DeclaringType);

            return CheckIfFlaggedRecursively(type);

            bool CheckIfFlaggedRecursively(Type? type)
            {
                if (type == null) return false;
                if (Flagged(type)) return true;
                else return CheckIfFlaggedRecursively(type.DeclaringType);
            }
        }
        public bool Flagged(Type type)
        {
            if (type == null) throw new ArgumentNullException(nameof(type), "Type reference required to check if flagged.");
            return type.GetCustomAttributes(AttributeFlag, true).Length > 0;
        }
        
        public bool Formattable(Type type)
        {
            if (type == null) throw new ArgumentNullException(nameof(type), "Type required to check if Type is formattable.");
            return formatters.ContainsKey(type);
        }

        public void Serialize(object obj)
        {
            if (obj == null)  throw new ArgumentNullException(nameof(obj), "Serialization without specifying a type requires a object reference.");

            Type type = obj.GetType();

            formatters.TryGetValue(type, out var formatter);

            if (formatter == null) throw new ArgumentException($"{type} object is not serializable. Might not be Flagged, added or supported.",nameof(obj));

            formatter.Serialize(obj);
        }
        public void Serialize<Type>(Type type)
        {
            formatters.TryGetValue(typeof(Type), out var formatter);

            if(formatter == null) throw new ArgumentException($"{type} object is not serializable. Might not be Flagged, added or supported.", nameof(type));
            if (type == null && !formatter.NullSupport) throw new ArgumentException($"The type {formatter.Type} doesnt support null value serialization.");

            formatter.Serialize(type);
        }
        public void Serialize(Type type, object? obj)
        {
            if (type == null) throw new ArgumentNullException(nameof(type), "Type required to serialize a object.");            
            formatters.TryGetValue(type, out var formatter);
            if (formatter == null) throw new ArgumentException($"{type} object is not serializable. Might not be Flagged, added or supported.", nameof(obj));
            if (obj == null)
            {
                if (!formatter.NullSupport) throw new ArgumentException($"The type {formatter.Type} doesnt support null value serialization.");
            }
            else
            {
                if (obj.GetType() != formatter.Type) throw (new ArgumentException("Object type different from the serialization type requested.",nameof(obj)));
            }

            formatter.Serialize(obj);
        }

        public Type Deserialize<Type>()
        {
            object? obj = Deserialize(typeof(Type));
            if (obj == null) return default;
            else return (Type)obj;
        }
        public object? Deserialize(Type type)
        {
            if (type == null) throw new ArgumentNullException(nameof(type), "Type required to deserialize a object.");

            formatters.TryGetValue(type, out IFormattable? formatter);

            if (formatter == null) throw new ArgumentException($"{type} object is not serializable. Might not be Flagged, added or supported.", nameof(type));

            return formatter.Deserialize();
        }

        public void AddType(Type type)
        {
            if (type == null) throw new ArgumentNullException(nameof(type), "Type required to create a formatter.");

            if (formatters.ContainsKey(type)) throw new ArgumentException("Cannot add type that is already formattable.",nameof(type));
        }
        public IFormattable GetFormatter(Type type)
        {
            if (type == null)
            {
                var msg = $"Formatter getter expects a non null type argument.";
                throw new ArgumentNullException(nameof(type), msg);
            }
            if (formatters.ContainsKey(type)) return formatters[type];


            var _msg = $"{type} doesnt have a formatter, Make sure the Type is supported and is flagged with the [{AttributeFlag.Name}] attribute.";
            throw new InvalidOperationException(_msg);
        }

        private void CreateFormattersForAssemblies(Assembly[] assemblies)
        {
            List<Type> flaggedTypes = new List<Type>();

            if (assemblies == null) return;
            foreach (Assembly assembly in assemblies)
            {
                foreach (Type type in assembly.GetTypes())
                {
                    if(Flagged(type)) flaggedTypes.Add(type);
                }
            }

            //Find representants
            foreach (Type type in flaggedTypes)
            {
                foreach (var @interface in type.GetInterfaces())
                    if (@interface.IsGenericType)
                        if (@interface.GetGenericTypeDefinition() == typeof(IRedefinition<>))
                        {
                            Type representedType = @interface.GetGenericArguments()[0];
                            representedTypes.Add(representedType, type);
                            break;
                        }
            }

            foreach (Type type in flaggedTypes)
            {
                if (formatters.ContainsKey(type)) continue;
                CreateTypeFormatter(type);
            }
        }
        private void CreatePrimitiveFormatters()
        {
            Add(Create<int>(obj => Writer.Write((int)obj), () => Reader.ReadInt32()));
            Add(Create<bool>(obj => Writer.Write((bool)obj), () => Reader.ReadBoolean()));
            Add(Create<byte>(obj => Writer.Write((byte)obj), () => Reader.ReadByte()));
            Add(Create<uint>(obj => Writer.Write((uint)obj), () => Reader.ReadUInt32()));
            Add(Create<char>(obj => Writer.Write((char)obj), () => Reader.ReadChar()));
            Add(Create<long>(obj => Writer.Write((long)obj), () => Reader.ReadInt64()));
            Add(Create<sbyte>(obj => Writer.Write((sbyte)obj), () => Reader.ReadSByte()));
            Add(Create<short>(obj => Writer.Write((short)obj), () => Reader.ReadInt16()));
            Add(Create<ulong>(obj => Writer.Write((ulong)obj), () => Reader.ReadUInt64()));
            Add(Create<float>(obj => Writer.Write((float)obj), () => Reader.ReadSingle()));
            Add(Create<ushort>(obj => Writer.Write((ushort)obj), () => Reader.ReadUInt16()));
            Add(Create<double>(obj => Writer.Write((double)obj), () => Reader.ReadDouble()));

            var stringFormatter = Create<string>(obj => Writer.Write((string)obj), () => Reader.ReadString());
            Add(new NullPrefixFormatter(this, stringFormatter));

            var bufferFormatter = Create<byte[]>(obj =>{ 
                Writer.Write(((byte[])obj).Length); 
                Writer.Write((byte[])obj); 
            }, () => Reader.ReadBytes(Reader.ReadInt32()));
            Add(new NullPrefixFormatter(this,bufferFormatter));


            void Add(IFormattable formatter)
            {
                if (formatters.ContainsKey(formatter.Type)) 
                    throw new ArgumentException($"Attempt to manually add primitive formatter {formatter.Type} when already added", nameof(formatter));

                AddFormatter(formatter);
            }
            static IFormattable Create<T>(SerializerMethod serializer, DeserializerMethod deserializer)
            {
                return new PrimitiveFormatter(typeof(T), serializer, deserializer);
            }
        }

        private ContainerFormatter CreateContainerFormatter(Type containerType)
        {
            var fieldFormatters = new List<ContainerFormatter.Field>();
            var fieldInfos = containerType.GetFields();

            foreach (FieldInfo fieldInfo in fieldInfos)
            {
                Type fieldType = fieldInfo.FieldType;
                formatters.TryGetValue(fieldType,out var fieldFormatter);

                fieldFormatter ??= CreateTypeFormatter(fieldType);

                if (fieldFormatter == null)
                {
                    Log.Error($"Field {fieldInfo.Name} from {containerType.FullName} will be ignored because its type {fieldInfo.FieldType} is unformattable.");
                    continue;
                }

                fieldFormatters.Add(new ContainerFormatter.Field(fieldInfo, fieldFormatter));
            }
            return new ContainerFormatter(containerType, fieldFormatters.ToArray());
        }
        private IFormattable? CreateRepresentedFormatter(Type representantType)
        {
            var representantInterface = representantType.GetInterfaces()[0];
            if (representantInterface.GetGenericTypeDefinition() != typeof(IRedefinition<>))
            {
                Log.Error($"The first interface of the representant Type {representantType} should be IRepresentant<\"representedTypeName\">");
                return null;
            }
            Type representedType = representantInterface.GetGenericArguments()[0];
            if (representantType == representedType)
            {
                Log.Error($"Representant {representantType} cannot represent itself. Change the generic value of the IRepesentant<> interface into a valid one.");
                return null;
            }
            bool noEmptyConstructor = representantType.GetConstructor(Type.EmptyTypes) == null;
            if (noEmptyConstructor)
            {
                Log.Error($"A empty constructor is required for a representant type. {representedType} will not be formatted.");
                return null;
            }
            formatters.TryGetValue(representantType, out var representantFormatter);
            representantFormatter ??= CreateTypeFormatter(representantType);
            if (representantFormatter == null)
            {
                Log.Error($"{representedType} cannot be formatted because its representant is not formattable.");
                return null;
            }
            var representedFormatter = new RepresentedFormatter(representedType, representantFormatter);
            AddFormatter(representedFormatter);

            return representedFormatter;
        }
        private IFormattable? CreateCollectionFormatter(Type collectionType)
        {
            Type[] genericArguments = collectionType.GetGenericArguments();
            if (genericArguments == null)
            {
                Log.Error($"Collection {collectionType} is unformattable because does not have generic arguments.");
                return null;
            }
            if (genericArguments.Length != 1)
            {
                Log.Error($"Collection {collectionType} is unformattable because the number of generic arguments is different from 1.");
                return null;
            }
            Type elementType = genericArguments[0];
            formatters.TryGetValue(elementType, out var elementFormatter);
            elementFormatter??= CreateTypeFormatter(elementType);
            if (elementFormatter == null)
            {
                Log.Error($"Collection {collectionType} is unformattable because its element type {elementType} is unformattable.");
                return null;
            }
            var collectionFormatter = new CollectionFormatter(collectionType, elementType, elementFormatter, this);
            var finalCollectionFormatter = new NullPrefixFormatter(this, collectionFormatter);
            AddFormatter(finalCollectionFormatter);

            return finalCollectionFormatter;
        }
        private IFormattable? CreateNullableFormatter(Type nullableType)
        {
            Type? underlyingType = Nullable.GetUnderlyingType(nullableType);
            if (underlyingType == null)
            {
                Log.Error($"Nullable {nullableType.FullName} is unformattable. Failed to get the underlyin type of the nullable type {nullableType}.");
                return null;
            }
            formatters.TryGetValue(underlyingType, out var underlyingTypeFormatter);
            underlyingTypeFormatter ??= CreateTypeFormatter(underlyingType);
            if (underlyingTypeFormatter == null)
            {
                Log.Error($"Nullable {nullableType.FullName} is unformattable because its underlying type {underlyingType.FullName} is unformattable.");
                return null;
            }
            var NullableFormatter = new NullPrefixFormatter(this, underlyingTypeFormatter);
            AddFormatter(NullableFormatter);

            return NullableFormatter;
        }
        private IFormattable? CreateStructFormatter(Type structType)
        {
            if (!Flagged(structType, true))
            {
                Log.Error($"Struct {structType.FullName} is unformattable because is not flagged with [{AttributeFlag.Name}].");
                return null;
            }
            ContainerFormatter containerFormatter = CreateContainerFormatter(structType);
            AddFormatter(containerFormatter);

            return containerFormatter;
        }
        private IFormattable? CreateClassFormatter(Type classType)
        {
            if (!Flagged(classType, true))
            {
                Log.Error($"Class {classType} is unformattable because is not flagged with [{AttributeFlag.Name}].");
                return null;
            }
            var containerFormatter = CreateContainerFormatter(classType);
            var classFormatter = new NullPrefixFormatter(this, containerFormatter);
            AddFormatter(classFormatter);

            return classFormatter;
        }
        private IFormattable? CreateArrayFormatter(Type arrayType)
        {
            Type? elementType = arrayType.GetElementType();
            if (elementType == null)
            {
                Log.Error($"Array {arrayType} is unformattable. Failed to get the element Type of the array type {arrayType}.");
                return null;
            }
            formatters.TryGetValue(arrayType,out var elementFormatter);
            elementFormatter??= CreateTypeFormatter(elementType);
            if (elementFormatter == null)
            {
                Log.Error($"Array {arrayType} is unformattable because its element type {elementType} is unformattable.");
                return null;
            }
            var collectionFormatter = new CollectionFormatter(arrayType, elementType,elementFormatter, this);
            var arrayFormatter = new NullPrefixFormatter(this, collectionFormatter);

            AddFormatter(arrayFormatter);
            return arrayFormatter;
        }
        private IFormattable? CreateEnumFormatter(Type enumType)
        {
            EnumFormatter enumFormatter = new EnumFormatter(this, enumType);
            AddFormatter(enumFormatter);

            return enumFormatter;
        }
        private IFormattable? CreateTypeFormatter(Type type)
        {
            if (type == null)
            {
                Log.Error("Attempt create formatter for a null type.");
                return null;
            }
            if (formatters.ContainsKey(type))
            {
                Log.Warn($"Attempt to create Formatter for {type} when it already has a formatter.");
                return formatters[type];
            }
            if (type.IsInterface)
            {
                Log.Error($"Attempt to create a Formatter for a interface type {type}. Interfaces are not supported !");
                return null;
            }

            representedTypes.TryGetValue(type, out var representantType);
            if (representantType != null) return CreateRepresentedFormatter(representantType);

            if (type.IsClass)
            {
                if (type.IsSubclassOf(typeof(Delegate)))
                {
                    Log.Error($"Attempt to create a Formatter for a delegate type {type}. Delegates are not supported !");
                    return null;
                }
                if (type.IsArray) return CreateArrayFormatter(type);
                if (typeof(ICollection).IsAssignableFrom(type))
                {
                    return CreateCollectionFormatter(type);
                }

                return CreateClassFormatter(type);
            }

            if (type.IsValueType)
            {
                if (type.IsPrimitive) return null;
                if (type.IsEnum) return CreateEnumFormatter(type);
                if (Nullable.GetUnderlyingType(type) != null) return CreateNullableFormatter(type);

                if (type.FullName == null) return null;
                if (!type.FullName.StartsWith("System.")) return CreateStructFormatter(type);
            }

            Log.Error($"Attempt to create formatter for Type {type} failed. Type unsupported or unimplemented !");
            return null;
        }

        private void AddFormatter(IFormattable formatter)
        {
            Log.Info($"{formatter} formattable");
            formatters.Add(formatter.Type, formatter);
        }
    }
}