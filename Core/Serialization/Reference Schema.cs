using Lithium.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;


namespace Lithium.Serialization
{
    internal partial class DataSchema
    {
        private partial class ReferenceSchema : ObjectSchema
        {
            private enum State { Null, Data, Reference }

            private readonly List<object> referenceBuffer;
            private readonly TypeSchema typeSchema;
            public ReferenceSchema(SchemaType schemaType, Type type, TypeSchema schema, List<object> referenceBuffer) : base(schemaType, type, true)
            {
                typeSchema = schema;
                this.referenceBuffer = referenceBuffer;
            }

            public override object? Deserialize(BinaryReader reader)
            {
                State state = (State)reader.ReadByte();

                if (state == State.Data)
                {
                    object value = typeSchema.Deserialize(reader);
                    referenceBuffer.Add(value);
                    return value;
                }
                if (state == State.Reference)
                {
                    int referenceID = reader.ReadInt32();

                    return referenceBuffer[referenceID];
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
                if (referenceBuffer.Contains(value))
                {
                    writer.Write((byte)State.Reference);
                    writer.Write(referenceBuffer.IndexOf(value));
                    return;
                }

                referenceBuffer.Add(value);
                writer.Write((byte)State.Data);
                typeSchema.Serialize(writer, value);
            }
        }
    }
}