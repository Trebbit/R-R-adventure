using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace HEAVYART.Racing.Netcode
{
    public class StartNewGamePopupUIController : MonoBehaviour
    {
        public Slider playersCountSlider;
        public Slider botCountSlider;
        public Slider lapsCountSlider;

        public Text playersCountText;
        public Text botCountText;
        public Text lapsCountText;

        public float playersCount { get; set; }
        public float botCount { get; set; }
        public float lapsCount { get; set; }

        void Start()
        {
            playersCount = SettingsManager.Instance.gameplay.defaultPlayerCount;
            botCount = SettingsManager.Instance.gameplay.botsCount;
            lapsCount = SettingsManager.Instance.gameplay.lapsCount;

            playersCountSlider.minValue = SettingsManager.Instance.gameplay.minPlayers;
            playersCountSlider.maxValue = SettingsManager.Instance.gameplay.maxPlayers;

            botCountSlider.minValue = 0;
            botCountSlider.maxValue = SettingsManager.Instance.gameplay.maxBotsCount;

            lapsCountSlider.minValue = 1;
            lapsCountSlider.maxValue = SettingsManager.Instance.gameplay.maxLapsCount;

            playersCountSlider.value = playersCount;
            botCountSlider.value = botCount;
            lapsCountSlider.value = lapsCount;
        }

        void FixedUpdate()
        {
            playersCountText.text = "NUMBER OF PLAYERS: " + playersCount;
            botCountText.text = "NUMBER OF BOTS: " + botCount;
            lapsCountText.text = "NUMBER OF LAPS: " + lapsCount;
        }

        public void OnStartPublicGame()
        {
            LobbyParameters lobbyParameters = new LobbyParameters();
            lobbyParameters.playersCount = (int)playersCount;
            lobbyParameters.botCount = (int)botCount;
            lobbyParameters.lapCount = (int)lapsCount;
            lobbyParameters.isPublic = true; //public
            lobbyParameters.version = SettingsManager.Instance.common.projectVersion;

            LobbyManager.Instance.CreateLobby(lobbyParameters);
            MainMenuUIManager.Instance.ShowWaitingForPublicGamePopup();
        }

        public void OnStartPrivateGame()
        {
            LobbyParameters lobbyParameters = new LobbyParameters();
            lobbyParameters.playersCount = (int)playersCount;
            lobbyParameters.botCount = (int)botCount;
            lobbyParameters.lapCount = (int)lapsCount;
            lobbyParameters.isPublic = false; //private
            lobbyParameters.version = SettingsManager.Instance.common.projectVersion;

            LobbyManager.Instance.CreateLobby(lobbyParameters);
            MainMenuUIManager.Instance.ShowWaitingForPrivateGamePopup();
        }

        public void CloseWindow()
        {
            gameObject.SetActive(false);
            MainMenuUIManager.Instance.ShowMainGamePanel();
        }
    }
}