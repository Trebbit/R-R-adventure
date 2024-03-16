using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace HEAVYART.Racing.Netcode
{
    public class InGameUI : MonoBehaviour
    {
        public HUDController hudStatusBar;

        [Space]
        public RectTransform endOfGamePopup;
        public RectTransform carDestroyedPopup;
        public RectTransform quitGamePopup;
        public RectTransform pcControlsPopup;

        [Space]
        public Text countdownTextComponent;
        public Text finalTimeoutTextComponent;

        [Space]
        public RectTransform mobileControls;

        private int pcControlsShowDuration = 10;
        private double pcControlsShowTime;

        private void Start()
        {
            GameManager.Instance.OnGameEnd += ShowEndOfGamePopup;

            if (Application.isMobilePlatform)
            {
                mobileControls.gameObject.SetActive(true);
            }
            else
            {
                mobileControls.gameObject.SetActive(false);

                pcControlsPopup.gameObject.SetActive(true);
                pcControlsShowTime = NetworkManager.Singleton.ServerTime.Time;
            }
        }

        public void ShowEndOfGamePopup()
        {
            HidePopups();
            endOfGamePopup.gameObject.SetActive(true);
        }

        public void ShowCarDestroyedPopup()
        {
            HidePopups();
            carDestroyedPopup.gameObject.SetActive(true);
        }

        public void ShowQuitGamePopup()
        {
            HidePopups();
            quitGamePopup.gameObject.SetActive(true);
        }

        public void HidePopups()
        {
            endOfGamePopup.gameObject.SetActive(false);
            carDestroyedPopup.gameObject.SetActive(false);
            quitGamePopup.gameObject.SetActive(false);
        }

        public void OnQuitButtonPressed()
        {
            GameManager.Instance.QuitGame();
        }

        public void OnRespawnButton()
        {
            GameManager.Instance.spawnControl.RespawnLocalPlayer();
            GameManager.Instance.UI.HidePopups();
        }

        public void OnRestartButtonPressed()
        {
            GameManager.Instance.RestartCurrentScene();
        }

        public void ShowHUD()
        {
            hudStatusBar.gameObject.SetActive(true);
        }

        public void HideHUD()
        {
            hudStatusBar.gameObject.SetActive(true);
        }

        private void FixedUpdate()
        {
            //Final countdown
            if (GameManager.Instance.gameState == GameState.FinalCountdown)
            {
                TimeSpan timeSpan = TimeSpan.FromSeconds(GameManager.Instance.gameEndTime - NetworkManager.Singleton.ServerTime.Time);

                //Show how much time left
                finalTimeoutTextComponent.text = timeSpan.ToString(@"mm\:ss"); //Format 00:00
            }
            else
                finalTimeoutTextComponent.text = string.Empty;

            //Countdown
            if (GameManager.Instance.gameState == GameState.WaitingForCountdown)
            {
                TimeSpan timeSpan = TimeSpan.FromSeconds(GameManager.Instance.gameStartTime - NetworkManager.Singleton.ServerTime.Time);

                //Show seconds
                if (timeSpan.Seconds <= 3)
                    countdownTextComponent.text = "" + timeSpan.Seconds;

                //But if seconds are less than one, show "GO!" text
                if (timeSpan.Seconds == 0)
                    countdownTextComponent.text = "GO!";
            }
            else
                countdownTextComponent.text = string.Empty;

            //Hide PC controls popup after set duration
            if (NetworkManager.Singleton.ServerTime.Time >= pcControlsShowTime + pcControlsShowDuration)
            {
                pcControlsPopup.gameObject.SetActive(false);
            }
        }
    }
}