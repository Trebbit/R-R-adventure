using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Text.RegularExpressions;

namespace MultiplayerShooter
{
public class CustomButtonEvent : MonoBehaviour {

	public delegate void OnActionPress( GameObject unit, bool state );
	public event OnActionPress onPress;
	EventTrigger eventTrigger;
	public GameManager gameManager;


	void Start () {

		eventTrigger = this.gameObject.GetComponent<EventTrigger>();
		AddEventTrgger( OnPointDown, EventTriggerType.PointerDown);
		AddEventTrgger(OnPointUp, EventTriggerType.PointerUp);
		//AddEventTrgger(onClick, EventTriggerType.PointerClick);
	}


	void AddEventTrgger( UnityAction action, EventTriggerType triggerType ){

		EventTrigger.TriggerEvent trigger = new EventTrigger.TriggerEvent();
		trigger.AddListener( (eventData) => action());

		EventTrigger.Entry entry = new EventTrigger.Entry() { callback = trigger, eventID = triggerType };
		eventTrigger.triggers.Add(entry);

	}


	void OnPointDown(){

		Debug.Log("user down:");
        if(!gameObject.name.Equals("FireButton"))
		{
		 gameManager.localPlayer.GetComponent<PlayerManager>().EnableKey (gameObject.name);
		}
		else
		{
		  gameManager.localPlayer.GetComponentInChildren<Gun>(). m_Shoot = true;
		}
         
		

		if( onPress != null  ){
			
			onPress(this.gameObject, true);

		}else{
			Debug.Log("Event null");
		}

	}

	void OnPointUp(){


		Debug.Log("user Up:");
		if(!gameObject.name.Equals("FireButton"))
		{
		  
          gameManager.localPlayer.GetComponent<PlayerManager>().DisableKey (gameObject.name);
		
		}
		else
		{
		  gameManager.localPlayer.GetComponentInChildren<Gun>(). m_Shoot = false;
		}

		
		if( onPress != null  ){
			Debug.Log("OnPointUp");
			onPress(this.gameObject, false);
			
		}
	}

}
}
