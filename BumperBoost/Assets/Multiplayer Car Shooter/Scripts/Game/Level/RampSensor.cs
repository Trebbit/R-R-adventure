using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerShooter
{
public class RampSensor : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnTriggerEnter(Collider colisor)
    {
	 
	  if (colisor.gameObject.tag.Equals("Player")&&
		colisor.gameObject.GetComponentInParent<PlayerManager>().view.IsMine)
		{
            Debug.Log("colision with ramp");
		    colisor.gameObject.GetComponentInParent<PlayerManager>().onRamp = true;
            
		}
	
	}
}
}
