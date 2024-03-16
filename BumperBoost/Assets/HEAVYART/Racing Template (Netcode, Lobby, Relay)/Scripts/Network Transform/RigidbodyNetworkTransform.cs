using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;
namespace HEAVYART.Racing.Netcode
{
    public class RigidbodyNetworkTransform : NetworkBehaviour
    {
        [Header("Network synchronization")]
        //Prediction multiplier, based on previous and current positions
        public float positionExtrapolationFactor = 1.2f;
        public int positionSmoothingFrames = 14;
        public int rotationSmoothingFrames = 10;

        public float teleportDistance = 10;

        [Header("Physics overrides")]
        //Analogue of default angular drag, but instead of drag we have fixed frames amount to fade down torque to zero.
        public int torqueFadeFrames = 6;

        [Space()]
        //Overrides default Rigidbody values for particular physics simulation
        public float drag = 0;
        public float angularDrag = 0;

        //Overrides stiffness parameters for particular physics simulation
        [Space()]
        public float friction = 0.5f;
        public PhysicMaterialCombine frictionCombine;


        [Space()]
        public float bounciness = 0;
        public PhysicMaterialCombine bouncinessCombine;

        //Variables for full synchronization
        private NetworkVariable<Vector3> fullPosition = new NetworkVariable<Vector3>(writePerm: NetworkVariableWritePermission.Owner);
        private NetworkVariable<Quaternion> fullRotation = new NetworkVariable<Quaternion>(writePerm: NetworkVariableWritePermission.Owner);
        private NetworkVariable<float> synchronizedVelocity = new NetworkVariable<float>(writePerm: NetworkVariableWritePermission.Owner);

        private Rigidbody rigidbodyComponent;

        private Vector3 previousPosition;
        private float lastUpdateTime;

        private Vector3 calculatedVelocity;

        //SmoothDamp temp variables
        private Vector3 positionSmoothVelocity;
        private Vector3 impulseFadeVelocity;
        private Vector3 rotationFadeVelocity;


        public override void OnNetworkSpawn()
        {
            rigidbodyComponent = GetComponent<Rigidbody>();

            //Synchronize position on spawn
            if (IsOwner)
                fullPosition.Value = transform.localPosition;

            //Override physics parameters
            if (GetComponent<CharacterIdentityControl>().IsOwner == false)
            {
                rigidbodyComponent.drag = drag;
                rigidbodyComponent.angularDrag = angularDrag;

                Collider[] colliders = gameObject.GetComponents<Collider>();

                //Override friction parameters
                for (int i = 0; i < colliders.Length; i++)
                {
                    colliders[i].material.dynamicFriction = friction;
                    colliders[i].material.staticFriction = friction;
                    colliders[i].material.bounciness = bounciness;

                    colliders[i].material.frictionCombine = frictionCombine;
                    colliders[i].material.bounceCombine = bouncinessCombine;
                }
            }

            //Local position automatically transforms to global, it there is no parent object. Same with local rotation.
            transform.localPosition = fullPosition.Value;
            previousPosition = transform.localPosition;
            lastUpdateTime = Time.time;
        }

        void FixedUpdate()
        {
            if (IsOwner) //Write 
            {
                fullPosition.Value = transform.localPosition;
                fullRotation.Value = transform.localRotation;

                synchronizedVelocity.Value = rigidbodyComponent.velocity.magnitude;
            }
            else //Read
            {
                Vector3 position = fullPosition.Value;
                Quaternion rotation = fullRotation.Value;

                //Teleport if we are too far from required position
                if ((transform.localPosition - position).magnitude > teleportDistance)
                {
                    transform.localPosition = position;
                }

                //Calculate velocity
                if (previousPosition != position)
                {
                    calculatedVelocity = (position - previousPosition).normalized * synchronizedVelocity.Value;
                    lastUpdateTime = Time.time;
                    previousPosition = position;
                }

                if (synchronizedVelocity.Value < 0.001f)
                {
                    calculatedVelocity = Vector3.zero;
                }

                float timeDelta = Time.time - lastUpdateTime;

                //Calculate current position, based on last received position, velocity and last update time
                Vector3 calculatedPosition = previousPosition + calculatedVelocity * timeDelta;

                //Calculate prediction offset. Value could be higher but it will be smoothed below.
                Vector3 calculatedExtrapolationOffset = calculatedVelocity * Mathf.Max(positionExtrapolationFactor - 1, 0);

                //Calculate rough predicted position
                Vector3 predictedPosition = calculatedPosition + calculatedExtrapolationOffset;

                //Smooth predicted position
                Vector3 smoothedPosition = Vector3.SmoothDamp(transform.localPosition, predictedPosition, ref positionSmoothVelocity, positionSmoothingFrames * Time.fixedDeltaTime);

                //Calculate required rigidBody velocity to "jump" to the smoothedPosition in one frame
                Vector3 calculatedTargetVelocity = (smoothedPosition - transform.localPosition) * (1f / Time.fixedDeltaTime);

                //Calculate difference between current velocity and required velocity. Add only the difference. Otherwise we would "jump" too far.
                Vector3 deltaVelocity = (calculatedTargetVelocity - rigidbodyComponent.velocity);

                //"Jump" to target position.
                //It's just like (transform.position = smoothedPosition) but with one difference:
                //Changing transform.position simply teleports game object, when rigidbody.AddForce moves it along deltaVelocity with all the collisions
                rigidbodyComponent.AddForce(deltaVelocity, ForceMode.VelocityChange);

                //Analogue of default angular drag, but instead of drag we have fixed frames amount to fade down torque to zero.
                Vector3 angularVelocityFade = Vector3.SmoothDamp(rigidbodyComponent.angularVelocity, Vector3.zero, ref impulseFadeVelocity, torqueFadeFrames * Time.fixedDeltaTime);
                rigidbodyComponent.angularVelocity = angularVelocityFade;

                //Apply smoothed rotation
                rigidbodyComponent.rotation = QuaternionSmoothDamp(rigidbodyComponent.rotation, rotation, ref rotationFadeVelocity, rotationSmoothingFrames * Time.fixedDeltaTime);
            }
        }

        private Quaternion QuaternionSmoothDamp(Quaternion current, Quaternion target, ref Vector3 currentVelocity, float smoothTime)
        {
            return Quaternion.Euler(
              Mathf.SmoothDampAngle(current.eulerAngles.x, target.eulerAngles.x, ref currentVelocity.x, smoothTime),
              Mathf.SmoothDampAngle(current.eulerAngles.y, target.eulerAngles.y, ref currentVelocity.y, smoothTime),
              Mathf.SmoothDampAngle(current.eulerAngles.z, target.eulerAngles.z, ref currentVelocity.z, smoothTime)
            );
        }
    }
}