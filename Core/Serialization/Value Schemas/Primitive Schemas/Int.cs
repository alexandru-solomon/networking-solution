
namespace Lithium.Serialization
{
    using System.IO;

    internal partial class DataSchema
    {
        private sealed partial class ValueSchema : TypeSchema
        {
            internal sealed class Int : StructuredSerializer
            {
                private Int() { }
                internal static ValueSchema Create()
                {
                    return new ValueSchema(StructureType.Struct, typeof(int), new Int());
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