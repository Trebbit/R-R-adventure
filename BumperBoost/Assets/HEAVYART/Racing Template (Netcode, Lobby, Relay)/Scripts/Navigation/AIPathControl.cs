using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HEAVYART.Racing.Netcode
{
    public class AIPathControl : MonoBehaviour
    {
        public bool drawPath = false;

        public AIWayPointGroup[] waypointGroups;

        private void Start()
        {
            for (int i = 0; i < waypointGroups.Length; i++)
            {
                waypointGroups[i].index = i;
            }
        }

        public AIWayPointGroup GetNextWayPointGroup(int currentGroupIndex)
        {
            int nextGroupIndex = currentGroupIndex + 1;

            if (nextGroupIndex >= waypointGroups.Length)
                nextGroupIndex = 0;

            return waypointGroups[nextGroupIndex];
        }

        public AIWayPointGroup GetNearestWayPointGroup(Vector3 position)
        {
            int index = 0;
            float minDistance = (waypointGroups[0].transform.position - position).magnitude;

            for (int i = 1; i < waypointGroups.Length; i++)
            {
                float distance = (waypointGroups[i].transform.position - position).magnitude;

                //Find nearest waypoint
                if (distance < minDistance)
                {
                    index = i;
                    minDistance = distance;
                }
            }

            //Return nearest waypoint
            return waypointGroups[index];
        }


        private void OnDrawGizmos()
        {
            //Draw path for debug
            if (drawPath == false) return;

            if (waypointGroups.Length > 0)
            {
                for (int i = 1; i < waypointGroups.Length; i++)
                {
                    for (int j = 0; j < waypointGroups[i - 1].wayPoints.Length; j++)
                        Debug.DrawLine(waypointGroups[i - 1].wayPoints[j].position, waypointGroups[i].wayPoints[j].position, Color.white);
                }

                for (int j = 0; j < waypointGroups[0].wayPoints.Length; j++)
                    Debug.DrawLine(waypointGroups[0].wayPoints[j].position, waypointGroups[waypointGroups.Length - 1].wayPoints[j].position, Color.white);
            }
        }
    }
}
