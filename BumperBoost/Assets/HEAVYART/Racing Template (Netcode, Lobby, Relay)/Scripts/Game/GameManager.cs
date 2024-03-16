using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HEAVYART.Racing.Netcode
{
    public class GameManager : NetworkBehaviour
    {
        public static GameManager Instance { get; private set; }
        public NetworkObjectsControl userControl { get; private set; }
        public NetworkObjectsSpawner spawnControl { get; private set; }
        public AIPathControl aiPathControl { get; private set; }
        public CheckpointControl checkPointControl { get; private set; }
        public InGameUI UI { get; private set; }

        //Object to store skid marks, UI status bars, etc.
        public GameObject temp { get; private set; }

        public int expectedPlayersCount { get; private set; }
        public int botsCount { get; private set; }
        public int lapsCount { get; private set; }

        public GameState gameState { get; private set; }

        public Action OnNetworkReady;
        public Action OnGameStart;
        public Action OnFinalCountdown;
        public Action OnGameEnd;
        public Action OnDisconnect;

        public Action<AmmoHit> OnAmmoDestroyed;

        public double gameStartTime { get; private set; }
        public double gameFinalCountdownStartTime { get; private set; }
        public double gameEndTime { get; private set; }


        public Dictionary<ulong, LeaderboardUserProfile> leaderboard = new Dictionary<ulong, LeaderboardUserProfile>();

        private void Awake()
        {
            Instance = this;
            userControl = GetComponent<NetworkObjectsControl>();
            spawnControl = GetComponent<NetworkObjectsSpawner>();
            aiPathControl = GetComponent<AIPathControl>();
            checkPointControl = GetComponent<CheckpointControl>();

            UI = FindObjectOfType<InGameUI>();

            gameState = GameState.WaitingForPlayers;

            //Run demo scene gameplay
            if (SceneManager.GetActiveScene().name == "Demo") NetworkManager.Singleton.StartHost();

            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnectCallback;
            NetworkManager.Singleton.GetComponent<UnityTransport>().OnTransportEvent += OnTransportEvent;

            SceneLoadManager.Instance.UnsubscribeNetworkSceneUpdates();

            StartCoroutine(WaitForNetworkReady());

            temp = new GameObject("Temp");
        }

        IEnumerator WaitForNetworkReady()
        {
            //OnNetworkSpawn is not working if gameObject was placed on scene instead of Instantiate
            //Here is the way to avoid this problem
            while (IsSpawned == false) yield return 0;

            OnNetworkReady?.Invoke();
        }

        private new void OnDestroy()
        {
            if (NetworkManager.Singleton != null)
            {
                NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnectCallback;
                NetworkManager.Singleton.GetComponent<UnityTransport>().OnTransportEvent -= OnTransportEvent;
            }
        }

        void FixedUpdate()
        {
            //Handle waiting for players
            if (gameState == GameState.WaitingForPlayers)
            {
                int connectedPlayersCount = userControl.playerSceneObjects.Count;
                int expectedPlayersCount = LobbyManager.Instance.players.Count;

                //Update laps count
                if (LobbyManager.Instance.TryGetLobbyParameter("laps", out string lapsValue))
                    lapsCount = Convert.ToInt32(lapsValue);
                else
                    lapsCount = SettingsManager.Instance.gameplay.lapsCount;

                //Update bots count
                if (LobbyManager.Instance.TryGetLobbyParameter("bots", out string botsValue))
                    botsCount = Convert.ToInt32(botsValue);
                else
                    botsCount = SettingsManager.Instance.gameplay.botsCount;

                //Set 1 player if it's Demo scene
                if (SceneManager.GetActiveScene().name == "Demo") expectedPlayersCount = 1;

                //Room is full
                if (connectedPlayersCount == expectedPlayersCount)
                {
                    float networkDelay = 0.5f;

                    double startTime =
                        NetworkManager.ServerTime.Time
                        + SettingsManager.Instance.gameplay.delayBeforeCountdown //a little time for user to figure out what's going on around, after scene has been loaded
                        + SettingsManager.Instance.gameplay.startGameCountdownTime // 3..2..1..GO!
                        + networkDelay;

                    //Broadcast start countdown command
                    if (IsServer) RunStartGameCountdownClientRpc(startTime);
                }
            }

            //Handle countdown
            if (gameState == GameState.WaitingForCountdown)
            {
                if (NetworkManager.ServerTime.Time >= gameStartTime)
                {
                    //Start game
                    gameState = GameState.ActiveGame;
                    OnGameStart?.Invoke();
                }
            }

            //Handle active game
            if (gameState == GameState.FinalCountdown)
            {
                if (NetworkManager.ServerTime.Time >= gameEndTime)
                {
                    //End game
                    gameState = GameState.GameIsOver;
                    OnGameEnd?.Invoke();

                    Debug.Log("End of game");
                }
            }
        }

        [ClientRpc]
        private void RunStartGameCountdownClientRpc(double gameStartTime)
        {
            //Receive and apply. Its client side.

            this.gameStartTime = gameStartTime;
            gameState = GameState.WaitingForCountdown;
        }

        [ClientRpc]
        public void RunEndOfGameCountdownClientRpc(double gameEndTime)
        {
            //Receive and apply. Its client side.
            this.gameEndTime = gameEndTime;
            gameFinalCountdownStartTime = NetworkManager.ServerTime.Time;
            gameState = GameState.FinalCountdown;

            Debug.Log("Final countdown");
        }

        [ClientRpc]
        public void ConfirmAmmoDestroyClientRpc(AmmoHit ammoHitData)
        {
            OnAmmoDestroyed?.Invoke(ammoHitData);
        }

        public void AddLeaderboardUser(CharacterIdentityControl characterIdentityControl)
        {
            LeaderboardUserProfile userProfile = new LeaderboardUserProfile();

            userProfile.id = characterIdentityControl.NetworkObjectId;
            userProfile.finishTimestamp = float.MaxValue;
            userProfile.distanceToFinish = float.MaxValue;
            userProfile.userName = characterIdentityControl.isPlayer ? characterIdentityControl.spawnParameters.Value.name : "Bot " + characterIdentityControl.NetworkObjectId;

            if (leaderboard.ContainsKey(userProfile.id) == false)
                leaderboard.Add(userProfile.id, userProfile);
        }

        private void OnClientDisconnectCallback(ulong clientID)
        {
            if (clientID == NetworkManager.Singleton.LocalClientId)
            {
                Debug.Log("Local client has been closed");
                OnDisconnect?.Invoke();

                gameState = GameState.GameIsOver;
                OnGameEnd?.Invoke();
            }

            if (clientID == 0)
            {
                Debug.Log("Server has been closed.");
                OnDisconnect?.Invoke();

                gameState = GameState.GameIsOver;
                OnGameEnd?.Invoke();
            }
        }

        public void QuitGame()
        {
            LobbyManager.Instance.QuitLobby();
            NetworkManager.Singleton.Shutdown();

            SceneLoadManager.Instance.LoadRegularScene("MainMenu", true);
        }

        public void RestartCurrentScene()
        {
            LobbyManager.Instance.QuitLobby();
            NetworkManager.Singleton.Shutdown();

            SceneLoadManager.Instance.LoadRegularScene(SceneManager.GetActiveScene().name, false);
        }

        private void OnTransportEvent(NetworkEvent eventType, ulong clientId, ArraySegment<byte> payload, float receiveTime)
        {
            //On connection lost (no internet)
            if (eventType == NetworkEvent.TransportFailure)
            {
                Debug.Log("Disconnected");
                LobbyManager.Instance.QuitLobby();
                SceneLoadManager.Instance.LoadRegularScene("MainMenu", true);
            }
        }

        void OnApplicationPause(bool pauseStatus)
        {
            //On hide application
            //Disconnects immediate
            if (pauseStatus == true) QuitGame();
        }

        public void UpdateLeadearboard(ulong networkObjectId, int currentLap, int currentCheckPointIndex, float distanceToFinish)
        {
            leaderboard[networkObjectId].currentLap = currentLap;
            leaderboard[networkObjectId].currentCheckPointIndex = currentCheckPointIndex;
            leaderboard[networkObjectId].distanceToFinish = distanceToFinish;

            //Someone has finished
            if (gameState != GameState.GameIsOver && currentLap == lapsCount && leaderboard[networkObjectId].isFinished == false)
            {
                leaderboard[networkObjectId].finishTimestamp = NetworkManager.ServerTime.Time;
                leaderboard[networkObjectId].isFinished = true;

                Debug.Log(networkObjectId + " has finished.");

                //Start end of game countdown
                if (IsServer == true && gameState != GameState.FinalCountdown)
                {
                    RunEndOfGameCountdownClientRpc(NetworkManager.ServerTime.Time + SettingsManager.Instance.gameplay.endOfGameCountdownTime);
                }
            }
        }
    }
}
