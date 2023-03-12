//Copyright 2022, Alexandru Solomon, All rights reserved.
//Contacts: Alexandru.Solomon.inbox@gmail.com

namespace Lithium
{
    using System;

    /// <summary> A serializable data container used to send data between the server and the client. </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum)] public class Packet : Attribute { }

    /// <summary> Network method invoked on the client by the server. </summary>
    [AttributeUsage(AttributeTargets.Method)] public class Client : Attribute { }

    /// <summary> Network method requested by the client to be invoked on the server. </summary>
    [AttributeUsage(AttributeTargets.Method)] public class Server : Attribute { }


    /// <summary> The allowed miliseconds delay between the network calls of the net method. </summary>
    [AttributeUsage(AttributeTargets.Method)] public class Delay : Attribute
    {
        public readonly int Milliseconds;
        public Delay(int milliseconds)
        {
            Milliseconds = milliseconds;
        }
    }

    /// <summary> The allowed amount of network calls of the net method. </summary>
    [AttributeUsage(AttributeTargets.Method)] public class Rate : Attribute
    {
        public readonly int CallsPerSecond;
        public Rate(int callsPerSecond)
        {
            CallsPerSecond = callsPerSecond;
        }
    }

    /// <summary> The allowed amount of network calls of the net method in relation to the tickrate (1 ≈ tickrate , 2 ≈ 2*tickrate). </summary>
    [AttributeUsage(AttributeTargets.Method)] public class Ratio : Attribute
    {
        public readonly float TickRateMultiplier;
        public Ratio(float tickRateMultiplier)
        {
            TickRateMultiplier = tickRateMultiplier;
        }
        public readonly int CallsPerSecond;
    }


    /// <summary> This method is used by the server to set the values of the class so that it matches the class on the server. </summary>
    [AttributeUsage(AttributeTargets.Method)] public class Sync: Attribute { }

    /// <summary> This method is used by the server to replicate the status of the class and transmit it to the clients' 
    /// class in order to make sure that it resembles the server's class. </summary>
    [AttributeUsage(AttributeTargets.Method)] public class Copy: Attribute { }
}