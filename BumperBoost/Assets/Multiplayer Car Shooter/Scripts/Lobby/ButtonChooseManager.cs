using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.UI;
using TMPro;
using Photon.Realtime;
using Photon.Pun;
using Hashtable = ExitGames.Client.Photon.Hashtable;

namespace MultiplayerShooter
{
public class ButtonChooseManager : MonoBehaviour {


	public static  ButtonChooseManager  instance;
	
    [Header("Car Models")]
	public GameObject[] cars;
	
	[Header("Max Characters")]
	public int maxCharacters = 3;
	
	[Header("Character Name")]
	public TextMeshProUGUI txtCharName;

    [Header("Slide Buttons")]
	[SerializeField] private GameObject nextButton, prevButton;

    [Header("ADS Skin Image")]
	public GameObject skin;

    [Header("ADS Button")]
	public GameObject adsBtn;

    [Header("Done Button")]
	public GameObject doneBtn;

    [Header("Power Slider")]
	public Slider powerSlider;

	[Header("Health Slider")]
	public Slider healthSlider;

	[Header("Speed Slider")]
	public Slider speedSlider;
	
	public int currentCar = 0;

	public Dictionary<int, CarSkills> skillList = new Dictionary<int, CarSkills>();

	public class CarSkills
		{
			public int  id;

			public string name;

			public int power;

			public int health;

			public int speed;

		}


	// Use this for initialization
	void Awake () {
	
		// if don't exist an instance of this class
		if (instance == null) {


			// define the class as a static variable
			instance = this;

			currentCar = 0;

		    PlayerPrefs.DeleteAll();
			HideADSButton();

			#if UNITY_ANDROID 

            //releases the first character's ADS
		    PlayerPrefs.SetString(currentCar.ToString(),"true");
		   
            #else

            //for Standalone versions of the game, we mark the characters without ADS
		    for(int i = 0; i<= maxCharacters;i++)
		    {
              PlayerPrefs.SetString(i.ToString(),"true");
		    }
	
            #endif
 
		   
		    Hashtable props = new Hashtable
            {
                {"currentCar",currentCar}
            };
            PhotonNetwork.LocalPlayer.SetCustomProperties(props);

		    //sets the skills of each character
			SetCarSkills();

			//configures and displays the first avatar as available
			SetCar(currentCar);

			//configure the slider buttons
			CheckButtonStatus();

		}
	}

	public void Reset()
	{
	    currentCar = 0;
		
	}

    /// <summary>
    /// sets the skill of each character
    /// </summary>
	public void SetCarSkills()
	{
		for(int i =0;i<=maxCharacters;i++)
		{
			CarSkills skill = new CarSkills ();
			skill.id = i;
		
			if(i==0)
			{
				skill.name = "Desert Vehicle Patrol";
				skill.power = 40;
				skill.health = 50;
				skill.speed = 90;
			}
			if(i==1)
			{
				skill.name = "Explorer 4X4 Vehicle";
				skill.power = 70;
				skill.health = 60;
				skill.speed = 50;
			}
			if(i==2)
			{
				skill.name = "Semi Truck";
				skill.power = 90;
				skill.health = 70;
				skill.speed = 40;
			}
			if(i==3)
			{
				skill.name = "Police Interceptor";
				skill.power = 80;
				skill.health = 70;
				skill.speed = 90;
			}
	
		    skillList.Add (skill.id, skill);
      
		
	}

	}
	

	/// <summary>
	/// method for controlling the avatars choice buttons
	/// </summary>
	private void CheckButtonStatus()
	{
	
		if (nextButton == null || prevButton == null)
			return;
		
		if (currentCar == 0) 
		{
			prevButton.SetActive(false);
			nextButton.SetActive(true);
		} else if (currentCar >= maxCharacters-1) 
		{
			prevButton.SetActive(true);
			nextButton.SetActive(false);
		} else 
		{
			prevButton.SetActive(true);
			nextButton.SetActive(true);
		}
		
	}


