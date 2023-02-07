//Copyright 2022, Alexandru Solomon, All rights reserved.
//Contacts: Alexandru.Solomon.inbox@gmail.com
using System;

namespace Networking
{
    
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum)]
    public class Packet : Attribute { }

    [AttributeUsage(AttributeTargets.Method)]
    public class Client: Attribute { }

    [AttributeUsage(AttributeTargets.Method)]
    public class Server: Attribute 
    {
        public int SendLimit;
        public Server(float sendLimit) 
        {
            
        }
        /// <summary> Default send rate <summary>
        public Server()
        {

        }
    }



    [AttributeUsage(AttributeTargets.Method)]
    public class Delay : Attribute { }

    [AttributeUsage(AttributeTargets.Method)]
    public class Rate : Attribute { }

    [AttributeUsage(AttributeTargets.Method)]
    public class Ratio : Attribute { }



    [AttributeUsage(AttributeTargets.Method)]
    public class Sync: Attribute { }

    [AttributeUsage(AttributeTargets.Method)]
    public class Echo: Attribute { }



    [AttributeUsage(AttributeTargets.Parameter)]
    public class Origin: Attribute { }
}
