using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MultiplayerShooter
{
public class RoomObjectSlot : MonoBehaviour
{
	public RoomObject powerUp;

	public bool isFree;

	public Image powerUpImage;

	public Text txtAmmoInPent;

	public Slider weaponsBulletSlider;

	public GameObject emptySpritePrefab;

	public void SetWeapon(RoomObject _powerUp)
	{
		isFree = false;
		this.powerUp = _powerUp;
		GetComponent<Button> ().interactable = true;
	
	}
		

	public void GetPowerUp(){}

	public void ClearSlot()
	{
		isFree = true;
		this.powerUp = null;
		GetComponent<Button> ().interactable = false;
		powerUpImage.sprite = emptySpritePrefab.GetComponent<SpriteRenderer> ().sprite;

	}

}//END_CLASS
}//END_NAMESPACE