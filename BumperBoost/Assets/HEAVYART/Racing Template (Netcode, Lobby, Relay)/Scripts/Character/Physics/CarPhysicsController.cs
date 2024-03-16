using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace HEAVYART.Racing.Netcode
{
    public class CarPhysicsController : MonoBehaviour
    {
        public int motorPower = 3600;

        //Wheel pairs control
        //Handles wheels toque, brakes and steering
        public Axle[] axles;

        //Basic gears for further extension
        //Every gear has a speed range and torque multiplier to adjust acceleration
        //Gears switch automatically, with no delay
        //Check HandleGears() method for more details
        public Gear[] gears;

        public float gravity = 9.81f;

        [Space]
        //Graph to setup common steering behaviour according to car's speed
        public AnimationCurve steering;

        //Graph to setup additional steering during active handbrake (according to car's speed)
        public AnimationCurve handbrakeSteering;

        //World space speed (Km\h)
        public float currentSpeed { get; private set; }

        //Local space speed (Km\h)
        public Vector3 relativeMovementSpeed { get; private set; }

        //Car statuses
        public bool isAccelerating => Mathf.Abs(verticalInput) > 0.01f;

        public bool isInAir => axles[0].isInAir == true && axles[1].isInAir == true;

        //Gas input
        private float verticalInput;

        //Steering input
        private float horizontalInput;

        //Handbrakes activity (true/false)
        private bool handbrakeInput;

        private int currentGear;

        //Counter of active axles.
        //Allows to spread verticalInput between axles in 4X4 cars 
        private int motorAxlesCount = 0;

        private Rigidbody localRigidbody;
        private CharacterIdentityControl identityControl;
        private NitroController nitroController;

        void Start()
        {
            identityControl = GetComponent<CharacterIdentityControl>();
            nitroController = GetComponent<NitroController>();

            localRigidbody = GetComponent<Rigidbody>();
            localRigidbody.useGravity = false;

            //Initializing center of mass.
            //It's position changes car's behaviour.
            localRigidbody.centerOfMass = transform.Find("CenterOfMass").localPosition;

            for (int i = 0; i < axles.Length; i++)
                if (axles[i].isMotor) motorAxlesCount++;

            if (identityControl.IsOwner == false)
                for (int i = 0; i < axles.Length; i++)
                    axles[i].DisablePhysics();
        }

        public void UpdateInput(float vertical, float horizontal, bool handbrake)
        {
            if (identityControl.IsOwner == false) return;

            verticalInput = vertical;
            horizontalInput = horizontal;
            handbrakeInput = handbrake;
        }

        void FixedUpdate()
        {
            if (identityControl.IsOwner == false) return;

            //Basic gear management
            HandleGears();

            //Calculate world space speed
            currentSpeed = (localRigidbody.velocity.magnitude / 1000f) * 3600;

            //Calculate local space speed
            relativeMovementSpeed = transform.InverseTransformDirection(localRigidbody.velocity) * 3.6f;

            Vector3 movementDirection = localRigidbody.velocity;
            movementDirection.y = 0;

            //Auto-brakes
            //Activates when car starts to gas in the opposite direction
            //Helps to stop the car faster
            bool isOppositeDirections = Mathf.Sign(Vector3.Dot(transform.forward, movementDirection)) != Mathf.Sign(verticalInput) && movementDirection.magnitude > 0.25f;
            int autoBrakeInput = isOppositeDirections ? 1 : 0;

            //Handling car behaviour
            for (int i = 0; i < axles.Length; i++)
            {
                //Auto brakes
                //autoBrakeInput would equal 0 when it's inactive
                axles[i].ApplyAutoBrakes(autoBrakeInput);

                //Gas
                if (axles[i].isMotor && currentSpeed <= gears[currentGear].maxSpeed && autoBrakeInput == 0)
                {
                    //Calculate torque
                    float torque = motorPower * gears[currentGear].torqueFactor * verticalInput / motorAxlesCount;

                    //Add torque if nitro is active
                    if (nitroController.isNitroActivated == true)
                        torque *= nitroController.torqueMultiplier;

                    //Apply final torque to the axle
                    axles[i].ApplyTorque(torque);
                }
                else
                {
                    axles[i].ApplyTorque(0);
                }

                //Calculate steering angle
                float steeringAngle = horizontalInput * steering.Evaluate(currentSpeed);

                if (handbrakeInput == true)
                {
                    axles[i].ApplyHandbrake();

                    //Add steering angle when handbrake is active
                    if (Mathf.Abs(horizontalInput) > 0.01)
                        steeringAngle += Mathf.Sign(horizontalInput) * handbrakeSteering.Evaluate(currentSpeed);
                }

                //Apply final steering to the axle
                axles[i].ApplySteering(steeringAngle);
            }

            //Apply gravity
            localRigidbody.AddForce(new Vector3(0, -gravity * localRigidbody.mass, 0));
        }

        void HandleGears()
        {
            //Gear up
            if (currentSpeed >= gears[currentGear].maxSpeed)
            {
                if (currentGear < gears.Length - 1)
                    currentGear++;
            }
            //Gear down
            else
            {
                if (currentGear > 0)
                    if (currentSpeed < gears[currentGear].minSpeed) currentGear--;
            }
        }

        void OnCollisionEnter(Collision col)
        {
            Vector3 collisionForce = col.impulse * Time.fixedDeltaTime;

            //Here is the place to handle collision reactions.
        }
    }
}
