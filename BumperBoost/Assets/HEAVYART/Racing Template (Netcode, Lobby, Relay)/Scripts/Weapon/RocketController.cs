using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;

namespace HEAVYART.Racing.Netcode
{
    public class RocketController : MonoBehaviour
    {
        public float explosionForce = 2800;
        public float stoppingForce = 250;
        public float velocityFactor = 3f;
        private GameObject explosionEffect;

        void Awake()
        {
            GetComponent<BulletController>().OnBulletHitConfirmed += OnServerBulletHit;
            explosionEffect = transform.Find("Particle").gameObject;
        }

        private void OnServerBulletHit(AmmoHit ammoHitData)
        {
            //Handle VFX
            explosionEffect.SetActive(true);
            explosionEffect.transform.parent = GameManager.Instance.temp.transform;
            explosionEffect.transform.position = ammoHitData.hitPoint;
            Destroy(explosionEffect, 2);

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

                        //Move explosion VFX a little bit closer to the center of car
                        //It makes it look much better on a high speed
                        float framesToPredictExplosionEffectPosition = 4;
                        explosionEffect.transform.position = rigidbodyComponent.position + ammoHitData.relativeHitPoint + rigidbodyComponent.velocity * Time.fixedDeltaTime * framesToPredictExplosionEffectPosition;
                    }
                }
            }
        }
    }
}