//Copyright 2022, Alexandru Solomon, All rights reserved.
//Contacts: Alexandru.Solomon.inbox@gmail.com

using System.Collections;
using System.Reflection;

namespace Networking.Serialization
{
    public class BinaryFormatter
    {
        public readonly Dictionary<Type, IFormattable> formatters;
        private readonly Dictionary<Type, Type> representedTypes;
        public readonly MemoryStream Stream;
        public readonly BinaryWriter Writer;
        public readonly BinaryReader Reader;
        public readonly Type AttributeFlag;
        public readonly Assembly[] Assemblies;

        public BinaryFormatter(Assembly[] assemblies, Type attributeFlag)
        {
            Assemblies = assemblies;
            AttributeFlag = attributeFlag;
            formatters = new Dictionary<Type, IFormattable>();
            representedTypes = new Dictionary<Type, Type>();
            Stream = new MemoryStream(1048576);
            Reader = new BinaryReader(Stream);
            Writer = new BinaryWriter(Stream);
        }
        public void Initialize()
        {
            Event("Binary Formatter Initialization Event");
            CreatePrimitiveFormatters();
            CreateFormattersForAssemblies(Assemblies);
            Event("Binary Formatter Initialized.");
        }

        public bool Flagged(Type type, bool hierarchically)
        {
            if (type == null)
            {
                Error($"Attempt to check if null type is flagged with [{AttributeFlag.Name}]");
                return false;
            }
            if (!hierarchically) return Flagged(type);
            else return DeclaringTypeFlagged(type);

            bool DeclaringTypeFlagged(Type? type)
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
                Error($"Attempt to check if null type is flagged with [{AttributeFlag.Name}]");
                return false;
            }
            return type.GetCustomAttributes(AttributeFlag, true).Length > 0;
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
                Error("Cannot get the type from a null object. To serialize a null value use \"Serialize(Type type, object obj)\" and specify a type.");
                return;
            }

