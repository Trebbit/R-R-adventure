using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HEAVYART.Racing.Netcode
{
    public class CommonSettings : MonoBehaviour
    {
        public int targetFPS = 60;
        public string projectVersion;

        void Start()
        {
            Application.targetFrameRate = targetFPS;
        }
    }
}
