
namespace Lithium.Serialization
{
    using System.IO;

    internal partial class DataSchema
    {
        private sealed partial class ValueSchema : TypeSchema
        {
            internal sealed class UInt : StructuredSerializer
            {
                private UInt() { }
                internal static ValueSchema Create()
                {
                    return new ValueSchema(StructureType.Struct, typeof(uint), new UInt());
                }

                public override object Deserialize(BinaryReader reader)
                {
                    return reader.ReadUInt32();
                }
                public override void Serialize(BinaryWriter writer, object obj)
                {
                    writer.Write((uint)obj);
                }
            }
        }
    }
}