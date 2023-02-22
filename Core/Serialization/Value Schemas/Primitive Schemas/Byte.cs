
namespace Lithium.Serialization
{
    using System.IO;

    internal partial class DataSchema
    {
        private sealed partial class ValueSchema : TypeSchema
        {
            internal sealed class Byte : StructuredSerializer
            {
                private Byte() { }
                internal static ValueSchema Create()
                {
                    return new ValueSchema(StructureType.Struct, typeof(byte), new Byte());
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