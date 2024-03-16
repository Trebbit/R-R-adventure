using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace HEAVYART.Racing.Netcode
{
    public class BulletController : MonoBehaviour
    {
        public float bulletRadius = 0.25f;
        public float bulletLifetime = 1;
        public float lagCompensationFactor = 0.1f;
        public float trackingDistance = 3f;

        private AmmoParameters bulletParameters;
        private GameObject bulletTrail;

        private Vector3 previousPosition;
        private float bulletSpawnTime = 0;
        private bool isWaitingForDestroy = false;

        public Action<AmmoHit> OnBulletHitConfirmed;

        private void Awake()
        {
            //Subscribe to global ammo destroy confirmation event
            GameManager.Instance.OnAmmoDestroyed += OnAmmoDestroyed;
        }

        void Start()
        {
            previousPosition = transform.position;
            bulletSpawnTime = Time.time;
        }

        private void OnDestroy()
        {
            GameManager.Instance.OnAmmoDestroyed -= OnAmmoDestroyed;
        }

        public void Initialize(AmmoParameters bulletParameters, Transform bulletSpawnPointTransform, Transform muzzleFlashPrefab)
        {
            previousPosition = bulletParameters.startPosition;
            transform.position = bulletSpawnPointTransform.position;

            this.bulletParameters = bulletParameters;

            //Create and destroy muzzle flash (with delay)
            Transform muzzleFlash = Instantiate(muzzleFlashPrefab, bulletSpawnPointTransform.position, Quaternion.LookRotation(bulletParameters.direction));
            muzzleFlash.parent = bulletSpawnPointTransform;
            Destroy(muzzleFlash.gameObject, 0.1f);

            //Find and deactivate bullet trail (activates at the end of frame)
            bulletTrail = transform.GetChild(0).gameObject;
            bulletTrail.SetActive(false);
        }

        void FixedUpdate()
        {
            double deltaTime = NetworkManager.Singleton.ServerTime.Time - bulletParameters.startTime;

            if (deltaTime < 0) return;

            //Calculated bullet position ("server bullet")
            Vector3 serverBulletPosition = bulletParameters.startPosition + bulletParameters.direction * (float)(bulletParameters.speed * deltaTime);

            Vector3 bulletLocalMovementStep = bulletParameters.direction * bulletParameters.speed * Time.fixedDeltaTime;

            //Difference between bullet position on server and bullet position on client 
            Vector3 difference = serverBulletPosition - (transform.position + bulletLocalMovementStep);

            //Check if we're not in the front of bullet's calculated position
            if (Vector3.Dot(bulletLocalMovementStep, difference) > 0)
            {
                Vector3 lagCompensation = difference * lagCompensationFactor;

                //Move closer to server bullet position
                transform.position += Vector3.ClampMagnitude(bulletLocalMovementStep + lagCompensation, bulletParameters.speed * Time.fixedDeltaTime);
                transform.rotation = Quaternion.LookRotation(bulletParameters.direction);
            }

            //Difference between current and previous bullet position on the server 
            Vector3 movementDelta = serverBulletPosition - previousPosition;

            //Find a point to start tracking collisions since last frame
            //We use trackingDistance as minimum raycast length to make sure that no collisions are missing on very high speeds
            //For example, car could travel up to 1.11 meters in one frame

            float raycastDistance = Mathf.Max(movementDelta.magnitude, trackingDistance);
            Vector3 startPoint = serverBulletPosition - movementDelta.normalized * raycastDistance;

            //Check bullet hit
            if (isWaitingForDestroy == false && Physics.SphereCast(startPoint, bulletRadius, movementDelta, out RaycastHit hit, raycastDistance))
            {
                bool allowToHandleHit = true;

                //Check if current player didn't hit itself. Doesn't suppose to, but could happen with high ping.
                NetworkObject sender = GameManager.Instance.userControl.FindCharacterByID(bulletParameters.senderID);
                if (sender != null && sender.transform == hit.transform) allowToHandleHit = false;

                if (allowToHandleHit == true && NetworkManager.Singleton.IsServer == true)
                {
                    //Server side only

                    CommandReceiver commandReceiver = hit.transform.GetComponent<CommandReceiver>();
                    ulong receiverNetworkObjectID = ulong.MaxValue;
                    Vector3 relativeHitPoint = Vector3.zero;

                    //Check if object is able to receive modifiers
                    if (commandReceiver != null)
                    {
                        //Receive hit modifiers (broadcast message from server to clients)
                        commandReceiver.ReceiveAmmoHitClientRpc(bulletParameters.modifiers, bulletParameters.senderID, NetworkManager.Singleton.ServerTime.Time);
                        receiverNetworkObjectID = commandReceiver.NetworkObjectId;

                        //Hit point in receiver's local space
                        //Required to simulate explosion wave from the correct point
                        relativeHitPoint = hit.point - hit.transform.position;
                    }

                    //Ammo destroy control
                    //Make everyone know, this ammo has been destroyed
                    GameManager.Instance.ConfirmAmmoDestroyClientRpc(new AmmoHit()
                    {
                        ammoUID = bulletParameters.ammoUID,
                        hitPoint = hit.point, //World space
                        relativeHitPoint = relativeHitPoint, //Local space
                        hitNetworkObjectID = receiverNetworkObjectID
                    });

                    //Deactivate collision tracking
                    isWaitingForDestroy = true;
                }
            }

            previousPosition = serverBulletPosition;

            //Activates bullet trail at the end of frame (it deactivates on spawn)
            //Here could be placed any other visual activation logic
            bulletTrail.gameObject.SetActive(true);

            //Destroy bullets by timeout
            if (Time.time > bulletSpawnTime + bulletLifetime)
            {
                Destroy(gameObject);
            }
        }

        private void OnAmmoDestroyed(AmmoHit ammoHitData)
        {
            if (ammoHitData.ammoUID == bulletParameters.ammoUID)
            {
                //Server confirmed, this ammo has been destroyed
                //Run destroy scenario
                StartCoroutine(RunAmmoDestroy(ammoHitData));
            }
        }

        private IEnumerator RunAmmoDestroy(AmmoHit ammoHitData)
        {
            //For better look we can wait for a few frames, to make ammo reach it's hit point.
            //It keeps flying while we wait.
            float delay = (transform.position - ammoHitData.hitPoint).magnitude / bulletParameters.speed;
            yield return new WaitForSeconds(delay);

            //BAM! Hit event.
            OnBulletHitConfirmed?.Invoke(ammoHitData);

            //Destroy ammo
            bulletTrail.SetActive(false);
            Destroy(gameObject);
        }
    }
}