
namespace Lithium.Serialization
{
    using System.IO;

    internal partial class DataSchema
    {
        private sealed partial class ValueSchema : TypeSchema
        {
            internal sealed class Decimal : StructuredSerializer
            {
                private Decimal() { }
                internal static ValueSchema Create()
                {
                    return new ValueSchema(StructureType.Struct, typeof(decimal), new Decimal());
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