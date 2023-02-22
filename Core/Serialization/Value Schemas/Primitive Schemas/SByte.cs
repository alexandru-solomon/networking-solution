
namespace Lithium.Serialization
{
    using System.IO;

    internal partial class DataSchema
    {
        private sealed partial class ValueSchema : TypeSchema
        {
            internal sealed class SByte : StructuredSerializer
            {
                private SByte() { }
                internal static ValueSchema Create()
                {
                    return new ValueSchema(StructureType.Struct, typeof(sbyte), new SByte());
                }

                public override object Deserialize(BinaryReader reader)
                {
                    return reader.ReadSByte();
                }
                public override void Serialize(BinaryWriter writer, object obj)
                {
                    writer.Write((sbyte)obj);
                }
            }
        }
    }
}