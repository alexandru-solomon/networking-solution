
namespace Lithium.Serialization
{
    using System.IO;

    internal partial class DataSchema
    {
        private sealed partial class ValueSchema : TypeSchema
        {
            internal sealed class Float : ValueSerializer
            {
                private Float():base(typeof(float), StructureType.Primitive) {  }
                internal static ValueSchema Create(DataSchema dataSchema)
                {
                    return new ValueSchema(dataSchema, new Float());
                }

                public override object Deserialize(BinaryReader reader)
                {
                    return reader.ReadSingle();
                }
                public override void Serialize(BinaryWriter writer, object obj)
                {
                    writer.Write((float)obj);
                }
            }
        }
    } 
}