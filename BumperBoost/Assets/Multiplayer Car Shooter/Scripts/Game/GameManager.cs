using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;
using Photon.Realtime;
using Photon.Pun;
using Photon.Pun.UtilityScripts;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using Cinemachine;

namespace MultiplayerShooter
{
public class GameManager :  MonoBehaviourPunCallbacks
{
	/*********************** PREFABS VARIABLES ******************************************************/
   [Header("Players Prefabs")]
   //store the local players' models
   public GameObject playersPrefab;

   public GameObject[] mapsPref;
 
  /*************************************************************************************************/

   public GameObject map;

   ExitGames.Client.Photon.Hashtable CustomeValue;

   public Vector3[] boddyFollowOffset;

    public Vector3[] trackedObjectOffset;
	
	public Transform currentCheckPoint;

   //store all players in game
   public Dictionary<string, PlayerManager> networkPlayers = new Dictionary<string, PlayerManager>();

   public GameObject localPlayer;

   public GameCanvas gameCanvas;

   ArrayList rankedUsers;

   List<PlayerManager> topPlayers;

   public GameObject rankedUserPref;

   public GameObject 	contentPlayers;

   public int ranking;

   [Header("Camera")]
   public CinemachineVirtualCamera cinemachineVirtualCam;

   [Header("StartCountdown time in seconds")] 
   [SerializeField] float startCountdown = 20f;

   [Header("EndCountdown time in seconds")] 
   [SerializeField] float endCountdown = 20f;
     
   [HideInInspector]
   float countdown = 20f;
    
   bool startTimer = false;
    
   int startTime;

   public bool timerEnds;

    int hours = 0;  
	int minutes = 0;  
	int seconds = 0;  


	bool gameStarted;

   
	
      // Start is called before the first frame update
    void Start()
    {
		if(LevelCheck.PreviousLevel!=null)
		{
			PhotonNetwork.AutomaticallySyncScene = true;

		    topPlayers = new List<PlayerManager>();

		    rankedUsers = new ArrayList();

		    string _map = (string)PhotonNetwork.CurrentRoom.CustomProperties["Map"];

		    SpawnMap(_map);
			
		    SpawnPlayer ();
		
		    Hashtable props = new Hashtable
            {
                {GameConstants.PLAYER_LOADED_LEVEL, true}
            };
            PhotonNetwork.LocalPlayer.SetCustomProperties(props);

		}
		else
		{
			Debug.Log("Please load the Lobby Scene first!"); 
		}
       
        
    }

    public void SpawnMap(string _map)
	{
		
		switch (_map)
		{
		    
		    case "map1":
			 map = Instantiate (mapsPref[0], mapsPref[0].transform.position,
            Quaternion.identity);
			
			break;
			 case "map2":
			 map = Instantiate (mapsPref[1], mapsPref[1].transform.position,
            Quaternion.identity);
		
			
			break;
			 case "map3":
			 map = Instantiate (mapsPref[2], mapsPref[2].transform.position,
            Quaternion.identity);
			
		
			break;
		}
	
	}

	/// <summary>
	/// Spawns the player.
	/// </summary>
	public void SpawnPlayer()
	{
		 //makes the draw of a point for the player to be spawn
	    int index = UnityEngine.Random.Range (0, map.GetComponent<MapManager>().spawnPoints.Length);

		// take a look in PlayerManager.cs script
		PlayerManager newPlayer;
				
		// newPlayer = GameObject.Instantiate( local player avatar or model, spawn position, spawn rotation)
		newPlayer = PhotonNetwork.Instantiate(playersPrefab.name,
		map.GetComponent<MapManager>().spawnPoints[index].position,
			Quaternion.identity).GetComponent<PlayerManager> ();
		
		newPlayer.sphere.gameObject.transform.position = map.GetComponent<MapManager>().spawnPoints[index].position;
		
			
		networkPlayers [newPlayer.view.ViewID.ToString()] = newPlayer;

		Debug.Log("player instantiated");

		
		ExitGames.Client.Photon.Hashtable PlayerProperties = new ExitGames.Client.Photon.Hashtable();
		PlayerProperties.Add("Id", newPlayer.view.ViewID.ToString());
        PhotonNetwork.LocalPlayer.SetCustomProperties(PlayerProperties);
		
       	
		newPlayer.view.RPC("SetUpPlayer", RpcTarget.All,newPlayer.view.ViewID.ToString(),
		    PhotonNetwork.LocalPlayer.NickName,(int)PhotonNetwork.LocalPlayer.CustomProperties["currentCar"]);
			
			
		localPlayer = newPlayer.gameObject;

		cinemachineVirtualCam.LookAt = newPlayer.player.transform;
		cinemachineVirtualCam.Follow = newPlayer.target.transform;;

		cinemachineVirtualCam.GetCinemachineComponent<CinemachineTransposer>().m_FollowOffset = boddyFollowOffset[(int)
		PhotonNetwork.LocalPlayer.CustomProperties["currentCar"]];
		cinemachineVirtualCam.GetCinemachineComponent<CinemachineTransposer>().m_XDamping = 1;

		cinemachineVirtualCam.GetCinemachineComponent<CinemachineComposer>().m_TrackedObjectOffset = trackedObjectOffset[(int)
		PhotonNetwork.LocalPlayer.CustomProperties["currentCar"]];

		cinemachineVirtualCam.GetCinemachineComponent<CinemachineComposer>().m_HorizontalDamping = 0.5f;
		cinemachineVirtualCam.GetCinemachineComponent<CinemachineComposer>().m_VerticalDamping = 3.5f;
			
		Debug.Log("player instantiated");

	}


