using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HEAVYART.Racing.Netcode
{
    [System.Serializable]
    public class Axle
    {
        public string name;

        [Space]
        public WheelCollider leftWheel;
        public WheelCollider rightWheel;

        [Space]
        public bool isMotor;
        public bool isSteering;

        [Space]
        public float handBrakeForce;
        public float autoBrakeForce;

        public bool isInAir => leftWheel.isGrounded == false && rightWheel.isGrounded == false;

        public void ApplyAutoBrakes(float brakeInput)
        {
            leftWheel.brakeTorque = brakeInput * autoBrakeForce;
            rightWheel.brakeTorque = brakeInput * autoBrakeForce;
        }

        public void ApplyHandbrake()
        {
            leftWheel.brakeTorque = handBrakeForce;
            rightWheel.brakeTorque = handBrakeForce;
        }

        internal void ApplyTorque(float torque)
        {
            leftWheel.motorTorque = torque;
            rightWheel.motorTorque = torque;

            //Slow down wheels spinning in air. They're spinning way to fast.
            //Doesn't cause any effect on car's behavior
            float torqueFactorWhenCarIsInAir = 0.25f;
            if (leftWheel.isGrounded == false || rightWheel.isGrounded == false)
            {
                leftWheel.motorTorque = torque * torqueFactorWhenCarIsInAir;
                rightWheel.motorTorque = torque * torqueFactorWhenCarIsInAir;
            }
        }

        public void ApplySteering(float angle)
        {
            if (isSteering == false) return;

            leftWheel.steerAngle = angle;
            rightWheel.steerAngle = angle;
        }

        public void DisablePhysics()
        {
            //Disable wheel physics for cars we don't own
            //Requires for proper work of RigidbodyNetworkTransform

            leftWheel.enabled = false;
            rightWheel.enabled = false;
        }
    }
}