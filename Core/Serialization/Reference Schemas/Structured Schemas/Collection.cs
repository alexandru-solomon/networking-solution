
namespace Lithium.Serialization
{
    using System;
    using System.IO;
    using UnityEngine;
    using System.Collections.Generic;
    using System.Collections;

    internal partial class DataSchema
    {
        private sealed partial class ReferenceSchema : TypeSchema
        {
            private sealed class Collection : StructuredSerializer
            {
                private readonly TypeSchema elementSchema;
                public Collection(Type collectionType, Schema elementTypeSchema) : base(StructureType.Collection, collectionType, false)
                {
                    this.elementTypeSchema = elementTypeSchema;
                }
                public static void TryCreate(Type type, out bool isCollection, out bool isValid, out StructuredSerializer? collectionSchema, DataSchema dataSchema)
                {
                    isValid = false; collectionSchema = null;

                    isCollection = typeof(ICollection).IsAssignableFrom(type); if (!isCollection) return;

                    Type elementType = type.GetGenericArguments()[0];

                    bool elementSchemaValid = dataSchema.TryGetTypeSchema(elementType, out TypeSchema? elementTypeSchema);

                    if (elementTypeSchema == null)
                    {
                        Log.Error($"{type} is not serializable because its element type {elementType} is not serializable");
                        return;
                    }

                    isValid = true;
                    collectionSchema = new CollectionSchema(type, elementTypeSchema);
                }

                public override void Serialize(BinaryWriter writer, object collectionInstance)
                {
                    ICollection collection = (ICollection)collectionInstance;
                    IEnumerable enumerator = (IEnumerable)collectionInstance;
                    writer.Write(collection.Count);
                    foreach (var element in enumerator) elementSchema.Serialize(writer, element);
                }
                public override object Deserialize(BinaryReader reader)
                {
                    int length = reader.ReadInt32();
                    Array array = Array.CreateInstance(elementTypeSchema.Type, length);
                    for (int elementId = 0; elementId < length; elementId++)
                        array.SetValue(elementTypeSchema.Deserialize(reader), elementId);

                    if (elementTypeSchema.Type.IsArray) return array;
                    else return Activator.CreateInstance(Type, array);
                }
            }
        }
    }
}
