
namespace Lithium.Serialization
{
    using System.IO;

    internal partial class DataSchema
    {
        private sealed partial class ValueSchema : ObjectSchema
        {
            internal sealed class Char : TypeSchema
            {
                private Char() { }
                internal static ValueSchema Create()
                {
                    return new ValueSchema(SchemaType.Struct, typeof(char), new Char());
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