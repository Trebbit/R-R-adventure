using System;
using Unity.Netcode;
using UnityEngine;

namespace HEAVYART.Racing.Netcode
{
    public struct AmmoParameters : INetworkSerializable
    {
        public ulong senderID;
        public int ammoUID;
        public float speed;
        public double startTime;
        public Vector3 startPosition;
        public Vector3 direction;

        public ModifierBase[] modifiers;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref senderID);
            serializer.SerializeValue(ref ammoUID);
            serializer.SerializeValue(ref speed);
            serializer.SerializeValue(ref startTime);
            serializer.SerializeValue(ref startPosition);
            serializer.SerializeValue(ref direction);

            serializer.SerializeValue(ref modifiers);
        }

        public void AddModifier(ModifierBase modifier)
        {
            if (modifiers == null)
            {
                modifiers = new ModifierBase[] { modifier };
                return;
            }

            ModifierBase[] newArray = new ModifierBase[modifiers.Length + 1];
            Array.Copy(modifiers, newArray, modifiers.Length);
            newArray[modifiers.Length - 1] = modifier;

            modifiers = newArray;
        }
    }
}