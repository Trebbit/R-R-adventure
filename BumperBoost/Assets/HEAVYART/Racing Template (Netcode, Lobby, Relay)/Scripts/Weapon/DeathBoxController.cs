using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using Unity.Netcode;
using UnityEngine;

namespace HEAVYART.Racing.Netcode
{
    public class DeathBoxController : MonoBehaviour
    {
        public float explosionForce = 2800;
        public float stoppingForce = 250;
        public float velocityFactor = 3f;
      //  private GameObject explosionEffect;

        private AmmoParameters ammoParameters;
        //private bool isWaitingForDestroy = false;

      //  private float timeToActivateMineForOwner = 1;

        private void Awake()
        {
            //Subscribe to global ammo destroy confirmation event
            //  GameManager.Instance.OnAmmoDestroyed += OnAmmoDestroyed;
            // explosionEffect = transform.Find("Particle").gameObject;
            AmmoParameters ammoParameters = new AmmoParameters();

            //Current character (scene object) ID.
            ammoParameters.senderID = new ulong();

            ammoParameters.ammoUID = 999999999;

            ammoParameters.speed = 0;
            ammoParameters.startTime = NetworkManager.Singleton.ServerTime.Time;
            ammoParameters.startPosition = transform.position;

            ammoParameters.direction = Vector3.zero;

            //Add instant damage modifier (command)
            ammoParameters.AddModifier(new InstantDamage() { damage = 100000.0f });
            this.ammoParameters = ammoParameters;
        }

        private void OnDestroy()
        {
            //GameManager.Instance.OnAmmoDestroyed -= OnAmmoDestroyed;
            //Prepare ammo data

        }


        private void OnTriggerEnter(Collider other)
        {
            //Precaution for player to not to explode on it's own mine while planting
            double timeLeftToActivateForOwner = NetworkManager.Singleton.ServerTime.Time - ammoParameters.startTime;

            if ( other.transform.root.TryGetComponent(out Rigidbody rigidbodyComponent))
            {

                if (NetworkManager.Singleton.IsServer == true)
                {
                    //Server side only

                    CommandReceiver commandReceiver = rigidbodyComponent.transform.GetComponent<CommandReceiver>();
                    ulong receiverNetworkObjectID = ulong.MaxValue;
                    Vector3 relativeHitPoint = Vector3.zero;

                    //Check if object is able to receive modifiers
                    if (commandReceiver != null)
                    {
                        //Receive hit modifiers (broadcast message from server to clients)
                        commandReceiver.ReceiveAmmoHitClientRpc(ammoParameters.modifiers, ammoParameters.senderID, NetworkManager.Singleton.ServerTime.Time);
                       // receiverNetworkObjectID = commandReceiver.NetworkObjectId;

                        //Hit point in receiver's local space
                        //Required to simulate explosion wave from the correct point
                       // relativeHitPoint = transform.position - commandReceiver.transform.position;
                    }

                    //GameManager.Instance.ConfirmAmmoDestroyClientRpc(new AmmoHit()
                    //{
                    //    ammoUID = ammoParameters.ammoUID,
                    //    hitNetworkObjectID = receiverNetworkObjectID,
                    //    relativeHitPoint = relativeHitPoint, //Local space
                    //});

                }
            }
        }

        //private void OnAmmoDestroyed(AmmoHit ammoHitData)
        //{
        //    if (ammoHitData.ammoUID == ammoParameters.ammoUID)
        //    {
        //        NetworkObject hitNetworkObject = GameManager.Instance.userControl.FindCharacterByID(ammoHitData.hitNetworkObjectID);

        //        //Check if receiver still exists 
        //        if (hitNetworkObject != null && hitNetworkObject.TryGetComponent(out Rigidbody rigidbodyComponent))
        //        {
        //            //Check if it's a character 
        //            if (rigidbodyComponent.TryGetComponent(out CharacterIdentityControl identityControl))
        //            {
        //                //Check if it's our character (car)
        //                if (identityControl.IsOwner == true)
        //                {
        //                    Vector3 stoppingForceVector = rigidbodyComponent.velocity.normalized * stoppingForce * -1;
        //                    stoppingForceVector *= rigidbodyComponent.velocity.magnitude * velocityFactor;

        //                    //Apply explosion force
        //                    rigidbodyComponent.AddForceAtPosition(Vector3.up * explosionForce + stoppingForceVector, rigidbodyComponent.position + ammoHitData.relativeHitPoint, ForceMode.Impulse);
        //                }
        //            }
        //        }

        //    }
        //}
    }
}