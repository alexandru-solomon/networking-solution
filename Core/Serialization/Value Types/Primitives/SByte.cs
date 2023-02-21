
namespace Lithium.Serialization
{
    using System.IO;

    internal partial class DataSchema
    {
        private sealed partial class ValueSchema : ObjectSchema
        {
            internal sealed class SByte : TypeSchema
            {
                private SByte() { }
                internal static ValueSchema Create()
                {
                    return new ValueSchema(SchemaType.Struct, typeof(sbyte), new SByte());
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