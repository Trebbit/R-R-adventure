using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Photon.Realtime;
using Photon.Pun;
namespace MultiplayerShooter
{
public class LobbyManager : MonoBehaviourPunCallbacks
{
    
	public static LobbyManager  instance;

	public enum Status {Offline,Online, Connecting,Connected, Joining, Creating, Rooming};

	[Header("Informative")]
	public Status status = Status.Offline;

    [Header("Main Menu")]
	public GameObject mainPanel;

    [Header("Lobby Room")]
	public GameObject lobbyRoom;

	[Header("Create Room Panel")]
	public GameObject createRoomPanel;

    [Header("Room List Panel")]
	public GameObject roomListPanel;

	[Header("Choose Character Panel")]
	public GameObject ChooseCharacterPanel;
	
	[Header("Alert Dialog panel")]
	public GameObject alertgameDialog;
    
	[Header("Alert Dialog Text")]
	public Text alertDialogText;
    
	[Header("Mesage Text")]
	public Text messageText;
	
	public GameObject lobbyCamera;

	public string currentMenu;
    
	[Header("loading Image")]
	public GameObject loadingImg;

	[Header("Background loading Image")]
	public GameObject backgroundLoadingImg;

	[Header("Input Login")]
	public InputField inputLogin;

    [Header("Private Room ID")]
	public InputField inputPrivateRoomID;
	
	[Header("Input Max Players")]
	public InputField inputMaxPlayers;
	
    [Header("Text Room ID")]
	public Text txtRoomID;

	public Button roomType;
    
	[Header("Current Room Players Text")]
	public Text txtCurrentRoomPlayers;

    [Header("Max Room Players Text")]
	public Text txtMaxRoomPlayers;

	public Text txtLog;
	
   [Header("Current Location Text")]
	public Text txtCurrentLocation,txtCurrentLocation2;
	
    [Header("Content Rooms")]
	public GameObject contentRooms;

    [Header("Room Prefab")]
	public GameObject roomPrefab;

	
	[Header("Map Slider Buttons")]
	[SerializeField] private GameObject nextButton, prevButton,nextButton2, prevButton2;
	
	[Header("Image Animator")]
	public Animator mapAnim,mapAnim2;
	
	[Header("Max Maps")]
	public int maxMaps = 3;
	
	public int chooseMap,chooseMap2 = 0;

    [Header("Avaliable Maps")]
	public GameObject[] avaliableMaps;

    [Header("Max Players Buttons")]
	public GameObject[] maxPlayersBtns;

	public GameObject btnPrivateRoom;

	public GameObject activeBtnSprite;

	public GameObject desabledBtnSprite;

	public string currentMap;

    [HideInInspector]
	public byte maxPlayers;
    [HideInInspector]
	public int maxPlayers2;

	public bool isPrivateRoom;

	ArrayList roomList;
	
	public string roomName = string.Empty;

	public float delay = 0f;

	public Dictionary<string, PhotonRoom> photonRooms = new Dictionary<string, PhotonRoom>();

	public float cont;
	
	public  AudioClip buttonSound;

	public class PhotonRoom
	{
		public string  name;

		public string ownerName;

		public string map;

		public string isPrivate;

		public string currentPlayers ;

		public string maxPlayers ;

	}

	public PhotonView view{ get{ return GetComponent<PhotonView>(); }}


	 public void Awake()
    {
     
        PhotonNetwork.AutomaticallySyncScene = true;

        inputLogin.text = "Player " + UnityEngine.Random.Range(1000, 10000);

        if (Application.internetReachability != NetworkReachability.NotReachable) { // Check if there is a connection here
            
            Debug.Log ("Connected to the Internet!"); // Here executes the desired action if there is a connection
            status = Status.Online;
			if (!PhotonNetwork.IsConnected)
            {
			  
               // Connect to the photon master-server. We use the settings saved in PhotonServerSettings (a .asset file in this project)
               PhotonNetwork.ConnectUsingSettings();

			   status = Status.Connecting;

              }
		
		}//END_IF

        if (Application.internetReachability == NetworkReachability.NotReachable) { // Here it is checked if there is no connection
            
            Debug.Log ("There is no connection!");// Here performs the desired action if there is no connection
           
        }
    }


   
	
	// Use this for initialization
	void Start () {

		if (instance == null) {

			instance = this;

			roomList = new ArrayList ();
			
			CloseAlertDialog ();

			CloseLoadingImg();

			maxPlayers = 1;

		    maxPlayers2 = 1;
			
			txtCurrentLocation.text = "MAP1";
			
			txtCurrentLocation2.text = "MAP1";
			
			OpenScreen("loading");
			
		}
		else
		{
			Destroy(this.gameObject);
		}



	}

