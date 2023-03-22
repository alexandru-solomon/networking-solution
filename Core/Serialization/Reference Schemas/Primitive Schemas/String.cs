
namespace Lithium.Serialization
{
    using System.IO;

    internal partial class DataSchema
    {
        private sealed partial class ReferenceSchema : TypeSchema
        {
            internal sealed class String : ValueSerializer
            {
                private String(): base(typeof(string), StructureType.Primitive) { }
                internal static ReferenceSchema Create(DataSchema dataSchema)
                {
                    return new ReferenceSchema(dataSchema, new String());
                }

                public override object Deserialize(BinaryReader reader)
                {
                    return reader.ReadString();
                }
                public override void Serialize(BinaryWriter writer, object value)
                {
                    writer.Write((string)value);
                }
            }
        }
    }
}