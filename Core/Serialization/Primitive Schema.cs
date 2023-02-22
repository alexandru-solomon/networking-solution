
namespace Lithium.Serialization
{
    using System;
    using System.IO;
    internal abstract class PrimitiveSerializer : ValueSerializer
    {
        protected PrimitiveSerializer(Type valueType) : base(valueType, StructureType.Primitive) { }

        public override abstract void Serialize(BinaryWriter writer, object value);
        public override abstract object Deserialize(BinaryReader reader);
    }
    internal interface a
    {

    }
}