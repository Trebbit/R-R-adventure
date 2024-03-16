using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace HEAVYART.Racing.Netcode
{
    public class PickUpItemController : NetworkBehaviour
    {
        public ModifierContainerBase container;
        public float restoreTime = 5f;

        private double nextActivationTimeStamp = 0;

        private GameObject graphics;
        private Collider physics;

        private void Start()
        {
            graphics = transform.GetChild(0).gameObject;
            physics = transform.GetComponent<Collider>();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (NetworkManager.Singleton.IsServer == false) return;

            CommandReceiver commandReceiver = other.transform.root.GetComponent<CommandReceiver>();

            if (commandReceiver != null)
            {
                //Pick up element and broadcast message. It's server side.
                commandReceiver.ReceiveModifiersClientRpc(new ModifierBase[] { container.GetConfig() }, 0, NetworkManager.Singleton.ServerTime.Time);

                OnPickUpItemHitClientRpc(NetworkManager.Singleton.ServerTime.Time + restoreTime);
            }
        }

        private void FixedUpdate()
        {
            if (NetworkManager.Singleton.ServerTime.Time < nextActivationTimeStamp)
            {
                if (graphics.activeSelf == true)
                {
                    graphics.SetActive(false);
                    physics.enabled = false;
                }
            }
            else
            {
                if (graphics.activeSelf == false)
                {
                    graphics.SetActive(true);
                    physics.enabled = true;
                }
            }
        }

        [ClientRpc]
        private void OnPickUpItemHitClientRpc(double nextActivationTimeStamp)
        {
            this.nextActivationTimeStamp = nextActivationTimeStamp;
        }
    }
}
