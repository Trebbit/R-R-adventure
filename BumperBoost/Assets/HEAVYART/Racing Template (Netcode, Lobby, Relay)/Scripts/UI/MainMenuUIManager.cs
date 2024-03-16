using System.Collections;
using System.Collections.Generic;
using Unity.Services.Authentication;
using UnityEngine;
using UnityEngine.UI;

namespace HEAVYART.Racing.Netcode
{
    public class MainMenuUIManager : Singleton<MainMenuUIManager>
    {
        public MainMenuCarSelectionController carSelectionController;

        [Space]
        public RectTransform mainGamePanel;
        public RectTransform startRaceMenuPopup;
        public RectTransform joinGamePopup;
        public RectTransform startGamePopup;
        public RectTransform errorPopup;

        [Space]
        public RectTransform waitForPublicGamePopup;
        public RectTransform waitForPrivateGamePopup;

        [Space]
        public InputField nameField;
        public Dropdown selectedServerDropdown;
        public List<Button> colorButtons;

        public string nameInput
        {
            get => PlayerDataKeeper.name;
            set => PlayerDataKeeper.name = value;
        }

        private IEnumerator Start()
        {
            nameField.text = PlayerDataKeeper.name;

            //Wait for available regions to update
            yield return new WaitUntil(() => LobbyManager.Instance.availableRegions.Count > 0);

            InitializeRegions();

            yield return new WaitUntil(() => AuthenticationService.Instance.IsSignedIn == true);

            InitializeColors();
            ShowMainGamePanel();

            LobbyManager.Instance.OnErrorOccurred += OnErrorOccurred;
        }

        private void OnDestroy()
        {
            if (LobbyManager.Instance != null)
                LobbyManager.Instance.OnErrorOccurred -= OnErrorOccurred;
        }

        private void InitializeColors()
        {
            //Set saved color as currently selected
            PickUpColor(PlayerDataKeeper.selectedColor);

            for (int i = 0; i < colorButtons.Count; i++)
            {
                colorButtons[i].GetComponent<Image>().color = SettingsManager.Instance.player.availableColors[i];

                //Avoiding lambda closure
                //https://en.wikipedia.org/wiki/Closure_(computer_programming)
                int buttonIndex = i;

                //Set color buttons callbacks
                colorButtons[i].onClick.AddListener(() =>
                {
                    PickUpColor(buttonIndex);
                });
            }
        }

        private void InitializeRegions()
        {
            //Convert regions list to dropdown menu elements
            selectedServerDropdown.options = LobbyManager.Instance.availableRegions.ConvertAll(x => new Dropdown.OptionData(x));

            //Set selected region
            selectedServerDropdown.value = ConvertSelectedRegionNameToIndex(LobbyManager.Instance.availableRegions);

            //Set region change callback
            selectedServerDropdown.onValueChanged.AddListener((updatedValue) =>
            {
                //Save changes to cache
                PlayerDataKeeper.selectedRegion = LobbyManager.Instance.availableRegions[updatedValue];
            });
        }

        private void PickUpColor(int index)
        {
            //Turn off all selection markers
            for (int i = 0; i < colorButtons.Count; i++)
                colorButtons[i].transform.GetChild(0).gameObject.SetActive(false);

            //Turn on selection marker on selected color button
            colorButtons[index].transform.GetChild(0).gameObject.SetActive(true);

            //Save changes to cache
            PlayerDataKeeper.selectedColor = index;

            carSelectionController.UpdateCarColor();
        }

        private int ConvertSelectedRegionNameToIndex(List<string> regions)
        {
            string currentlySelectedRegion = PlayerDataKeeper.selectedRegion;

            //Get region index (for dropdown menu)
            for (int i = 0; i < regions.Count; i++)
            {
                if (regions[i] == currentlySelectedRegion)
                {
                    return i;
                }
            }

            return 0;
        }

        public void ShowMainGamePanel()
        {
            HideAll();
            mainGamePanel.gameObject.SetActive(true);
        }

        public void ShowStartRaceMenuPopup()
        {
            HideAll();
            startRaceMenuPopup.gameObject.SetActive(true);
        }

        public void ShowWaitingForPublicGamePopup()
        {
            HideAll();
            waitForPublicGamePopup.gameObject.SetActive(true);
        }

        public void ShowWaitingForPrivateGamePopup()
        {
            HideAll();
            waitForPrivateGamePopup.gameObject.SetActive(true);
        }

        public void ShowJoinGamePopup()
        {
            HideAll();
            joinGamePopup.gameObject.SetActive(true);
        }

        public void ShowStartGamePopup()
        {
            HideAll();
            startGamePopup.gameObject.SetActive(true);
        }

        private void HideAll()
        {
            mainGamePanel.gameObject.SetActive(false);
            startRaceMenuPopup.gameObject.SetActive(false);
            joinGamePopup.gameObject.SetActive(false);
            startGamePopup.gameObject.SetActive(false);
            errorPopup.gameObject.SetActive(false);
            waitForPublicGamePopup.gameObject.SetActive(false);
            waitForPrivateGamePopup.gameObject.SetActive(false);
        }

        private void OnErrorOccurred(string errorText)
        {
            HideAll();
            errorPopup.gameObject.SetActive(true);

            Debug.Log(errorText);
        }
    }
}