	void Update()
	{
		delay += Time.deltaTime;

		if (Input.GetKey ("escape") && delay > 1f) {

		  switch (currentMenu) {

			case "main_menu":
			 delay = 0f;
			 Application.Quit ();
			break;

			case "lobby_room":
			 delay = 0f;
			OpenScreen( "main_menu");
			break;
			case "roomList":
			 delay = 0f;
			OpenScreen( "main_menu");
			break;
			case "choose_character":
			 delay = 0f;
			OpenScreen( "main_menu");
			break;



		

		 }//END_SWITCH

	 }//END_IF
}
	/// <summary>
	/// opens a selected screen.
	/// </summary>
	/// <param name="_current">Current screen</param>
	public void  OpenScreen(string _current)
	{
		switch (_current)
		{
			case "loading":
			Screen.orientation = ScreenOrientation.LandscapeLeft;
			currentMenu = _current;
			mainPanel.SetActive(false);
			lobbyRoom.SetActive(false);
			createRoomPanel.SetActive(false);
			roomListPanel.SetActive(false);
			ChooseCharacterPanel.SetActive(false);

			ShowLoadingImg();

			break;
		    //lobby menu
		    case "main_menu":
			CloseLoadingImg();
			currentMenu = _current;
			mainPanel.SetActive(true);
			lobbyRoom.SetActive(false);
			createRoomPanel.SetActive(false);
			roomListPanel.SetActive(false);
			ChooseCharacterPanel.SetActive(false);
			
			break;

			case "lobby_room":
			currentMenu = _current;
			ShowLoadingImg();
			mainPanel.SetActive(false);
			lobbyRoom.SetActive(true);
			createRoomPanel.SetActive(false);
			roomListPanel.SetActive(false);
			ChooseCharacterPanel.SetActive(false);
			
			break;


		    case "roomList":
			CheckButtonStatus2();
			currentMenu = _current;
			mainPanel.SetActive(false);
			lobbyRoom.SetActive(false);
			createRoomPanel.SetActive(false);
			roomListPanel.SetActive(true);
			ChooseCharacterPanel.SetActive(false);
			
		
			break;
			case "create_room":
			chooseMap = 0;
			//configures and displays the first avatar as available
			SetMap("map1");
			//configure the slider buttons
			CheckButtonStatus();
			currentMenu = _current;
			mainPanel.SetActive(false);
			lobbyRoom.SetActive(false);
			createRoomPanel.SetActive(true);
			roomListPanel.SetActive(false);
			ChooseCharacterPanel.SetActive(false);
			break;

			case "choose_character":
			currentMenu = _current;
			mainPanel.SetActive(false);
			lobbyRoom.SetActive(false);
			createRoomPanel.SetActive(false);
			roomListPanel.SetActive(false);
			ChooseCharacterPanel.SetActive(true);
			break;
	
		}

	}

	 #region PUN CALLBACKS

      /// <summary>
      /// callback that processes the server's response after the user is connected to the server
      /// </summary>
	  public override void OnConnected(){
        Debug.Log("onConnected stage achieved: next one should be ConnectedToMaster");
    }

    /// <summary>
    /// callback that processes the server's response after the user is connected to the server
    /// </summary>
    public override void OnConnectedToMaster(){
     
	    status = Status.Connected;
        Debug.Log("Server host: "+ PhotonNetwork.CloudRegion + 
		",  ping:" + PhotonNetwork.GetPing());
      PhotonNetwork.JoinLobby();
	

    }

    /// <summary>
    /// callback that processes server response after the user enters the lobby room
    /// </summary>
    public override void OnJoinedLobby(){
        
        Debug.Log("You are now at the lobby.");

		status = Status.Rooming;
      
	    OpenScreen("main_menu");

    }

	

    public override void OnLeftLobby(){}



   /// <summary>
   ///  lists all rooms corresponding to map chosen by the user
  /// </summary>
  /// <param name="_map"> map chosen by the user</param>

