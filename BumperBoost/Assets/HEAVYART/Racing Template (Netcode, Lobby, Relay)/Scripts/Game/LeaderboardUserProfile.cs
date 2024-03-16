using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HEAVYART.Racing.Netcode
{
    public class LeaderboardUserProfile
    {
        public ulong id;
        public string userName;

        public int currentLap;
        public int currentCheckPointIndex;
        public float distanceToFinish;

        public bool isFinished;
        public double finishTimestamp;
    }
}
