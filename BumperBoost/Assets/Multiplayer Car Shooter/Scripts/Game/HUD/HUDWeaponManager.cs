using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MultiplayerShooter
{
public class HUDWeaponManager : MonoBehaviour {


	public static  HUDWeaponManager instance;

    [Header("Weapons Container")]
	public GameObject weaponsContainer;

    [Header("Slot Prefab")]
	public GameObject slotPref;

    [Header("Empty Sprite Prefab")]
	public GameObject emptySpritePrefab;

    [Header("Current Weapon Panel")]
	public Canvas currentWeaponPanel;

    [Header("Current Weapon Image")]
	public Image currentWeaponImage;

    [Header("Text Current Ammo In Pent")]
	public Text txtCurrentAmmoInPent;
    
	[Header("Current Ammo Slider")]
	public Slider currentAmmoSlider;

	[HideInInspector]
	public int currentGun;

	public ArrayList inventorySlots = new ArrayList();

   


	// Use this for initialization
	void Start () {

		if (instance == null) {

			DontDestroyOnLoad (this.gameObject);
			instance = this;

		}
		else
		{
			Destroy(this.gameObject);
		}

	}

	
	/// <summary>
	/// Inicializes the weapon inventory.
	/// </summary>
	public void InicializeInventory ()
	{

		GameManager gameManager = GameObject.Find("GameManager").GetComponent<GameManager>() as GameManager;
		
		foreach (KeyValuePair<int, CustonGun> entry in gameManager.localPlayer.GetComponentInChildren<Gun>().guns)
		{
            //  check that it is not the player's current primary weapon
			if (!gameManager.localPlayer.GetComponentInChildren<Gun>().
			guns[currentGun].Equals (entry.Value)) {
				
				//spawn slot weapon game object
				GameObject slotInstance = Instantiate (slotPref) as GameObject;
			    slotInstance.transform.parent = weaponsContainer.transform;
				slotInstance.GetComponent<RectTransform> ().localScale = new Vector3 (1, 1, 1);

				Slot new_slot = slotInstance.GetComponent<Slot> ();

                //checks if the player has the weapon in the guns inventory
				if (!gameManager.localPlayer.GetComponentInChildren<Gun>().guns[currentGun].Equals (entry.Value)) {
                    
					//add gun on slot
					new_slot.SetWeapon(entry.Value.id);

				} else {
					//set with empty sprite 
					new_slot.SetEmptyWeapon ();
				}

				inventorySlots.Add (slotInstance.GetComponent<Slot> ());

			}
		
			}

			
	}
	
	/// <summary>
	/// Adds the weapon in inventory.
	/// </summary>
	/// <param name="_weapon">Weapon.</param>
	public void AddWeaponInInventory(int _gunIndex)
	{

		foreach(Slot  s in inventorySlots)
		{
			if (s.isFree)
			{
				s.SetWeapon (_gunIndex);
				break;
			}
		}

	

	}
	
	/// <summary>
	/// Removes the weapon from inventory.
	/// </summary>
	/// <param name="_weapon">Weapon.</param>
	public void RemoveWeaponFromInventory(int _weapon)
	{


		foreach(Slot  s in inventorySlots)
		{
			if (!s.isFree)
			{
				if (s.currentGun.Equals (_weapon)) 
				{
					s.ClearSlot ();
				}
			}
		}
			
	}

	public void SetSecudarySlot(int _weapon)
	{
		foreach(Slot  s in inventorySlots)
		{
			if (!s.isFree)
			{
				if (s.currentGun.Equals (_weapon)) 
				{
					s.SelectThisWeapon();
				}
			}
		}
	}


	

	/// <summary>
	/// Switchs the weapon.
	/// </summary>
	/// <param name="_weapon">Weapon.</param>
	public void SwitchWeapon(int _currentGun)
	{
	    currentGun = _currentGun;
		GameManager gameManager = GameObject.Find("GameManager").GetComponent<GameManager>() as GameManager;
		gameManager.localPlayer.GetComponentInChildren<Gun>().currentGun = _currentGun;

		SetUpCurrentWeapon(_currentGun);
	}


	/// <summary>
	/// configures the current weapon to be empty. means the player has no weapon
	/// </summary>
	public void SetUpEmptyCurrentweapon()
	{
		//clear current weapon image
		currentWeaponImage.sprite = emptySpritePrefab.GetComponent<SpriteRenderer> ().sprite;
		
		//clear current ammo Text
		txtCurrentAmmoInPent.text = string.Empty;
        
		//desable current ammo slider
		currentAmmoSlider.GetComponent<Canvas> ().enabled = false;

	}


	/// <summary>
	///set the current weapon of the player
	/// </summary>
	/// <param name="_weapon">Weapon.</param>
	public void SetUpCurrentWeapon(int _currentGun)
	{

		GameManager gameManager = GameObject.Find("GameManager").GetComponent<GameManager>() as GameManager;
		
	    // set current weapon image
	    currentWeaponImage.sprite = gameManager.localPlayer.GetComponentInChildren<Gun>().
		spriteWeapons[_currentGun].GetComponent<SpriteRenderer> ().sprite;
        
		//enable slider
		currentAmmoSlider.GetComponent<Canvas> ().enabled = true;

        //set slider max value
		currentAmmoSlider.maxValue = 	gameManager.localPlayer.GetComponentInChildren<Gun>().
		guns[_currentGun].ammoPerPents;

		 //set slider current value
		currentAmmoSlider.value = gameManager.localPlayer.GetComponentInChildren<Gun>().
		guns[_currentGun].ammoInPent;


		/**********************************display on hud current ammo value****************************/
		int currentAmmoMinusammoInPent = 0;

		currentAmmoMinusammoInPent = gameManager.localPlayer.GetComponentInChildren<Gun>().
		guns[_currentGun].currentAmmo - 	gameManager.localPlayer.GetComponentInChildren<Gun>().
		guns[_currentGun].ammoInPent;

		if (currentAmmoMinusammoInPent < 0)
		{
			currentAmmoMinusammoInPent =	gameManager.localPlayer.GetComponentInChildren<Gun>().
			guns[_currentGun].ammoInPent;
		}

		txtCurrentAmmoInPent.text = gameManager.localPlayer.GetComponentInChildren<Gun>().
		guns[_currentGun].ammoInPent.ToString()+" / "+
				currentAmmoMinusammoInPent.ToString();
		
		/*****************************************************************************************/

		
	}

	/// <summary>
	/// Updates the current ammo info.
	/// </summary>
	public void UpdateCurrentAmmoInfo()
	{

		GameManager gameManager = GameObject.Find("GameManager").GetComponent<GameManager>() as GameManager;
		
	    // set current weapon image
		currentWeaponImage.sprite = gameManager.localPlayer.GetComponentInChildren<Gun>().
		spriteWeapons[currentGun].GetComponent<SpriteRenderer> ().sprite;

		currentAmmoSlider.GetComponent<Canvas> ().enabled = true;

        //set slider max value
		currentAmmoSlider.maxValue = gameManager.localPlayer.GetComponentInChildren<Gun>().
		guns[currentGun].ammoPerPents;

        //set slider current value
		currentAmmoSlider.value =gameManager.localPlayer.GetComponentInChildren<Gun>().
		guns[currentGun].ammoInPent;


        /**********************************display on hud current ammo value ****************************/
		int currentAmmoMinusammoInPent = 0;

		currentAmmoMinusammoInPent = gameManager.localPlayer.GetComponentInChildren<Gun>().
		guns[currentGun].currentAmmo - gameManager.localPlayer.GetComponentInChildren<Gun>().
		guns[currentGun].ammoInPent;

		if (currentAmmoMinusammoInPent < 0) {
			currentAmmoMinusammoInPent = gameManager.localPlayer.GetComponentInChildren<Gun>().
			guns[currentGun].ammoInPent;
		}
        
		txtCurrentAmmoInPent.text = gameManager.localPlayer.GetComponentInChildren<Gun>().
		guns[currentGun].ammoInPent.ToString () + " / " +
		currentAmmoMinusammoInPent.ToString ();
	   /*****************************************************************************************/

	}

	public void UpdatSecundaryAmmoInfo(int weaponID, string ammoInPent)
	{
		foreach(Slot  s in inventorySlots)
		{
			if (!s.isFree)
			{
				if (s.currentGun.Equals (weaponID)) 
				{
					s.txtAmmoInPent.text = ammoInPent;
					s.weaponsBulletSlider.value = s.weaponsBulletSlider.maxValue;
		           

				}
			}
		}

	}

	public void ClearTxtammoInPent()
	{
		txtCurrentAmmoInPent.text = string.Empty;
	}
	

}
}
