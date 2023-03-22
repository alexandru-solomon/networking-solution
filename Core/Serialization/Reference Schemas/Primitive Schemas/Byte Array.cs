
namespace Lithium.Serialization
{
    using System.IO;

    internal partial class DataSchema
    {
        private sealed partial class ReferenceSchema : TypeSchema
        {
            internal sealed class ByteArray : ValueSerializer
            {
                private ByteArray() : base(typeof(byte[]), StructureType.Primitive) { }
                internal static ReferenceSchema Create(DataSchema dataSchema)
                {
                    return new ReferenceSchema(dataSchema, new ByteArray());
                }
                public override object Deserialize(BinaryReader reader)
                {
                    int lenght = reader.ReadInt32();
                    return reader.ReadBytes(lenght);
                }
                public override void Serialize(BinaryWriter writer, object reference)
                {
                    byte[] bytes = (byte[])reference;
                    writer.Write(bytes.Length);
                    writer.Write(bytes);
                }
            }
        }
    }
}