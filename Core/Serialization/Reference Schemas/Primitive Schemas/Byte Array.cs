
namespace Lithium.Serialization
{
    using System.IO;
    using System.Collections.Generic;

    internal partial class DataSchema
    {
        private sealed partial class ReferenceSchema : TypeSchema
        {
            internal sealed class ByteArray : PrimitiveSerializer
            {
                private ByteArray() { }
                internal static ReferenceSchema Create(List<object> referenceBuffer)
                {
                    return new ReferenceSchema(StructureType.Class, typeof(byte[]), new ByteArray(), referenceBuffer);
                }

                public override object Deserialize(BinaryReader reader)
                {
                    int lenght = reader.ReadInt32();
                    return reader.ReadBytes(lenght);
                }
                public override void Serialize(BinaryWriter writer, object obj)
                {
                    byte[] bytes = (byte[])obj;
                    writer.Write(bytes.Length);
                    writer.Write(bytes);
                }
            }
        }
    }
}