
namespace Lithium.Serialization
{
    using System.IO;

    internal partial class DataSchema
    {
        private sealed partial class ValueSchema : ObjectSchema
        {
            internal sealed class Byte : TypeSchema
            {
                private Byte() { }
                internal static ValueSchema Create()
                {
                    return new ValueSchema(SchemaType.Struct, typeof(byte), new Byte());
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