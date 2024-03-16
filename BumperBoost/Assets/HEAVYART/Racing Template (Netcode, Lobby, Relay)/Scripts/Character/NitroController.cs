using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace HEAVYART.Racing.Netcode
{
    public class NitroController : NetworkBehaviour
    {
        public float currentNitro { get; private set; }
        public float maxNitro { get; private set; }

        public bool isNitroActivated { get; private set; }
        public float torqueMultiplier { get; private set; }

        private float nitroUsagePerSecond;
        private bool nitroInput = false;

        private CarPhysicsController carPhysicsController;
        private CharacterIdentityControl identityControl;
        private ModifiersControlSystem modifiersControlSystem;

        private NetworkVariable<bool> synchronizedNitroStatus = new NetworkVariable<bool>(writePerm: NetworkVariableWritePermission.Owner);
        private NetworkVariable<float> synchronizedNitroAmount = new NetworkVariable<float>(writePerm: NetworkVariableWritePermission.Owner);

        private void Start()
        {
            identityControl = GetComponent<CharacterIdentityControl>();
            carPhysicsController = GetComponent<CarPhysicsController>();
            modifiersControlSystem = GetComponent<ModifiersControlSystem>();
        }

        public void Initialize(float maxNitro, float torqueMultiplier, float nitroUsagePerSecond)
        {
            currentNitro = maxNitro;
            this.maxNitro = maxNitro;
            this.nitroUsagePerSecond = nitroUsagePerSecond;
            this.torqueMultiplier = torqueMultiplier;
        }

        public void UpdateInput(bool nitroInput)
        {
            this.nitroInput = nitroInput;
        }

        void FixedUpdate()
        {
            if (identityControl.IsOwner == true)
            {
                //Local client side

                //Receive nitro from pick-ups
                currentNitro = modifiersControlSystem.HandleNitroAmountModifiers(currentNitro);

                //If car is accelerating and nitro button is pressed
                if (carPhysicsController.isAccelerating == true && nitroInput == true)
                {
                    //Activate nitro
                    if (currentNitro > 0)
                    {
                        isNitroActivated = true;
                        currentNitro -= nitroUsagePerSecond * Time.fixedDeltaTime;
                    }
                    else
                        isNitroActivated = false;
                }
                else
                    isNitroActivated = false;

                currentNitro = Mathf.Clamp(currentNitro, 0, maxNitro);

                //Update status and nitro amount
                //Remote clients will use this data to synchronize nitro VFX and UI status bars
                synchronizedNitroStatus.Value = isNitroActivated;
                synchronizedNitroAmount.Value = currentNitro;
            }
            else
            {
                //Remote client side

                //Get synchronized nitro activity status
                isNitroActivated = synchronizedNitroStatus.Value;

                //Get synchronized value
                currentNitro = synchronizedNitroAmount.Value;
            }

        }

        public void Restore()
        {
            currentNitro = maxNitro;
        }
    }
}