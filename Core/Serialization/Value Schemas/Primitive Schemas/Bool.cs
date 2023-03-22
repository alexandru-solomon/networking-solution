
namespace Lithium.Serialization
{
    using System.IO;

    internal partial class DataSchema
    {
        private sealed partial class ValueSchema : TypeSchema
        {
            internal sealed class Bool : ValueSerializer
            {
                private Bool():base(typeof(bool), StructureType.Primitive) { }
                internal static ValueSchema Create(DataSchema dataSchema)
                {
                    return new ValueSchema(dataSchema,new Bool());
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