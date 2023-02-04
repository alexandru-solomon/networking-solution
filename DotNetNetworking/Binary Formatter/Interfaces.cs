//Copyright 2022, Alexandru Solomon, All rights reserved.
//Contacts: Alexandru.Solomon.inbox@gmail.com

namespace CNet.Serialization
{
    public interface IRepresentant<out RepresentedType>
    {
        public RepresentedType GetRepresented();
    }

    public interface IFormattable
    {
        public void Serialize(object obj);
        public object Deserialize();
    }
}
