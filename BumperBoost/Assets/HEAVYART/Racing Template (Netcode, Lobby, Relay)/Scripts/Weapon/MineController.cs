using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using Unity.Netcode;
using UnityEngine;

namespace HEAVYART.Racing.Netcode
{
    public class MineController : MonoBehaviour
    {
        public float explosionForce = 2800;
        public float stoppingForce = 250;
        public float velocityFactor = 3f;
        private GameObject explosionEffect;

        private AmmoParameters ammoParameters;
        private bool isWaitingForDestroy = false;

        private float timeToActivateMineForOwner = 1;

        private void Awake()
        {
            //Subscribe to global ammo destroy confirmation event
            GameManager.Instance.OnAmmoDestroyed += OnAmmoDestroyed;
            explosionEffect = transform.Find("Particle").gameObject;
        }

        private void OnDestroy()
        {
            GameManager.Instance.OnAmmoDestroyed -= OnAmmoDestroyed;
        }

        public void Initialize(AmmoParameters ammoParameters)
        {
            this.ammoParameters = ammoParameters;

            //Plant on a ground
            if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit))
            {
                transform.position = hit.point;
                transform.rotation = Quaternion.FromToRotation(transform.up, hit.normal) * transform.rotation;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            //Precaution for player to not to explode on it's own mine while planting
            double timeLeftToActivateForOwner = NetworkManager.Singleton.ServerTime.Time - ammoParameters.startTime;

            if (isWaitingForDestroy == false && other.transform.root.TryGetComponent(out Rigidbody rigidbodyComponent))
            {
                //Check if current player didn't hit itself. Doesn't suppose to, but could happen with high ping.
                NetworkObject sender = GameManager.Instance.userControl.FindCharacterByID(ammoParameters.senderID);
                bool isAmmoOwner = sender != null && sender.transform == rigidbodyComponent.transform;

                if (isAmmoOwner && timeLeftToActivateForOwner < timeToActivateMineForOwner) return;

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
                        receiverNetworkObjectID = commandReceiver.NetworkObjectId;

                        //Hit point in receiver's local space
                        //Required to simulate explosion wave from the correct point
                        relativeHitPoint = transform.position - commandReceiver.transform.position;
                    }

                    GameManager.Instance.ConfirmAmmoDestroyClientRpc(new AmmoHit()
                    {
                        ammoUID = ammoParameters.ammoUID,
                        hitNetworkObjectID = receiverNetworkObjectID,
                        relativeHitPoint = relativeHitPoint, //Local space
                    });

                    //Deactivate collision tracking
                    isWaitingForDestroy = true;
                }
            }
        }

        private void OnAmmoDestroyed(AmmoHit ammoHitData)
        {
            if (ammoHitData.ammoUID == ammoParameters.ammoUID)
            {
                NetworkObject hitNetworkObject = GameManager.Instance.userControl.FindCharacterByID(ammoHitData.hitNetworkObjectID);

                //Check if receiver still exists 
                if (hitNetworkObject != null && hitNetworkObject.TryGetComponent(out Rigidbody rigidbodyComponent))
                {
                    //Check if it's a character 
                    if (rigidbodyComponent.TryGetComponent(out CharacterIdentityControl identityControl))
                    {
                        //Check if it's our character (car)
                        if (identityControl.IsOwner == true)
                        {
                            Vector3 stoppingForceVector = rigidbodyComponent.velocity.normalized * stoppingForce * -1;
                            stoppingForceVector *= rigidbodyComponent.velocity.magnitude * velocityFactor;

                            //Apply explosion force
                            rigidbodyComponent.AddForceAtPosition(Vector3.up * explosionForce + stoppingForceVector, rigidbodyComponent.position + ammoHitData.relativeHitPoint, ForceMode.Impulse);
                        }
                    }
                }

                //Handle VFX
                explosionEffect.SetActive(true);
                explosionEffect.transform.parent = GameManager.Instance.temp.transform;

                Destroy(gameObject);
                Destroy(explosionEffect, 2);
            }
        }
    }
}