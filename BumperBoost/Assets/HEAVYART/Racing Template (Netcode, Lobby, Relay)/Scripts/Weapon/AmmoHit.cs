using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace HEAVYART.Racing.Netcode
{
    public class AmmoHit : INetworkSerializable
    {
        public int ammoUID;
        public Vector3 hitPoint;
        public Vector3 relativeHitPoint;
        public ulong hitNetworkObjectID;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref ammoUID);
            serializer.SerializeValue(ref hitPoint);
            serializer.SerializeValue(ref relativeHitPoint);
            serializer.SerializeValue(ref hitNetworkObjectID);
        }
    }
}