	/// <summary>
	/// method called by the BtnNext button that selects the next avatar
	/// </summary>
	public void NextAvatar()
	{
	  if(currentCar+1< maxCharacters)
	  {	   
		currentCar++;
		

		#if UNITY_ANDROID 
		 // checks if the character is unlocked by ADS
		 if (PlayerPrefs.HasKey (currentCar.ToString()) &&
		 PlayerPrefs.GetString(currentCar.ToString()).Equals("true"))
		  {
            	
             HideADSButton();
            

		  }
		  else
		  {   //if the character is not unlocked 
		  
             //show ADS button
			  ShowADSButton();
          
			 
		  }
		  #endif

		    Hashtable props = new Hashtable
            {
                {"currentCar",currentCar}
            };
			Debug.Log("current car: "+currentCar);
			//LobbyManager.instance.txtLog.text = currentCar.ToString();
			//saves the chosen character for the game scene
            PhotonNetwork.LocalPlayer.SetCustomProperties(props);
		
		//configures the current character for display to the user
		SetCar(currentCar);
		
		
		
		if(currentCar>=maxCharacters)
		{
			currentCar = maxCharacters - 1;
		}

		CheckButtonStatus();
	
	  }
	}
	
	/// <summary>
	/// method called by the BtnPrev button that selects the previous avatar
	/// </summary>
	public void PrevAvatar()
	{
	  if(currentCar-1 >= 0)
	   {
				    
		  currentCar--;

           // checks if the character is unlocked by ADS
		   if (PlayerPrefs.HasKey (currentCar.ToString()) &&
		 PlayerPrefs.GetString(currentCar.ToString()).Equals("true"))
		  {
			
             
			 
			#if UNITY_ANDROID 
             HideADSButton();
            #endif
			 
		   Hashtable props = new Hashtable
            {
                {"currentCar",currentCar}
            };

			//saves the chosen character for the game scene
            PhotonNetwork.LocalPlayer.SetCustomProperties(props);
		  }
		 else
		  { //if the character is not unlocked 
		  
		    #if UNITY_ANDROID 
            //show ADS button
			  ShowADSButton();
            #endif
			 
		  }
		
		  //configures the current character for display to the user
		  SetCar(currentCar);
		
		
		 		
		   if(currentCar<0)
		   {
			  currentCar =0;
		   }

		   CheckButtonStatus();
		  
		}
	}

    /// <summary>
    /// displays the lock for ADS
    /// </summary>
	public void ShowADSButton()
	{
		skin.SetActive(true);
		GameObject.Find("ADSBtn").SetActive(true);
		GameObject.Find("DoneBtn").SetActive(false);
	}

    /// <summary>
    /// removes the block for ADS
    /// </summary>
	public void HideADSButton()
	{
	   
		skin.SetActive(false);
		GameObject.Find("ADSBtn").SetActive(false);
		GameObject.Find("DoneBtn").SetActive(true);
	}
	
	
	/// <summary>
	///   helper method to set HUD avatar 
	/// </summary>
	/// <param name="index">current character index</param>
    public void SetCar(int index)
    {
		
		for(int i=0;i<cars.Length;i++)
		{
			
			MeshRenderer[] rends = cars[i].GetComponentsInChildren<MeshRenderer> ();
		

			if(i.Equals(index))
			{
				
				foreach(MeshRenderer rend in rends)
		        {
		          rend.enabled = true;
		        }

			}
			else
			{
			
				foreach(MeshRenderer rend in rends)
		        {
				
		          rend.enabled = false;
				
		        }
				
			}
			
		}
		
		  
		 txtCharName.text = skillList[index].name;
		 powerSlider.value = skillList[index].power;
		 healthSlider.value = skillList[index].health;
		 speedSlider.value = skillList[index].speed;

       
    }
}//END_CLASS
}//END_NAMESPACE
