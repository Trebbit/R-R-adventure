using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HEAVYART.Racing.Netcode
{
    [Serializable]
    public class MeshColorAdjuster
    {
        public string title;
        public MeshRenderer meshRenderer;
        public int materialID;
        public float colorFactor = 0.5f;

        private Color originalColor;

        public void Initialize()
        {
            originalColor = meshRenderer.materials[materialID].color;
        }

        public void ApplyColor(Color targetColor)
        {
            meshRenderer.materials[materialID].color = Color.Lerp(originalColor, targetColor, colorFactor);
        }
    }
}
