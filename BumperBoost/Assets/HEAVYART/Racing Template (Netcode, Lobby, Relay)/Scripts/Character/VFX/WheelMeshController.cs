using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HEAVYART.Racing.Netcode
{
    public class WheelMeshController : MonoBehaviour
    {
        [Header("Wheel")]
        public Transform wheelMesh;
        public Vector3 wheelMeshOffset;
        public float wheelSteerSmoothness = 0.25f;

        [Header("Remote Car")]
        public float steeringAngleMultiplier = 1.5f;
        public float steeringAngleLimit = 35f;

        private float smoothedSteeringAngle;
        private Transform rootTransform;

        private float remoteCarWheelRotationAngle = 0;

        void Start()
        {
            //Car transform
            rootTransform = transform.root;
        }

        public void UpdateLocalCarWheel(WheelCollider collider)
        {
            collider.GetWorldPose(out Vector3 position, out Quaternion rotation);
            wheelMesh.position = position + transform.TransformDirection(wheelMeshOffset);
            wheelMesh.rotation = rotation;

            //Visual only. Value doesn't cause any effect on car's behaviour.
            float steeringAngleFactor = 0.5f;
            smoothedSteeringAngle = Mathf.LerpAngle(smoothedSteeringAngle, collider.steerAngle * steeringAngleFactor, wheelSteerSmoothness);

            wheelMesh.rotation = Quaternion.AngleAxis(smoothedSteeringAngle, rootTransform.up) * rotation;
        }

        public void UpdateRemoteCarWheel(WheelCollider wheel, Vector3 velocity, bool isSteeringWheel)
        {
            //Calculate wheel rotation
            float delta = velocity.magnitude / (Mathf.PI * wheel.radius * 2);
            delta *= Mathf.Sign(Vector3.Dot(transform.forward, velocity));

            remoteCarWheelRotationAngle += delta * 180f;
            remoteCarWheelRotationAngle = Mathf.LerpAngle(0, remoteCarWheelRotationAngle, 1);

            wheelMesh.localRotation = Quaternion.identity;

            //Front (steering) wheels only
            if (isSteeringWheel == true)
            {
                //Calculate steering angle
                float steeringAngle = Vector3.Dot(velocity, transform.right) * steeringAngleMultiplier * 360 * -1f;
                steeringAngle = Mathf.Clamp(steeringAngle, -steeringAngleLimit, steeringAngleLimit);

                //Apply steering angle
                wheelMesh.localRotation = Quaternion.Euler(0, steeringAngle, 0);
            }

            //Apply wheel rotation
            wheelMesh.localRotation *= Quaternion.Euler(remoteCarWheelRotationAngle, 0, 0);

            //Update wheel position
            if (Physics.Raycast(wheel.transform.position, -rootTransform.up, out RaycastHit hit))
            {
                float distance = Mathf.Clamp(hit.distance - wheel.radius, 0, wheel.suspensionDistance);
                wheelMesh.position = wheel.transform.position - (rootTransform.up * distance);
            }
        }
    }
}
