
namespace Lithium.Serialization
{
    using System.IO;

    internal partial class DataSchema
    {
        private sealed partial class ValueSchema : ObjectSchema
        {
            internal sealed class Decimal : TypeSchema
            {
                private Decimal() { }
                internal static ValueSchema Create()
                {
                    return new ValueSchema(SchemaType.Struct, typeof(decimal), new Decimal());
                }

                public override object Deserialize(BinaryReader reader)
                {
                    return reader.ReadDecimal();
                }
                public override void Serialize(BinaryWriter writer, object obj)
                {
                    writer.Write((decimal)obj);
                }
            }
        }
    }
}