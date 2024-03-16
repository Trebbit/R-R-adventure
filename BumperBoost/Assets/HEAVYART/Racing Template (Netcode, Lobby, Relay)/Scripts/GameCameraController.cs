
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.UIElements;

namespace HEAVYART.Racing.Netcode
{
    public class GameCameraController : MonoBehaviour
    {
        private float cameraDistance;
        private float cameraHeight;
        private float cameraHorizontalSpeed;

        private float cameraZoomFactor;
        private float nitroAdditionalZoom;

        //Graph to setup blur intensity according to car's speed
        private AnimationCurve blurIntensity;
        private float nitroAdditionalBlur;

        //Graph to setup camera shake intensity according to car's speed
        private AnimationCurve cameraShakeIntensity;
        private Vector3 cameraShakeMultiplier;
        private float nitroAdditionalShake;

        private float cameraShakeRoughness;

        private float defaultCameraFieldOfView;

        private Transform cameraPointTransform;
        private Camera cameraComponent;
        private Rigidbody playerRigidbodyComponent;
        private NitroController playerNitroComponent;

        private Vector3 smoothedCameraShakeOffset;
        private float smoothedAcceleration;
        private float velocitySmoothFactor = 0.25f;

        private PostProcessVolume postProcessVolume;

        private void Start()
        {
            StartCoroutine(LateFixedUpdate());
            cameraComponent = GetComponent<Camera>();
            defaultCameraFieldOfView = cameraComponent.fieldOfView;
            postProcessVolume = GetComponent<PostProcessVolume>();

            //Apply settings
            CameraSettings cameraSettings = SettingsManager.Instance.camera;

            cameraDistance = cameraSettings.cameraDistance;
            cameraHeight = cameraSettings.cameraHeight;
            cameraHorizontalSpeed = cameraSettings.cameraHorizontalSpeed;

            cameraZoomFactor = cameraSettings.cameraZoomFactor;
            nitroAdditionalZoom = cameraSettings.nitroAdditionalZoom;

            blurIntensity = cameraSettings.blurIntensity;
            nitroAdditionalBlur = cameraSettings.nitroAdditionalBlur;

            cameraShakeIntensity = cameraSettings.cameraShakeIntensity;
            cameraShakeMultiplier = cameraSettings.cameraShakeMultiplier;
            nitroAdditionalShake = cameraSettings.nitroAdditionalShake;

            cameraShakeRoughness = cameraSettings.cameraShakeRoughness;
        }

        void FixedUpdate()
        {
            NetworkObject localPlayer = GameManager.Instance.userControl.localPlayer;

            if (localPlayer != null)
            {
                if (cameraPointTransform == null)
                {
                    //Linking to car's components. Works once.

                    cameraPointTransform = GameManager.Instance.userControl.localPlayer.transform.Find("CameraPointTransform");
                    playerRigidbodyComponent = GameManager.Instance.userControl.localPlayer.GetComponent<Rigidbody>();
                    playerNitroComponent = playerRigidbodyComponent.GetComponent<NitroController>();
                }
            }
        }

        private IEnumerator LateFixedUpdate()
        {
            while (true)
            {
                yield return new WaitForFixedUpdate();

                if (cameraPointTransform == null) continue;

                //Calculate delta angle
                //Looks a little bit difficult, because we use LerpAngle to avoid 360, 180 and all other angle-related issues
                float horizontalOffsetAngle = Mathf.LerpAngle(transform.eulerAngles.y, cameraPointTransform.eulerAngles.y, cameraHorizontalSpeed * Time.fixedDeltaTime);

                //Convert angle to offset
                //Note: Quaternion * Vector3 = Vector3. It returns vector in direction of the Quaternion.
                //Example: transform.rotation * Vector3.forward = transform.forward
                Vector3 horizontalOffsetVector = Quaternion.Euler(0, horizontalOffsetAngle, 0) * Vector3.forward * cameraDistance;

                //Smoothed car velocity
                smoothedAcceleration = Mathf.Lerp(smoothedAcceleration, playerRigidbodyComponent.velocity.magnitude, velocitySmoothFactor);
                float currentSpeed = (smoothedAcceleration / 1000f) * 3600;

                //Nitro additions
                float additionalZoom = 0;
                float additionalBlur = 0;
                float additionalShake = 0;

                if (playerNitroComponent.isNitroActivated == true)
                {
                    additionalZoom = nitroAdditionalZoom;
                    additionalBlur = nitroAdditionalBlur;
                    additionalShake = nitroAdditionalShake;
                }

                //Shake offset
                smoothedCameraShakeOffset = Vector3.Lerp(smoothedCameraShakeOffset, new Vector3(
                    Random.Range(-1f, 1f) * cameraShakeMultiplier.x,
                    Random.Range(-1f, 1f) * cameraShakeMultiplier.y,
                    Random.Range(-1f, 1f) * cameraShakeMultiplier.z), cameraShakeRoughness);

                //Shake offset (local space)
                Vector3 relativeShakeOffset = transform.TransformDirection(smoothedCameraShakeOffset) * (cameraShakeIntensity.Evaluate(currentSpeed) + additionalShake) * Time.fixedDeltaTime;

                //Camera position
                Vector3 cameraPosition = cameraPointTransform.position + (Vector3.up * cameraHeight) - horizontalOffsetVector;
                transform.position = cameraPosition + relativeShakeOffset;

                //Camera rotation
                transform.rotation = Quaternion.LookRotation((cameraPointTransform.position - cameraPosition) + relativeShakeOffset, Vector3.up);

                //Field of view
                cameraComponent.fieldOfView = defaultCameraFieldOfView + smoothedAcceleration * cameraZoomFactor + additionalZoom;

                //Blur
                if (postProcessVolume.profile.TryGetSettings(out ChromaticAberration chromaticAberration))
                    chromaticAberration.intensity.value = blurIntensity.Evaluate(currentSpeed) + additionalBlur;
            }
        }
    }
}
