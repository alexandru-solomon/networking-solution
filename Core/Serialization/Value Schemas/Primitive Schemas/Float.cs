
namespace Lithium.Serialization
{
    using System.IO;

    internal partial class DataSchema
    {
        private sealed partial class ValueSchema : TypeSchema
        {
            internal sealed class Float : StructuredSerializer
            {
                private Float() { }
                internal static ValueSchema Create()
                {
                    return new ValueSchema(StructureType.Struct, typeof(float), new Float());
                }

                public override object Deserialize(BinaryReader reader)
                {
                    return reader.ReadSingle();
                }
                public override void Serialize(BinaryWriter writer, object obj)
                {
                    writer.Write((float)obj);
                }
            }
        }
    } 
}