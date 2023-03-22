
using System;
using System.IO;

namespace Lithium.Serialization
{
    internal abstract class ValueSerializer
    {
        public readonly Type ValueType;
        public readonly StructureType StructureType;

        protected ValueSerializer(Type valueType, StructureType structureType)
        {
            ValueType = valueType;
            StructureType = structureType;
        }

        public abstract object Deserialize(BinaryReader reader);
        public abstract void Serialize(BinaryWriter writer, object value);
    }
}