	public void GetRooms(string _map)
	{
       
		OpenScreen("roomList");

		ClearRooms();

		foreach (KeyValuePair<string, PhotonRoom> room in photonRooms) {
          Debug.Log("is private: "+room.Value.isPrivate);

		  if(room.Value.map.Equals(_map) && room.Value.isPrivate.Equals("False") )
		  {
             SpawnRoom(room.Value.name,room.Value.ownerName, room.Value.currentPlayers, room.Value.maxPlayers);
		
		  }
		 	

		}//END_FOREACH

        
	}

   
	/// <summary>
	/// this method checks for new rooms every 5 seconds and only evidently executes if there are new rooms.
	/// </summary>
	/// <param name="roomList"></param>
	public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
		Debug.Log("On Received room");
        foreach (RoomInfo room in roomList)
        {
            RoomReceived(room);
        }

    }


	
	/// <summary>
	/// configures a room object to be used in the future by the GetRooms method.
	/// </summary>
	/// <param name="room"></param>
    private void RoomReceived(RoomInfo room)
    {
      Debug.Log("room name: "+room.Name);
	  if (room.IsVisible && room.PlayerCount < room.MaxPlayers && !photonRooms.ContainsKey (room.Name))
      {
		 PhotonRoom new_room = new PhotonRoom ();

		 new_room.name = room.Name;//set room name
		 new_room.ownerName = (string) room.CustomProperties["OwnerName"];
		 new_room.map = (string)room.CustomProperties["Map"];
		 new_room.isPrivate = (string) room.CustomProperties["isPrivateRoom"];
		 new_room.currentPlayers = room.PlayerCount.ToString();
		 new_room.maxPlayers = room.MaxPlayers.ToString();

		 photonRooms.Add (room.Name, new_room);
		 Debug.Log("room: "+room.Name+" created");


	  }

    }


    	 
	/// <summary>
	/// method called automatically after the player has created the room.
	/// </summary>
	public override void OnCreatedRoom()
	{
       Debug.Log("Room Created Successfully");
	}


	/// <summary>
	///  CREATE_ROOM.
	/// </summary>
	public void CreateRoom()
	{
	 
		 roomName = "Room " + UnityEngine.Random.Range(1000, 10000);

        RoomOptions roomOptions = new RoomOptions {MaxPlayers = maxPlayers};

		roomOptions.CustomRoomPropertiesForLobby = new string[3] { "OwnerName","Map", "isPrivateRoom"};
		roomOptions.CustomRoomProperties = new ExitGames.Client.Photon.Hashtable();
	
        roomOptions.CustomRoomProperties.Add("OwnerName", inputLogin.text);
		roomOptions.CustomRoomProperties.Add("Map", currentMap);
        roomOptions.CustomRoomProperties.Add("isPrivateRoom",isPrivateRoom.ToString());

        PhotonNetwork.CreateRoom(roomName, roomOptions, null);

        status = Status.Creating;
			  
	}

    /// <summary>
	/// adds the player to the chosen room.
	/// </summary>
	 public void JoinRoom(string _roomName){
 
        PhotonNetwork.JoinRoom(_roomName);
  
    }
	

	 
	/// <summary>
	/// method called automatically after the player has joined the room.
	/// </summary>
	public override void OnJoinedRoom(){

        status = Status.Rooming;
		
		Debug.Log ("\n joining ...\n");
		
		Debug.Log("RoomJoined " + PhotonNetwork.CurrentRoom.Name);

		PhotonNetwork.LocalPlayer.NickName = inputLogin.text;
      

		OpenScreen("lobby_room");

		
         
		SetUpRoom( PhotonNetwork.CurrentRoom.Name,PhotonNetwork.CurrentRoom.PlayerCount.ToString(),
		                                       PhotonNetwork.CurrentRoom.MaxPlayers.ToString(), (string)PhotonNetwork.CurrentRoom.CustomProperties["Map"]);
        view.RPC("Matchmaking", RpcTarget.AllBuffered, PhotonNetwork.LocalPlayer);

	 
	}
   
	/// <summary>
	/// method called when a player enters the room.
	/// check if the room is already full so you can start the game
	///(as it is RPC, the method is called for all players in the Lobby)
	/// </summary>
	[PunRPC]
    public void Matchmaking(Player player) {

      
		UpdateCurrentPlayers(PhotonNetwork.CurrentRoom.PlayerCount.ToString());

		
       

         if (PhotonNetwork.CurrentRoom.PlayerCount.Equals(PhotonNetwork.CurrentRoom.MaxPlayers)) {
            
		    //unlocks the button to start the game
		    LoadLevel();
            PhotonNetwork.CurrentRoom.IsOpen = false;
            PhotonNetwork.CurrentRoom.IsVisible = false;
			

         }
            
    }

