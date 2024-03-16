using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

namespace MultiplayerShooter
{
public class RoomObject : MonoBehaviour
{
  
	public string name;
	public string id;
	public int weapon_id;
	public ObjectType objectType;
	public int amount;
	[SerializeField] public float respawnDuration = 1f ;
	public RoomObjectSpawnArea myPowerUpSpawnArea;
	public bool isWaiting;
	public float maxDistance = 1f;
	/***********************  EFFECTS VARIABLES **********************************/

	[Header("Effects variables :")]

	public GameObject sparkPref;  // set in inspector.


	
	void OnTriggerEnter(Collider colisor)
    {
	 
	  if (colisor.gameObject.tag.Equals("Player")&&
		colisor.gameObject.GetComponentInParent<PlayerManager>().view.IsMine)
		{
		    PickUpItem(); 
		}
	
	}
	
	 

	
	/// <summary>
	/// Picks up item.
	/// </summary>
	public void PickUpItem()
	{
		GameManager gameManager = GameObject.Find("GameManager").GetComponent<GameManager>() as GameManager;

		
		switch (objectType)
		{
		    case ObjectType.HEALTH:
			if(	gameManager.localPlayer.GetComponent<PlayerHealth>().health < 
			gameManager.localPlayer.GetComponent<PlayerHealth>().maxHealth)
			{
				GameCanvas.instance.ShowInfoText("HEALTH PICKUP");
			    gameManager.localPlayer.GetComponent<PlayerHealth>().AddHealth (amount);

				 //instantiate an explosion effect
		         GameObject sparkParticle = Instantiate (sparkPref, transform.position, transform.rotation);
	
		         sparkParticle.transform.parent = gameManager.localPlayer.GetComponent<PlayerManager>().model.transform;

				 PhotonNetwork.Destroy(gameObject);


			}
			else
			{
				GameCanvas.instance.ShowInfoText("FULL HEALTH");
			}
			
			break;
			 case ObjectType.POWERUP:

			 if(gameManager.localPlayer.GetComponent<PlayerManager>().nitro.Equals(0) )
			{
				 GameCanvas.instance.ShowInfoText("NITRO PICKUP");
			     gameManager.localPlayer.GetComponent<PlayerManager>().AddNitro ();

				  //instantiate an explosion effect
		         GameObject sparkParticle = Instantiate (sparkPref, transform.position, transform.rotation);
	
		         sparkParticle.transform.parent = gameManager.localPlayer.GetComponent<PlayerManager>().model.transform;

				 PhotonNetwork.Destroy(gameObject);

			}
			else
			{
				GameCanvas.instance.ShowInfoText("FULL NITRO");
			}
			
			break;

		    case ObjectType.WEAPON:

			
			 
			 foreach (KeyValuePair<int, CustonGun> entry in 
				gameManager.localPlayer.GetComponentInChildren<Gun>().guns) {
				
				if (entry.Key.Equals(weapon_id)) {

					if (entry.Value.isDropped)
					{
						entry.Value.isDropped = false;
					}//END_IF

                      //checks whether the addition exceeds the maximum ammo limit
			          if (!gameManager.localPlayer.GetComponentInChildren<Gun>().guns[weapon_id].currentAmmo.
			                 Equals(gameManager.localPlayer.GetComponentInChildren<Gun>().guns[weapon_id].maxAmmo))
			          {

						 GameCanvas.instance.ShowInfoText("WEAPON PICKUP");
				         gameManager.localPlayer.GetComponentInChildren<Gun>(). AddWeaponInInventory (weapon_id,amount);
						 
						 //instantiate an explosion effect
		                 GameObject sparkParticle = Instantiate (sparkPref, transform.position, transform.rotation);
	
		                 sparkParticle.transform.parent = gameManager.localPlayer.GetComponent<PlayerManager>().model.transform;

						  PhotonNetwork.Destroy(gameObject);


			          }
					  else
					  {
						 GameCanvas.instance.ShowInfoText("FULL AMMO");  
					  }
			
						
				
				}//END_IF
				
				}//END_FOREACH
			
			break;

		}//END_SWITCH

	

	}

		[PunRPC] public void SetUpObject(string _name,string _view_id,string _id)
		{
			if(view.ViewID.ToString().Equals(_view_id))
			{
				name = _name;
				//myPowerUpSpawnArea = _sapawnArea;
			    id =_id;
				RoomObjectManager.instance.spawnedPowerUpList.Add (this);

			}
		
		}

	public PhotonView view{ get{ return GetComponent<PhotonView>(); }}

		
}//END_CLASS
}//END_NAMESPACE
