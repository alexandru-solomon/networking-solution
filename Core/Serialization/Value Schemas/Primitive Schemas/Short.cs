
namespace Lithium.Serialization
{
    using System.IO;

    internal partial class DataSchema
    {
        private sealed partial class ValueSchema : TypeSchema
        {
            internal sealed class Short : ValueSerializer
            {
                private Short():base(typeof(short), StructureType.Primitive) { }
                internal static ValueSchema Create(DataSchema dataSchema)
                {
                    return new ValueSchema(dataSchema, new Short());
                }

                public override object Deserialize(BinaryReader reader)
                {
                    return reader.ReadInt16();
                }
                public override void Serialize(BinaryWriter writer, object obj)
                {
                    writer.Write((short)obj);
                }
            }
        }
    }
}