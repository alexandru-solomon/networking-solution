

namespace Lithium.Serialization
{
    using System;
    using System.IO;

    internal partial class DataSchema
    {
        private sealed partial class ValueSchema : TypeSchema
        {
            internal class Enum : ValueSerializer
            {
                private Enum(Type enumType) : base(enumType, StructureType.Enum) { }
                public static void TryCreate(DataSchema dataSchema,Type enumType, out bool isEnum, out bool isValid, out TypeSchema? enumSchema)
                {
                    enumSchema = null;

                    isEnum = enumType.IsEnum;
                    isValid = enumType.IsEnum;

                    if (!isValid) return;

                    enumSchema = new ValueSchema(dataSchema,new Enum(enumType));
                }

                public override object Deserialize(BinaryReader reader)
                {
                    return System.Enum.ToObject(ValueType, reader.ReadInt32());
                }
                public override void Serialize(BinaryWriter writer, object enumValue)
                {
                    writer.Write((int)enumValue);
                }
            }
        }
    }
}