    /// <summary>
	///  LoadLevel is a function that loads a scene, its parameter being the scene's index in the build settings.
	/// </summary>
	public void LoadLevel()
	{
		if (PhotonNetwork.IsMasterClient)
		{
			
            PhotonNetwork.LoadLevel("Game");		
              
		 
		}
	}

	#endregion

	public void SetUpRoom(string room_id, string current_players,string max_players,string map)
	{
	
	  txtRoomID.text = room_id;
      
	  txtCurrentRoomPlayers.text = current_players;

	  txtMaxRoomPlayers.text = max_players;

	}

	public void SetPrivateRoom()
	{
		isPrivateRoom =!isPrivateRoom;

		if(isPrivateRoom)
		{
          btnPrivateRoom.GetComponent<Image> ().sprite = activeBtnSprite.GetComponent<SpriteRenderer> ().sprite;
		  btnPrivateRoom.GetComponent<Image> ().color = activeBtnSprite.GetComponent<SpriteRenderer> ().color;
		
		}
		else
		{
          btnPrivateRoom.GetComponent<Image> ().sprite = desabledBtnSprite.GetComponent<SpriteRenderer> ().sprite;
		  btnPrivateRoom.GetComponent<Image> ().color = desabledBtnSprite.GetComponent<SpriteRenderer> ().color;
		}

		

	}

    	 
	/// <summary>
	/// method called automatically after the player has joined the room.
	/// </summary>
	public void SetMap(string _map)
	{
		currentMap = _map;
		mapAnim.SetTrigger (currentMap);
	}

	public void ChooseAvaliableMaps(string _map)
	{
		currentMap = _map;
		mapAnim2.SetTrigger (currentMap);
		GetRooms(_map);
		
		
	}

	public void SetMaxPlayers()
	{
	    int  _max_players = int.Parse(inputMaxPlayers.text);
	
		maxPlayers = (byte)_max_players;

		maxPlayers2 = _max_players;

	}



	/// <summary>
	/// Shows the alert dialog.
	/// </summary>
	/// <param name="_message">Message.</param>
	public void ShowAlertDialog(string _message)
	{
		alertDialogText.text = _message;
		alertgameDialog.SetActive(true);
	}

	public void ShowLoadingImg()
	{
		loadingImg.SetActive(true);
		backgroundLoadingImg.SetActive(true);


	}
	public void CloseLoadingImg()
	{
		loadingImg.SetActive(false);
		backgroundLoadingImg.SetActive(false);

	}
	/// <summary>
	/// Closes the alert dialog.
	/// </summary>
	public void CloseAlertDialog()
	{
		alertgameDialog.SetActive(false);
	}
	
		/// <summary>
	/// Shows the alert dialog.Debug.Log
	/// </summary>
	/// <param name="_message">Message.</param>
	public void ShowMessage(string _message)
	{
		messageText.text = _message;
		messageText.enabled = true;
		StartCoroutine (CloseMessage() );//chama corrotina para esperar o player colocar o outro pé no chão
	}
	
	/// <summary>
	/// Closes the alert dialog.
	/// </summary>

	IEnumerator CloseMessage() 
	{

		yield return new WaitForSeconds(4);
		messageText.text = "";
		messageText.enabled = false;
	} 




	public void PlayAudio(AudioClip _audioclip)
	{
		
	   GetComponent<AudioSource> ().PlayOneShot (_audioclip);

	}

	public void PlayButtonSound()
	{
		
	   GetComponent<AudioSource> ().PlayOneShot (buttonSound);

	}




	public void UpdateCurrentPlayers( string current_players)
	{
	
	  txtCurrentRoomPlayers.text = current_players;


	}

	/// <summary>
	/// Clears rooms.
	/// </summary>
	public void ClearRooms()
	{
		foreach (GameObject room in roomList)
		{

			Destroy (room.gameObject);
		}

		roomList.Clear ();
	}

	public void SpawnRoom(string id,string name, string current_players, string max_players)
	{
		
       
		GameObject newRoom = Instantiate (roomPrefab) as GameObject;

		newRoom.GetComponent<Room>().id = id;
		newRoom.GetComponent<Room>().txtRoomName.text = name;
		newRoom.GetComponent<Room>().txtPlayers.text = current_players+" / "+max_players.ToString();
	
		newRoom.transform.parent = contentRooms.transform;
		newRoom.GetComponent<RectTransform> ().localScale = new Vector3 (1, 1, 1);

		roomList.Add (newRoom);
				

	}
  
