
namespace Lithium.Serialization
{
    using System.IO;

    internal partial class DataSchema
    {
        private sealed partial class ValueSchema : TypeSchema
        {
            internal sealed class Char : StructuredSerializer
            {
                private Char() { }
                internal static ValueSchema Create()
                {
                    return new ValueSchema(StructureType.Struct, typeof(char), new Char());
                }

                public override object Deserialize(BinaryReader reader)
                {
                    return reader.ReadChar();
                }
                public override void Serialize(BinaryWriter writer, object obj)
                {
                    writer.Write((char)obj);
                }
            }
        }
    }
}