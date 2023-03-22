
namespace Lithium.Serialization
{
    using System.IO;

    internal partial class DataSchema
    {
        private sealed partial class ValueSchema : TypeSchema
        {
            internal sealed class Double : ValueSerializer
            {
                private Double():base(typeof(double), StructureType.Primitive) { }
                internal static ValueSchema Create(DataSchema dataSchema)
                {
                    return new ValueSchema(dataSchema, new Double());
                }

                public override object Deserialize(BinaryReader reader)
                {
                    return reader.ReadDouble();
                }
                public override void Serialize(BinaryWriter writer, object obj)
                {
                    writer.Write((double)obj);
                }
            }
        }
    } 
}