	public void PlayerRespawn()
	{
		Hashtable props = new Hashtable
        {
            {GameConstants.PLAYER_RESPAWN, true}
        };
        PhotonNetwork.LocalPlayer.SetCustomProperties(props);
	
	}

   

	
	public void LeftRoom()
	{
      
	  if (PhotonNetwork.InRoom)
      {
		if (PhotonNetwork.IsMasterClient)
        {
			if(PhotonNetwork.MasterClient.GetNext()!=null)
			{
				PhotonNetwork.SetMasterClient(PhotonNetwork.MasterClient.GetNext());
			}
            
        }
		 
        PhotonNetwork.LeaveRoom();
      }
	}


	#region PUN CALLBACKS

	 public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {	


		 //checks if any notification has arrived from the master client to start the game	 
		 if (changedProps.ContainsKey(GameConstants.PLAYER_LOADED_LEVEL))
         {
			 if(CheckAllPlayerLoadedLevel())
			 {
			   Debug.Log("all players loaded level");
			   startTime = PhotonNetwork.ServerTimestamp;
               startTimer = true;
			   countdown = startCountdown;

			 }
			
		 }

			 
		//checks if a player killl
        if (changedProps.ContainsKey(GameConstants.PLAYER_KILL))
         {
			 //checks if a player has been qualified
			 if( (bool)targetPlayer.CustomProperties[GameConstants.PLAYER_KILL])
			 {
				 UpdateLeaderBoard();
			 }
			
	     }//END_IF

		  if (changedProps.ContainsKey(GameConstants.PLAYER_RESPAWN))
         {
			
			//networkPlayers[(string)targetPlayer.CustomProperties["Id"]]
			foreach (KeyValuePair<string, PlayerManager> entry in networkPlayers) {
              
                if(entry.Value.isLocalPlayer&&entry.Value.id.Equals((string)targetPlayer.CustomProperties["Id"]))
                {
                   
				   //makes the draw of a point for the player to be spawn
	               int index = UnityEngine.Random.Range (0, map.GetComponent<MapManager>().spawnPoints.Length);
                   entry.Value.transform.position = map.GetComponent<MapManager>().spawnPoints[index].position;


				}
			    else if(!entry.Value.isLocalPlayer&&
				entry.Value.id.Equals((string)targetPlayer.CustomProperties["Id"]))
			    {
					entry.Value.gameObject.SetActive(true);
					entry.Value.GetComponent<PlayerHealth>().Restore();
				   
			    }
		       
		    }//END_FOREACH
			Debug.Log("Player respawn");
		 }

		
        if (!PhotonNetwork.IsMasterClient)
        {
            return;	
        }
    }
	

    public override void OnLeftRoom()
    {
        Debug.Log("player leave room");
        PhotonNetwork.Disconnect();
    }
	
	

	public override void OnMasterClientSwitched(Player newMasterClient)
    {
        if (PhotonNetwork.LocalPlayer.ActorNumber == newMasterClient.ActorNumber){}
		
		if (PhotonNetwork.IsMasterClient)
        {
		  Debug.Log("new master client");
		 
        }
		
    }
    public override void OnDisconnected(DisconnectCause cause)
    {
         Debug.Log("player disconnected");

         UnityEngine.SceneManagement.SceneManager.LoadScene("LobbyScene");	
                     	
    }

