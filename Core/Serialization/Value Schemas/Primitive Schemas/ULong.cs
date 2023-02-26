
namespace Lithium.Serialization
{
    using System.IO;

    internal partial class DataSchema
    {
        private sealed partial class ValueSchema : TypeSchema
        {
            internal sealed class ULong : ValueSerializer
            {
                private ULong():base(typeof(ulong), StructureType.Primitive) { }
                internal static ValueSchema Create(DataSchema dataSchema)
                {
                    return new ValueSchema(dataSchema, new ULong());
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