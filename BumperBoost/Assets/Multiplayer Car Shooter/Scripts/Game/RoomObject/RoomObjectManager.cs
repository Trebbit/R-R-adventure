using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Photon.Pun;

namespace MultiplayerShooter
{
public class RoomObjectManager : MonoBehaviourPun
{

	public static RoomObjectManager instance;

	public bool isWaiting;

	public RoomObjectSpawnArea[] PowerUpSpawnAreas;

	public GameObject[] PowerUpsPref;

	public ArrayList spawnedPowerUpList = new ArrayList();

	public ArrayList respawnPowerUpList = new ArrayList();


    // Start is called before the first frame update
    void Start()
    {
		if (instance == null)
		{
			DontDestroyOnLoad(this.gameObject);


			instance = this;

			spawnedPowerUpList = new ArrayList();

			respawnPowerUpList = new ArrayList();

		    if (PhotonNetwork.IsMasterClient)
            {
               SpawnAllPowerUps ();
            }
		

		}
		else
		{
			Destroy(this.gameObject);
		}
    }

  
	/// <summary>
	/// Spawns all power ups when the game starts.
	/// </summary>
	public void SpawnAllPowerUps()
	{
		
			foreach (RoomObjectSpawnArea a in PowerUpSpawnAreas) {

				int amount = (int)a.powerUpAmount;

				for (int k = 0; k < amount; k++) 
				{

				for (int i = 0; i < PowerUpsPref.Length; i++) {
					
					switch (PowerUpsPref [i].GetComponent<RoomObject> ().objectType) {

					case ObjectType.POWERUP:
						
						GameObject newPowerUp = PhotonNetwork.InstantiateSceneObject (PowerUpsPref [i].name,
				               a.GetRandomAreaPosition (), PowerUpsPref [i].transform.rotation) as GameObject;
						newPowerUp.GetComponent<RoomObject> ().myPowerUpSpawnArea = a;
					
						newPowerUp.transform.parent = a.gameObject.transform;
				
						newPowerUp.GetComponent<RoomObject> ().view.RPC("SetUpObject", RpcTarget.All,PowerUpsPref [i].name,newPowerUp.GetComponent<RoomObject> ().view.ViewID.ToString(),generateID ());
						
					break;

					case ObjectType.HEALTH:
						
						GameObject newHealth = PhotonNetwork.InstantiateSceneObject (PowerUpsPref [i].name,
				               a.GetRandomAreaPosition (), PowerUpsPref [i].transform.rotation) as GameObject;

						newHealth.GetComponent<RoomObject> ().myPowerUpSpawnArea = a;
	
						newHealth.transform.parent = a.gameObject.transform;
			
						newHealth .GetComponent<RoomObject> ().view.RPC("SetUpObject", RpcTarget.All,PowerUpsPref [i].name,newHealth.GetComponent<RoomObject> ().view.ViewID.ToString(),generateID ());
						
					break;

					case ObjectType.WEAPON:

						if (!PowerUpsPref [i].GetComponent<RoomObject> ().weapon_id.Equals (string.Empty))
						{
						    GameObject newWeapon  = PhotonNetwork.InstantiateSceneObject (PowerUpsPref [i].name,
				               a.GetRandomAreaPosition (), PowerUpsPref [i].transform.rotation) as GameObject;
							
						
							newWeapon.GetComponent<RoomObject> ().myPowerUpSpawnArea = a;
						
							newWeapon.transform.parent = a.gameObject.transform;
						
							newWeapon.GetComponent<RoomObject> ().view.RPC("SetUpObject", RpcTarget.All,PowerUpsPref [i].name,newWeapon.GetComponent<RoomObject> ().view.ViewID.ToString(),generateID ());
						}
						else
						{
							Debug.LogError ("weapon_id field is empty in "+PowerUpsPref [i].name +" prefab");
						}
					
					break;

					}//END_SWITCH
				}//END_FOR
					
			}//END_FOR

         }//END_FOREACH

	}


	
	




	/// <summary>
	/// Spawns the dropped weapon.
	/// </summary>
	/// <param name="_pos">Position.</param>
	/// <param name="_weapon">Weapon.</param>
	public void SpawnDroppedWeapon(Vector3 _pos, int _weapon)
	{
		for (int i = 0; i < PowerUpsPref.Length; i++) {


			if (PowerUpsPref [i].GetComponent<RoomObject>().weapon_id.Equals(_weapon)) 
			{
				GameObject newPowerUp = GameObject.Instantiate (PowerUpsPref [i],_pos, Quaternion.identity);


				newPowerUp.GetComponent<RoomObject> ().weapon_id = _weapon;
				newPowerUp.GetComponent<RoomObject> ().myPowerUpSpawnArea = null;

				newPowerUp.GetComponent<RoomObject> ().id = generateID ();

				spawnedPowerUpList.Add (newPowerUp);

			}

		}
	}

	//it generates a random id for the local player
	public string generateID()
	{
		string id = Guid.NewGuid().ToString("N");

		//reduces the size of the id
		id = id.Remove (id.Length - 15);

		return id;
	}
}//END_CLASS
}//END_NAMESPACE
