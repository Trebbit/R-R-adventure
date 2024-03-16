using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HEAVYART.Racing.Netcode
{
    [CreateAssetMenu(fileName = "Nitro", menuName = "Modifier Container/Instant Nitro")]
    public class InstantNitroContainer : ModifierContainerBase
    {
        public float nitro;

        public override ModifierBase GetConfig()
        {
            return new InstantNitro()
            {
                nitroAmount = nitro
            };
        }
    }
}
