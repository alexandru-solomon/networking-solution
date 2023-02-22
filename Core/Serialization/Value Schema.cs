using System.IO;
using System;

namespace Lithium.Serialization
{
    internal partial class DataSchema
    {
        private partial class ValueSchema : TypeSchema
        {
            private ValueSchema(DataSchema dataSchema, ValueSerializer valueSerializer) : base(dataSchema,valueSerializer, false) { }

            public override object? Deserialize(BinaryReader reader)
            {
                return ValueSerializer.Deserialize(reader);
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
                ValueSerializer.Serialize(writer, value);
            }
        }
    }
}