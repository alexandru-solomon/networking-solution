
namespace Lithium.Serialization
{
    using System.IO;

    internal partial class DataSchema
    {
        private sealed partial class ValueSchema : ObjectSchema
        {
            internal sealed class ULong : TypeSchema
            {
                private ULong() { }
                internal static ValueSchema Create()
                {
                    return new ValueSchema(SchemaType.Struct, typeof(ulong), new ULong());
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