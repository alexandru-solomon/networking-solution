

namespace Networking.Serialization
{
    using Reader = System.IO.BinaryReader;
    using Writer = System.IO.BinaryWriter;
    using System.Collections.Generic;
    using Networking.Logging;
    using System.Reflection;
    using System;
    using System.Collections;
    using System.Reflection.Emit;

    internal static class SerializationUtility
    {
        internal enum SchemaTypes { Primitive, Class, Struct, Enum, Array, Collection, StructRedefinition, ClassRedefinition, Nullable, }

        internal abstract class Schema
        {
            public readonly SchemaTypes SchemaType;
            public readonly bool NullSupport;
            public readonly Type Type;

            protected Schema(SchemaTypes schemaType, Type type, bool nullSupport)
            {
                Type = type;
                NullSupport = nullSupport;
                SchemaType = schemaType;
            }

            public abstract void Serialize(Writer writer, object obj);
            public abstract object? Deserialize(Reader reader);

            public override string ToString() => Type.ToString();
        }
        internal sealed class RedefinitionSchema : Schema
        {
            private RedefinitionSchema(Type type, SchemaTypes collectionType, bool nullSupport) : base(collectionType, type, nullSupport)
            {

            }
            public static void TryCreate(Type type, out bool isRedefinition, out bool isValid, out Schema? redefinitionSchema)
            {
                isRedefinition = false; isValid = false; redefinitionSchema = null;

                Type? redefinitionInterfaceType = null;
                foreach (var @interface in type.GetInterfaces())
                {
                    if (@interface == typeof(IRedefinition<>))
                    {
                        redefinitionInterfaceType = @interface;
                        break;
                    }
                }

                if (redefinitionInterfaceType == null) return;
                isRedefinition = true;

                Type redefinedType = redefinitionInterfaceType.GetGenericArguments()[0];

                TryCreateSchema(type, out var redefinitionTypeSchema);



                if (redefinitionInterface.GetGenericTypeDefinition() != typeof(IRedefinition<>))
                {
                    Log.Error($"The first interface of the representant Type {} should be IRepresentant<\"representedTypeName\">");
                    return;
                }
                Type redefinedType = redefinitionInterface.GetGenericArguments()[0];
                if (redefinitionType == redefinedType)
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


                Type genericRedefinitionType = typeof(IRedefinition<>).MakeGenericType(redefinedType);
                var redefinerMethod = genericRedefinitionType.GetMethod(nameof(IRedefinition<Type>.GetRedefined));
            }

            public override void Serialize(Writer writer, object obj)
            {
                throw new NotImplementedException();
            }
            public override object Deserialize(Reader reader)
            {
                throw new NotImplementedException();
            }
        }
        internal sealed class EnumerationSchema : Schema
        {
            private EnumerationSchema(Type enumType) : base(SchemaTypes.Enum, enumType, false) { }
            public static void TryCreate(Type type, out bool isEnum, out bool isValid, out Schema? enumeratorSchema)
            {
                enumeratorSchema = null;

                isEnum = type.IsEnum;
                isValid = type.IsEnum;

                if (!isValid) return;

                enumeratorSchema = new EnumerationSchema(type);
            }

            public override void Serialize(Writer writer, object obj)
            {
                writer.Write((int)obj);
            }
            public override object Deserialize(Reader reader)
            {
                return Enum.ToObject(Type, reader.ReadInt32());
            }
        }
        internal sealed class ContainerSchema : Schema
        {
            public class FieldSchema
            {
                readonly FieldInfo field;
                readonly Schema schema;
                public FieldSchema(FieldInfo fieldInfo, Schema fieldTypeSchema)
                {
                    schema = fieldTypeSchema;
                    field = fieldInfo;
                }
                public void Serialize(Writer writer, object instance)
                {
                    schema.Serialize(writer, field.GetValue(instance));
                }
                public void Deserialize(Reader reader, object instance)
                {
                    field.SetValue(instance, schema.Deserialize(reader));
                }
            }

            public readonly FieldSchema[] fieldSchemas;

            private ContainerSchema(SchemaTypes collectionSchemaType, Type containerType) : base(collectionSchemaType, containerType, false)
            {
                var fieldsInfo = Type.GetFields();
                var fieldSchemas = new List<FieldSchema>();

                foreach (FieldInfo fieldInfo in fieldsInfo)
                {
                    Type fieldType = fieldInfo.FieldType;

                    bool SchemaExists = TryGetSchema(fieldType, out var fieldTypeSchema);
                    if (!SchemaExists) TryCreateSchema(fieldType, out fieldTypeSchema);

                    if (fieldTypeSchema == null)
                    {
                        Log.Error($"Field \"{fieldInfo.FieldType} {fieldInfo.Name}\" from the {containerType} {collectionSchemaType} will be ignored.");
                        continue;
                    }

                    fieldSchemas.Add(new FieldSchema(fieldInfo, fieldTypeSchema));
                }

                this.fieldSchemas = fieldSchemas.ToArray();
            }
            public static bool TryCreate(Type type, out bool isContainer, out bool isValid, out Schema? schema)
            {
                isContainer = false; isValid = false; schema = null;

                if (type.IsClass)
                {
                    isValid = true; isContainer = true;

                    ContainerSchema valueSchema = new ContainerSchema(SchemaTypes.Class, type);
                    NullableSchema classSchema = new NullableSchema(valueSchema);

                    schema = classSchema;
                    return true;
                }
                if (type.IsValueType)
                {
                    isValid = true; isContainer = true;

                    ContainerSchema structSchema = new ContainerSchema(SchemaTypes.Struct, type);

                    schema = structSchema;
                    return true;
                }

                return false;
            }

