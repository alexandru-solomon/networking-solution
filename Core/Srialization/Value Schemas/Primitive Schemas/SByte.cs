
namespace Lithium.Serialization
{
    using System.IO;

    internal partial class DataSchema
    {
        private sealed partial class ValueSchema : TypeSchema
        {
            internal sealed class SByte : ValueSerializer
            {
                private SByte():base(typeof(sbyte), StructureType.Primitive) { }
                internal static ValueSchema Create(DataSchema dataSchema)
                {
                    return new ValueSchema(dataSchema, new SByte());
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