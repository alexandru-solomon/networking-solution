
namespace Lithium.Serialization
{
    using System.IO;
    using System.Collections.Generic;
    internal partial class DataSchema
    {
        private partial class ReferenceSchema : ObjectSchema
        {
            internal class ByteArray : TypeSchema
            {
                private ByteArray() { }
                internal static ReferenceSchema Create(List<object> referenceBuffer)
                {
                    return new ReferenceSchema(SchemaType.Class, typeof(byte[]), new ByteArray(), referenceBuffer);
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