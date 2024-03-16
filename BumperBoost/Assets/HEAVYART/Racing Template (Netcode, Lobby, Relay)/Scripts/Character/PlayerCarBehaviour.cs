using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace HEAVYART.Racing.Netcode
{
    public class PlayerCarBehaviour : NetworkBehaviour
    {
        private CarPhysicsController carPhysicsController;
        private CharacterIdentityControl identityControl;
        private RaceStatusController carRaceStatusController;
        private HealthController healthController;
        private NitroController nitroController;
        private CarInputControls inputActions;
        private WeaponControlSystem weaponControlSystem;

        private Vector3 movementVelocity = Vector3.zero;
        private Vector3 currentMovementInput;
        private float smoothMovementTime = 0.1f;

        private Weapon machinegunWeapon;
        private Weapon rocketWeapon;
        private Weapon mineWeapon;

        private void Awake()
        {
            //Register spawned player object
            GameManager.Instance.userControl.AddPlayerObject(NetworkObject);
        }

        void Start()
        {
            //Components
            carPhysicsController = GetComponent<CarPhysicsController>();
            identityControl = GetComponent<CharacterIdentityControl>();
            carRaceStatusController = GetComponent<RaceStatusController>();
            weaponControlSystem = GetComponent<WeaponControlSystem>();
            healthController = GetComponent<HealthController>();
            nitroController = GetComponent<NitroController>();

            GameManager.Instance.AddLeaderboardUser(identityControl);

            int modelIndex = identityControl.spawnParameters.Value.modelIndex;
            PlayerConfig config = SettingsManager.Instance.player.configs[modelIndex];

            //Health
            healthController.Initialize(config.health);
            healthController.OnDeath += () =>
            {
                //Show popup
                if (IsOwner == true) GameManager.Instance.UI.ShowCarDestroyedPopup();
            };

            nitroController.Initialize(config.nitro, config.nitroTorqueMultiplier, config.nitroUsagePerSecond);

            //Weapons
            machinegunWeapon = weaponControlSystem.GetWeapon(WeaponType.MachineGun);
            rocketWeapon = weaponControlSystem.GetWeapon(WeaponType.RocketLauncher);
            mineWeapon = weaponControlSystem.GetWeapon(WeaponType.MineLauncher);

            //Inputs
            inputActions = new CarInputControls();
            inputActions.Player.Move.Enable();
            inputActions.Player.Handbrake.Enable();
            inputActions.Player.Nitro.Enable();
            inputActions.Player.Machinegun.Enable();
            inputActions.Player.Rocket.Enable();
            inputActions.Player.Mine.Enable();
            inputActions.Player.Respawn.Enable();

            gameObject.name = "Player: " + identityControl.spawnParameters.Value.name;
        }

        void FixedUpdate()
        {
            //Check if game is in active state
            GameState gameState = GameManager.Instance.gameState;
            bool isActiveGame = gameState == GameState.ActiveGame || gameState == GameState.FinalCountdown;

            //Check if car hasn't finished yet
            if (carRaceStatusController.isFinished) isActiveGame = false;

            //Check if car wasn't destroyed
            if (healthController.isAlive == false) isActiveGame = false;

            if (identityControl.IsOwner == false)
                return;

            //Stop movement if car is not ready to drive
            if (isActiveGame == false)
            {
                carPhysicsController.UpdateInput(0, 0, true);
                return;
            }

            //Inputs
            Vector2 positionInput = inputActions.Player.Move.ReadValue<Vector2>();
            currentMovementInput = Vector3.SmoothDamp(currentMovementInput, positionInput, ref movementVelocity, smoothMovementTime);

            //Turn on/off nitro
            nitroController.UpdateInput(inputActions.Player.Nitro.inProgress);

            //Drive
            carPhysicsController.UpdateInput(Mathf.Clamp(currentMovementInput.y, -1, 1), currentMovementInput.x, inputActions.Player.Handbrake.inProgress);

            //Respawn
            if (inputActions.Player.Respawn.inProgress) GetComponent<RespawnController>().Respawn(false);

            //Use machinegun
            if (inputActions.Player.Machinegun.inProgress) machinegunWeapon.Fire();

            //Use rocket launcher
            if (inputActions.Player.Rocket.inProgress) rocketWeapon.Fire();

            //Use mine launcher
            if (inputActions.Player.Mine.inProgress) mineWeapon.Fire();
        }
    }
}