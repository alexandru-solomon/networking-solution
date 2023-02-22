
namespace Lithium.Serialization
{
    using System.IO;

    internal partial class DataSchema
    {
        private sealed partial class ValueSchema : TypeSchema
        {
            internal sealed class ULong : StructuredSerializer
            {
                private ULong() { }
                internal static ValueSchema Create()
                {
                    return new ValueSchema(StructureType.Struct, typeof(ulong), new ULong());
                }

                public override object Deserialize(BinaryReader reader)
                {
                    return reader.ReadUInt64();
                }
                public override void Serialize(BinaryWriter writer, object obj)
                {
                    writer.Write((ulong)obj);
                }
            }
        }
    }
}