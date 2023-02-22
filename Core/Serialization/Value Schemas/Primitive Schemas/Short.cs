
namespace Lithium.Serialization
{
    using System.IO;

    internal partial class DataSchema
    {
        private sealed partial class ValueSchema : TypeSchema
        {
            internal sealed class Short : StructuredSerializer
            {
                private Short() { }
                internal static ValueSchema Create()
                {
                    return new ValueSchema(StructureType.Struct, typeof(short), new Short());
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