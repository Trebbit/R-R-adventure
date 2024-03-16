using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using Hashtable = ExitGames.Client.Photon.Hashtable;


namespace MultiplayerShooter
{
public class Gun : MonoBehaviour {
    
	[Header("Current weapon index")]
	public int currentGun;
	//store all guns
	public Dictionary<int, CustonGun> guns = new Dictionary<int, CustonGun>();
	[HideInInspector]
	public bool isLocked = true;
	[Header("Bullet Prefabs")]
	public GameObject[] bulletPref;
	[Header("Spawn bullet Transforms")]
	public Transform[] spawnBulletTransf;
	[Header("UI Sprite Weapons")]
	public GameObject[] spriteWeapons;
	private float reloadTimer ;
	bool reloading;
	[HideInInspector]
	public bool  m_Shoot;
	[Header("Time Between Bullets")]
	public float[] timeBetweenBullets ;//delay among the shots
	[HideInInspector]
	public float timer;
	GameObject bullet;
	[Header("Crosshair")]
	public GameObject target;

	PlayerManager playerManager;
	
	/*********************** AUDIO VARIABLES ***************************/
    [Header("Audio Variables")]
	public AudioClip colectedAudioClip;

	public  AudioClip[] shootSound;
	
	public AudioClip reloadAudioClip;
	
	public AudioClip outOfAmmoAudioClip;
	
 /*********************** END GUN EFFECTS VARIABLES ***************************/

	
	// Use this for initialization
	void Start () {
		
		isLocked = true;
		SetGuns();
	    playerManager = GetComponentInParent<PlayerManager>();	
	
	}

    /// <summary>
    /// set player weapons
    /// </summary>
	public void SetGuns()
	{
		CustonGun gun = new CustonGun ();

		gun.id = 0;
			
		gun.name = "gun1";

		gun.ammoInPent = 0;

		gun.ammoPerPents = 30;

		gun.maxAmmo = 60;

		gun.currentAmmo = 0;
		
		gun.onInventory = true;
		
		guns.Add (gun.id, gun);

		CustonGun gun2 = new CustonGun ();

		gun2.id = 1;
			
		gun2.name = "gun2";
		
		gun2.onInventory = true;

		gun2.maxAmmo = 2;

		gun2.ammoInPent = 0;

		gun2.ammoPerPents = 1;

		gun2.currentAmmo = 0;
		
		guns.Add (gun2.id, gun2);

		CustonGun gun3 = new CustonGun ();

		gun3.id = 2;
			
		gun3.name = "gun3";
		
		gun3.onInventory = true;

		gun3.maxAmmo = 60;

		gun3.ammoInPent = 0;

		gun3.ammoPerPents = 30;

		gun3.currentAmmo = 0;
		
		guns.Add (gun3.id, gun3);
        if(GetComponentInParent<PlayerManager>().view.IsMine)
		{
		  HUDWeaponManager.instance.InicializeInventory();	
		  HUDWeaponManager.instance.SetUpCurrentWeapon(currentGun);
		}
		
	}

	void Update() {

	  timer += Time.deltaTime;   //increments the timer
	  
	  if(!GetComponentInParent<PlayerHealth>().isDead && GetComponentInParent<PlayerManager>().GetState().Equals("gamming"))
	  {
		Shoot(); //calls shoot method

		  if(GetComponentInParent<PlayerManager>().view.IsMine)
	        {
		       if (Input.GetKeyDown ("1"))
		        {
			      SwitchWeapon(0);
           
		        }
		        if (Input.GetKeyDown ("2"))
		        {

			      SwitchWeapon(1);

		        }
		        if (Input.GetKeyDown ("3"))
		        {
		          SwitchWeapon(2);

		        }
		        if (Input.GetKeyDown ("4"))
		        {
		          SwitchWeapon(3);

		        }
	        }//END_IF
	  }
	 
	
	}

	/// <summary>
	/// Swiths the gun.
	/// </summary>
	/// <param name="gun_index">Gun index.</param>
	public void SwitchWeapon(int _weapon_id)
	{
		int lastCurrentWeapon = currentGun;
        
	
		HUDWeaponManager.instance.SetSecudarySlot(_weapon_id);

		//set new weapon index
		currentGun = _weapon_id;
	
		HUDWeaponManager.instance.SwitchWeapon(currentGun);

	
	}
	
