using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace HEAVYART.Racing.Netcode
{
    public class CommandReceiver : NetworkBehaviour
    {
        private ModifiersControlSystem modifiersControlSystem;

        private void Awake()
        {
            modifiersControlSystem = GetComponent<ModifiersControlSystem>();
        }

        [ClientRpc]
        public void ReceiveAmmoHitClientRpc(ModifierBase[] modifiers, ulong senderID, double startTime)
        {
            //Receive. Its client side.
            for (int i = 0; i < modifiers.Length; i++)
            {
                modifiersControlSystem.AddModifier(modifiers[i], senderID, startTime);
            }
        }

        [ClientRpc]
        public void ReceiveModifiersClientRpc(ModifierBase[] modifiers, ulong senderID, double startTime)
        {
            //Receive. Its client side.
            for (int i = 0; i < modifiers.Length; i++)
            {
                modifiersControlSystem.AddModifier(modifiers[i], senderID, startTime);
            }
        }
    }
}