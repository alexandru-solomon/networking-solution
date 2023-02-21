using System.Collections.Generic;
using Lithium.Logging;
using System.IO;
using System;

namespace Lithium.Serialization
{
    internal partial class DataSchema
    {
        internal DataSchema()
        {
            schemas = new Dictionary<Type, ObjectSchema>();
            referenceBuffer = new List<object>();
            Log = new Log(this);
        }
        internal Log Log;
        private Dictionary<Type, ObjectSchema> schemas;
        private List<object> referenceBuffer;

        internal void Serialize(object? value,ObjectSchema schema, BinaryWriter writer) 
        {
            referenceBuffer.Clear();
            schema.Serialize(writer, value);
        }
        internal object? Deserialize(ObjectSchema schema, BinaryReader reader)
        {
            referenceBuffer.Clear();
            return schema.Deserialize(reader);
        }
    }
}
