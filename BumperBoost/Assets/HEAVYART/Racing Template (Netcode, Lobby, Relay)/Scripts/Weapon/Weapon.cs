using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Unity.Netcode;
using UnityEngine;
using Random = UnityEngine.Random;

namespace HEAVYART.Racing.Netcode
{
    public class Weapon : NetworkBehaviour
    {
        public WeaponType weaponType;
        public Transform firePointTransform;

        public int ammo { get; private set; }
        public bool isInfiniteAmmo => weaponConfig.infiniteAmmo;

        private WeaponConfig weaponConfig;
        private float lastFireTime = 0;
        private int ammoUIDCounter;

        private CharacterIdentityControl identityControl;
        private ModifiersControlSystem modifiersControlSystem;

        private NetworkVariable<int> synchronizedAmmo = new NetworkVariable<int>(writePerm: NetworkVariableWritePermission.Owner);


        private void Awake()
        {
            weaponConfig = SettingsManager.Instance.weapon.GetWeaponConfig(weaponType);
            identityControl = transform.root.GetComponent<CharacterIdentityControl>();
            modifiersControlSystem = transform.root.GetComponent<ModifiersControlSystem>();
        }

        private void FixedUpdate()
        {
            //Update ammo
            if (identityControl.IsOwner == true)
            {
                //Receive ammo from pick-ups
                ammo = modifiersControlSystem.HandleAmmoCountModifiers(weaponConfig.weaponType, ammo);
                synchronizedAmmo.Value = ammo;
            }
            else
                ammo = synchronizedAmmo.Value;
        }

        public void Fire()
        {
            float currentFireRate = weaponConfig.fireRate;
            if (ammo <= 0 && weaponConfig.infiniteAmmo == false) return;

            //Weapon fire
            if (lastFireTime + currentFireRate < Time.time)
            {
                //Prepare ammo data
                AmmoParameters ammoParameters = new AmmoParameters();

                //Current character (scene object) ID.
                ammoParameters.senderID = NetworkObjectId;

                //Update UID counter
                ammoUIDCounter++;
                ammoParameters.ammoUID = ammoUIDCounter;

                ammoParameters.speed = weaponConfig.movementSpeed;
                ammoParameters.startTime = NetworkManager.Singleton.ServerTime.Time;
                ammoParameters.startPosition = transform.position;

                //Set ammo direction according to accuracy settings and active modifiers
                float range = 1f - weaponConfig.accuracyRange;
                Vector3 accuracyOffset = new Vector3(Random.Range(-range, range), 0, Random.Range(-range, range));
                ammoParameters.direction = (transform.forward + accuracyOffset).normalized;

                //Add instant damage modifier (command)
                ammoParameters.AddModifier(new InstantDamage() { damage = weaponConfig.damage });

                //Send to server (it will broadcast this message to clients)
                SendFireServerRPC(ammoParameters);

                //Spawn is immediate, without waiting for response from server. 
                //Doesn't cause any damage or hit reactions, gameplay etc. It's just a visual. Real "bullets" are processing with formulas on server.
                //However, even "just visual" bullets contain lag compensation algorithms, to be closer to "server bullets" positions.
                //For more details check Bullet class.
                if (weaponConfig.duplicateAmmoLocally && identityControl.isBot == false)
                    SpawnAmmo(ammoParameters);

                ammo--;
                lastFireTime = Time.time;
            }
        }

        [ServerRpc(RequireOwnership = false)]
        public void SendFireServerRPC(AmmoParameters ammoParameters)
        {
            //Broadcasting message. It's server side.
            SendFireClientRPC(ammoParameters);
        }

        [ClientRpc]
        private void SendFireClientRPC(AmmoParameters ammoParameters)
        {
            //It's client side.

            //Don't spawn our own ammo if local ammo duplication is enabled (it's already spawned)
            bool isAmmoOwner = ammoParameters.senderID == GameManager.Instance.userControl.localPlayer.NetworkObjectId;
            if (weaponConfig.duplicateAmmoLocally == true && isAmmoOwner == true)
                return;

            //Process regular ammo spawn
            SpawnAmmo(ammoParameters);
        }

        private void SpawnAmmo(AmmoParameters ammoParameters)
        {
            Transform instantiatedAmmo = Instantiate(weaponConfig.ammoPrefab, firePointTransform.position, transform.rotation, GameManager.Instance.temp.transform);

            //Bullet or missile
            if (instantiatedAmmo.TryGetComponent(out BulletController bulletComponent))
            {
                bulletComponent.Initialize(ammoParameters, firePointTransform, weaponConfig.muzzleFlashPrefab);
                return;
            }

            //Mine
            if (instantiatedAmmo.TryGetComponent(out MineController mineComponent))
            {
                mineComponent.Initialize(ammoParameters);
            }
        }

        public void InitializeAmmoUIDCounter(ulong weaponOwnerNetworkID, int weaponIndex)
        {
            //Every fired ammo has a unique ID
            //This ID allows us to synchronize ammo hit and destroy precisely
            //UID example: 20010003 (user id = 2, weapon id = 1, fired ammo count (from this particular weapon) = 3)

            int counterOffsetPerOwnerID = 1000000;//Million
            int counterOffsetPerWeapon = 10000;

            ammoUIDCounter = (int)weaponOwnerNetworkID * counterOffsetPerOwnerID + weaponIndex * counterOffsetPerWeapon;
        }
    }
}
