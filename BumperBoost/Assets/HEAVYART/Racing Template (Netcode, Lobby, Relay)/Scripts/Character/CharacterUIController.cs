using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace HEAVYART.Racing.Netcode
{
    public class CharacterUIController : NetworkBehaviour
    {
        public Transform statusBarPrefab;
        private StatusBarUIController statusBarUIController;
        private HealthController healthController;
        private NitroController nitroController;
        private CharacterIdentityControl identityControl;
        private RaceStatusController carRaceStatusController;
        private CarPhysicsController carPhysicsController;
        private WeaponControlSystem weaponControlSystem;

        private void Awake()
        {
            healthController = GetComponent<HealthController>();
            nitroController = GetComponent<NitroController>();
            identityControl = GetComponent<CharacterIdentityControl>();
            carRaceStatusController = GetComponent<RaceStatusController>();
            carPhysicsController = GetComponent<CarPhysicsController>();
            weaponControlSystem = GetComponent<WeaponControlSystem>();
        }

        public override void OnNetworkSpawn()
        {
            //Instantiate status bar, if it's not our player
            if (identityControl.IsLocalPlayer == false)
            {
                Transform statusBar = Instantiate(statusBarPrefab, GameManager.Instance.temp.transform);
                statusBarUIController = statusBar.GetComponent<StatusBarUIController>();

                if (identityControl.isPlayer == true)
                    statusBarUIController.ShowUserName(identityControl.spawnParameters.Value.name);
            }
        }

        new private void OnDestroy()
        {
            DestroyStatusBar();
        }

        private void DestroyStatusBar()
        {
            if (statusBarUIController != null)
                Destroy(statusBarUIController.gameObject);
        }

        void FixedUpdate()
        {
            if (IsSpawned == false) return;

            //Use HUD
            if (identityControl.IsLocalPlayer == true)
            {
                GameManager.Instance.UI.ShowHUD();
                GameManager.Instance.UI.hudStatusBar.UpdateHealthAmount(healthController.currentHealth, healthController.maxHealth);
                GameManager.Instance.UI.hudStatusBar.UpdateNitroAmount(nitroController.currentNitro, nitroController.maxNitro);
                GameManager.Instance.UI.hudStatusBar.UpdateRaceStatus(carRaceStatusController.currentLap, GameManager.Instance.lapsCount, carRaceStatusController.CalculatePlace());
                GameManager.Instance.UI.hudStatusBar.UpdateSpeed(carPhysicsController.currentSpeed);
                GameManager.Instance.UI.hudStatusBar.UpdateWeaponIndicators(weaponControlSystem.weapons);
            }
            //Use status bar (above head)
            else
            {
                //Update status bar (every frame)
                statusBarUIController.UpdateHealthAmount(healthController.currentHealth, healthController.maxHealth);
                statusBarUIController.UpdateNitroAmount(nitroController.currentNitro, nitroController.maxNitro);
                statusBarUIController.UpdateWeaponIndicators(weaponControlSystem.weapons);
                statusBarUIController.UpdateStatusBarPosition(transform.position);
            }
        }
    }
}