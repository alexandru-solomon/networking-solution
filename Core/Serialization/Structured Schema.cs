
namespace Lithium.Serialization
{
    using System;
    using System.IO;
    internal abstract class StructuredSerializer : ValueSerializer
    {
        protected StructuredSerializer(Type valueType, StructureType structureType) : base(valueType, structureType) { }

        public override abstract void Serialize(BinaryWriter writer, object value);
        public override abstract object Deserialize(BinaryReader reader);
    }
}