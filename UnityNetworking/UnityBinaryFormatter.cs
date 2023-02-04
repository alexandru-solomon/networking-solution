//Copyright 2022, Alexandru Solomon, All rights reserved.
//Contacts: Alexandru.Solomon.inbox@gmail.com

using CNet.Serialization;
using System.Reflection;
using UnityEngine;
using System;

namespace CNet.Unity.Serialization
{
    public class UnityBinaryFormatter : BinaryFormatter
    {
        public UnityBinaryFormatter(Assembly[] assembly, Type flagType) : base(assembly, flagType) 
        {

        }
        
    }
        [Packet] public struct NetVector2:IRepresentant<Vector2>
        {
            public float x;
            public float y;

            public NetVector2(Vector2 vector2)
            {
                x = vector2.x;
                y = vector2.y;
            }
            public Vector2 GetRepresented()
            {
                return new Vector2(x, y); 
            }
        }
}
