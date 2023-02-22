

namespace Lithium.Serialization
{
    using System.IO;

    internal partial class DataSchema
    {
        private sealed partial class ValueSchema : TypeSchema
        {
            internal sealed class UShort : StructuredSerializer
            {
                private UShort() { }
                internal static ValueSchema Create()
                {
                    return new ValueSchema(StructureType.Struct, typeof(ushort), new UShort());
                }

                public override object Deserialize(BinaryReader reader)
                {
                    return reader.ReadUInt16();
                }
                public override void Serialize(BinaryWriter writer, object obj)
                {
                    writer.Write((ushort)obj);
                }
            }
        }
    }
}