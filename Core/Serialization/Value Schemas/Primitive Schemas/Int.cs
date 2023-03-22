
namespace Lithium.Serialization
{
    using System.IO;

    internal partial class DataSchema
    {
        private sealed partial class ValueSchema : TypeSchema
        {
            internal sealed class Int : ValueSerializer
            {
                private Int():base(typeof(int), StructureType.Primitive) { }
                internal static ValueSchema Create(DataSchema dataSchema)
                {
                    return new ValueSchema(dataSchema, new Int());
                }

                public override object Deserialize(BinaryReader reader)
                {
                    return reader.ReadInt32();
                }
                public override void Serialize(BinaryWriter writer, object obj)
                {
                    writer.Write((int)obj);
                }
            }
        }
    }
}