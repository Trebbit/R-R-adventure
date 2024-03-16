
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HEAVYART.Racing.Netcode
{
    public class AIWayPointGroup : MonoBehaviour
    {
        [HideInInspector]
        public int index;

        public Transform[] wayPoints;

        public Vector3 GetRandomWayPoint()
        {
            return wayPoints[Random.Range(0, wayPoints.Length)].position;
        }
    }
}
