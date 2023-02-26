
namespace Lithium.Serialization
{
    using System.IO;

    internal partial class DataSchema
    {
        private sealed partial class ValueSchema : TypeSchema
        {
            internal sealed class Char : ValueSerializer
            {
                private Char():base(typeof(char),StructureType.Primitive) { }
                internal static ValueSchema Create(DataSchema dataSchema)
                {
                    return new ValueSchema(dataSchema, new Char());
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