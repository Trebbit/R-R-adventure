using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using Hashtable = ExitGames.Client.Photon.Hashtable;

namespace MultiplayerShooter
{
public class  CustonGun {

	public int  id;

	public string name;

	//total ammo in Weapon Pent
	public int ammoInPent;

	// current ammo in inventory
	public int currentAmmo;

	// max ammunition for Pents
	public int ammoPerPents;

	// max ammo in inventory
	public int maxAmmo;
	
	public bool onInventory;

	public bool isDropped;
}
}
