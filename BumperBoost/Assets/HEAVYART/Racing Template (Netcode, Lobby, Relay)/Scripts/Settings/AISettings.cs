using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HEAVYART.Racing.Netcode
{
    public class AISettings : MonoBehaviour
    {
        public List<AIConfig> configs = new List<AIConfig>();

        [Space()]
        public List<Color> availableColors;

        public Color GetRandomColor()
        {
            return availableColors[Random.Range(0, availableColors.Count)];
        }
    }
}
