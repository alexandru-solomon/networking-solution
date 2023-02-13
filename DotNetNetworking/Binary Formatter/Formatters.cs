//Copyright 2022, Alexandru Solomon, All rights reserved.
//Contacts: Alexandru.Solomon.inbox@gmail.com


namespace Networking.Serialization
{
    using System.Collections;
    using System.Reflection;
    using System;

    class NullPrefixFormatter : IFormattable
    {
        public Type Type { get; }
        public bool NullSupport { get; }

        private readonly IFormattable boolFormatter;
        private readonly IFormattable contentFormatter;
        public NullPrefixFormatter(BinaryFormatter binaryFormatter,IFormattable contentFormatter)
        {
            Type= contentFormatter.Type;
            NullSupport = true;
            boolFormatter = binaryFormatter.GetFormatter(typeof(bool));
            this.contentFormatter = contentFormatter;
        }
        public void Serialize(object value)
        {
            boolFormatter.Serialize(value == null);
            if (value == null) return;
            contentFormatter.Serialize(value);
        }
        public object Deserialize()
        {
            bool isNull = (bool)boolFormatter.Deserialize();

            if (isNull) return null;
            return contentFormatter.Deserialize();
        }

        public override string ToString()
        {
            return $"?{contentFormatter}";
        }
    }

    class PrimitiveFormatter:IFormattable
    {
        public Type Type { get; }
        public bool NullSupport { get; } 

        private readonly DeserializerMethod deserializer;
        private readonly SerializerMethod serializer;
        public PrimitiveFormatter(Type type, SerializerMethod serializer, DeserializerMethod deserializer)
        {
            Type= type;
            this.serializer = serializer;
            this.deserializer = deserializer;
            NullSupport = false;
        }

        public object Deserialize()
        {
            return deserializer.Invoke();
        }

        public void Serialize(object obj)
        {
            serializer.Invoke(obj);
        }

        public override string ToString()
        {
            return Type.ToString();
        }
    }
    class EnumFormatter : IFormattable
    {
        public bool NullSupport { get; }
        private readonly IFormattable intFormatter;
        public Type Type { get; }

        public EnumFormatter(BinaryFormatter binaryFormatter, Type type)
        {
            NullSupport = false;
            intFormatter = binaryFormatter.GetFormatter(typeof(int));
            Type = type;
        }

        public void Serialize(object enumValue)
        {
            intFormatter.Serialize(enumValue);
        }
        public object Deserialize()
        {
            return Enum.ToObject(Type, (int)intFormatter.Deserialize());
        }

        public override string ToString()
        {
            return $"{Type} Enum";
        }
    }
    class RepresentedFormatter : IFormattable
    {
        public Type Type { get; }
        public bool NullSupport { get; }

        private readonly IFormattable representantFormatter;
        private readonly Type representantType;

        private readonly MethodInfo? deserializeMethod; 

        public RepresentedFormatter(Type representedType, IFormattable representantFormatter)
        {
            Type = representedType;
            NullSupport = false;
            Type genericInterfaceType = typeof(IRedefinition<>).MakeGenericType(representedType);
            deserializeMethod = genericInterfaceType.GetMethod(nameof(IRedefinition<Type>.ToInitialDefinition));

            this.representantFormatter = representantFormatter;
            representantType = representantFormatter.Type;
        }


        public object? Deserialize()
        {   
            object representant = representantFormatter.Deserialize();
            if (representant == null) return null;
            return deserializeMethod?.Invoke(representant,null);
        }
        public void Serialize(object represented)
        {
            object representant;
            if (represented == null) representant = null;
            else representant = Activator.CreateInstance(representantType, represented);
            representantFormatter.Serialize(representant);
        }

        public override string ToString()
        {
            string containerType = Type.IsClass ? "Class" : "Struct";
            return $"{Type} {containerType} -> {representantFormatter}";
        }
    }
    class CollectionFormatter : IFormattable
    {
        public Type Type { get; }
        public bool NullSupport { get; }

        private readonly IFormattable elementFormatter;
        private readonly IFormattable uIntFormatter;
        private readonly Type elementType;

        public CollectionFormatter(Type type, Type elementType, IFormattable elementFormatter, BinaryFormatter binaryFormatter)
        {
            Type = type;
            NullSupport = false;
            uIntFormatter = binaryFormatter.GetFormatter(typeof(uint));
            this.elementFormatter = elementFormatter;
            this.elementType = elementType;
        }

        public void Serialize(object instance)
        {
            ICollection collection = (ICollection)instance;
            IEnumerable enumerator = (IEnumerable)instance;
            uIntFormatter.Serialize((uint)collection.Count);
            foreach (var item in enumerator) elementFormatter.Serialize(item);
        }
        public object Deserialize()
        {
            uint length = (uint)uIntFormatter.Deserialize();
            Array array = Array.CreateInstance(elementType, length);
            for (int i = 0; i < length; i++)
                array.SetValue(elementFormatter.Deserialize(), i);

            if (Type.IsArray) return array;
            else return Activator.CreateInstance(Type, array);
        }

        public override string ToString()
        {
            return $"System.{Type.Name}[{elementFormatter}]";
        }
    }
    class ContainerFormatter : IFormattable
    {
        public class Field
        {
            public readonly IFormattable formatter;
            public readonly FieldInfo field;
            public Field(FieldInfo field, IFormattable formatter)
            {
                this.field = field;
                this.formatter = formatter;
            }

            public void Serialize(object instance)
            {
                formatter.Serialize(field.GetValue(instance));
            }
            public void Deserialize(object instance)
            {
                field.SetValue(instance, formatter.Deserialize());
            }
        }

        public Type Type { get; }
        public bool NullSupport { get; }

        private readonly Field[] fieldFormatters;

        public ContainerFormatter(Type type , Field[] fieldFormatters)
        {
            Type = type;
            NullSupport = false;
            this.fieldFormatters = fieldFormatters;
        }

        public void Serialize(object instance)
        {
            for (int i = 0; i < fieldFormatters.Length; i++) fieldFormatters[i].Serialize(instance);
        }
        public object Deserialize()
        {
            object instance = Activator.CreateInstance(Type);
            for (int i = 0; i < fieldFormatters.Length; i++) fieldFormatters[i].Deserialize(instance);

            return instance;
        }

        public override string ToString()
        {
            return Type.IsClass ? $"{Type} Class" : $"{Type} Struct";
        }
    }
}