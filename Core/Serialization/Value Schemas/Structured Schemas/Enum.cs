

namespace Lithium.Serialization
{
    using System;
    using System.IO;

    internal partial class DataSchema
    {
        private sealed partial class ValueSchema
        {
            internal class Enum : StructuredSerializer
            {
                private readonly Type enumType;
                private Enum(Type enumType)
                {
                    this.enumType = enumType;
                }
                public static void TryCreate(Type type, out bool isEnum, out bool isValid, out TypeSchema? enumSchema)
                {
                    enumSchema = null;

                    isEnum = type.IsEnum;
                    isValid = type.IsEnum;

                    if (!isValid) return;

                    enumSchema = new ValueSchema(StructureType.Enum, type, new Enum(type));
                }

                public override object Deserialize(BinaryReader reader)
                {
                    return System.Enum.ToObject(enumType, reader.ReadInt32());
                }
                public override void Serialize(BinaryWriter writer, object enumValue)
                {
                    writer.Write((int)enumValue);
                }
            }
        }
    }
}