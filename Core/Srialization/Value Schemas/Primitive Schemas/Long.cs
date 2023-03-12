
namespace Lithium.Serialization
{
    using System.IO;

    internal partial class DataSchema
    {
        private sealed partial class ValueSchema : TypeSchema
        {
            internal sealed class Long : ValueSerializer
            {
                private Long():base(typeof(long),StructureType.Primitive) { }
                internal static ValueSchema Create(DataSchema dataSchema)
                {
                    return new ValueSchema(dataSchema,new Long());
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