

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
            internal class Class : StructuredSerializer
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
                private readonly Type classType;

                private Class(Type classType, DataSchema utility)
                {
                    var fieldsInfo = classType.GetFields();
                    var fieldSchemas = new List<FieldSchema>();

                    foreach (FieldInfo fieldInfo in fieldsInfo)
                    {
                        Type fieldType = fieldInfo.FieldType;

                        bool schemaValid = utility.TryGetTypeSchema(fieldType, out var fieldTypeSchema);
                        if (!schemaValid || fieldTypeSchema == null)
                        {
                            utility.Log.Error($"Field \"{fieldInfo.FieldType} {fieldInfo.Name}\" from the {classType} class will be ignored.");
                            continue;
                        }

                        fieldSchemas.Add(new FieldSchema(fieldInfo, fieldTypeSchema));
                    }

                    this.classType = classType;
                    this.fieldSchemas = fieldSchemas.ToArray();
                }
                public static void TryCreate(Type type, out bool isClass, out bool classValid, out TypeSchema? schema, DataSchema utility)
                {
                    isClass = false; classValid = false; schema = null;

                    if (type.IsValueType)
                    {
                        classValid = true; isClass = true;
                        schema = new ValueSchema(StructureType.Class, type, new Class(type, utility));
                    }
                }

                public override void Serialize(BinaryWriter writer, object classInstance)
                {
                    foreach (FieldSchema fieldSchema in fieldSchemas)
                    {
                        fieldSchema.Serialize(writer, classInstance);
                    }
                }
                public override object Deserialize(BinaryReader reader)
                {
                    object structInstance = Activator.CreateInstance(classType);
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