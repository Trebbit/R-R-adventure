using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HEAVYART.Racing.Netcode
{
    [Serializable]
    public class AIConfig
    {
        public string label;

        [Space]
        public GameObject botPrefab;
        public float health = 100;

        [Space]
        public float nitro = 100;
        public float nitroUsagePerSecond = 20;
        public float nitroTorqueMultiplier = 3;

        [Space]
        public float respawnDelay = 3;

        [Space]
        public float steeringFactor = 2f;

        [Space]
        public float angleToStopAccelerate = 15f;
        public float speedToStopAccelerate = 180f;

        [Space]
        public float angleForHandbrake = 35f;
        public float speedForHandbrake = 100f;

        [Space]
        public float angleToUseNitro = 15f;
        public float maxSpeedToUseNitro = 90f;

        [Space]
        public float minLifetimeToUseNitro = 4f;
        public float minLifetimeToUseMachinegun = 5f;

        [Space]
        public float stuckTimeToMoveBackwards = 0.2f;
        public float moveBackwardsTime = 1.5f;
        public float speedToConsiderAsStuck = 2f;

        [Space]
        public float maxTimeToReachWayPoint = 7;
        public float speedLimitToRespawn = 20;

        [Space]
        public float minDistanceToChangePathPoint = 20;

        [Space]
        public float distanceToFireMachinegun = 100;
        public float distanceToFireRocket = 30;
        public float distanceToPlantMine = 30;

        [Space]
        public string weaponTargetingTag = "Player";
    }
}
