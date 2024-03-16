using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace HEAVYART.Racing.Netcode
{
    public class PlayerProfile : INetworkSerializable
    {
        public string name;
        public Color color;

        //Required to serialize custom types (INetworkSerializable method)
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref name);
            serializer.SerializeValue(ref color);
        }
    }
}
