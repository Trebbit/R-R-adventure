using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace HEAVYART.Racing.Netcode
{
    public class HealthController : NetworkBehaviour
    {
        public float currentHealth { get; private set; }
        public float maxHealth { get; private set; }
        public bool isAlive => currentHealth > 0;

        public Action OnDeath;

        private ModifiersControlSystem modifiersControlSystem;
        private CharacterIdentityControl identityControl;

        private NetworkVariable<float> synchronizedHealthAmount = new NetworkVariable<float>(100, writePerm: NetworkVariableWritePermission.Owner);

        public void Awake()
        {
            modifiersControlSystem = GetComponent<ModifiersControlSystem>();
            identityControl = GetComponent<CharacterIdentityControl>();
        }

        public void Initialize(float maxHealth)
        {
            currentHealth = maxHealth;
            this.maxHealth = maxHealth;
        }

        private void FixedUpdate()
        {
            if (identityControl.IsOwner == true)
            {
                //Local client side

                if (isAlive == false) return;

                //Receive health from pick-ups or take damage
                float updatedHealth = modifiersControlSystem.HandleHealthModifiers(currentHealth, OnDeathEvent);
                currentHealth = Mathf.Clamp(updatedHealth, 0, maxHealth);

                //Update value for synchronization on remote clients side
                synchronizedHealthAmount.Value = currentHealth;
            }
            else
            {
                //Remote client side

                //Get synchronized value
                currentHealth = synchronizedHealthAmount.Value;
            }
        }

        private void OnDeathEvent(ActiveModifierData activeModifier)
        {
            //Broadcast death event. It's local client side.
            BroadcastDeathEventServerRPC();
        }

        [ServerRpc]
        public void BroadcastDeathEventServerRPC()
        {
            //Broadcasting message. It's server side.
            ConfirmCharacterDeathClientRpc();
        }

        [ClientRpc]
        private void ConfirmCharacterDeathClientRpc()
        {
            //Receive and apply. It's client side.
            currentHealth = 0;
            OnDeath?.Invoke();
        }

        public void Restore()
        {
            currentHealth = maxHealth;
        }
    }
}