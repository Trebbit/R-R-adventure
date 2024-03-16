using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace HEAVYART.Racing.Netcode
{
    public class EndOfGamePopupUIController : MonoBehaviour
    {
        public Text leaderboardTextComponent;

        private void FixedUpdate()
        {
            UpdateLeaderboard();
        }

        private void UpdateLeaderboard()
        {
            string outputText = "";

            //Get leaderboard
            var sortedLeaderboard = GameManager.Instance.leaderboard.ToList();

            //Sort users by finishing time
            sortedLeaderboard.Sort((a, b) =>
            {
                return a.Value.finishTimestamp.CompareTo(b.Value.finishTimestamp);
            });

            //Pack leaderboard in one string 
            for (int i = 0; i < sortedLeaderboard.Count; i++)
            {
                if (sortedLeaderboard[i].Value.isFinished)
                {
                    string time = TimeSpan.FromSeconds(sortedLeaderboard[i].Value.finishTimestamp - GameManager.Instance.gameStartTime).ToString(@"mm\:ss\:ff");
                    outputText += (i + 1).ToString() + " : " + sortedLeaderboard[i].Value.userName + " : " + time + "\n";
                }
                else
                {
                    if (GameManager.Instance.gameState == GameState.GameIsOver)
                        outputText += (i + 1).ToString() + " : " + sortedLeaderboard[i].Value.userName + " : -- : -- : -- " + "\n";
                }
            }

            //Show it on a screen
            leaderboardTextComponent.text = outputText;
        }
    }
}