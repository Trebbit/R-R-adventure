using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HEAVYART.Racing.Netcode
{
    public class MainMenuPreviewCarController : MonoBehaviour
    {
        public List<MeshColorAdjuster> meshColorAdjusters = new List<MeshColorAdjuster>();

        private void Awake()
        {
            for (int i = 0; i < meshColorAdjusters.Count; i++)
            {
                meshColorAdjusters[i].Initialize();
            }
        }

        public void UpdateColor()
        {
            for (int i = 0; i < meshColorAdjusters.Count; i++)
            {
                meshColorAdjusters[i].ApplyColor(SettingsManager.Instance.player.availableColors[PlayerDataKeeper.selectedColor]);
            }
        }
    }
}