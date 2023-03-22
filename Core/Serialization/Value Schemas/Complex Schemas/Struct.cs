

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
            internal class Struct : ValueSerializer
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

                private Struct(Type structType, DataSchema dataSchema):base(structType,StructureType.Struct)
                {
                    var fieldsInfo = structType.GetFields();
                    var fieldSchemas = new List<FieldSchema>();

                    foreach (FieldInfo fieldInfo in fieldsInfo)
                    {
                        Type fieldType = fieldInfo.FieldType;

                        bool schemaValid = dataSchema.TryGetTypeSchema(fieldType, out var fieldTypeSchema);
                        if (!schemaValid || fieldTypeSchema == null)
                        {
                            dataSchema.Log.Error($"Field \"{fieldInfo.FieldType} {fieldInfo.Name}\" from the {structType} struct will be ignored.");
                            continue;
                        }

                        fieldSchemas.Add(new FieldSchema(fieldInfo, fieldTypeSchema));
                    }

                    this.fieldSchemas = fieldSchemas.ToArray();
                }
                public static void TryCreate(DataSchema dataSchema, Type structType, out bool isStruct, out bool structValid, out TypeSchema? schema)
                {
                    isStruct = false; structValid = false; schema = null;

                    if (structType.IsValueType)
                    {
                        structValid = true; isStruct = true;
                        schema = new ValueSchema(dataSchema,new Struct(structType, dataSchema));
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