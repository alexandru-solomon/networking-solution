

namespace Lithium.Serialization
{
    using System.Collections.Generic;
    using System.Reflection;
    using Lithium.Logging;
    using System.IO;
    using System;

    internal partial class DataSchema
    {
        internal sealed partial class ValueSchema : ObjectSchema
        {
            internal class Struct : TypeSchema
            {
                public class FieldSchema
                {
                    readonly FieldInfo field;
                    readonly ObjectSchema schema;
                    public FieldSchema(FieldInfo fieldInfo, ObjectSchema fieldTypeSchema)
                    {
                        schema = fieldTypeSchema;
                        field = fieldInfo;
                    }
                    public void Serialize(BinaryWriter writer, object instance)
                    {
                        schema.Serialize(writer, field.GetValue(instance));
                    }
                    public void Deserialize(BinaryReader reader, object instance)
                    {
                        field.SetValue(instance, schema.Deserialize(reader));
                    }
                }

                private readonly FieldSchema[] fieldSchemas;
                private readonly Type structType;

                private Struct(Type structType, DataSchema utility)
                {
                    var fieldsInfo = structType.GetFields();
                    var fieldSchemas = new List<FieldSchema>();

                    foreach (FieldInfo fieldInfo in fieldsInfo)
                    {
                        Type fieldType = fieldInfo.FieldType;

                        bool SchemaExists = utility.TryGetSchema(fieldType, out var fieldTypeSchema);
                        if (!SchemaExists) utility.TryCreateSchema(fieldType, out fieldTypeSchema);

                        if (fieldTypeSchema == null)
                        {
                            utility.Log.Error($"Field \"{fieldInfo.FieldType} {fieldInfo.Name}\" from the {structType} struct will be ignored.");
                            continue;
                        }

                        fieldSchemas.Add(new FieldSchema(fieldInfo, fieldTypeSchema));
                    }

                    this.structType = structType;
                    this.fieldSchemas = fieldSchemas.ToArray();
                }
                public static bool TryCreate(Type type, out bool isStruct, out bool isValid, out ObjectSchema? schema, DataSchema utility)
                {
                    isStruct = false; isValid = false; schema = null;

                    if (type.IsValueType)
                    {
                        isValid = true; isStruct = true;
                        schema = new ValueSchema(SchemaType.Struct, type, new Struct(type,utility));
                        return true;
                    }

                    return false;
                }

                public override void Serialize(BinaryWriter writer, object structInstance)
                {
                    foreach(FieldSchema fieldSchema in fieldSchemas)
                    {
                        fieldSchema.Serialize(writer, structInstance);
                    }
                }
                public override object Deserialize(BinaryReader reader)
                {
                    object structInstance = Activator.CreateInstance(structType);
                    foreach (FieldSchema fieldSchema in fieldSchemas)
                    {
                        fieldSchema.Deserialize(reader, structInstance);
                    }
                    return structInstance;
                }
            }
        }
    }
}