//Copyright 2022, Alexandru Solomon, All rights reserved.
//Contacts: Alexandru.Solomon.inbox@gmail.com

namespace Networking.Serialization
{
    using Networking.Logging;
    using System.Collections.Generic;
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
            if (obj == null) throw new ArgumentNullException(nameof(obj), "Serialization without specifying a type requires a object reference.");

            Type type = obj.GetType();

            if (!Formattable(type)) throw new ArgumentException($"{type} object is not serializable. Might not be Flagged, added or supported.",nameof(obj));

            GetFormatter(type).Serialize(obj);
        }

        public void Serialize(Type type, object? obj)
        {
            if (type == null) throw new ArgumentNullException(nameof(type), "Type required to serialize a object.");            

            TryGetFormatter(type, out var formatter);

            if (formatter == null) throw new ArgumentException($"{type} object is not serializable. Might not be Flagged, added or supported.", nameof(obj));

            if (obj == null)
            {
                if (formatter.NullSupport) throw new ArgumentException($"The type {formatter.Type} doesnt support null value serialization.");
            }
            else
            {
                if (obj.GetType() != formatter.Type) throw (new ArgumentException("Object type different from the serialization type requested.",nameof(obj)));
            }

            {
                var msg = $"The {obj?.GetType()} object argument specified should have the same type as the specified type argument. You specified {type} instead.";
                throw (new InvalidOperationException(msg));
            }

            if (formatter.NullSupport && obj == null)
            {
                var msg = $"Type {type} doesnt support null value serialization.";
                throw new InvalidOperationException(msg);
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
            if (type == null)
            {
                Log.Error("Attempt to deserialize a object of null type. The deserialization process requires the type of the serialized object.");
                return null;
            }
            formatters.TryGetValue(type, out IFormattable? formatter);
            if (formatter == null)
            {
                Log.Error($"Type {type} cannot be deserialized. Make sure the Type is supported and is flagged with the [{AttributeFlag.Name}] attribute.");
                return null;
            }

            return formatter.Deserialize();
        }

        private void AddFormatter(IFormattable formatter)
        {
            if(formatter == null)
            {
                var msg = "Cannot add Null Formatter to the type formatters dictionary.";
                throw new ArgumentNullException(nameof(formatter),msg);
            }
            if(formatters.ContainsKey(formatter.Type))
            {
                var msg = $"Attempt to add a formatter for {formatter.Type} but formatter already existent.";
                throw new InvalidOperationException(msg);
            }

            Log.Info($"Added {formatter} formatter");
            formatters.Add(formatter.Type,formatter);
        }

        public void TryGetFormatter(Type type, out IFormattable? formatter)
        {
            if(type == null)
            {
                var msg = $"Formatter getter expects a non null type argument.";
                throw new ArgumentNullException(nameof(type), msg);
            }
            
            formatter = Formattable(type)? GetFormatter(type) : null;
        }
        public IFormattable GetFormatter(Type type)
        {
            if (type == null)
            {
                var msg = $"Formatter getter expects a non null type argument.";
                throw new ArgumentNullException(nameof(type), msg);
            }
            if (Formattable(type)) return formatters[type];


            var _msg = $"{type} doesnt have a formatter, Make sure the Type is supported and is flagged with the [{AttributeFlag.Name}] attribute.";
            throw new InvalidOperationException(_msg);
        }

        public void TryGetRepresentant(Type type, out Type? representantType)
        {
            if (type == null)
            {
                var msg = "Representant getter method expects a non null argument.";
                throw new ArgumentNullException(nameof(type), msg);
            }
            representedTypes.TryGetValue(type, out representantType);
        }

        private void CreateFormattersForAssemblies(Assembly[] assemblies)
        {
            List<Type> flaggedTypes = new List<Type>();

            if (assemblies == null) return;
            foreach (Assembly assembly in assemblies)
            {
                foreach (Type type in assembly.GetTypes())
                {
                    flaggedTypes.Add(type);
                }
            }

            //Fiend representants
            foreach (Type type in flaggedTypes)
            {
                foreach (var @interface in type.GetInterfaces())
                    if (@interface.IsGenericType)
                        if (@interface.GetGenericTypeDefinition() == typeof(IRepresents<>))
                        {
                            Type representedType = @interface.GetGenericArguments()[0];
                            representedTypes.Add(representedType, type);
                            break;
                        }
            }

            foreach (Type type in flaggedTypes)
            {
                if (Formattable(type)) continue;

                bool isDirectlyFlagged = Flagged(type);
                if (isDirectlyFlagged) CreateTypeFormatter(type);
            }
        }
        private void CreatePrimitiveFormatters()
        {
            AddFormatter(Get<int>(obj => Writer.Write((int)obj), () => Reader.ReadInt32()));
            AddFormatter(Get<bool>(obj => Writer.Write((bool)obj), () => Reader.ReadBoolean()));
            AddFormatter(Get<byte>(obj => Writer.Write((byte)obj), () => Reader.ReadByte()));
            AddFormatter(Get<uint>(obj => Writer.Write((uint)obj), () => Reader.ReadUInt32()));
            AddFormatter(Get<char>(obj => Writer.Write((char)obj), () => Reader.ReadChar()));
            AddFormatter(Get<long>(obj => Writer.Write((long)obj), () => Reader.ReadInt64()));
            AddFormatter(Get<sbyte>(obj => Writer.Write((sbyte)obj), () => Reader.ReadSByte()));
            AddFormatter(Get<short>(obj => Writer.Write((short)obj), () => Reader.ReadInt16()));
            AddFormatter(Get<ulong>(obj => Writer.Write((ulong)obj), () => Reader.ReadUInt64()));
            AddFormatter(Get<float>(obj => Writer.Write((float)obj), () => Reader.ReadSingle()));
            AddFormatter(Get<ushort>(obj => Writer.Write((ushort)obj), () => Reader.ReadUInt16()));
            AddFormatter(Get<double>(obj => Writer.Write((double)obj), () => Reader.ReadDouble()));

            var stringFormatter = Get<string>(obj => Writer.Write((string)obj), () => Reader.ReadString());
            AddFormatter(new NullPrefixFormatter(this, stringFormatter));

            var bufferFormatter = Get<byte[]>(obj =>{ 
                Writer.Write(((byte[])obj).Length); 
                Writer.Write((byte[])obj); 
            }, () => Reader.ReadBytes(Reader.ReadInt32()));
            AddFormatter(new NullPrefixFormatter(this,bufferFormatter));

            static IFormattable Get<T>(SerializerMethod serializer, DeserializerMethod deserializer)
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
                IFormattable? fieldFormatter;
                if (Formattable(fieldType)) fieldFormatter = GetFormatter(fieldType);
                else fieldFormatter = CreateTypeFormatter(fieldType);

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
            if (representantInterface.GetGenericTypeDefinition() != typeof(IRepresents<>))
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

            IFormattable? representantFormatter;
            if (Formattable(representantType)) representantFormatter = GetFormatter(representantType);
            else representantFormatter = CreateTypeFormatter(representantType);
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

            IFormattable? elementFormatter;
            if (Formattable(elementType)) elementFormatter = GetFormatter(elementType);
            else elementFormatter = CreateTypeFormatter(elementType);

            if (elementFormatter == null)
            {
                Log.Error($"Collection {collectionType} is unformattable because its element type {elementType} is unformattable.");
                return null;
            }

            var collectionFormatter = new CollectionFormatter(collectionType, elementType, elementFormatter, this);
            var nullableCollectionFormatter = new NullPrefixFormatter(this, collectionFormatter);
            AddFormatter(nullableCollectionFormatter);

            return nullableCollectionFormatter;
        }
        private IFormattable? CreateNullableFormatter(Type nullableType)
        {
            Type? underlyingType = Nullable.GetUnderlyingType(nullableType);

            if (underlyingType == null)
            {
                Log.Error($"Nullable {nullableType.FullName} is unformattable. Failed to get the underlyin type of the nullable type {nullableType}.");
                return null;
            }

            IFormattable? underlyingTypeFormatter;
            if (Formattable(underlyingType)) underlyingTypeFormatter = GetFormatter(underlyingType);
            else underlyingTypeFormatter = CreateTypeFormatter(underlyingType);

            if (underlyingTypeFormatter == null)
            {
                Log.Error($"Nullable {nullableType.FullName} is unformattable because its underlying type {underlyingType.FullName} is unformattable.");
                return null;
            }

            var nullPrefixFormatter = new NullPrefixFormatter(this, underlyingTypeFormatter);
            AddFormatter(nullPrefixFormatter);

            return nullPrefixFormatter;
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
            var nullPrefixFormatter = new NullPrefixFormatter(this, containerFormatter);
            AddFormatter(nullPrefixFormatter);

            return nullPrefixFormatter;
        }
        private IFormattable? CreateArrayFormatter(Type arrayType)
        {
            Type? elementType = arrayType.GetElementType();

            if (elementType == null)
            {
                Log.Error($"Array {arrayType} is unformattable. Failed to get the element Type of the array type {arrayType}.");
                return null;
            }

            IFormattable? elementFormatter;
            if (Formattable(elementType)) elementFormatter = GetFormatter(elementType);
            else elementFormatter = CreateTypeFormatter(elementType);

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
            if (Formattable(type))
            {
                Log.Warn($"Attempt to create Formatter for {type} when it already has a formatter.");
                return GetFormatter(type);
            }
            if (type.IsInterface)
            {
                Log.Error($"Attempt to create a Formatter for a interface type {type}. Interfaces are not supported !");
                return null;
            }

            TryGetRepresentant(type, out var representantType);
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
    }
}