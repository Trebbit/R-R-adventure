using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

namespace HEAVYART.Racing.Netcode
{
    public class RaceStatusController : NetworkBehaviour
    {
        public int currentCheckPointIndex { get; private set; }
        public bool isFinished { get; private set; }
        public int currentLap { get; private set; }
        private int nextCheckPointIndex;
        private bool isFirstCheckPointPassed = false;
        private float lapStartTime;

        private int calculatedPlace;
        private CharacterIdentityControl characterIdentityControl;

        void Start()
        {
            characterIdentityControl = GetComponent<CharacterIdentityControl>();
            currentLap = -1;
        }

        private void FixedUpdate()
        {
            //This code works for all the cars

            //if (isFirstCheckPointPassed == true)
            //{
            //    //Calculate total distance to the end of the race
            //    float distanceToFinish = GameManager.Instance.checkPointControl.CalculateDistanceToFinish(nextCheckPointIndex, (GameManager.Instance.lapsCount - currentLap), transform.position);

            //    //Don't compete with finished players
            //    if (isFinished == true) distanceToFinish = -1;

            //    //Update race status (lap, check point, distance)
            //    GameManager.Instance.UpdateLeadearboard(NetworkObjectId, currentLap, currentCheckPointIndex, distanceToFinish);
            //}
        }

        public int CalculatePlace()
        {
            //Car is not participated in a race yet. It needs to cross the start line first. 
            //UI will show -- instead of lap and place values
            if (isFirstCheckPointPassed == false) return -1;

            //Don't recalculate place, when car is finished
            if (isFinished == true) return calculatedPlace;

            //Get leaderboard
            var sortedLeaderboard = GameManager.Instance.leaderboard.ToList();

            //Sort users by distance to finish
            sortedLeaderboard.Sort((oneUser, anotherUser) =>
            {
                return oneUser.Value.distanceToFinish.CompareTo(anotherUser.Value.distanceToFinish);
            });

            for (int i = 0; i < sortedLeaderboard.Count; i++)
            {
                //Find our car in the leaderboard
                if (sortedLeaderboard[i].Key == NetworkObjectId)
                {
                    //Return car's place in a race
                    calculatedPlace = i + 1;
                    return calculatedPlace;
                }
            }

            return -1;
        }

        private void OnTriggerEnter(Collider other)
        {
            ////Checkpoint hit
            //if (other.TryGetComponent(out CheckPoint checkPoint))
            //{
            //    //If we hit expected (next) checkpoint
            //    if (checkPoint.index == nextCheckPointIndex)
            //    {
            //        //If we hit start line (new lap)
            //        if (checkPoint.index == 0 && nextCheckPointIndex == 0)
            //        {
            //           // currentLap++;

            //            //Info logs
            //            if (characterIdentityControl.IsLocalPlayer == true)
            //            {
            //                //First lap started
            //                if (isFirstCheckPointPassed == false)
            //                    Debug.Log("First lap started");
            //                else
            //                    //Full round passed
            //                    Debug.Log($"Lap " + currentLap + " : " + (Time.time - lapStartTime));
            //            }

            //            //Last lap is finished
            //            if (GameManager.Instance.lapsCount == currentLap)
            //            {
            //                if (characterIdentityControl.IsLocalPlayer == true)
            //                {
            //                    Debug.Log("Finished");

            //                    GameManager.Instance.UI.ShowEndOfGamePopup();
            //                }

            //                isFinished = true;
            //            }

            //            //Start lap timer
            //            lapStartTime = Time.time;
            //            isFirstCheckPointPassed = true;
            //        }

            //        //Get next checkpoint
            //        CheckPoint nextCheckPoint = GameManager.Instance.checkPointControl.GetNextCheckPoint(checkPoint.index);

            //        //Update current and expected (next) checkpoints
            //        nextCheckPointIndex = nextCheckPoint.index;
            //        currentCheckPointIndex = checkPoint.index;
           //     }
          //  }
        }
    }
}