            public override void Serialize(Writer writer, object containerInstance)
            {
                for (int fieldId = 0; fieldId < fieldSchemas.Length; fieldId++)
                {
                    fieldSchemas[fieldId].Serialize(writer, containerInstance);
                }
            }
            public override object Deserialize(Reader reader)
            {
                object containerInstance = Activator.CreateInstance(Type);
                for(int fieldId = 0; fieldId<fieldSchemas.Length; fieldId++)
                {
                    fieldSchemas[fieldId].Deserialize(reader, containerInstance);
                }
                return containerInstance;
            }
        }
        internal sealed class PrimitiveSchema : Schema
        {
            public delegate void WriteMethod(Writer writerInstance, object value);
            public delegate object ReadMethod(Reader readerInstance);

            private readonly WriteMethod writeMethod;
            private readonly ReadMethod readMethod;

            public PrimitiveSchema(Type type, WriteMethod writeMethod, ReadMethod readMethod) : base(SchemaTypes.Primitive, type, false)
            {
                this.writeMethod = writeMethod;
                this.readMethod = readMethod;
            }
            public override void Serialize(Writer writer, object value)
            {
                writeMethod(writer, value);
            }
            public override object Deserialize(Reader reader)
            {
                return readMethod(reader);
            }
        }
        internal sealed class NullableSchema : Schema
        {
            public readonly Schema ValueSchema;

            internal NullableSchema(Schema valueSerializer) : base(valueSerializer.SchemaType, valueSerializer.Type, true)
            {
                ValueSchema = valueSerializer;
            }
            private NullableSchema(Type nullableType, Schema valueSchema) : base(SchemaTypes.Nullable, nullableType, true)
            {
                ValueSchema = valueSchema;
            }
            public static void TryCreate(Type type, out bool isNullable, out bool isValid, out Schema? nullableSchema)
            {
                isNullable = false; isValid = false; nullableSchema = null;

                Type underlyingType = Nullable.GetUnderlyingType(type);
                if (underlyingType == null) return; else isNullable = true;

                if (TryGetSchema(underlyingType, out Schema? underlyingTypeSchema) == false)
                    TryCreateSchema(underlyingType, out underlyingTypeSchema);

                if (underlyingTypeSchema == null)
                {
                    Log.Error($"{type} is not serializable because its underlaying type {underlyingType} is not serializable");
                    return;
                }

                isValid = true;
                nullableSchema = new NullableSchema(underlyingTypeSchema);
            }

            public override void Serialize(Writer writer, object? obj)
            {
                if (obj == null)
                {
                    writer.Write(false);
                    return;
                }
                else
                {
                    writer.Write(true);
                    ValueSchema.Serialize(writer, obj);
                }
            }
            public override object? Deserialize(Reader reader)
            {
                if (reader.ReadBoolean())
                {
                    return ValueSchema.Deserialize(reader);
                }
                else return null;
            }

            public override string ToString() => base.ToString() + "?";
        }
        internal sealed class CollectionSchema : Schema
        {
            private readonly Schema elementTypeSchema;
            public CollectionSchema(Type collectionType, Schema elementTypeSchema) : base(SchemaTypes.Collection, collectionType, false)
            {
                this.elementTypeSchema = elementTypeSchema;
            }
            public static void TryCreate(Type type, out bool isCollection, out bool isValid, out Schema? collectionSchema)
            {
                isValid = false; collectionSchema = null;

                isCollection = typeof(ICollection).IsAssignableFrom(type); if (!isCollection) return;

                Type elementType = type.GetGenericArguments()[0];

                if (TryGetSchema(elementType, out Schema? elementTypeSchema) == false)
                    TryCreateSchema(elementType, out elementTypeSchema);

                if (elementTypeSchema == null)
                {
                    Log.Error($"{type} is not serializable because its element type {elementType} is not serializable");
                    return;
                }

                isValid = true;
                collectionSchema = new CollectionSchema(type, elementTypeSchema);
            }

