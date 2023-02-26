using System.Collections.Generic;
using System.Reflection;
using Lithium.Logging;
using System;

namespace Lithium.Serialization
{
    internal partial class DataSchema
    {
        internal DataSchema()
        {
            Log = new Log(this);
            schemas = new Dictionary<Type, TypeSchema>();
            redefinitions = new Dictionary<Type, Type>();
            referenceBuffer = new List<object>();
            
            ForceAttribute = true;
            AddPrimitiveSchemas();
        }

        internal readonly Log Log;
        internal bool ForceAttribute;

        private readonly Dictionary<Type, TypeSchema> schemas;
        private readonly Dictionary<Type, Type> redefinitions;
        private readonly List<object> referenceBuffer;

        internal void RedefineTypes(IEnumerable<Assembly> assembliesContainingTypeRedefinitions)
        {
            foreach(Assembly assembly in assembliesContainingTypeRedefinitions)
            {
                Type[] types = assembly.GetTypes();

                foreach(Type type in types)
                {
                    foreach (Type implementedInterface in type.GetInterfaces())
                        if (implementedInterface.IsGenericType)
                            if (implementedInterface.GetGenericTypeDefinition() == typeof(IRedefinition<>))
                            {
                                Type redefinedType = implementedInterface.GetGenericArguments()[0];
                                redefinitions.Add(redefinedType, type);
                                break;
                            }
                }
            }
        }
        private void AddPrimitiveSchemas()
        {
            void AddSchema(TypeSchema typeSchema) => schemas.Add(typeSchema.Type, typeSchema);

            AddSchema(ValueSchema.Bool.Create(this));
            AddSchema(ValueSchema.SByte.Create(this));
            AddSchema(ValueSchema.Byte.Create(this));
            AddSchema(ValueSchema.Short.Create(this));
            AddSchema(ValueSchema.UShort.Create(this));
            AddSchema(ValueSchema.Int.Create(this));
            AddSchema(ValueSchema.UInt.Create(this));
            AddSchema(ValueSchema.Long.Create(this));
            AddSchema(ValueSchema.ULong.Create(this));
            AddSchema(ValueSchema.Decimal.Create(this));
            AddSchema(ValueSchema.Float.Create(this));
            AddSchema(ValueSchema.Double.Create(this));
            AddSchema(ValueSchema.Char.Create(this));
            
            AddSchema(ReferenceSchema.String.Create(this));
            AddSchema(ReferenceSchema.ByteArray.Create(this));
        }
        internal bool TryGetTypeSchema(Type type, out TypeSchema? typeSchema)
        {
            //Get the typeSchema from the dictionary if existent
            if(schemas.TryGetValue(type, out typeSchema)) return true;

            //Try to create type schema if not existent
            bool created = TryCreateTypeSchema(type, out typeSchema);
            if(created && typeSchema!=null)
            {
                schemas.Add(type, typeSchema);
                Log.Info($"{typeSchema} created.");
                return true;
            }
            else return false;

            bool TryCreateTypeSchema(Type type, out TypeSchema? typeSchema)
            {
                if (redefinitions.ContainsKey(type)) 
                {
                    Type redefinitionType = redefinitions[type];
                    RedefinitionSchema.TryCreate(this, type, redefinitionType, out bool redefinitionValid, out typeSchema); return redefinitionValid;
                }

                NullableSchema.TryCreate(this, type, out bool isNullable, out bool nullableValid, out typeSchema); if(isNullable) return nullableValid;
                ValueSchema.Enum.TryCreate(this,type, out bool isEnum, out bool enumValid, out typeSchema); if (isEnum) return enumValid;
                ReferenceSchema.Collection.TryCreate(this, type, out bool isColl, out bool collValid, out typeSchema); if (isColl) return collValid;
                ValueSchema.Struct.TryCreate(this, type, out bool isStruct, out bool structValid, out typeSchema); if (isStruct) return structValid;
                ReferenceSchema.Class.TryCreate(this, type, out bool isClass, out bool classValid, out typeSchema); if (isClass) return classValid;

                Log.Error($"Type {type} not supported."); return false;
            }
        }
    }
}