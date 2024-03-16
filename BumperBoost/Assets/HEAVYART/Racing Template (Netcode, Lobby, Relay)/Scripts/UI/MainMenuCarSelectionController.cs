using System.Collections;
using System.Collections.Generic;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;

namespace HEAVYART.Racing.Netcode
{
    public class MainMenuCarSelectionController : MonoBehaviour
    {
        private List<MainMenuPreviewCarController> carModels = new List<MainMenuPreviewCarController>();

        private int selectedCarIndex;

        IEnumerator Start()
        {
            yield return new WaitUntil(() => UnityServices.State == ServicesInitializationState.Initialized);
            yield return new WaitUntil(() => AuthenticationService.Instance.IsSignedIn == true);

            if (PlayerDataKeeper.selectedCarModel == -1)
                PlayerDataKeeper.selectedCarModel = 0;

            selectedCarIndex = PlayerDataKeeper.selectedCarModel;

            for (int i = 0; i < SettingsManager.Instance.player.configs.Count; i++)
            {
                GameObject model = Instantiate(SettingsManager.Instance.player.configs[i].previewModelPrefab, transform);
                carModels.Add(model.GetComponentInChildren<MainMenuPreviewCarController>());
            }

            ShowCar(selectedCarIndex);
        }

        private void ShowCar(int modelIndex)
        {
            //Turn off all car models
            for (int i = 0; i < carModels.Count; i++)
                carModels[i].gameObject.SetActive(false);

            //Turn on selected one
            carModels[modelIndex].gameObject.SetActive(true);
            UpdateCarColor();
        }

        public void ShowNextCar()
        {
            selectedCarIndex++;

            if (selectedCarIndex >= carModels.Count)
                selectedCarIndex = 0;

            PlayerDataKeeper.selectedCarModel = selectedCarIndex;

            ShowCar(selectedCarIndex);
        }

        public void ShowPreviousCar()
        {
            selectedCarIndex--;

            if (selectedCarIndex < 0)
                selectedCarIndex = carModels.Count - 1;

            PlayerDataKeeper.selectedCarModel = selectedCarIndex;

            ShowCar(selectedCarIndex);
        }

        public void UpdateCarColor()
        {
            carModels[selectedCarIndex].UpdateColor();
        }
    }
}