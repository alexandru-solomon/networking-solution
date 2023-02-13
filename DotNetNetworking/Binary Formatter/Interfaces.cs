//Copyright 2022, Alexandru Solomon, All rights reserved.
//Contacts: Alexandru.Solomon.inbox@gmail.com


namespace Networking.Serialization
{
    using System;
    public interface IRedefinition<RedefinedType>
    {
        public RedefinedType ToInitialDefinition();
    }

    public interface IFormattable
    {
        public Type Type { get; }
        public bool NullSupport { get; }
        public void Serialize(object obj);
        public object Deserialize();
    }
}
