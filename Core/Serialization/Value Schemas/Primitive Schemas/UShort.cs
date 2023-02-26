

namespace Lithium.Serialization
{
    using System.IO;

    internal partial class DataSchema
    {
        private sealed partial class ValueSchema : TypeSchema
        {
            internal sealed class UShort : ValueSerializer
            {
                private UShort():base(typeof(ushort), StructureType.Primitive) { }
                internal static ValueSchema Create(DataSchema dataSchema)
                {
                    return new ValueSchema(dataSchema, new UShort());
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