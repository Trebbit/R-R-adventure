using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HEAVYART.Racing.Netcode
{
    public class StartGamePanelUIController : MonoBehaviour
    {
        public void StartQuickGame()
        {
            LobbyParameters lobbyParameters = new LobbyParameters();
            lobbyParameters.playersCount = SettingsManager.Instance.gameplay.defaultPlayerCount;
            lobbyParameters.botCount = SettingsManager.Instance.gameplay.botsCount;
            lobbyParameters.lapCount = SettingsManager.Instance.gameplay.lapsCount;
            lobbyParameters.version = SettingsManager.Instance.common.projectVersion;

            LobbyManager.Instance.JoinOrCreateLobby(lobbyParameters);
            MainMenuUIManager.Instance.ShowWaitingForPublicGamePopup();
        }
    }
}