	#endregion


 int Compare( PlayerManager a, PlayerManager b ) {
  
  if (a.kills > b.kills) {
    if(a.isDead)
	{
	  return 1;
	}	
    return -1;
  }
  if (a.kills < b.kills) {
     if(b.isDead)
	{
	  return -1;
	}	
    return 1;
  }
  
  return 0;
}


    /// <summary>
    /// updates leaderboard panel
    /// </summary>
	public void UpdateLeaderBoard()
	{

         ClearLeaderBoard();
		 
         foreach (KeyValuePair<string, PlayerManager> entry in networkPlayers) {
             Debug.Log("add: "+entry.Value.name);
			 topPlayers.Add(entry.Value);
        
		       
		  }//END_FOREACH

		  topPlayers.Sort( Compare );

		  int i=0;

		foreach (PlayerManager player in topPlayers)
		{
			i+=1;

			if(localPlayer.GetComponent<PlayerManager>().view.ViewID.Equals(player.view.ViewID))
			{
				ranking = i;
			}
            
		    GameObject newRankedUser = Instantiate (rankedUserPref) as GameObject;

	
		    // newRankedUser.GetComponent<Room>().id = id;
		    newRankedUser.GetComponent<User>().name.text = player.name;
		 
		    newRankedUser.GetComponent<User>().ranking.text = i.ToString();
		
		    newRankedUser.GetComponent<User>().kills.text = player.kills.ToString();

		    newRankedUser.transform.parent = contentPlayers.transform;

		    newRankedUser.GetComponent<RectTransform> ().localScale = new Vector3 (1, 1, 1);

		    rankedUsers.Add(newRankedUser);
		

        }

	}

    /// <summary>
	/// Clears the leader board.
	/// </summary>
	public void ClearLeaderBoard()
	{
		foreach (GameObject player in rankedUsers)
		{
			Destroy (player);
		}

		rankedUsers.Clear ();
		topPlayers.Clear ();
	}



	
	//runs the countdown to the start of the race
    void RunCountdown()
	{
        if (!startTimer) return;
    
	    float countdown = TimeRemaining();

		
		hours = (int)(Mathf.Floor((countdown / 3600)));
        countdown = countdown % 3600;
        minutes = (int)(Mathf.Floor((countdown / 60)));
        seconds = (int)countdown % 60;
        string time =  minutes + ":" + seconds;
	    
		if(!gameStarted)
		{
			GameCanvas.instance.txtCountdown.text = string.Format("Starts in {0} seconds",  countdown.ToString("n0"));	
		}
		else
		{
		   GameCanvas.instance.txtCountdown.text = string.Format("Game ends in {0} ",  time);
		
		}
		
	    if (countdown > 0.0f) return;

        OnTimerEnds();
	}

	void Update()
	{
		RunCountdown();
	}

	 private float TimeRemaining()
     {
        int timer = PhotonNetwork.ServerTimestamp - startTime;
        return countdown - timer / 1000f;
     }

	private void OnTimerEnds()
    {
		if(gameStarted)
		{
			startTimer = false;
		    localPlayer.GetComponent<PlayerHealth>().isDead = true;
		    GameCanvas.instance.OpenLeaderBoard();
		    GameCanvas.instance.OpenEndLobby();
			
		}
		else
		{
			gameStarted = true;
			countdown = endCountdown;
			//Timer Completed
            foreach (KeyValuePair<string, PlayerManager> entry in networkPlayers)
	        {

			   if(entry.Value.isLocalPlayer)
			   {
			     entry.Value.SetState("gamming");
				 GameCanvas.instance.OpenApresentation(map.GetComponent<MapManager>().levelName,
				 map.GetComponent<MapManager>().levelDescription);
				break;
			   }
			
		    }//END_FOREACH
			

		}
       
           
    }

	
////////////////////////////////////////////////////////////////////////////////////////////////////////////////
/////////////////////////////////////////////SYNCHRONIZATION METHODS////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	 private bool CheckAllPlayerLoadedLevel()
    {
            foreach (Player p in PhotonNetwork.PlayerList)
            {
                object playerLoadedLevel;

                if (p.CustomProperties.TryGetValue(GameConstants.PLAYER_LOADED_LEVEL, out playerLoadedLevel))
                {
                    if ((bool) playerLoadedLevel)
                    {
                        continue;
                    }
                }

                return false;
            }

            return true;
        }

////////////////////////////////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////////////////


}//END_CLASS
}//END_NAMESPACE