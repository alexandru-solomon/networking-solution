using System.IO;
using System;

namespace Lithium.Serialization
{
    internal enum SchemaType
    {
        Class, Struct, Enum, Array,
        Collection, StructRedefinition,
        ClassRedefinition, Nullable
    }
    internal abstract class ObjectSchema
    {
        public readonly bool NullSupport;
        public readonly Type Type;
        public readonly SchemaType SchemaType;

        protected ObjectSchema(SchemaType schemaType, Type type, bool nullSupport)
        {
            Type = type;
            NullSupport = nullSupport;
            SchemaType = schemaType;
        }

        public abstract void Serialize(BinaryWriter writer, object? data);
        public abstract object? Deserialize(BinaryReader reader);

        public override string ToString() => Type.ToString();
    }
}
