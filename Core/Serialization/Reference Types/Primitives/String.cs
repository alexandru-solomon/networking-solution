
namespace Lithium.Serialization
{
    using System.IO;
    using System.Collections.Generic;
    internal partial class DataSchema
    {
        private partial class ReferenceSchema : ObjectSchema
        {
            internal class String : TypeSchema
            {
                private String() { }
                internal static ReferenceSchema Create(List<object> referenceBuffer)
                {
                    return new ReferenceSchema(SchemaType.Class, typeof(string), new String(), referenceBuffer);
                }

                public override object Deserialize(BinaryReader reader)
                {
                    return reader.ReadString();
                }
                public override void Serialize(BinaryWriter writer, object reference)
                {
                    writer.Write((string)reference);
                }
            }
        }
    }
}