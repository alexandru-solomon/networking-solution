//Copyright 2022, Alexandru Solomon, All rights reserved.
//Contacts: Alexandru.Solomon.inbox@gmail.com

namespace Networking.Serialization
{
    public interface IRepresents<RepresentedType>
    {
        public RepresentedType GetRepresented();
    }

    public interface IFormattable
    {
        public void Serialize(object obj);
        public object Deserialize();
    }
}
