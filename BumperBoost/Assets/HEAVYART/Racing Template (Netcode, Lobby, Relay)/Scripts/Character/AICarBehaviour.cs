using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace HEAVYART.Racing.Netcode
{
    public class AICarBehaviour : NetworkBehaviour
    {
        private float steeringFactor;

        private float angleToStopAccelerate;
        private float speedToStopAccelerate;

        private float angleForHandbrake;
        private float speedForHandbrake;

        private float angleToUseNitro;
        private float maxSpeedToUseNitro;

        private float minLifetimeToUseNitro;
        private float minLifetimeToUseMachinegun;

        private float stuckTimeToMoveBackwards;
        private float moveBackwardsTime;
        private float speedToConsiderAsStuck;

        private float maxTimeToReachWayPoint;
        private float speedLimitToRespawn;

        private float minDistanceToChangePathPoint;

        private float distanceToFireMachinegun;
        private float distanceToFireRocket;
        private float distanceToPlantMine;

        private string targetTag;

        private CarPhysicsController carPhysicsController;
        private CharacterIdentityControl identityControl;
        private RaceStatusController carRaceStatusController;
        private HealthController healthController;
        private NitroController nitroController;
        private WeaponControlSystem weaponControlSystem;

        private int lastPathPointGroupIndex;
        private Vector3 targetWayPoint;

        private MovementState movementState;

        private float currentStuckTime;
        private float currentMoveBackwardsTime;
        private float timeFromLastWayPointUpdate;
        private double spawnTime;

        private Weapon machinegunWeapon;
        private Weapon rocketWeapon;
        private Weapon mineWeapon;

        void Start()
        {
            //Components
            carPhysicsController = GetComponent<CarPhysicsController>();
            identityControl = GetComponent<CharacterIdentityControl>();
            carRaceStatusController = GetComponent<RaceStatusController>();
            healthController = GetComponent<HealthController>();
            nitroController = GetComponent<NitroController>();
            weaponControlSystem = GetComponent<WeaponControlSystem>();

            //Weapons
            machinegunWeapon = weaponControlSystem.GetWeapon(WeaponType.MachineGun);
            rocketWeapon = weaponControlSystem.GetWeapon(WeaponType.RocketLauncher);
            mineWeapon = weaponControlSystem.GetWeapon(WeaponType.MineLauncher);

            GameManager.Instance.userControl.AddAIObject(NetworkObject);

            //Navigation
            AIWayPointGroup nextPathPointGroup = GameManager.Instance.aiPathControl.GetNextWayPointGroup(0);
            targetWayPoint = nextPathPointGroup.GetRandomWayPoint();

            //Get config
            int modelIndex = identityControl.spawnParameters.Value.modelIndex;
            AIConfig config = SettingsManager.Instance.ai.configs[modelIndex];

            //Apply behaviour settings
            ApplySettings(config);

            //Health
            healthController.Initialize(config.health);
            healthController.OnDeath += () =>
            {
                if (IsOwner == true)
                {
                    StartCoroutine(Respawn(config.respawnDelay));
                }
            };

            //Nitro
            nitroController.Initialize(config.nitro, config.nitroTorqueMultiplier, config.nitroUsagePerSecond);

            GameManager.Instance.AddLeaderboardUser(identityControl);

            movementState = MovementState.MoveForward;

            spawnTime = GameManager.Instance.gameStartTime;

            gameObject.name = config.botPrefab.name;
        }

        void FixedUpdate()
        {
            if (identityControl.IsOwner == false) return;

            //Inputs
            float vertical = 0;
            float horizontal = 0;
            bool handbrake = false;

            GameState gameState = GameManager.Instance.gameState;
            bool isReadyToDrive = true;

            //Check if game is in active state
            if (gameState == GameState.WaitingForCountdown || gameState == GameState.GameIsOver)
            {
                carPhysicsController.UpdateInput(0, 0, true);
                isReadyToDrive = false;
            }
            //Check if car hasn't finished yet
            if (carRaceStatusController.isFinished == true)
            {
                carPhysicsController.UpdateInput(0, 0, true);
                isReadyToDrive = false;
            }

            //Check if car wasn't destroyed
            if (healthController.isAlive == false)
            {
                carPhysicsController.UpdateInput(0, 0, false);
                isReadyToDrive = false;
            }

            //Stop movement if car is not ready to drive
            if (isReadyToDrive == false)
            {
                timeFromLastWayPointUpdate = 0;
                currentStuckTime = 0;
                return;
            }

            switch (movementState)
            {
                case MovementState.MoveForward:
                    {
                        //Acceleration (gas pedal) input
                        vertical = 1;

                        Vector3 carForward = transform.forward;
                        carForward.y = 0; //Skip vertical axis

                        //Direction to next waypoint
                        Vector3 carTargetDirection = targetWayPoint - transform.position;
                        carTargetDirection.y = 0; //Skip vertical axis

                        //Angle for car to drive right to target waypoint
                        float angle = Vector3.SignedAngle(carForward, carTargetDirection, Vector3.up);

                        //Steering input
                        horizontal = (angle / 90) * steeringFactor;

                        //Handle accelerating conditions
                        if (carPhysicsController.currentSpeed > speedToStopAccelerate && Mathf.Abs(angle) > angleToStopAccelerate)
                            vertical = 0;

                        //Handle handbrake
                        if (carPhysicsController.currentSpeed > speedForHandbrake && Mathf.Abs(angle) > angleForHandbrake * 0.5f)
                            handbrake = true;

                        //Handle nitro conditions
                        bool useNitro = false;
                        if (angle < angleToUseNitro && carPhysicsController.currentSpeed < maxSpeedToUseNitro && carPhysicsController.isInAir == false)
                            if (NetworkManager.Singleton.ServerTime.Time > spawnTime + minLifetimeToUseNitro)
                                useNitro = true;

                        //Turn on/off nitro
                        nitroController.UpdateInput(useNitro);

                        //Respawn if car can't reach waypoint in time (probably stuck)
                        if (timeFromLastWayPointUpdate > maxTimeToReachWayPoint && carPhysicsController.currentSpeed < speedLimitToRespawn)
                        {
                            //Respawn on last checkpoint (not waypoint) 
                            GetComponent<RespawnController>().Respawn(false);

                            //Update target waypoint
                            AIWayPointGroup nextPathPointGroup = GameManager.Instance.aiPathControl.GetNearestWayPointGroup(transform.position);

                            targetWayPoint = nextPathPointGroup.GetRandomWayPoint();
                            lastPathPointGroupIndex = nextPathPointGroup.index;

                            timeFromLastWayPointUpdate = 0;
                        }

                        //Move backwards if car hits a wall
                        if (currentStuckTime > stuckTimeToMoveBackwards)
                        {
                            movementState = MovementState.MoveBackwards;
                        }

                        //Update target waypoint
                        if ((transform.position - targetWayPoint).magnitude < minDistanceToChangePathPoint)
                        {
                            //Find next group of waypoints
                            AIWayPointGroup nextPathPointGroup = GameManager.Instance.aiPathControl.GetNextWayPointGroup(lastPathPointGroupIndex);

                            //Pick random random waypoint from group
                            targetWayPoint = nextPathPointGroup.GetRandomWayPoint();
                            lastPathPointGroupIndex = nextPathPointGroup.index;

                            timeFromLastWayPointUpdate = 0;
                        }

                        //Handle weapons
                        HandleWeapons();

                        break;
                    }

                case MovementState.MoveBackwards:
                    {
                        //Move backwards
                        vertical = -1;
                        horizontal = 0;

                        //Return to moving forward
                        if (currentMoveBackwardsTime > moveBackwardsTime)
                        {
                            movementState = MovementState.MoveForward;
                            currentMoveBackwardsTime = 0;
                        }

                        currentMoveBackwardsTime += Time.fixedDeltaTime;

                        break;
                    }
            }

            //Update waypoint timer
            timeFromLastWayPointUpdate += Time.fixedDeltaTime;

            //Update "stuck" timer
            if (Mathf.Abs(carPhysicsController.relativeMovementSpeed.z) < speedToConsiderAsStuck && carPhysicsController.isInAir == false)
                currentStuckTime += Time.fixedDeltaTime;
            else
                currentStuckTime = 0;

            //Handle car physics
            carPhysicsController.UpdateInput(vertical, Mathf.Clamp(horizontal, -1, 1), handbrake);
        }

        private IEnumerator Respawn(float respawnDelay)
        {
            yield return new WaitForSeconds(respawnDelay);

            //Respawn and restore
            GetComponent<RespawnController>().Respawn(true);

            movementState = MovementState.MoveForward;

            //Reset timers
            timeFromLastWayPointUpdate = 0;
            currentStuckTime = 0;
            currentMoveBackwardsTime = 0;
        }

        private void HandleWeapons()
        {
            Vector3 startPoint = transform.position + transform.up;
            Vector3 forward = transform.forward;

            //Machinegun
            if (NetworkManager.Singleton.ServerTime.Time > spawnTime + minLifetimeToUseMachinegun)
                if (HandleWeaponTargetTracking(startPoint, forward, distanceToFireMachinegun, targetTag))
                    machinegunWeapon.Fire();

            //Missile
            if (rocketWeapon.ammo > 0)
            {
                if (HandleWeaponTargetTracking(startPoint, forward, distanceToFireRocket, targetTag))
                    rocketWeapon.Fire();
            }

            //Mine
            if (mineWeapon.ammo > 0)
            {
                if (HandleWeaponTargetTracking(startPoint, -forward, distanceToPlantMine, targetTag))
                    mineWeapon.Fire();
            }
        }

        private bool HandleWeaponTargetTracking(Vector3 startPoint, Vector3 direction, float distance, string targetTag)
        {
            if (Physics.Raycast(startPoint, direction, out RaycastHit hit, distance))
            {
                if (hit.transform.tag == targetTag)
                    return true;
            }

            return false;
        }

        private void ApplySettings(AIConfig config)
        {
            steeringFactor = config.steeringFactor;

            //Acceleration
            angleToStopAccelerate = config.angleToStopAccelerate;
            speedToStopAccelerate = config.speedToStopAccelerate;

            //Handbrake
            angleForHandbrake = config.angleForHandbrake;
            speedForHandbrake = config.speedForHandbrake;

            //Nitro
            angleToUseNitro = config.angleToUseNitro;
            maxSpeedToUseNitro = config.maxSpeedToUseNitro;

            //Niro & Machinegun usage on game start
            minLifetimeToUseNitro = config.minLifetimeToUseNitro;
            minLifetimeToUseMachinegun = config.minLifetimeToUseMachinegun;

            //Stuck
            stuckTimeToMoveBackwards = config.stuckTimeToMoveBackwards;
            moveBackwardsTime = config.moveBackwardsTime;
            speedToConsiderAsStuck = config.speedToConsiderAsStuck;

            //Navigation
            maxTimeToReachWayPoint = config.maxTimeToReachWayPoint;
            minDistanceToChangePathPoint = config.minDistanceToChangePathPoint;

            speedLimitToRespawn = config.speedLimitToRespawn;

            //Weapon
            distanceToFireMachinegun = config.distanceToFireMachinegun;
            distanceToFireRocket = config.distanceToFireRocket;
            distanceToPlantMine = config.distanceToPlantMine;

            targetTag = config.weaponTargetingTag;
        }
    }

    enum MovementState
    {
        MoveForward,
        MoveBackwards
    }
}
