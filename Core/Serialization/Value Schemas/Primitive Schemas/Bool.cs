
namespace Lithium.Serialization
{
    using System.IO;

    internal partial class DataSchema
    {
        private sealed partial class ValueSchema : TypeSchema
        {
            internal sealed class Bool : StructuredSerializer
            {
                private Bool() { }
                internal static ValueSchema Create()
                {
                    return new ValueSchema(StructureType.Struct, typeof(bool), new Bool());
                }

                public override object Deserialize(BinaryReader reader)
                {
                    return reader.ReadBoolean();
                }
                public override void Serialize(BinaryWriter writer, object obj)
                {
                    writer.Write((bool)obj);
                }
            }
        }
    }
}