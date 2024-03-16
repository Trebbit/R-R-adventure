using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HEAVYART.Racing.Netcode
{
    public class CameraSettings : MonoBehaviour
    {
        public float cameraDistance = 4f;
        public float cameraHeight = 1.6f;
        public float cameraHorizontalSpeed = 2f;

        [Space]
        public float cameraZoomFactor = 0.7f;
        public float nitroAdditionalZoom = 0.1f;

        [Space]
        //Graph to setup blur intensity according to car's speed
        public AnimationCurve blurIntensity;
        public float nitroAdditionalBlur = 0.9f;

        [Space]
        //Graph to setup camera shake intensity according to car's speed
        public AnimationCurve cameraShakeIntensity;
        public Vector3 cameraShakeMultiplier = Vector3.one;
        public float nitroAdditionalShake = 1;

        [Space]
        [Range(0.5f, 1f)]
        public float cameraShakeRoughness = 1;
    }
}
