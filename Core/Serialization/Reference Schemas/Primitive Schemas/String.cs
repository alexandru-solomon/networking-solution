
namespace Lithium.Serialization
{
    using System.IO;
    using System.Collections.Generic;

    internal partial class DataSchema
    {
        private sealed partial class ReferenceSchema : TypeSchema
        {
            internal sealed class String : StructuredSerializer
            {
                private String() { }
                internal static ReferenceSchema Create(List<object> referenceBuffer)
                {
                    return new ReferenceSchema(StructureType.Class, typeof(string), new String(), referenceBuffer);
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