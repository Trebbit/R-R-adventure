using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HEAVYART.Racing.Netcode
{
    public class TireSkidMarksController : MonoBehaviour
    {
        public bool enableSkidmarks = true;
        public float trailMinForwardSlip = 0.6f;
        public float trailMinSidewaysSlip = 0.3f;
        public float maxSpeedToShowVFX = 40;

        [Space]
        public float skidmarksOffsetAboveGround = 0.01f;
        public int maxPointsInTrail = 80;
        public float minDistanceBetweenPoints = 0.5f;
        public float trailLifetime = 15;
        public Transform trailPrefab;

        public List<string> groundTags = new List<string>() { "Ground" };

        //Activity statuses for WheelsVFXSynchronizer
        public bool isActiveSkidmarks { get; private set; }

        private LineRenderer lineRenderer;
        private Vector3[] points = new Vector3[0];
        private Vector3 lastPoint;

        private bool isTrailExsists = false;
        private Transform rootTransform;

        void Start()
        {
            //Car transform
            rootTransform = transform.root;
        }

        public void UpdateLocalCarSkidMarks(WheelCollider wheel, float carSpeed)
        {
            if (wheel.GetGroundHit(out WheelHit wheelHit))
            {
                bool isAccelerationTrailAllowed = Mathf.Abs(wheelHit.forwardSlip) > trailMinForwardSlip && carSpeed < maxSpeedToShowVFX && wheel.motorTorque > 0.01f;
                bool isSlipTrailAllowed = Mathf.Abs(wheelHit.sidewaysSlip) > trailMinSidewaysSlip;
                bool isBrakeTrailAllowed = wheel.brakeTorque > 0;

                bool isSkidMarksSlipConditionsAccepted = isAccelerationTrailAllowed || isSlipTrailAllowed || isBrakeTrailAllowed;

                if (isSkidMarksSlipConditionsAccepted == true && CheckWheelHitTag(wheelHit.collider.tag))
                {
                    //Show skid marks
                    if (enableSkidmarks == true)
                    {
                        Vector3 wheelMarkPoint =
                            wheelHit.point //Wheel hit (calculated on previous frame)
                            + (wheelHit.normal * skidmarksOffsetAboveGround) //Offset from ground
                            + (wheel.attachedRigidbody.velocity * Time.fixedDeltaTime * 2f); //Predict current position below wheel

                        AddSkidMarksPoint(wheelMarkPoint);

                        //Update activity status
                        isActiveSkidmarks = true;
                    }
                }
                else
                    //Cut skid marks trail
                    CutSkidMarksTrail();
            }
            else
            {
                //Hide everything
                CutSkidMarksTrail();
            }
        }

        public void UpdateRemoteCarSkidMarks(WheelCollider wheel, Vector3 velocity)
        {
            if (Physics.Raycast(transform.position, -rootTransform.up, out RaycastHit hit))
            {
                if (CheckWheelHitTag(hit.transform.tag) == true)
                {
                    Vector3 wheelMarkPoint =
                      hit.point //Wheel hit (calculated on previous frame)
                      + (hit.normal * skidmarksOffsetAboveGround) //Offset from ground
                      + (velocity * Time.fixedDeltaTime * 2f); //Predict current position below wheel

                    AddSkidMarksPoint(wheelMarkPoint);
                }
                else
                    CutSkidMarksTrail();
            }
        }

        private bool CheckWheelHitTag(string tag)
        {
            //Check if wheel hits the ground
            for (int i = 0; i < groundTags.Count; i++)
            {
                if (groundTags[i] == tag)
                {
                    return true;
                }
            }
            return false;
        }

        public void AddSkidMarksPoint(Vector3 position)
        {
            //Start new trail if it doesn't exist
            if (isTrailExsists == false)
            {
                points = new Vector3[maxPointsInTrail];

                for (int i = 0; i < points.Length; i++)
                    points[i] = position;

                lastPoint = position;

                //Instantiate new trail
                Transform trail = Instantiate(trailPrefab, Vector3.zero, Quaternion.Euler(90, 0, 0), GameManager.Instance.temp.transform);
                lineRenderer = trail.GetComponent<LineRenderer>();
                lineRenderer.positionCount = maxPointsInTrail;

                isTrailExsists = true;
            }

            //Add point to trail
            if ((position - lastPoint).magnitude > minDistanceBetweenPoints)
            {
                //Shift trail points. Move [0] to [1], [1] to [2], [2] to [3] etc.
                //Eventually we have free element [0] for new point
                //Element [0] is the closest point to a wheel

                for (int i = points.Length - 1; i > 0; i--)
                    points[i] = points[i - 1];

                //Set new point coordinate
                points[0] = position;
                lastPoint = position;
            }
            else
            {
                //Wheel isn't far enough to shift trail points, so we update only nearest one
                points[0] = position;
            }

            lineRenderer.SetPositions(points);
        }

        public void CutSkidMarksTrail()
        {
            //Cut existing trail and prepare everything for a new trail

            isTrailExsists = false;

            if (lineRenderer != null)
                //Fade trail
                StartCoroutine(FadeSkidMars(lineRenderer));

            lineRenderer = null;

            //Update activity status
            isActiveSkidmarks = false;
        }

        private IEnumerator FadeSkidMars(LineRenderer lineRenderer)
        {
            Color color = lineRenderer.material.color;
            float alpha = color.a;

            yield return new WaitForSeconds(trailLifetime);

            float fadeSpeedMultiplier = 1;
            while (alpha > 0)
            {
                yield return new WaitForFixedUpdate();
                alpha = Mathf.MoveTowards(alpha, 0, Time.fixedDeltaTime * fadeSpeedMultiplier);
                color.a = alpha;

                lineRenderer.material.color = color;
            }

            //Trail became invisible. Destroy it.
            Destroy(lineRenderer.gameObject);
        }
    }
}
