
namespace Lithium.Serialization
{
    using System.IO;

    internal partial class DataSchema
    {
        private sealed partial class ValueSchema : TypeSchema
        {
            internal sealed class Byte : ValueSerializer
            {
                private Byte():base(typeof(byte), StructureType.Primitive) { }
                internal static ValueSchema Create(DataSchema dataSchema)
                {
                    return new ValueSchema(dataSchema, new Byte());
                }

                public override object Deserialize(BinaryReader reader)
                {
                    return reader.ReadByte();
                }
                public override void Serialize(BinaryWriter writer, object obj)
                {
                    writer.Write((byte)obj);
                }
            }
        }
    }
}