

namespace Lithium.Serialization
{
    using System.Collections.Generic;
    using System.Reflection;
    using System.IO;
    using System;

    internal partial class DataSchema
    {
        private sealed partial class ValueSchema : TypeSchema
        {
            internal class Struct : StructuredSerializer
            {
                public class FieldSchema
                {
                    readonly FieldInfo field;
                    readonly TypeSchema schema;
                    public FieldSchema(FieldInfo fieldInfo, TypeSchema fieldTypeSchema)
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

                        bool schemaValid = utility.TryGetTypeSchema(fieldType, out var fieldTypeSchema);
                        if (!schemaValid || fieldTypeSchema == null)
                        {
                            utility.Log.Error($"Field \"{fieldInfo.FieldType} {fieldInfo.Name}\" from the {structType} struct will be ignored.");
                            continue;
                        }

                        fieldSchemas.Add(new FieldSchema(fieldInfo, fieldTypeSchema));
                    }

                    this.structType = structType;
                    this.fieldSchemas = fieldSchemas.ToArray();
                }
                public static void TryCreate(Type type, out bool isStruct, out bool structValid, out TypeSchema? schema, DataSchema utility)
                {
                    isStruct = false; structValid = false; schema = null;

                    if (type.IsValueType)
                    {
                        structValid = true; isStruct = true;
                        schema = new ValueSchema(StructureType.Struct, type, new Struct(type,utility));
                    }
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