

namespace Lithium.Serialization
{
    using System.Collections.Generic;
    using System.Reflection;
    using System.IO;
    using System;

    internal partial class DataSchema
    {
        private sealed partial class ReferenceSchema : TypeSchema
        {
            internal class Class : ValueSerializer
            {
                public class FieldSchema
                {
                    readonly FieldInfo fieldInfo;
                    readonly TypeSchema typeSchema;
                    public FieldSchema(FieldInfo fieldInfo, TypeSchema fieldTypeSchema)
                    {
                        typeSchema = fieldTypeSchema;
                        this.fieldInfo = fieldInfo;
                    }
                    public void Serialize(BinaryWriter writer, object instance)
                    {
                        typeSchema.Serialize(writer, fieldInfo.GetValue(instance));
                    }
                    public void Deserialize(BinaryReader reader, object instance)
                    {
                        fieldInfo.SetValue(instance, typeSchema.Deserialize(reader));
                    }
                }

                private readonly FieldSchema[] fieldSchemas;

                private Class(Type classType, DataSchema dataSchema):base(classType,StructureType.Class)
                {
                    var fieldsInfo = classType.GetFields();
                    var fieldSchemas = new List<FieldSchema>();

                    foreach (FieldInfo fieldInfo in fieldsInfo)
                    {
                        Type fieldType = fieldInfo.FieldType;

                        bool schemaValid = dataSchema.TryGetTypeSchema(fieldType, out var fieldTypeSchema);
                        if (!schemaValid || fieldTypeSchema == null)
                        {
                            dataSchema.Log.Error($"Field \"{fieldInfo.FieldType} {fieldInfo.Name}\" from the {classType} class will be ignored.");
                            continue;
                        }

                        fieldSchemas.Add(new FieldSchema(fieldInfo, fieldTypeSchema));
                    }

                    this.fieldSchemas = fieldSchemas.ToArray();
                }
                public static void TryCreate(DataSchema dataSchema, Type classType, out bool isClass, out bool classValid, out TypeSchema? schema)
                {
                    isClass = false; classValid = false; schema = null;

                    if (classType.IsValueType)
                    {
                        classValid = true; isClass = true;
                        schema = new ReferenceSchema(dataSchema,new Class(classType, dataSchema));
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
                    object structInstance = Activator.CreateInstance(ValueType);
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