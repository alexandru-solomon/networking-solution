//Copyright 2022, Alexandru Solomon, All rights reserved.
//Contacts: Alexandru.Solomon.inbox@gmail.com

using System.Collections.Generic;
using Lithium.Serialization;
using System.Reflection;
using System;

namespace Lithium
{
    public abstract class NetworkManager
    {
        internal DataSchema DataSchema;

        public NetworkManager()
        {
            DataSchema = new DataSchema();

        }

        public enum QOSType { ReliableSequenced, UnreliableSequenced, Reliable, Unreliable}
        [Packet] public class NetworkChannel
        {

        }
        [Packet] public class NetworkTopology
        {
            
        }
        [Packet] internal class Contract
        {
            public readonly string Version; 
            public readonly List<RpcDefinition> ClientRPCs;
            public readonly List<RpcDefinition> ServerRPCs;

            public static bool operator ==(Contract contract1, Contract contract2)
            {
                if (ReferenceEquals(contract1,contract2)) return true;
                if (contract1.ClientRPCs.Count != contract2.ClientRPCs.Count) return false;
                if (contract1.ServerRPCs.Count != contract2.ServerRPCs.Count) return false;

                for(int id = 0; id< contract1.ClientRPCs.Count; id++)
                {
                    if(contract1.ClientRPCs[id] != contract2.ClientRPCs[id]) return false;
                }
                for (int id = 0; id < contract1.ServerRPCs.Count; id++)
                {
                    if (contract1.ServerRPCs[id] != contract2.ServerRPCs[id]) return false;
                }
                return true;
            }
            public static bool operator !=(Contract contract1, Contract contract2)
            {
                return !(contract1 == contract2);
            }
        }
        [Packet]internal class RpcDefinition
        {
            internal RpcDefinition(MethodInfo method)
            {
                Parameters = new List<string>();
                Namespace = method.DeclaringType.Namespace;
                DeclaringType = method.DeclaringType.ToString();
                MethodName = method.Name;
                foreach(var parameter in method.GetParameters())
                    Parameters.Add(parameter.ParameterType.ToString());
            }
            internal string Namespace;
            internal string DeclaringType;
            internal string MethodName;
            internal List<string> Parameters;
            public static void Equals()
            {

            }
            public static bool operator ==(RpcDefinition def1, RpcDefinition def2)
            {
                if (ReferenceEquals(def1, def2)) return true;

                if(def1.Namespace != def2.Namespace) return false;
                if(def1.DeclaringType != def2.DeclaringType) return false;
                if(def1.MethodName != def2.MethodName) return false;
            
                if(def1.Parameters.Count != def2.Parameters.Count) return false;

                for(int id = 0; id< def1.Parameters.Count; id++)
                    if (def1.Parameters[id] != def2.Parameters[id]) return false;

                return true;
            }
            public static bool operator !=(RpcDefinition def1, RpcDefinition def2)
            {
                return !(def1 == def2);
            }
        }
    }
}