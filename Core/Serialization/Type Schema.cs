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
        internal readonly DataSchema DataSchema;
        internal readonly bool NullSupport;
        internal Type Type { get { return ValueSerializer.ValueType; } }
        internal StructureType StructureType { get { return ValueSerializer.StructureType; } }

        internal readonly ValueSerializer ValueSerializer;

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
