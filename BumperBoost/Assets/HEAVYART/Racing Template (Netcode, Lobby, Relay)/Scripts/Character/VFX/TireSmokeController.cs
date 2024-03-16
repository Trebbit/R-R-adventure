using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HEAVYART.Racing.Netcode
{
    public class TireSmokeController : MonoBehaviour
    {
        [Header("Smoke")]
        public bool enableSmoke = true;
        public float smokeMinForwardSlip = 0.6f;
        public float smokeMinSidewaySlip = 0.3f;
        public float smokeEmissionRate = 800;
        public float maxSpeedToShowVFX = 40;
        public ParticleSystem smokeParticleSystem;

        public List<string> groundTags = new List<string>() { "Ground" };

        public bool isActiveSmoke { get; private set; }

        void Start()
        {
            ParticleSystem.EmissionModule emissionModule = smokeParticleSystem.emission;
            emissionModule.rateOverTimeMultiplier = 0;
        }

        public void UpdateSmokeVFX(WheelCollider wheel, float carSpeed)
        {
            if (wheel.GetGroundHit(out WheelHit wheelHit))
            {
                bool isAccelerationSmokeAllowed = Mathf.Abs(wheelHit.forwardSlip) > smokeMinForwardSlip && carSpeed < maxSpeedToShowVFX && wheel.motorTorque > 0.01f;
                bool isSlipSmokeAllowed = Mathf.Abs(wheelHit.sidewaysSlip) > smokeMinSidewaySlip;
                bool isBrakeSmokeAllowed = wheel.brakeTorque > 0;

                bool isSmokeSlipConditionsAccepted = isAccelerationSmokeAllowed || isSlipSmokeAllowed || isBrakeSmokeAllowed;

                if (isSmokeSlipConditionsAccepted == true && CheckWheelHitTag(wheelHit.collider.tag))
                {
                    //Show smoke
                    if (enableSmoke == true)
                    {
                        ShowSmokeParticles(Mathf.Abs(wheelHit.forwardSlip) + Mathf.Abs(wheelHit.sidewaysSlip));
                    }
                }
                else
                    //Hide smoke
                    HideSmokeParticles();
            }
            else
            {
                //Hide smoke
                HideSmokeParticles();
            }
        }

        public void ShowSmokeParticles(float emissionFactor)
        {
            ParticleSystem.EmissionModule emissionModule = smokeParticleSystem.emission;
            emissionModule.rateOverTimeMultiplier = smokeEmissionRate * emissionFactor;

            //Update activity status
            if (emissionFactor > 0.1)
                isActiveSmoke = true;
            else
                isActiveSmoke = false;
        }

        public void HideSmokeParticles()
        {
            if (isActiveSmoke == true)
            {
                ParticleSystem.EmissionModule emissionModule = smokeParticleSystem.emission;
                emissionModule.rateOverTimeMultiplier = 0;
            }

            //Update activity status
            isActiveSmoke = false;
        }

        private bool CheckWheelHitTag(string tag)
        {
            //Check if wheel hits the ground
            for (int i = 0; i < groundTags.Count; i++)
            {
                if (groundTags[i] == tag)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
