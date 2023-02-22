
namespace Lithium.Serialization
{
    using System.IO;

    internal partial class DataSchema
    {
        private sealed partial class ValueSchema : TypeSchema
        {
            internal sealed class Double : StructuredSerializer
            {
                private Double() { }
                internal static ValueSchema Create()
                {
                    return new ValueSchema(StructureType.Struct, typeof(double), new Double());
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