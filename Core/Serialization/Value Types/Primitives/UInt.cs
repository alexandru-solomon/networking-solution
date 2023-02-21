
namespace Lithium.Serialization
{
    using System.IO;

    internal partial class DataSchema
    {
        private sealed partial class ValueSchema : ObjectSchema
        {
            internal sealed class UInt : TypeSchema
            {
                private UInt() { }
                internal static ValueSchema Create()
                {
                    return new ValueSchema(SchemaType.Struct, typeof(uint), new UInt());
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