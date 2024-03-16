using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace MultiplayerShooter
{
public class Teleport : MonoBehaviour
{
    void OnTriggerEnter(Collider colisor)
	{
	 
        GameManager gameManager = GameObject.Find("GameManager").GetComponent<GameManager>() as GameManager;
		if (colisor.gameObject.tag.Equals("Player")&&
        colisor.gameObject.GetComponentInParent<PlayerManager>().isLocalPlayer)
		{

            colisor.gameObject.transform.position = gameManager.currentCheckPoint.position;
		
		}
	}

	  void OnCollisionEnter(Collision colisor)
	{
	 
        GameManager gameManager = GameObject.Find("GameManager").GetComponent<GameManager>() as GameManager;
		if (colisor.gameObject.tag.Equals("Player")&&
        colisor.gameObject.GetComponentInParent<PlayerManager>().isLocalPlayer)
		{

            colisor.gameObject.transform.position = gameManager.currentCheckPoint.position;
		
		}
	}
}
}
