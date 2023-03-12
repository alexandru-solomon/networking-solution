
namespace Lithium.Serialization
{
    using System.IO;

    internal partial class DataSchema
    {
        private sealed partial class ValueSchema : TypeSchema
        {
            internal sealed class UInt : ValueSerializer
            {
                private UInt():base(typeof(uint),StructureType.Primitive) { }
                internal static ValueSchema Create(DataSchema dataSchema)
                {
                    return new ValueSchema(dataSchema, new UInt());
                }

                public override object Deserialize(BinaryReader reader)
                {
                    return reader.ReadUInt32();
                }
                public override void Serialize(BinaryWriter writer, object obj)
                {
                    writer.Write((uint)obj);
                }
            }
        }
    }
}