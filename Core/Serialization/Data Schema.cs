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
            schemas = new Dictionary<Type, TypeSchema>();
            referenceBuffer = new List<object>();
            Log = new Log(this);
        }

        internal readonly Log Log;
        private readonly Dictionary<Type, TypeSchema> schemas;
        private readonly List<object> referenceBuffer;

        internal bool TryGetTypeSchema(Type type, out TypeSchema? typeSchema)
        {
            if(schemas.TryGetValue(type, out typeSchema)) return true;


            ValueSchema.Enum.TryCreate(type, out bool isEnum, out bool enumValid, out typeSchema); if (isEnum) return enumValid;
            ValueSchema.Struct.TryCreate(type, out bool isStruct, out bool isValid, out typeSchema,this); if(isStruct) return isValid;

            return false;
        }
    }
}
