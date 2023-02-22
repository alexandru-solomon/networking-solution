using Lithium.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;


namespace Lithium.Serialization
{
    internal partial class DataSchema
    {
        private partial class ReferenceSchema : TypeSchema
        {
            private enum State { Null, Data, Reference }

            private ReferenceSchema(ValueSerializer valueSerializer, DataSchema dataSchema) : base(dataSchema, valueSerializer, true) { }

            public override object? Deserialize(BinaryReader reader)
            {
                State state = (State)reader.ReadByte();

                if (state == State.Data)
                {
                    object value = ValueSerializer.Deserialize(reader);
                    DataSchema.referenceBuffer.Add(value);
                    return value;
                }
                if (state == State.Reference)
                {
                    int referenceID = reader.ReadInt32();

                    return DataSchema.referenceBuffer[referenceID];
                }

                return null;
            }

            public override void Serialize(BinaryWriter writer, object? value)
            {
                if (value == null)
                {
                    writer.Write((byte)State.Null);
                    return;
                }
                if (DataSchema.referenceBuffer.Contains(value))
                {
                    writer.Write((byte)State.Reference);
                    writer.Write(DataSchema.referenceBuffer.IndexOf(value));
                    return;
                }

                DataSchema.referenceBuffer.Add(value);
                writer.Write((byte)State.Data);
                ValueSerializer.Serialize(writer, value);
            }
        }
    }
}