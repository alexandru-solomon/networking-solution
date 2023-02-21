
namespace Lithium.Serialization
{
    using System.IO;

    internal partial class DataSchema
    {
        private sealed partial class ValueSchema : ObjectSchema
        {
            internal sealed class Double : TypeSchema
            {
                private Double() { }
                internal static ValueSchema Create()
                {
                    return new ValueSchema(SchemaType.Struct, typeof(double), new Double());
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