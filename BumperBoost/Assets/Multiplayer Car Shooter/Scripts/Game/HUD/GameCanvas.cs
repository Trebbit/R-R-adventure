using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MultiplayerShooter
{
public class GameCanvas : MonoBehaviour
{

    public static GameCanvas  instance;

	GameManager gameManager;
    
	[Header("Mobile Buttons")]
	public  GameObject mobileButtons;

    [Header("Leader Board")]
	public  GameObject leaderBoard;

	[Header("Txt Countdown")]
	public Text txtCountdown;

	[Header("Button close End Lobby")]
	public  GameObject  btnCloseEndLobby;
    
	[Header("Button close Alert dialog")]
	public  GameObject  btnClosedialog;

    [Header("Button close Leader Board")]
	public  GameObject  btnCloseLeaderBoard;

    [Header("User Interface Game Objects")]
	public RectTransform [] uiObjects;

    [Header("Txt Info")]
	public Text txtInfo;
	
	
    [Header("Txt Health")]
	public Text txtLocalPlayerHealth;

	public GameObject jumpBtn;

	public GameObject flyBtn;

	public  GameObject attackBtn;

	[Header("Alert Dialog panel")]
	public  GameObject alertgameDialog;

    [Header("Alert Dialog Text")]
	public Text alertDialogText;

    [Header("Alert Game Over Dialog Canvas")]
	public  GameObject  alertGameOverDialog;

    [Header("Alert Game Over Dialog Text")]
	public Text  alertGameOverText;

    [Header("Level Name Text")]
	public Text txtLevelName;
    
	[Header("Description Level Text")]
	public Text txtLevelDescription;

    [Header("Level Apresentation Game Object")]
	public GameObject LevelApresentation;

	int currentMenu;
    
	[HideInInspector]
	public float delay = 0f;

	public bool gameOver;

	[HideInInspector]
	public int health;

	
	/***********************DAMAGE SKIN***************************/
	[Header("Damage Image :")]

	[Header("Damage Variables")]
	public Image damageImage; // set in inspector.

    [Header("Flash Speed")]
	public float flashSpeed = 5f; // set in inspector.

    [Header("Flash Color")]
	public Color flashColour = new Color(1f, 0f, 0f, 0.1f);  // set in inspector.

    [Header("Health Slider")]
	public Slider healthSlider;  // set in inspector.
    
	[Header("Txt Health")]
	public Text txtHealth;  // set in inspector.

	[HideInInspector]
	public bool damaged; 

	/***********************************************************/

	[Header("Nitro Variables :")]

	[HideInInspector]
	public float progress = 100;

   	[Header("NITRO Delay UI Time")]
	public float nitroDelay = 1f;

	[Header("NITRO UI Time")]
	public float nitroTime = 0.01f;
	
	[HideInInspector]
	public bool onUseNitro;
	
	[Header("NITRO Slider")]
	public GameObject nitroSlider;  // set in inspector.
	
	[Header("Text Nitro")]
	public  GameObject txtNitro;


	/***********************************************************/

	[Header("Failed Audio Clip :")]
	
	[Header("Audio Variables")]
	public AudioClip failedAudioClip;

    [Header("Victory Audio Clip")]
	public AudioClip victoryAudioClip;

    [Header("Colected Audio Clip")]
	public AudioClip colectedAudioClip;
	
	 [Header("Game Music Audio Source")]
	public AudioSource music;

	/********************************************************/
	

    /// <summary>
    ///  controls the nitroglycerin slider displayed on the screen
    /// </summary>
	void UpdateNitroSlider()
	{
        //Debug.Log("onUseNitro: "+onUseNitro);
		
		//creates the slider decreasing effect of the slide
		if (onUseNitro && progress >0 && progress <=100) {
		    
			progress = progress - 1;
			nitroSlider.SetActive(true);
			txtNitro.SetActive(true);
			nitroSlider.GetComponent<Slider>().value = progress;
	        
		}
	    //ends the slider animation
		else if (progress <= 0 && onUseNitro)
		{
			//Debug.Log("0%");
			onUseNitro = false;
			nitroSlider.SetActive(false);
			txtNitro.SetActive(false);
			progress = 100;
			gameManager.localPlayer.GetComponent<PlayerManager>().StopNitro();
	
		}
	}
	


    	// Use this for initialization
	void Start () {

		if (instance == null) {

			instance = this;
			btnClosedialog.SetActive(false);
			CloseLeaderBoard();
			gameManager = GameObject.Find("GameManager").GetComponent<GameManager>() as GameManager;
			
			InvokeRepeating ("UpdateNitroSlider",nitroDelay,nitroTime);

			#if UNITY_ANDROID 
			
			#else
			    mobileButtons.SetActive(false);
		     #endif
		
			OpenScreen("game");
			
		}
		else
		{
			Destroy(this.gameObject);
		}



	}
	
	public void AddHealth()
	{
		health += 10;

		txtLocalPlayerHealth.text = "HP: "+ health.ToString ();
	}

	public void OpenHelp()
	{
		if(!Input.GetButton("Jump"))
		{
			#if !UNITY_ANDROID
				ShowAlertDialog("Welcome to Car Royale Online, Be the last to survive to win!"+ "\n"+ "\n"+
			"Press ARROW KEYS or WASD to control "+ "\n"+
			"Press B to use NITRO"+ "\n"+
			"Press MOUSE_LEFT_BUTTON to fire weapon"+ "\n"+
			"Press the weapon buttons to switch weapon"+ "\n"

			);
 
		    #else
				ShowAlertDialog("Welcome to  Car Royale Online, Be the last to survive to win!"+ "\n"+ "\n"+
			"Press Left or Right buttons to control "+ "\n"+
			"Press Nitro Button to use NITRO"+ "\n"+
			"Press Fire Button to fire weapon"+ "\n"+
			"Press the weapon buttons to switch weapon"+ "\n"

			);
		    #endif
		}
		
	}

	//reset the game for the losing player
	public void GameOver()
	{
	  gameOver = true;
	  ShowGameOverMessage("looser","Respaw in 5 seconds");
	
	
	}

	public void ShowGameOverMessage(string _result,string _message)
	{
	
		alertGameOverText.text = _message;
		
		StartCoroutine (CloseGameOverMessage() );

	}

	
	/// <summary>
	/// Closes the alert dialog.
	/// </summary>
	IEnumerator CloseGameOverMessage() 
	{

		yield return new WaitForSeconds(5);

		alertGameOverText.text = string.Empty;
		GameManager gameManager = GameObject.Find("GameManager").GetComponent<GameManager>() as GameManager;
		
		gameManager.localPlayer.GetComponent<PlayerManager>().EnablePlayer();
	} 

	//reset the game for the losing player
	public void Win()
	{
	   gameOver = true;
	   ShowGameOverMessage("victory",gameManager.localPlayer.GetComponent<PlayerManager>().name+" winner!!!");
	  
	
	}

	/// <summary>
	/// Shows the alert dialog.
	/// </summary>
	/// <param name="_message">Message.</param>
	public void ShowAlertDialog(string _message)
	{
		alertDialogText.text = _message;
		alertgameDialog.SetActive(true);
		btnClosedialog.SetActive(true);
			
	}

	public void ShowInfoText(string _text)
	{
		txtInfo.text = _text;
		txtInfo.enabled = true;
		StartCoroutine (ShowTxtAnimation() );
	}

	IEnumerator ShowTxtAnimation() 
	{
        txtInfo.GetComponent<Animator>().SetTrigger ("OnShow");
		PlaycolectedAudioClip();
		yield return new WaitForSeconds(2);
		txtInfo.text = "";
		txtInfo.enabled = false; 
	}

	 /// <summary>
	/// Shows the  Level Apresentation.
	/// </summary>
	public void OpenApresentation(string levelName,string levelDescription)
	{
	    txtLevelName.text = levelName;
		txtLevelDescription.text = levelDescription;
		LevelApresentation.GetComponent<Animator>().SetTrigger("Open");
		StartCoroutine (CloseApresentation() );
	}

	/// <summary>
	/// Closes the level apresentation.
	/// </summary>

	IEnumerator CloseApresentation() 
	{

		yield return new WaitForSeconds(4);
		LevelApresentation.GetComponent<Animator>().SetTrigger("Close");
	} 

	public void PlaycolectedAudioClip()
	{

	   if (!GetComponent<AudioSource> ().isPlaying )
		{
		
		  GetComponent<AudioSource>().PlayOneShot(colectedAudioClip);

		}


	}

    

	/// <summary>
	/// Closes the alert dialog.
	/// </summary>
	public void CloseAlertDialog()
	{
		btnClosedialog.SetActive(false);
	   
		alertgameDialog.SetActive(false);

	  
	
	}

	void Update()
	{
	 	
	 	/***********************DAMAGE SKIN***************************/
		if(damaged)
		{
			damageImage.color = flashColour;
		}
		else
		{
			damageImage.color = Color.Lerp (damageImage.color, Color.clear, flashSpeed * Time.deltaTime);
		}
		damaged = false;
      /*************************************************************************/
	  
	  if (Input.GetKey(KeyCode.Tab))
	  {
		if(leaderBoard.active)
	    {
		  CloseLeaderBoard();
	    }
	    else
	    {
	  	  OpenLeaderBoard();
	    }
		  
	  }
	  if (Input.GetKey ("escape") )
	  {
		OpenHelp();
	  }
		

    }
	

	
/// <summary>
	/// Opens the screen.
	/// </summary>
	/// <param name="_current">Current.</param>
	public void  OpenScreen(string _current)
	{
		switch (_current)
		{
		    
			case "game":
			Screen.orientation = ScreenOrientation.LandscapeLeft;
			alertgameDialog.SetActive(false);
			break;

	
		}

	}

	public void OpenLeaderBoard()
	{
	  if(!Input.GetButton("Jump"))
		{
         leaderBoard.SetActive(true);
	     btnCloseLeaderBoard.SetActive(true);
	  }
	}
	
	public void CloseLeaderBoard()
	{
		  leaderBoard.SetActive(false);
		  btnCloseLeaderBoard.SetActive(false);
	}


	
	
	public void StopMusic()
	{
	
		music.Stop();
	}

	public void PlayAudio(AudioClip _audioclip)
	{
		if (!GetComponent<AudioSource> ().isPlaying) {
			GetComponent<AudioSource> ().PlayOneShot (_audioclip);
		}
	}

	public void OpenEndLobby()
	{
		txtCountdown.text = string.Empty;
		btnCloseEndLobby.SetActive(true);

	}

	public void CloseEndLobby()
	{
		gameManager.LeftRoom();
	}

}
}