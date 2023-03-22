using System.IO;
using System;

namespace Lithium.Serialization
{
    internal partial class DataSchema
    {
        internal sealed partial class NullableSchema : TypeSchema
        {
            private NullableSchema(DataSchema dataSchema, ValueSerializer valueSerializer) : base(dataSchema, valueSerializer, true) { }

            public static void TryCreate(DataSchema dataSchema, Type nullableType, out bool isNullable, out bool isValid, out TypeSchema? nullableSchema)
            {
                isNullable = false; isValid = false; nullableSchema = null;

                Type underlyingType = Nullable.GetUnderlyingType(nullableType);
                if (underlyingType == null) return; else isNullable = true;

                bool validSchema = dataSchema.TryGetTypeSchema(underlyingType, out TypeSchema? underlyingTypeSchema);

                ValueSchema? valueSchema = (ValueSchema?)underlyingTypeSchema;
                if ((validSchema == false) || (valueSchema == null))
                {
                    dataSchema.Log.Error($"{nullableType} is not serializable because its underlaying type {underlyingType} is not serializable");
                    return;
                }

                nullableSchema = new NullableSchema(dataSchema, valueSchema.ValueSerializer);
                isValid = true;
            }

            public override object? Deserialize(BinaryReader reader)
            {
                if (reader.ReadBoolean())
                {
                    return ValueSerializer.Deserialize(reader);
                }
                else return null;
            }

            public override void Serialize(BinaryWriter writer, object? value)
            {
                if (value == null)
                {
                    writer.Write(false);
                }
                else
                {
                    writer.Write(true);
                    ValueSerializer.Serialize(writer, value);
                }
            }
        }
    }
}