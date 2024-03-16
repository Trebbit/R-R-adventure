using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HEAVYART.Racing.Netcode
{
    [Serializable]
    public struct WeaponConfig
    {
        public string title;

        public WeaponType weaponType;

        public float damage;
        public float movementSpeed;
        public float fireRate;

        [Range(0, 1)] public float accuracyRange;

        public Transform ammoPrefab;
        public Transform muzzleFlashPrefab;
        public bool duplicateAmmoLocally;

        public bool infiniteAmmo;
    }
}
