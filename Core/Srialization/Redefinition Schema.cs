
namespace Lithium.Serialization
{
    using System.Reflection;
    using System.IO;
    using System;
    
    public interface IRedefinition<RedefinedType>
    {
        public RedefinedType ToInitialDefinition();
    }

    internal partial class DataSchema
    {
        private sealed class RedefinitionSchema : TypeSchema
        {
            private readonly TypeSchema RedefinitionTypeSchema;
            private readonly MethodInfo redefinitionConverter;
            private readonly Type redefinitionType;
            private readonly Type redefinedType;
            private RedefinitionSchema(DataSchema dataSchema, Type redefinedType, TypeSchema redefinitionTypeSchema, MethodInfo redefinitionConverter) :
                                  base(dataSchema, redefinitionTypeSchema.ValueSerializer, redefinitionTypeSchema.NullSupport)
            {
                RedefinitionTypeSchema = redefinitionTypeSchema;
                redefinitionType = redefinitionTypeSchema.Type;
                this.redefinitionConverter = redefinitionConverter;
                this.redefinedType = redefinedType;
            }
            public static void TryCreate(DataSchema dataSchema, Type redefinitionType, Type redefinedType, out bool isValid, out TypeSchema? redefinitionSchema)
            {
                isValid = false; redefinitionSchema = null;

                bool issuesFound = false;
                if ((redefinedType.IsValueType != redefinitionType.IsValueType) || (redefinitionType.IsClass != redefinedType.IsClass))
                {
                    dataSchema.Log.Error($"The redefined type {redefinedType} and the redefinition {redefinitionType} " +
                        $"must be either both structs or both classes (struct<struct> or class<class>)");
                    issuesFound = true;
                }
                if (redefinitionType == redefinedType)
                {
                    dataSchema.Log.Error($"The redefined type <{redefinedType}> must be different from the redefinition type {redefinitionType}");
                    issuesFound = true;
                }
                if (redefinitionType.GetConstructor(Type.EmptyTypes) == null)
                {
                    dataSchema.Log.Error($"The redefinition type {redefinitionType} must have a empty constructor.");
                    issuesFound = true;
                }
                if (redefinitionType.GetConstructor(new[] { redefinedType }) == null)
                {
                    dataSchema.Log.Error($"The redefinition type {redefinitionType} must have a one parameter constructor of the redefined type {redefinedType}");
                    issuesFound = true;
                }
                if (dataSchema.TryGetTypeSchema(redefinedType, out TypeSchema? _))
                {
                    dataSchema.Log.Error($"Cannot redefine type {redefinedType} when already supported or added.");
                    issuesFound = true;
                }

                bool validSchema = dataSchema.TryGetTypeSchema(redefinitionType, out var redefinitionTypeSchema);
                if ((redefinitionTypeSchema == null) || (validSchema == false))
                {
                    dataSchema.Log.Error($"Redefinition type {redefinitionType} of the original type {redefinedType} is not serializable.");
                    return;
                }
                if (issuesFound) return;

                isValid = true;

                Type genericRedefinitionType = typeof(IRedefinition<>).MakeGenericType(redefinedType);
                var redefinitionConverter = genericRedefinitionType.GetMethod(nameof(IRedefinition<Type>.ToInitialDefinition));

                redefinitionSchema = new RedefinitionSchema(dataSchema, redefinedType, redefinitionTypeSchema, redefinitionConverter);
            }

            public override void Serialize(BinaryWriter writer, object? originalInstance)
            {
                object redefinedInstance = Activator.CreateInstance(redefinitionType, originalInstance);
                RedefinitionTypeSchema.Serialize(writer, redefinedInstance);
            }
            public override object? Deserialize(BinaryReader reader)
            {
                object? redefinedValue = RedefinitionTypeSchema.Deserialize(reader);
                if (redefinedValue == null) return null;
                return redefinitionConverter.Invoke(redefinedValue, null);
            }
        }
    }
}