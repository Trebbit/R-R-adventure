using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HEAVYART.Racing.Netcode
{
    [Serializable]
    public class PlayerConfig
    {
        public string label;

        public GameObject playerPrefab;
        public GameObject previewModelPrefab;

        public float health = 100;

        [Space]
        public float nitro = 100;
        public float nitroUsagePerSecond = 20;
        public float nitroTorqueMultiplier = 3;
    }
}