     /// <summary>
     /// manage player shooting
     /// </summary>
	 public void Shoot()
	 {
	   #if !UNITY_ANDROID  && !UNITY_IOS
	   if (!m_Shoot &&GetComponentInParent<PlayerManager>().view.IsMine &&
	    !GetComponentInParent<PlayerHealth>().isDead )
		{
			m_Shoot = Input.GetButton("Fire1");
		}
		#endif
	 
	   if(GetComponentInParent<PlayerManager>().view.IsMine)
	   {
	   if (m_Shoot && timer >= timeBetweenBullets[currentGun]
			&& Time.timeScale != 0) {
			
		  m_Shoot = false;
		  timer = 0f;  //reset the timer

		  if (GetAmmoInPent () > 0 ) {

			   if(currentGun.Equals(0))
		        {

					SpawnBullet(0); //spawn bullets
						  
				}
				if(currentGun.Equals(1))
		        {

					SpawnBullet(1); //spawn bullets
						  
				}
				 if(currentGun.Equals(2))
		        {
					SpawnBullet(2); //spawn bullets		  
				}
					 
			    SetCurrentAmmo (GetCurrentAmmo () - 1);

			    SetAmmoInPent (GetAmmoInPent () - 1);

				if(GetComponentInParent<PhotonView>().IsMine)
				{
				    HUDWeaponManager.instance.UpdateCurrentAmmoInfo();
				}
			   
		  }//END_IF
		  //without ammunition in paint but with ammo in inventory
			if (GetAmmoInPent () <= 0 && guns[currentGun].currentAmmo > 0 ) {

				ReloadWeapon ();

			}//END_IF

			if (guns[currentGun].currentAmmo <= 0) {

				PlayOutOfAmmoSound();
			}//ENDD_IF		
		  
	   }//END_IF
	 }
	 

	 }
	 
   /// <summary>
   /// spawn bullet
   /// </summary>
   /// <param name="_index"></param>
   public void SpawnBullet(int currentGun_index)
   {
   
    PlayShootSound(currentGun_index);

    object[] data = new object[]{ GetComponentInParent<PlayerManager>().view.ViewID.ToString()};
		
	GameObject bullet = PhotonNetwork.Instantiate (bulletPref[currentGun_index].name,
	GetComponentInParent<PlayerManager>().currentCar.GetComponent<ModelManager>().
	spawnBulletTransf[0].position
  ,bulletPref[currentGun_index].transform.rotation,0,data) as GameObject;
				
	
	Transform cameraTransform = Camera.main.transform;
	
	//if not bumb
	if(currentGun_index!=1)
	{
	 bullet.GetComponent<BulletController> ().Shoot (target.transform.TransformDirection(Vector3.forward));
	
	 GameObject bullet2 = PhotonNetwork.Instantiate (bulletPref[currentGun_index].name,
			GetComponentInParent<PlayerManager>().currentCar.GetComponent<ModelManager>().
	spawnBulletTransf[1].position,bulletPref[currentGun_index].transform.rotation,0,data) as GameObject;
	
	
	 bullet2.GetComponent<BulletController> ().Shoot (target.transform.TransformDirection(Vector3.forward));
	}
	else
	{// drop bomb

	  bullet.GetComponent<BombManager> ().Drop(target.transform.position);
	
	}
	//display effects throuw network
	GetComponentInParent<PlayerManager>().view.RPC("DisplayEffects", RpcTarget.All);
			 
			 
  }


   /// <summary>
	/// Reloads the weapon.
	/// </summary>
	void ReloadWeapon()
	{
		
		if (guns[currentGun].currentAmmo != 0 &&  GetComponentInParent<PhotonView>().IsMine)
	    {
		  HUDWeaponManager.instance.UpdateCurrentAmmoInfo();
		}

		if (guns[currentGun].currentAmmo - guns[currentGun].ammoPerPents >= 0) {

			//reload weapon pent
			SetAmmoInPent (guns[currentGun].ammoPerPents);
		}
	    else
		 {
			SetAmmoInPent (guns[currentGun].currentAmmo);
		 }

	}

	/// <summary>
	/// Gets the ammo in pent.
	/// </summary>
	/// <returns>The ammo in pent.</returns>
	public int GetAmmoInPent()
	{
		return guns[currentGun].ammoInPent;
	}


	/// <summary>
	/// 
	/// </summary>
	/// <param name="_ammo">Ammo.</param>
	public void SetAmmoInPent(int _ammo)
	{
		if (_ammo >= 0) {
			guns[currentGun].ammoInPent = _ammo;
		}
	}


