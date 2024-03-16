using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;


namespace MultiplayerShooter
{
public class BulletController :MonoBehaviourPun, IPunObservable {

	 /*********************** DEFAULT VARIABLES ***************************/
	[Header("Default variables :")]
    public string shooterID;

	[HideInInspector]
    public string ownersName = "";   // name of player that launched bullet

	public bool isLocalBullet;	

	Rigidbody myRigidybody;
    
	[HideInInspector]
	public bool canMove;

	Vector3 target;

	Vector3 target_position;
		
	public float bulletSpeed;  // set in inspector.

	public float offsetY;
	
	public float damage = 10;

	Vector3 latestPos  = Vector3.zero;
	   
   /****************************************************************************/

  /***********************  EFFECTS VARIABLES **********************************/

	[Header("Effects variables :")]

	public GameObject explosionPref;  // set in inspector.


    private void Awake ()
    {
		myRigidybody = GetComponent<Rigidbody> ();
	    object[] data = photonView.InstantiationData;
        shooterID =(string) data[0];
		if(!photonView.IsMine)
	    {
          offsetY = -1f;
	    }
      
    }

    void Update()
    {
	   if (!photonView.IsMine)
       {
          //Update remote player (smooth this, this looks good, at the cost of some accuracy)
          transform.position = latestPos;
	   }
    }
	// Update is called once per frame
	void FixedUpdate () {

        MoveBullet();
	
	}

	void MoveBullet()
	{
      if (canMove)
		 {
			myRigidybody.AddForce (bulletSpeed * target);
		 }
	}
		

	public void Shoot(Vector3 _target )
	{
		   canMove = true;
		   target = _target;
		   target_position = new Vector3(target.x,target.y,target.z);
		  
	}

public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
			 //We own this player: send the others our data
            stream.SendNext(transform.position);
            
        }
        else
        {
            //Network player, receive data
            latestPos = (Vector3)stream.ReceiveNext();
			latestPos =  new Vector3 (latestPos.x, latestPos.y+offsetY, latestPos.z);
           
        }
    }

		
	void OnTriggerEnter(Collider colisor)
     {
			
		
		if (colisor.gameObject.tag.Equals("Player")&&
		colisor.gameObject.GetComponentInParent<PlayerManager>().view.ViewID.ToString() != shooterID)
		{
		   
			 var playerHealth = colisor.gameObject.GetComponentInParent<PlayerHealth>();
		     //sends notification to the server with the shooter ID and target ID
		     playerHealth.view.RPC("TakeDamage", RpcTarget.All, shooterID,damage);
		    
			
            //instantiate an explosion effect
		    Instantiate (explosionPref, transform.position, transform.rotation);
			
			 Destroy (gameObject);
		 
		}//END_IF
			
			
	 }
	


}//END_CLASS
}//END_NAMESPACE