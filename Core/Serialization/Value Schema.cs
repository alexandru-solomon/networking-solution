using System.IO;
using System;

namespace Lithium.Serialization
{
    internal partial class DataSchema
    {
        private partial class ValueSchema : ObjectSchema
        {
            private readonly TypeSchema typeSchema;
            private ValueSchema(SchemaType schemaType, Type type, TypeSchema typeSchema) : base(schemaType, type, false)
            {
                this.typeSchema = typeSchema;
            }

            public override object? Deserialize(BinaryReader reader)
            {
                return typeSchema.Deserialize(reader);
            }

            public override void Serialize(BinaryWriter writer, object? value)
            {
                if (value == null)
                {
                    value = Activator.CreateInstance(Type);
                }
                else
                {
                    if (value.GetType() != Type) throw new ArgumentException();
                }
                typeSchema.Serialize(writer, value);
            }
        }
    }
}