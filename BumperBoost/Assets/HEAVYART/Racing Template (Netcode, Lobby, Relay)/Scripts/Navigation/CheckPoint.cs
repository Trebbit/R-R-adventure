using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HEAVYART.Racing.Netcode
{
    public class CheckPoint : MonoBehaviour
    {
        [HideInInspector] public int index;

        public Transform GetRespawnPoint()
        {
            return transform.Find("RespawnPoint");
        }
    }
}
