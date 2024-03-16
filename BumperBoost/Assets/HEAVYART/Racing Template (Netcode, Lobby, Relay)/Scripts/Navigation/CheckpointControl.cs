using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HEAVYART.Racing.Netcode
{
    public class CheckpointControl : MonoBehaviour
    {
        public CheckPoint[] checkpoints;
        private float fullRoundLength;

        private void Start()
        {
            for (int i = 0; i < checkpoints.Length; i++)
            {
                checkpoints[i].index = i;
            }

            //Calculate total distance between all checkpoints
            for (int i = 1; i < checkpoints.Length; i++)
            {
                fullRoundLength += (checkpoints[i - 1].transform.position - checkpoints[i].transform.position).magnitude;
            }

            //Add distance between last and first checkpoints
            fullRoundLength += (checkpoints[0].transform.position - checkpoints[checkpoints.Length - 1].transform.position).magnitude;
        }

        public float CalculateDistanceToFinish(int nextCheckpointIndex, int lapsLeft, Vector3 position)
        {
            //Prepare plane for further projection
            //https://docs.unity3d.com/ScriptReference/Plane.html
            Plane plane = new Plane(checkpoints[nextCheckpointIndex].transform.forward, checkpoints[nextCheckpointIndex].transform.position);

            //Get projected point
            //Short explanation of Plane: imagine Plane as a mirror, placed inside the checkpoint and faced in checkpoint's forward direction. 
            //ClosestPointOnPlane() would be the point on the mirror's surface, where we see our reflection.
            //We need this point to know how far we are from closest point on checkpoint's trigger (not from it's center)
            Vector3 projectedPosition = plane.ClosestPointOnPlane(position);

            if (nextCheckpointIndex == 0) lapsLeft--;

            //Further rounds distance left
            float summaryLengthOfFurtherRounds = lapsLeft * fullRoundLength;

            //Distance between further checkpoint's in current lap
            float currentRoundDistanceLeft = 0;
            for (int i = nextCheckpointIndex; i < checkpoints.Length; i++)
            {
                currentRoundDistanceLeft += (checkpoints[i].transform.position - GetNextCheckPoint(i).transform.position).magnitude;
            }

            //Distance from the car and next checkpoint
            float distanceToNearestCheckPoint = (projectedPosition - position).magnitude;

            //Distance left to the end of the race
            return summaryLengthOfFurtherRounds + currentRoundDistanceLeft + distanceToNearestCheckPoint;
        }

        public CheckPoint GetNextCheckPoint(int currentCheckPointIndex)
        {
            int nextCheckPointIndex = currentCheckPointIndex + 1;

            if (nextCheckPointIndex >= checkpoints.Length)
                nextCheckPointIndex = 0;

            return checkpoints[nextCheckPointIndex];
        }
    }
}
