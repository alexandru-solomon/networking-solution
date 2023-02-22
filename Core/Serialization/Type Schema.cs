using System.IO;
using System;

namespace Lithium.Serialization
{
    internal enum StructureType
    {
        Class, Struct, Enum, Array,
        Collection, StructRedefinition,
        ClassRedefinition, Nullable, Primitive
    }
    internal abstract class TypeSchema
    {
        public readonly DataSchema DataSchema;
        public readonly bool NullSupport;
        public Type Type { get { return ValueSerializer.valueType; } }
        public StructureType StructureType { get { return ValueSerializer.StructureType; } }

        protected readonly ValueSerializer ValueSerializer;

        protected TypeSchema(DataSchema dataSchema,ValueSerializer valueSerializer, bool nullSupport)
        {
            DataSchema = dataSchema;
            ValueSerializer = valueSerializer;
            NullSupport = nullSupport;
        }

        public abstract void Serialize(BinaryWriter writer, object? data);
        public abstract object? Deserialize(BinaryReader reader);

        public override string ToString() => Type.ToString();
    }
}