            public override void Serialize(Writer writer, object collectionInstance)
            {
                ICollection collection = (ICollection)collectionInstance;
                IEnumerable enumerator = (IEnumerable)collectionInstance;
                writer.Write(collection.Count);
                foreach (var element in enumerator) elementTypeSchema.Serialize(writer,element);
            }
            public override object Deserialize(Reader reader)
            {
                int length = reader.ReadInt32();
                Array array = Array.CreateInstance(elementTypeSchema.Type, length);
                for (int elementId = 0; elementId < length; elementId++)
                    array.SetValue(elementTypeSchema.Deserialize(reader), elementId);

                if (elementTypeSchema.Type.IsArray) return array;
                else return Activator.CreateInstance(Type, array);
            }
        }

        internal static readonly Log Log;
        private static readonly Dictionary<Type, Schema> schemas;

        static SerializationUtility()
        {
            Log = new Log(nameof(SerializationUtility));
            schemas = new Dictionary<Type, Schema>();

            AddPrimitives();
        }

        private static void AddPrimitives()
        {
            Add<bool>((Writer writer, object value) => writer.Write((bool)value), (Reader reader) => { return reader.ReadBoolean(); }, false);
            Add<byte>((Writer writer, object value) => writer.Write((byte)value), (Reader reader) => { return reader.ReadByte(); }, false);
            Add<sbyte>((Writer writer, object value) => writer.Write((sbyte)value), (Reader reader) => { return reader.ReadSByte(); }, false);
            Add<short>((Writer writer, object value) => writer.Write((short)value), (Reader reader) => { return reader.ReadInt16(); }, false);
            Add<ushort>((Writer writer, object value) => writer.Write((ushort)value), (Reader reader) => { return reader.ReadUInt16(); }, false);
            Add<int>((Writer writer, object value) => writer.Write((int)value), (Reader reader) => { return reader.ReadInt32(); }, false);
            Add<uint>((Writer writer, object value) => writer.Write((uint)value), (Reader reader) => { return reader.ReadUInt32(); }, false);
            Add<long>((Writer writer, object value) => writer.Write((long)value), (Reader reader) => { return reader.ReadInt64(); }, false);
            Add<ulong>((Writer writer, object value) => writer.Write((ulong)value), (Reader reader) => { return reader.ReadUInt64(); }, false);
            Add<float>((Writer writer, object value) => writer.Write((float)value), (Reader reader) => { return reader.ReadSingle(); }, false);
            Add<double>((Writer writer, object value) => writer.Write((double)value), (Reader reader) => { return reader.ReadDouble(); }, false);
            Add<decimal>((Writer writer, object value) => writer.Write((decimal)value), (Reader reader) => { return reader.ReadDecimal(); }, false);
            Add<char>((Writer writer, object value) => writer.Write((char)value), (Reader reader) => { return reader.ReadChar(); }, false);
            
            Add<string>((Writer writer, object value) => writer.Write((string)value), (Reader reader) => { return reader.ReadString(); }, true);

            Add<char[]>((Writer writer, object value) => { writer.Write(((char[])value).Length); writer.Write((char[])value); },
            (Reader reader) => { return reader.ReadChars(reader.ReadInt32()); }, true);

            Add<byte[]>((Writer writer, object value) => { writer.Write(((byte[])value).Length); writer.Write((byte[])value); },
            (Reader reader) => { return reader.ReadBytes(reader.ReadInt32()); }, true);

            static void Add<T>(PrimitiveSchema.WriteMethod writeMethod, PrimitiveSchema.ReadMethod readMethod, bool nullSupport)
            {
                PrimitiveSchema primitiveSchema = new PrimitiveSchema(typeof(T), writeMethod, readMethod);

                bool success;
                if (nullSupport)
                {
                    success = AddSchema(primitiveSchema);
                }
                else
                {
                    NullableSchema nullableSchema = new NullableSchema(primitiveSchema);
                    success = AddSchema(nullableSchema);
                }
                if (!success) Log.Error($"Failed to add {primitiveSchema.Type} primitive Schema.");
            }
        }
        private static bool AddSchema(Schema schema)
        {
            bool success = schemas.TryAdd(schema.Type, schema);
            if (success) Log.Info(schema.ToString());
            else Log.Error($"{schema} already added.");
            return success;
        }

        public static bool TryGetSchema(Type type, out Schema? schema)
        {
            return schemas.TryGetValue(type, out schema);
        }
        public static bool TryCreateSchema(Type type, out Schema? schema)
        {
            if (TryGetSchema(type, out schema)) { Log.Warn($"Attempt to create Schema for {type} when already existent."); return true; }

            NullableSchema.TryCreate(type, out bool isNullable, out bool nullableValid, out schema); if (isNullable) return nullableValid;
            EnumerationSchema.TryCreate(type, out bool isEnum, out bool enumValid, out schema); if (isEnum) return enumValid;
            ContainerSchema.TryCreate(type, out bool isContainer, out bool containerValid, out schema); return containerValid;
        }
    }
}

//Add the schemas when created
//fix the stack overflow when creating redefined schema
//Test
//make as instance