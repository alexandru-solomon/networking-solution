
namespace Lithium.Serialization
{
    using System.IO;

    internal partial class DataSchema
    {
        private sealed partial class ValueSchema : ObjectSchema
        {
            internal sealed class Long : TypeSchema
            {
                private Long() { }
                internal static ValueSchema Create()
                {
                    return new ValueSchema(SchemaType.Struct, typeof(long), new Long());
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