using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HEAVYART.Racing.Netcode
{
    [Serializable]
    public class InstantAmmo : InstantModifier
    {
        public WeaponType weapon;
        public int ammo;

        public InstantAmmo()
        {
            type = GetType().ToString();
        }

        protected override void SerializeModifier()
        {
            object[] outputData = new object[] { (int)weapon, ammo };

            serializedData = Newtonsoft.Json.JsonConvert.SerializeObject(outputData);
        }

        protected override ModifierBase DeserializeModifier(string inputData)
        {
            object[] data = Newtonsoft.Json.JsonConvert.DeserializeObject<object[]>(inputData);

            weapon = (WeaponType)Convert.ToInt32(data[0]);
            ammo = Convert.ToInt32(data[1]);

            return this;
        }
    }
}
