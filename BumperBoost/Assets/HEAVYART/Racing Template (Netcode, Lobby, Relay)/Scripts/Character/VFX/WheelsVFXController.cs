using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace HEAVYART.Racing.Netcode
{
    public class WheelsVFXController : NetworkBehaviour
    {
        public List<WheelCollider> wheels = new List<WheelCollider>();

        private List<WheelMeshController> wheelMeshControllers = new List<WheelMeshController>();
        private List<TireSkidMarksController> tireSkidMarksControllers = new List<TireSkidMarksController>();
        private List<TireSmokeController> tireSmokeControllers = new List<TireSmokeController>();

        private NetworkVariable<bool> synchronizedWheelSmokeStatus = new NetworkVariable<bool>(false, writePerm: NetworkVariableWritePermission.Owner);
        private NetworkVariable<bool> synchronizedWheelMarksStatus = new NetworkVariable<bool>(false, writePerm: NetworkVariableWritePermission.Owner);

        private CarPhysicsController carPhysicsController;
        private CharacterIdentityControl identityControl;

        private Vector3 previousPosition;

        void Start()
        {
            identityControl = GetComponent<CharacterIdentityControl>();
            carPhysicsController = GetComponent<CarPhysicsController>();

            for (int i = 0; i < wheels.Count; i++)
            {
                wheelMeshControllers.Add(wheels[i].GetComponentInChildren<WheelMeshController>());
                tireSkidMarksControllers.Add(wheels[i].GetComponentInChildren<TireSkidMarksController>());
                tireSmokeControllers.Add(wheels[i].GetComponentInChildren<TireSmokeController>());
            }

            previousPosition = transform.position;
        }

        void FixedUpdate()
        {
            if (identityControl.IsOwner == true)
            {
                //Update local car wheel effects
                for (int i = 0; i < wheels.Count; i++)
                {
                    //Update wheel rotation
                    wheelMeshControllers[i].UpdateLocalCarWheel(wheels[i]);

                    //Update skid marks
                    tireSkidMarksControllers[i].UpdateLocalCarSkidMarks(wheels[i], carPhysicsController.currentSpeed);

                    //Update smoke
                    tireSmokeControllers[i].UpdateSmokeVFX(wheels[i], carPhysicsController.currentSpeed);
                }

                SynchronizeLocalCarWheelsVFX();
            }
            else
                HandleRemoteCarWheelsVisuals();
        }

        private void SynchronizeLocalCarWheelsVFX()
        {
            //Synchronize smoke and skid marks activity statuses
            //Local client side. We are the owner of this car.
            //Note: AI cars are owned by server(host).

            bool marksActive = false;
            bool smokeActive = false;

            //Take statuses
            for (int i = 0; i < tireSkidMarksControllers.Count; i++)
            {
                if (tireSkidMarksControllers[i].isActiveSkidmarks == true)
                    marksActive = true;

                if (tireSmokeControllers[i].isActiveSmoke == true)
                    smokeActive = true;
            }

            //Update statuses
            //Remote clients will use this statuses to synchronize smoke and skid marks activity
            synchronizedWheelMarksStatus.Value = marksActive;
            synchronizedWheelSmokeStatus.Value = smokeActive;
        }

        private void HandleRemoteCarWheelsVisuals()
        {
            //Remote car synchronization. 
            //We are NOT the owner of this car, but we see it and we synchronize it's wheels.

            Vector3 velocity = transform.position - previousPosition;

            for (int i = 0; i < wheels.Count; i++)
            {
                //Synchronize smoke VFX
                if (synchronizedWheelSmokeStatus.Value == true)
                    tireSmokeControllers[i].ShowSmokeParticles(1);
                else
                    tireSmokeControllers[i].HideSmokeParticles();

                //Synchronize skid marks
                if (synchronizedWheelMarksStatus.Value == true)
                {
                    tireSkidMarksControllers[i].UpdateRemoteCarSkidMarks(wheels[i], velocity);
                }
                else
                    tireSkidMarksControllers[i].CutSkidMarksTrail();
            }

            //Synchronize wheel rotation and position
            for (int i = 0; i < wheels.Count; i++)
            {
                bool isSteeringWheel = i < 2;
                wheelMeshControllers[i].UpdateRemoteCarWheel(wheels[i], velocity, isSteeringWheel);
            }

            previousPosition = transform.position;
        }
    }
}
