using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MultiplayerShooter
{
public class Slot : MonoBehaviour {

	public int currentGun;
	public bool isFree;
	public Image weaponsImage;
	public Text txtAmmoInPent;
	public Slider weaponsBulletSlider;
	public GameObject emptySpritePrefab;

	/// <summary>
	/// Sets the weapon.
	/// </summary>
	/// <param name="_weapon">Weapon.</param>
	public void SetWeapon(int _currentGun)
	{
		currentGun = _currentGun;
		GameManager gameManager = GameObject.Find("GameManager").GetComponent<GameManager>() as GameManager;
	

		isFree = false;
	    
		//actvate weapon slot
		GetComponent<Button> ().interactable = true;

		//set weapon image
		weaponsImage.sprite = gameManager.localPlayer.
		GetComponentInChildren<Gun>().spriteWeapons[_currentGun].GetComponent<SpriteRenderer> ().sprite; 
		
		//activate  weapon slider
		weaponsBulletSlider.GetComponent<Canvas> ().enabled = true;

		//set slider max value
		weaponsBulletSlider.maxValue = gameManager.localPlayer.GetComponentInChildren<Gun>().guns[_currentGun].ammoPerPents;
		
		//set slider current value
		weaponsBulletSlider.value = gameManager.localPlayer.GetComponentInChildren<Gun>().guns[_currentGun].ammoInPent;
		
		/**********************************display on hud current ammo value****************************/
		int currentAmmoMinusammoInPent = 0;
		currentAmmoMinusammoInPent =  gameManager.localPlayer.GetComponentInChildren<Gun>().guns[_currentGun].
		currentAmmo - gameManager.localPlayer.GetComponentInChildren<Gun>().guns[_currentGun].ammoInPent;
		txtAmmoInPent.text = gameManager.localPlayer.GetComponentInChildren<Gun>().
			guns[_currentGun].ammoInPent.ToString()+" / "+
				currentAmmoMinusammoInPent.ToString();
		/*************************************************************************************************/


	}


	/// <summary>
	/// Sets the empty weapon.
	/// </summary>
	public void SetEmptyWeapon()
	{
		isFree = true;

		GetComponent<Button> ().interactable = false;
		weaponsImage.sprite = emptySpritePrefab.GetComponent<SpriteRenderer> ().sprite;
		txtAmmoInPent.text = string.Empty;
		weaponsBulletSlider.GetComponent<Canvas> ().enabled = false;

	}

    /// <summary>
    /// called when player select the weapon attached to this slot
    /// </summary>
	public void SelectThisWeapon()
	{
		GameManager gameManager = GameObject.Find("GameManager").GetComponent<GameManager>() as GameManager;
		int lastGun = gameManager.localPlayer.GetComponentInChildren<Gun>().currentGun;
	    HUDWeaponManager.instance.SwitchWeapon(currentGun);
		HUDWeaponManager.instance.SetUpCurrentWeapon(currentGun);
		SetWeapon (lastGun);
		
	}

	public void ClearSlot()
	{
		isFree = true;
		GetComponent<Button> ().interactable = false;
		weaponsImage.sprite = emptySpritePrefab.GetComponent<SpriteRenderer> ().sprite;
		txtAmmoInPent.text = string.Empty;
		weaponsBulletSlider.GetComponent<Canvas> ().enabled = false;
	}
}
}
