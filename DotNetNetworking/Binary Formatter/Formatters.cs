//Copyright 2022, Alexandru Solomon, All rights reserved.
//Contacts: Alexandru.Solomon.inbox@gmail.com

using System.Collections;
using System.Reflection;
using System;

namespace CNet.Serialization
{
    class PrimitiveFormatter:IFormattable
    {
        private readonly DeserializerMethod deserializer;
        private readonly SerializerMethod serializer;
        public PrimitiveFormatter(SerializerMethod serializer, DeserializerMethod deserializer)
        {
            this.serializer = serializer;
            this.deserializer = deserializer;
        }

        public object Deserialize()
        {
            return deserializer.Invoke();
        }

        public void Serialize(object obj)
        {
            serializer.Invoke(obj);
        }
    }
    class NullPrefixFormatter : IFormattable
    {
        private readonly IFormattable boolFormatter;
        private readonly IFormattable contentFormatter;
        public NullPrefixFormatter(BinaryFormatter binaryFormatter,IFormattable contentFormatter)
        {
            boolFormatter = binaryFormatter.GetFormatter(typeof(bool));
            this.contentFormatter = contentFormatter;
        }
        public void Serialize(object value)
        {
            bool isNull = value == null;
            boolFormatter.Serialize(!isNull);
            if (isNull) return;
            contentFormatter.Serialize(value);
        }
        public object Deserialize()
        {
            bool isNull = !(bool)boolFormatter.Deserialize();

            if (isNull) return null;
            return contentFormatter.Deserialize();
        }
    }
    class EnumFormatter : IFormattable
    {
        private readonly IFormattable intFormatter;
        private readonly Type type;

        public EnumFormatter(BinaryFormatter binaryFormatter, Type type)
        {
            intFormatter = binaryFormatter.GetFormatter(typeof(int));
            this.type = type;
        }

        public void Serialize(object enumValue)
        {
            intFormatter.Serialize(enumValue);
        }
        public object Deserialize()
        {
            return Enum.ToObject(type, (int)intFormatter.Deserialize());
        }
    }
    class RepresentedFormatter : IFormattable
    {
        private readonly IFormattable representantFormatter;
        private readonly Type representantType;

        public RepresentedFormatter(IFormattable representantFormatter, Type representantType)
        {
            this.representantFormatter = representantFormatter;
            this.representantType = representantType;
        }
        public object Deserialize()
        {
            IRepresentant<object> representant = (IRepresentant<object>)representantFormatter.Deserialize();
            if (representant == null) return null;
            return representant.GetRepresented();
        }
        public void Serialize(object represented)
        {
            object representant;
            if (represented == null) representant = null;
            else representant = Activator.CreateInstance(representantType, represented);
            representantFormatter.Serialize(representant);
        }
    }
    class CollectionFormatter : IFormattable
    {
        private readonly IFormattable elementFormatter;
        private readonly IFormattable uIntFormatter;
        private readonly Type elementType;
        private readonly Type type;

        public CollectionFormatter(BinaryFormatter binaryFormatter, IFormattable elementFormatter, Type elementType, Type type)
        {
            uIntFormatter = binaryFormatter.GetFormatter(typeof(uint));
            this.elementFormatter = elementFormatter;
            this.elementType = elementType;
            this.type = type;
        }

        public void Serialize(object instance)
        {
            ICollection collection = instance as ICollection;
            IEnumerable enumerator = instance as IEnumerable;
            uIntFormatter.Serialize((uint)collection.Count);
            foreach (var item in enumerator) elementFormatter.Serialize(item);
        }
        public object Deserialize()
        {
            uint length = (uint)uIntFormatter.Deserialize();
            Array array = Array.CreateInstance(elementType, length);
            for (int i = 0; i < length; i++)
                array.SetValue(elementFormatter.Deserialize(), i);

            if (type.IsArray) return array;
            else return Activator.CreateInstance(type, array);
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

        private readonly Type containerType;
        private readonly Field[] fieldFormatters;

        public ContainerFormatter(Type containerType , Field[] fieldFormatters)
        {
            this.containerType = containerType;
            this.fieldFormatters = fieldFormatters;
        }

        public void Serialize(object instance)
        {
            for (int i = 0; i < fieldFormatters.Length; i++) fieldFormatters[i].Serialize(instance);
        }
        public object Deserialize()
        {
            object instance = Activator.CreateInstance(containerType);
            for (int i = 0; i < fieldFormatters.Length; i++) fieldFormatters[i].Deserialize(instance);

            return instance;
        }
    }
}