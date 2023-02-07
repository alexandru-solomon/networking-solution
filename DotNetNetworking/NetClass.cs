//Copyright 2022, Alexandru Solomon, All rights reserved.
//Contacts: Alexandru.Solomon.inbox@gmail.com

using System.Reflection;
using System;

namespace Networking 
{
    public class NetClass
    {
        public bool IsServer { get; internal set; }
        public bool IsClient { get; internal set; }
        public bool IsOnline { get; internal set; }
        public bool Authorized { get; internal set; }

        public virtual void OnConnected()
        {
            
        }

        #if Client

        public ConInfo ConInfo { get; internal set; }

        public void Server(Action action) { ToServer(action.Method); }
        public void Server<T>(Action<T> action, T arg) { ToServer(action.Method, arg); }
        public void Server<T1, T2>(Action<T1, T2> action, T1 arg1, T2 arg2) { ToServer(action.Method, arg1, arg2); }
        public void Server<T1, T2, T3>(Action<T1, T2, T3> action, T1 arg1, T2 arg2, T3 arg3) { ToServer(action.Method, arg1, arg2, arg3); }

        public void Server(Action<ConInfo> action) { ToServer(action.Method); }
        public void Server<T>(Action<ConInfo,T> action, T arg) { ToServer(action.Method, arg); }
        public void Server<T1, T2>(Action<ConInfo,T1, T2> action, T1 arg1, T2 arg2) { ToServer(action.Method, arg1, arg2); }
        public void Server<T1, T2, T3>(Action<ConInfo,T1, T2, T3> action, T1 arg1, T2 arg2, T3 arg3) { ToServer(action.Method, arg1, arg2, arg3); }
        
        private void ToServer(MethodInfo method, params object[] args)
        {
            if(!IsOnline) { Error("Attempt to send data to server through offline class"); return; }
            if(IsServer && !IsClient) { Error("Attempt to send data from Server to Server"); return; }

            //Send data to server

        }

        #endif 

        #if Server
        public void Client(Action action) { ToClient(null,action.Method); }
        public void Client<T>(Action<T> action, T arg) { ToClient(null,action.Method, arg); }
        public void Client<T1, T2>(Action<T1, T2> action, T1 arg1, T2 arg2) { ToClient(null,action.Method, arg1, arg2); }
        public void Client<T1, T2, T3>(Action<T1, T2, T3> action, T1 arg1, T2 arg2, T3 arg3) { ToClient(null,action.Method, arg1, arg2, arg3); }

        public void Client<T>(int conId, Action<T> action, T arg) { ToClient(conId,action.Method, arg); }
        public void Client<T1, T2>(int conId, Action<T1, T2> action, T1 arg1, T2 arg2) { ToClient(conId,action.Method, arg1, arg2); }
        public void Client<T1, T2, T3>(int conId, Action<T1, T2, T3> action, T1 arg1, T2 arg2, T3 arg3) { ToClient(conId,action.Method, arg1, arg2, arg3); }


        private void ToClient(int? conId, MethodInfo method, params object[] args)
        {
            if (!IsOnline) { Error("Attempt to send data to client through offline class"); return; }
            if (!IsServer) { Error("Attempt to send data from Client to Client"); return; }
            //Send data to clients/client
        }
        public void Connect()
        {
            if (IsOnline) Error("Attempt to connect an already connected NetClass with all the clients");
        }
        public void Connect(int conId) 
        {
            //if () Error($"Attempt to connect an already connected NetClass to the client {conId}");
        }
        public void Connect(NetClass netClass) 
        {
            if (netClass.IsOnline) Error("Attempt to connect an already connected NetClass with all the clients");
        }
        public void Connect(int conId, NetClass netClass)
        {
            //if () Debug.Log($"Attempt to connect an already connected NetClass to the client {conId}");
        }


        public Action<object, NetClass> infoEvent, errorEvent, warningEvent;
        protected void Info(object message) => infoEvent?.Invoke(message,this);
        protected void Warn(object message) => warningEvent?.Invoke(message,this);
        protected void Error(object message) => errorEvent?.Invoke(message,this);
#endif
    } 
}