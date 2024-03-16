using System.Collections;
using System.Collections.Generic;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;

namespace HEAVYART.Racing.Netcode
{
    public class GameplaySettings : MonoBehaviour
    {
        public string defaultGameSceneName;

        [Space]
        public int defaultPlayerCount = 2;

        public int minPlayers = 1;
        public int maxPlayers = 5;

        [Space]
        public int botsCount = 5;
        public int maxBotsCount = 5;

        [Space]
        public int lapsCount = 3;
        public int maxLapsCount = 5;

        [Space]
        public float delayBeforeCountdown;
        public float startGameCountdownTime;
        public float endOfGameCountdownTime;

        private IEnumerator Start()
        {
            yield return new WaitUntil(() => UnityServices.State == ServicesInitializationState.Initialized);
            yield return new WaitUntil(() => AuthenticationService.Instance.IsSignedIn == true);

            if (PlayerDataKeeper.selectedScene == "none")
                PlayerDataKeeper.selectedScene = defaultGameSceneName;
        }
    }
}
