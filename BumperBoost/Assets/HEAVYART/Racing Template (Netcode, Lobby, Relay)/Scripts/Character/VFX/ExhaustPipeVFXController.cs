using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HEAVYART.Racing.Netcode
{
    public class ExhaustPipeVFXController : MonoBehaviour
    {
        public List<ParticleSystem> smokeParticleSystems;
        public List<ParticleSystem> nitroParticleSystems;

        public float idleSmokeParticlesEmission = 10;
        public float accelerationSmokeParticlesEmission = 50;

        private CarPhysicsController carPhysicsController;
        private NitroController nitroController;

        void Start()
        {
            carPhysicsController = GetComponent<CarPhysicsController>();
            nitroController = GetComponent<NitroController>();
        }

        void FixedUpdate()
        {
            //Smoke
            for (int i = 0; i < smokeParticleSystems.Count; i++)
            {
                ParticleSystem.EmissionModule emissionModule = smokeParticleSystems[i].emission;

                //Gas pedal is pressed
                if (carPhysicsController.isAccelerating == true)
                    emissionModule.rateOverTime = accelerationSmokeParticlesEmission;
                else
                    //Gas pedal is not pressed
                    emissionModule.rateOverTime = idleSmokeParticlesEmission;
            }

            //Nitro
            for (int i = 0; i < nitroParticleSystems.Count; i++)
            {
                //Nitro is activated
                if (nitroController.isNitroActivated == true)
                    nitroParticleSystems[i].gameObject.SetActive(true);
                else
                    //Nitro is not activated
                    nitroParticleSystems[i].gameObject.SetActive(false);
            }
        }
    }
}
