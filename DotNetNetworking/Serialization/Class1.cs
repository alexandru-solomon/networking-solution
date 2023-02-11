

namespace Networking.Serialization
{
    using Reader = System.IO.BinaryReader;
    using Writer = System.IO.BinaryWriter;
    using System.Collections.Generic;
    using Networking.Logging;
    using System.Reflection;
    using System;

    internal static class SerializationUtility 
    {
        internal enum SchemaTypes { Primitive, Class, Struct, Enum, Array, Collection, Redefinition, Nullable, }

        internal abstract class Schema
        {
            public readonly SchemaTypes SchemaType;
            public readonly bool NullSupport;
            public readonly Type Type;
            
            public Schema(SchemaTypes schemaType, Type type, bool nullSupport)
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
            public RedefinitionSchema(Type type, bool nullSupport) : base(SchemaTypes.Redefinition, type, nullSupport)
            {

            }

            public override object Deserialize(Reader reader)
            {
                throw new NotImplementedException();
            }

            public override void Serialize(Writer writer, object obj)
            {
                throw new NotImplementedException();
            }
        }
        internal sealed class EnumeratorSchema : Schema
        {
            public EnumeratorSchema(Type enumType) : base(SchemaTypes.Enum, enumType, false) { }

            public override object Deserialize(Reader reader)
            {
                return Enum.ToObject(Type, reader.ReadInt32());
            }

            public override void Serialize(Writer writer, object obj)
            {
                writer.Write((int)obj);
            }
        }
        internal sealed class ContainerSchema : Schema
        {
            public class FieldSchema
            {
                readonly FieldInfo field;
                readonly Schema schema;

                public FieldSchema(FieldInfo field, Schema serializer)
                {
                    this.field = field;
                    this.schema = serializer;
                }

                public void Serialize(object classInstance, Writer writer)
                {
                    schema.Serialize(field.GetValue(classInstance), writer);
                }
            }

            public readonly FieldSchema[] fieldSchemas;

            public ContainerSchema(SchemaTypes collectionSchemaType, Type containerType) : base(collectionSchemaType, containerType, false)
            {
                var fieldsInfo = Type.GetFields();
                var fieldSchemas = new List<FieldSchema>();

                foreach(FieldInfo fieldInfo in fieldsInfo)
                {
                    GetSchema(Type, out var schema);

                    if(schema == null)
                    {

                        fieldSchemas.Add(new FieldSchema(fieldInfo, schema));
                    }
                }
                
            }
      
            public override void Serialize(Writer writer, object? obj)
            {
                

            }
            public override object Deserialize(Reader reader)
            {
                throw new NotImplementedException();
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

            public override object Deserialize(Reader reader)
            {
                return readMethod(reader);
            }

            public override void Serialize(Writer writer, object value)
            {
                writeMethod(writer,value);
            }
        }
        internal sealed class NullableSchema : Schema
        {
            public readonly Schema ValueSchema;

            public NullableSchema(Type nullableType, Schema valueSchema) : base(SchemaTypes.Nullable, nullableType, true)
            {
                ValueSchema = valueSchema;
            }
            public NullableSchema(Schema valueSerializer) : base(valueSerializer.SchemaType,valueSerializer.Type, true)
            {
                ValueSchema = valueSerializer;
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

            public override string ToString() => Type.ToString() + "?";
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
            Add<byte>((Writer writer, object value) =>  writer.Write((byte)value), (Reader reader) => { return reader.ReadByte(); }, false);
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
            (Reader reader) => { return reader.ReadBytes(reader.ReadInt32()); },  true);

            static void Add<T>(PrimitiveSchema.WriteMethod writeMethod, PrimitiveSchema.ReadMethod readMethod, bool nullSupport)
            {
                PrimitiveSchema primitiveSchema = new PrimitiveSchema(typeof(T) , writeMethod,readMethod);
                
                if (nullSupport)
                {
                    AddSchema(primitiveSchema);
                }
                else
                {
                    NullableSchema nullableSchema = new NullableSchema(primitiveSchema);
                    AddSchema(nullableSchema);
                }
            }
        }
        private static void AddSchema(Schema schema)
        {
            schemas.Add(schema.Type,schema);
        }

        /*
        public static bool HasSchema(Type type)
        {
            return schemas.ContainsKey(type);
        }
        public static bool GetSchema(Type type, out Schema? schema)
        {
            return schemas.TryGetValue(type, out schema);
        }
        public static bool AddSchema(Type type, out Schema? schema)
        {
            error = string.Empty; schema = null;
            return false; 
        }
        public static bool Supported(Type type, out SchemaTypes? structure)
        {
            structure = SchemaTypes.Primitive;
            return false;
        }
        */
    }
}