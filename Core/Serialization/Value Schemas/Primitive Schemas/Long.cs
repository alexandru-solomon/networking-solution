
namespace Lithium.Serialization
{
    using System.IO;

    internal partial class DataSchema
    {
        private sealed partial class ValueSchema : TypeSchema
        {
            internal sealed class Long : StructuredSerializer
            {
                private Long() { }
                internal static ValueSchema Create()
                {
                    return new ValueSchema(StructureType.Struct, typeof(long), new Long());
                }

                public override object Deserialize(BinaryReader reader)
                {
                    return reader.ReadInt64();
                }
                public override void Serialize(BinaryWriter writer, object obj)
                {
                    writer.Write((long)obj);
                }
            }
        }
    }
}