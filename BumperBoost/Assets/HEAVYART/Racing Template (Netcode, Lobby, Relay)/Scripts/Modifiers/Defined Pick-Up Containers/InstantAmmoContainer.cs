using UnityEngine;

namespace HEAVYART.Racing.Netcode
{
    [CreateAssetMenu(fileName = "Ammo", menuName = "Modifier Container/Instant Ammo")]
    public class InstantAmmoContainer : ModifierContainerBase
    {
        public WeaponType weapon;
        public int ammo;

        public override ModifierBase GetConfig()
        {
            return new InstantAmmo
            {
                weapon = weapon,
                ammo = ammo
            };
        }
    }
}
