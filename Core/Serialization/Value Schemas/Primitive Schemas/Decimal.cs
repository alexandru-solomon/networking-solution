
namespace Lithium.Serialization
{
    using System.IO;

    internal partial class DataSchema
    {
        private sealed partial class ValueSchema : TypeSchema
        {
            internal sealed class Decimal: ValueSerializer
            {
                private Decimal():base(typeof(decimal),StructureType.Primitive) { }
                internal static ValueSchema Create(DataSchema dataSchema)
                {
                    return new ValueSchema(dataSchema, new Decimal());
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