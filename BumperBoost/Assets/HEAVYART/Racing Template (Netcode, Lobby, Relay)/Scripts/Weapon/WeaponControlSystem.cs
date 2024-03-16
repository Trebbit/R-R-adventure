using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace HEAVYART.Racing.Netcode
{
    public class WeaponControlSystem : NetworkBehaviour
    {
        public List<Weapon> weapons = new List<Weapon>();

        public override void OnNetworkSpawn()
        {
            for (int i = 0; i < weapons.Count; i++)
            {
                //Initialize ammo unique id counter
                weapons[i].InitializeAmmoUIDCounter(NetworkObjectId, i);
            }
        }

        public Weapon GetWeapon(WeaponType weaponType)
        {
            for (int i = 0; i < weapons.Count; i++)
            {
                //Find the right one
                if (weapons[i].weaponType == weaponType)
                {
                    return weapons[i];
                }
            }

            return null;
        }
    }
}