    /// <summary>
	/// joins player to private room
	/// </summary>
	public void JoinToPrivateRoom()
	{
	    if(!inputPrivateRoomID.text.Equals(string.Empty))
		{
		  JoinRoom(inputPrivateRoomID.text);
		}
		else
		{
		 ShowAlertDialog("Please insert a valid room ID");
		}
		
	}
	
	/// <summary>
	/// method for controlling the avatars choice buttons
	/// </summary>
	private void CheckButtonStatus()
	{
	
		if (nextButton == null || prevButton == null)
			return;
		
		if (chooseMap == 0) 
		{
			prevButton.SetActive(false);
			nextButton.SetActive(true);
		} else if (chooseMap >= maxMaps-1) 
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
	public void NextMap()
	{
	  if(chooseMap+1< maxMaps)
	  {	   
		chooseMap++;

		switch (chooseMap) { 

		  case 0:
		  SetMap("map1");
		  txtCurrentLocation.text = "MAP1";
		  break;
		  case 1:
		  SetMap("map2");
		  txtCurrentLocation.text = "MAP2";
		  break;
		  case 2:
		  SetMap("map3");
		  txtCurrentLocation.text = "MAP3";
		  break;
		 
		}
		
		if(chooseMap>=maxMaps)
		{
			chooseMap = maxMaps - 1;
		}

		CheckButtonStatus();
	
	  }
	}
	
	/// <summary>
	/// method called by the BtnPrev button that selects the previous avatar
	/// </summary>
	public void PrevMap()
	{
	
	
	  if(chooseMap-1 >= 0)
	   {
				    
		  chooseMap--;

         
		 switch (chooseMap) { 

		  case 0:
		  SetMap("map1");
		  txtCurrentLocation.text = "MAP1";
		  break;
		  case 1:
		  SetMap("map2");
		  txtCurrentLocation.text = "MAP2";
		  break;
		  case 2:
		  SetMap("map3");
		  txtCurrentLocation.text =  "MAP3";
		  break;

		 
		  
		}
		 		
		   if(chooseMap<0)
		   {
			  chooseMap =0;
		   }

		   CheckButtonStatus();
		  
		}
	}
	
	/// <summary>
	/// method for controlling the avatars choice buttons
	/// </summary>
	private void CheckButtonStatus2()
	{
	
		if (nextButton2 == null || prevButton2 == null)
			return;
		
		if (chooseMap2 == 0) 
		{
			prevButton2.SetActive(false);
			nextButton2.SetActive(true);
		} else if (chooseMap2 >= maxMaps-1) 
		{
			prevButton2.SetActive(true);
			nextButton2.SetActive(false);
		} else 
		{
			prevButton2.SetActive(true);
			nextButton2.SetActive(true);
		}
		
	}
	
	/// <summary>
	/// method called by the BtnNext button that selects the next avatar
	/// </summary>
	public void NextMap2()
	{
	  if(chooseMap2+1< maxMaps)
	  {	   
		chooseMap2++;

		switch (chooseMap2) { 

		  case 0:
		
			ChooseAvaliableMaps("map1");
			txtCurrentLocation2.text = "MAP1";
		  break;
		  case 1:
			ChooseAvaliableMaps("map2");
			txtCurrentLocation2.text = "MAP2";
		  break;
		  case 2:
			ChooseAvaliableMaps("map3");
			txtCurrentLocation2.text =  "MAP3";
		  break;
		}
		
		if(chooseMap2>=maxMaps)
		{
			chooseMap2 = maxMaps - 1;
		}

		CheckButtonStatus2();
	
	  }
	}
	
	/// <summary>
	/// method called by the BtnPrev button that selects the previous avatar
	/// </summary>
	public void PrevMap2()
	{
	
	
	  if(chooseMap2-1 >= 0)
	   {
				    
		  chooseMap2--;

         
		 switch (chooseMap2) { 

		  case 0:
		  
			ChooseAvaliableMaps("map1");
			txtCurrentLocation2.text = "MAP1";
		  break;
		  case 1:
		  
			ChooseAvaliableMaps("map2");
			txtCurrentLocation2.text = "MAP2";
		  break;
		  case 2:
		  ChooseAvaliableMaps("map3");
		  txtCurrentLocation2.text =  "MAP3";
		  break;

		}
		 		
		   if(chooseMap2<0)
		   {
			  chooseMap2 =0;
		   }

		   CheckButtonStatus2();
		  
		}
	}

	
	
}//END_CLASS
}//END_NAMESPACE