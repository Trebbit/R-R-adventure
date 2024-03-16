using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HEAVYART.Racing.Netcode
{
    public class HealthVFXIndicationController : MonoBehaviour
    {
        public ParticleSystem smokeParticleSystem;

        //Graph to setup smoke intensity according to car's health
        public AnimationCurve particleIntensity;

        [Space]
        public Transform deathEffectExplosionPrefab;

        private HealthController healthController;

        private ParticleSystem.EmissionModule emissionModule;

        void Start()
        {
            healthController = GetComponent<HealthController>();
            healthController.OnDeath += OnDeath;
            emissionModule = smokeParticleSystem.emission;
        }

        void FixedUpdate()
        {
            if (GameManager.Instance.gameState == GameState.WaitingForPlayers || GameManager.Instance.gameState == GameState.WaitingForCountdown)
            {
                emissionModule.rateOverTimeMultiplier = 0;
                return;
            }

            //Update smoke intensity
            emissionModule.rateOverTimeMultiplier = particleIntensity.Evaluate(healthController.currentHealth);
        }

        private void OnDeath()
        {
            //Show explosion
            Transform deathEffect = Instantiate(deathEffectExplosionPrefab, transform.position, Quaternion.identity, transform);
            Destroy(deathEffect.gameObject, 2);
        }
    }
}
