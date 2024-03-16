using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Advertisements;
namespace MultiplayerShooter
{
public class RewardedAdsButton : MonoBehaviour, IUnityAdsListener
{
     #if UNITY_ANDROID
    private string gameId = "4200223";
    #else
    private string gameId = "4200223";
    #endif

    Button myButton;
    public string myPlacementId = "Rewarded_Android";

    public ButtonChooseManager btnChooseManager;

    public  Text txtLog;

    void Start () {   

        myButton = GetComponent <Button> ();

         if(Advertisement.isSupported)
        { 
             // Map the ShowRewardedVideo function to the button’s click listener:
             if (myButton) myButton.onClick.AddListener (ShowRewardedVideo);

            // Initialize the Ads listener and service:
            Advertisement.Initialize(gameId, false); // ...initialize. 
            Advertisement.AddListener (this);
        }
       

        Debug.Log("listenner added");
    }

    
     

    // Implement a function for showing a rewarded video ad:
    void ShowRewardedVideo () {
        Advertisement.Show (myPlacementId);
    }

    // Implement IUnityAdsListener interface methods:
    public void OnUnityAdsReady (string placementId) {
        Debug.Log("placementId: "+placementId);
       
        // If the ready Placement is rewarded, activate the button: 
        if (placementId == myPlacementId) {   
               
            myButton.interactable = true;
        }
    }

    public void OnUnityAdsDidFinish (string placementId, ShowResult showResult) {
        // Define conditional logic for each ad completion status:
        if (showResult == ShowResult.Finished) {
            // Reward the user for watching the ad to completion.
            PlayerPrefs.SetString(btnChooseManager.currentCar.ToString(),"true");
           
            btnChooseManager.currentCar -=1;
            btnChooseManager.NextAvatar();
			Debug.Log("ads done");
  
        } 
    }

    public void OnUnityAdsDidError (string message) {
        // Log the error.
    }

    public void OnUnityAdsDidStart (string placementId) {
        // Optional actions to take when the end-users triggers an ad.
    } 
}
}