            Serialize(obj.GetType(), obj);
        }
        public void Serialize<Type>(Type obj)
        {
            Serialize(typeof(Type), obj);
        }
        public void Serialize(Type type, object? obj)
        {
            if(type == null)
            {
                Error($"The parameter \"type\" of the function shall not be null. Use Serialize(object obj) to get the type automatically from the object.");
                return;
            }
            if(obj == null)
            {
                if (type.IsValueType)
                {
                    Error($"The value type {type} cannot be serialized with a null value. Only reference types support null reference serialization.");
                    return;
                }
            }
            else
            {
                if(obj.GetType() != type) 
                {
                    Error($"The object cannot be serialized because the specified type {obj.GetType} is different from the objects type {type}");
                    return;
                }
            }

            formatters.TryGetValue(type, out IFormattable? formatter);
            if (formatter == null)
            {
                Error($"Type {type} does not have a formatter. Make sure the Type is supported and is flagged with the [{AttributeFlag.Name}] attribute.");
                return;
            }
            formatter.Serialize(obj);

        }
        public void Serialize(IFormattable formatter, object value)
        {
            if(formatter == null)
            {
                Error("Cannot serialize with a null formatter."); 
                return;
            }
            formatter.Serialize(value);
        }

        public Type? Deserialize<Type>()
        {
            object? obj = Deserialize(typeof(Type));
            if (obj == null) return default;
                        else return (Type)obj;
        }
        public object? Deserialize(Type type)
        {
            if (type == null)
            {
                Error("Attempt to deserialize a object of null type. The deserialization process requires the type of the serialized object.");
                return null;
            }
            formatters.TryGetValue(type, out IFormattable? formatter);
            if (formatter == null)
            {
                Error($"Type {type} cannot be deserialized. Make sure the Type is supported and is flagged with the [{AttributeFlag.Name}] attribute.");
                return null;
            }
            else return formatter.Deserialize();
        }

        public IFormattable? GetFormatter(Type type)
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
        public Type? GetRepresentantType(Type type)
        {
            representedTypes.TryGetValue(type, out Type? representant);
            return representant;
        } 

        private void CreateFormattersForAssemblies(Assembly[] assemblies)
        {
            List<Type> flaggedTypes = new();
    
            if (assemblies == null) return;
            foreach (Assembly assembly in assemblies)
            {
                foreach (Type type in assembly.GetTypes())
                {
                    flaggedTypes.Add(type);
                }
            }

            //Fiend representants
            foreach(Type type in flaggedTypes)
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
                if (isDirectlyFlagged) AddTypeFormatter(type);
            }
        }
        private void CreatePrimitiveFormatters()
        {
            Add<int>(obj => Writer.Write((int)obj), () => Reader.ReadInt32());
            Add<bool>(obj => Writer.Write((bool)obj), () => Reader.ReadBoolean());
            Add<byte>(obj => Writer.Write((byte)obj), () => Reader.ReadByte());
            Add<uint>(obj => Writer.Write((uint)obj), () => Reader.ReadUInt32());
            Add<char>(obj => Writer.Write((char)obj), () => Reader.ReadChar());
            Add<long>(obj => Writer.Write((long)obj), () => Reader.ReadInt64());
            Add<sbyte>(obj => Writer.Write((sbyte)obj), () => Reader.ReadSByte());
            Add<short>(obj => Writer.Write((short)obj), () => Reader.ReadInt16());
            Add<ulong>(obj => Writer.Write((ulong)obj), () => Reader.ReadUInt64());
            Add<float>(obj => Writer.Write((float)obj), () => Reader.ReadSingle());
            Add<ushort>(obj => Writer.Write((ushort)obj), () => Reader.ReadUInt16());
            Add<double>(obj => Writer.Write((double)obj), () => Reader.ReadDouble());
            Add<string>(obj => Writer.Write((string)obj), () => Reader.ReadString());
            Add<byte[]>(obj => 
            {
                if (obj == null) { Writer.Write(0); return; }
                Writer.Write(((byte[])obj).Length); 
                Writer.Write((byte[])obj);
            }, () =>
            {
                int len = Reader.Read();
                return Reader.ReadBytes(len);
            } );

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
                IFormattable? fieldFormatter;
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

        private IFormattable? AddRepresentedType(Type representantType)
        {

            var representantInterface = representantType.GetInterfaces()[0];
            if(representantInterface.GetGenericTypeDefinition() != typeof(IRepresents<>))
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

            bool noEmptyConstructor = representantType.GetConstructor(Type.EmptyTypes) == null;
            if(noEmptyConstructor)
            {
                Error($"A empty constructor is required for a representant type. {representedType} will not be formatted.");
                return null;
            }

            IFormattable? representantFormatter;
            if (Formattable(representantType)) representantFormatter = GetFormatter(representantType);
            else representantFormatter = AddTypeFormatter(representantType);
            if(representantFormatter == null)
            {
                Error($"{representedType} cannot be formatted because its representant is not formattable.");
                return null;
            }

            RepresentedFormatter representedFormatter = new(representantFormatter, representantType, representedType);
            formatters.Add(representedType, representedFormatter);
            Info($"Set {representantType} as representant for {representedType}");

            return representedFormatter;
        }
        private IFormattable? AddPrimitiveFormatter(PrimitiveFormatter formatter, Type type)
        {
            formatters.Add(type, formatter);
            Info($"Created Primitive formatter for type {type}");
            return formatter;
        }
        private IFormattable? AddCollectionFormatter(Type collectionType)
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

            IFormattable? elementFormatter;
            if (Formattable(elementType)) elementFormatter = GetFormatter(elementType);
            else elementFormatter = AddTypeFormatter(elementType);

            if(elementFormatter == null)
            {
                Error($"Collection {collectionType} is unformattable because its element type {elementType} is unformattable.");
                return null;
            }


            CollectionFormatter collectionFormatter = new(this, elementFormatter, elementType, collectionType);
            NullPrefixFormatter nullableCollectionFormatter = new(this, collectionFormatter);
            formatters.Add(collectionType,nullableCollectionFormatter);

            Info($"Created Collection formatter for type {collectionType}");

            return nullableCollectionFormatter;
        }
        private IFormattable? AddNullableFormatter(Type nullableType)
        {
            Type? underlyingType = Nullable.GetUnderlyingType(nullableType);

            if(underlyingType == null)
            {
                Error($"Nullable {nullableType.FullName} is unformattable. Failed to get the underlyin type of the nullable type {nullableType}.");
                return null;
            }

            IFormattable? underlyingTypeFormatter;
            if (Formattable(underlyingType)) underlyingTypeFormatter = GetFormatter(underlyingType);
            else underlyingTypeFormatter = AddTypeFormatter(underlyingType);

            if (underlyingTypeFormatter == null)
            {
                Error($"Nullable {nullableType.FullName} is unformattable because its underlying type {underlyingType.FullName} is unformattable.");
                return null;
            }

            NullPrefixFormatter nullPrefixFormatter = new(this, underlyingTypeFormatter);

            formatters.Add(nullableType, nullPrefixFormatter);
            Info($"Created Nullable formatter for type {nullableType}");
            return nullPrefixFormatter;
        }
        private IFormattable? AddStructFormatter(Type structType)
        {
            if (!Flagged(structType, true))
            {
                Error($"Struct {structType.FullName} is unformattable because is not flagged with [{AttributeFlag.Name}].");
                return null;
            }
            ContainerFormatter containerFormatter = CreateContainerFormatter(structType);
            formatters.Add(structType, containerFormatter);
            Info($"Created Struct formatter for type {structType}");
            return containerFormatter;
        }
        private IFormattable? AddClassFormatter(Type classType)
        {
            if (!Flagged(classType, true))
            {
                Error($"Class {classType.FullName} is unformattable because is not flagged with [{AttributeFlag.Name}].");
                return null;
            }
            ContainerFormatter containerFormatter = CreateContainerFormatter(classType);
            NullPrefixFormatter nullPrefixFormatter = new(this, containerFormatter);
            formatters.Add(classType, nullPrefixFormatter);
            Info($"Created Class formatter for type {classType}");
            return nullPrefixFormatter;
        }
        private IFormattable? AddArrayFormatter(Type arrayType)
        {
            Type? elementType = arrayType.GetElementType();

            if (elementType == null)
            {
                Error($"Array {arrayType} is unformattable. Failed to get the element Type of the array type {arrayType}.");
                return null;
            }

            IFormattable? elementFormatter;
            if (Formattable(elementType)) elementFormatter = GetFormatter(elementType);
            else elementFormatter = AddTypeFormatter(elementType);

            if (elementFormatter == null)
            {
                Error($"Array {arrayType} is unformattable because its element type {elementType} is unformattable.");
                return null;
            }

            CollectionFormatter collectionFormatter = new(this, elementFormatter, elementType, arrayType);
            NullPrefixFormatter arrayFormatter = new(this, collectionFormatter);

            formatters.Add(arrayType,arrayFormatter);
            Info($"Created Array formatter for type {arrayType}");

            return arrayFormatter;
        }
        private IFormattable? AddEnumFormatter(Type enumType)
        {
            EnumFormatter enumFormatter = new(this, enumType);
            formatters.Add(enumType, enumFormatter);
            Info($"Created Enum formatter for type {enumType}");
            return enumFormatter;
        }
        private IFormattable? AddTypeFormatter(Type type)
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

            Type? representantType = GetRepresentantType(type);
            if(representantType != null) return AddRepresentedType(representantType);

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

                if(type.FullName == null) return null;
                if (!type.FullName.StartsWith("System.")) return AddStructFormatter(type);
            }

            Error($"Attempt to create formatter for Type {type.FullName} failed. (Unsupported or Unimplemented)");
            return null;
        }


        public Action<object,BinaryFormatter> infoEvent, errorEvent, warningEvent, eventEvent;
        protected void Info(object message) => infoEvent?.Invoke(message, this);
        protected void Warn(object message) => warningEvent?.Invoke(message, this);
        protected void Error(object message) => errorEvent?.Invoke(message, this);
        protected void Event(object message) => eventEvent?.Invoke(message, this);
    }
}