	/// <summary>
	/// Gets the ammo in paint.
	/// </summary>
	/// <returns>The ammo in paint.</returns>
	public int GetCurrentAmmo()
	{
		return guns[currentGun].currentAmmo;
	}


	/// <summary>
	/// 
	/// </summary>
	/// <param name="_ammo">Ammo.</param>
	public void SetCurrentAmmo(int _ammo)
	{

		if (_ammo >= 0) {
			guns[currentGun].currentAmmo = _ammo;
		}

	}


	
	/// <summary>
	/// Adds the weapon in inventory.
	/// </summary>
	/// <param name="index">Index.</param>
	/// <param name="amount">Amount.</param>
	public void AddWeaponInInventory(int index,int amount)
	{
		 
		PlaycolectedAudioClip();
        
		// checks if the weapon is stored in the inventory
		if ( guns[index].onInventory) {

            //checks whether the addition exceeds the maximum ammo limit
			if (guns[index].currentAmmo + amount <= guns[index].maxAmmo) {
				guns[index].currentAmmo += amount;

				if(guns[index].ammoInPent.Equals(0))
				{

					guns[index].ammoInPent = guns[index].ammoPerPents;

				}
			}
			else
			{ 
			  guns[index].currentAmmo = guns[index].maxAmmo;
			}

            //verify it’s the main weapon
			if (currentGun.Equals (index)) 
			{
				//updates the on-screen hud
				HUDWeaponManager.instance.UpdateCurrentAmmoInfo ();
			}
			else
			{
			    int currentAmmoMinusammoInPent = 0;

		        currentAmmoMinusammoInPent =  guns[index].currentAmmo - guns[index].ammoInPent;

				string  currentAmmoInPent =  guns[index].ammoInPent+" / "+ currentAmmoMinusammoInPent.ToString ();
		
				HUDWeaponManager.instance.UpdatSecundaryAmmoInfo (index,currentAmmoInPent);
			}
			

		} 
		else 
		{// adds the new weapon to the inventory

			guns[index].onInventory = true;
			Debug.Log ("add weapon in inventory!!!");
			
			HUDWeaponManager.instance.AddWeaponInInventory (index);
			
		}


	}

	/// <summary>
	/// Removes the weapon from inventory.
	/// </summary>
	/// <param name="index">Index.</param>
	public void RemoveWeaponFromInventory(int index)
	{
		
	
			if (currentGun.Equals(index))
			{
				guns[index].currentAmmo = 0;//clear current ammo
				bool lastWeaponToRemove = true;
				
				foreach (KeyValuePair<int, CustonGun> entry in guns) {
				
				 if(!entry.Key.Equals(currentGun)&&!entry.Value.isDropped && entry.Value.onInventory)
					{
						lastWeaponToRemove = false;
						HUDWeaponManager.instance.RemoveWeaponFromInventory (entry.Value.id);
						
						HUDWeaponManager.instance.SetUpCurrentWeapon(entry.Value.id);
						break;
					}
				
				}
				
				

				if (lastWeaponToRemove)
				{
					HUDWeaponManager.instance.SetUpEmptyCurrentweapon ();
					RemoveCurrentWeapon ();

				} 	

			}
			guns [index].isDropped = true;
			guns  [index].onInventory = false;
			HUDWeaponManager.instance.RemoveWeaponFromInventory (index);


			
			//spawn dropped weapon here
			RoomObjectManager.instance. SpawnDroppedWeapon(transform.position,index);
			


	}


	/// <summary>
	/// Removes the current weapon.
	/// </summary>
	public void RemoveCurrentWeapon()
	{
		currentGun = -1;
	}
	
	

	//---------------AUDIO METHODS--------
	public void PlayOutOfAmmoSound()
	{
		GetComponent<AudioSource>().PlayOneShot(outOfAmmoAudioClip);
	}

	public void PlayReloadSound()
	{
		GetComponent<AudioSource>().PlayOneShot(reloadAudioClip);
	}

	public void PlayShootSound(int _index)
	{
	   
		GetComponent<AudioSource>().PlayOneShot(shootSound[_index]);
	}
	
	public void PlaycolectedAudioClip()
	{

	   if (!GetComponent<AudioSource> ().isPlaying )
		{
		
		  GetComponent<AudioSource>().PlayOneShot(colectedAudioClip);

		}


	}
	
	

}//END_CLASS
}//END_NAMESPACE