using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HEAVYART.Racing.Netcode
{
    public class LobbySettings : MonoBehaviour
    {
        [Header("Timeouts (ms)")]
        public int waitForPlayersToInitializeDelay = 2000;
        public int waitForPlayersReadyResponseDelay = 2000;
        public int waitForPlayersToRemoveDelay = 2000;
        public int waitBeforeLoadSceneDelay = 2000;

        [Space()]
        public int lobbyHeartbeatRate = 20000;
        public int autoRefreshRate = 10000;

        [Space()]
        public int regionsUpdateRateHours = 24;
    }
}
