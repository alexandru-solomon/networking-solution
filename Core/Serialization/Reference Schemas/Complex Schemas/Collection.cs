
namespace Lithium.Serialization
{
    using System;
    using System.IO;
    using System.Collections;

    internal partial class DataSchema
    {
        private sealed partial class ReferenceSchema : TypeSchema
        {
            internal sealed class Collection : ValueSerializer
            {
                private readonly TypeSchema elementTypeSchema;
                public Collection(Type collectionType, TypeSchema elementTypeSchema) : base(collectionType, StructureType.Collection)
                {
                    this.elementTypeSchema = elementTypeSchema;
                }
                public static void TryCreate(DataSchema dataSchema, Type collectionType, out bool isCollection, out bool isValid, out TypeSchema? collectionSchema)
                {
                    isValid = false; collectionSchema = null;

                    isCollection = typeof(ICollection).IsAssignableFrom(collectionType); if (!isCollection) return;

                    Type elementType = collectionType.GetGenericArguments()[0];

                    bool schemaValid = dataSchema.TryGetTypeSchema(elementType, out TypeSchema? elementTypeSchema);

                    if (elementTypeSchema == null ||schemaValid == false)
                    {
                        dataSchema.Log.Error($"{collectionType} is not serializable because its element type {elementType} is not serializable");
                        return;
                    }

                    isValid = true;
                    collectionSchema = new ReferenceSchema(dataSchema,new Collection(collectionType, elementTypeSchema));
                }

                public override void Serialize(BinaryWriter writer, object collectionInstance)
                {
                    ICollection collection = (ICollection)collectionInstance;
                    IEnumerable enumerator = (IEnumerable)collectionInstance;
                    writer.Write(collection.Count);
                    foreach (var element in enumerator) elementTypeSchema.Serialize(writer, element);
                }
                public override object Deserialize(BinaryReader reader)
                {
                    int length = reader.ReadInt32();
                    Array array = Array.CreateInstance(elementTypeSchema.Type, length);
                    for (int elementId = 0; elementId < length; elementId++)
                        array.SetValue(elementTypeSchema.Deserialize(reader), elementId);

                    if (elementTypeSchema.Type.IsArray) return array;
                    else return Activator.CreateInstance(ValueType, array);
                }
            }
        }
    }
}
