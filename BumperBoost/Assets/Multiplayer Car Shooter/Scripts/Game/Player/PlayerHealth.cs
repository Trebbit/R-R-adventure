using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Photon.Pun;

/// <summary>
/// Player health.
/// </summary>
namespace MultiplayerShooter
{
public class PlayerHealth : MonoBehaviour {

    [Header("Health current value")]
	public float health = 100;

    [Header("Max health value")]
	public float maxHealth = 100;

	[Header("Explosion Prefab")]
    public GameObject explosionPref;  // set in inspector.
   
	bool damaged;

	public bool isDead;

	public AudioClip damageAudioClip;

	public AudioClip deathAudioClip;

	public AudioClip colectedAudioClip;
	


	
    

	void Awake()
	{
		isDead = false;
	}

	// Update is called once per frame
	void Update () {

		damaged = false;

	}

	

	
	public void AddHealth(float _amount)
	{
		  if(health + _amount<= maxHealth)
		  {
			 PlaycolectedAudioClip();
			 health = health + _amount;
		     GameCanvas.instance.txtLocalPlayerHealth.text = health.ToString();

		  }
		  else
		  {
			  health = maxHealth; 
		  }
		
	}

	public void PlaycolectedAudioClip()
	{

	   if (!GetComponent<AudioSource> ().isPlaying )
		{
		
		  GetComponent<AudioSource>().PlayOneShot(colectedAudioClip);

		}


	}

	
    /// <summary>
    /// RPC Take Damage method
    /// </summary>
    /// <param name="_shooterID"></param>
    /// <param name="_damageValue"></param>
	[PunRPC]public void TakeDamage (string _shooterID, float _damageValue)
	{
		
		if(!_shooterID.Equals(GetComponent<PlayerManager>().view.ViewID.ToString()))
		{
           
           damaged = true;
		   PlayDamageSound();
			
		   if (health - _damageValue > 0)
		   {
				health = health - _damageValue;
		   }
		   else
		   {    if(!isDead)
				{
		            
					Death (_shooterID);
				}
			}
		
		    if(view.IsMine)
		    {
			
			  GameCanvas.instance.healthSlider.value = health;
		      GameCanvas.instance.txtHealth.text = "HP " + health + " / " 
					+ maxHealth;
					
		      GameCanvas.instance.damaged = true;

		    }
		}
	}
	
	
	public void Death (string _shooterID)
    {
	  
	  health = 0;

	  isDead = true;

	  Instantiate (explosionPref, transform.position, transform.rotation);
			
      
	  if (view.IsMine) 
	  {
		  GameManager gameManager = GameObject.Find("GameManager").GetComponent<GameManager>() as GameManager;
		  if( gameManager.networkPlayers.ContainsKey(_shooterID))
		  {
			 gameManager.networkPlayers[_shooterID].view.RPC("AddKill", RpcTarget.All);
		  }
		  
          StartCoroutine ("deathCutScene"); 
		
	  }
	  else
	  {
		  //hide network remote player
		gameObject.SetActive(false);
	    
	  }
	 
    }
	
	IEnumerator deathCutScene( )
	{ 
		
		yield return new WaitForSeconds(3f);
		GameOver ();
		GameCanvas.instance.GameOver();
		
	}


	public void GameOver()
	{
	    GetComponent<PlayerManager>().DisablePlayer();
	}

	public void Restore()
	{
		isDead = false;
		health = maxHealth;
		if(GetComponent<PlayerManager>().isLocalPlayer)
		{
			GameCanvas.instance.healthSlider.value = health;
		    GameCanvas.instance.txtHealth.text = "HP " + health + " / " 
					                                       + maxHealth;

		}
	

	}


	//---------------AUDIO METHODS--------
	public void PlayDeathSound()
	{
	   if (!GetComponent<AudioSource> ().isPlaying)
	    {
		  GetComponent<AudioSource>().PlayOneShot(deathAudioClip);
		}
		
	}
		
	public void PlayDamageSound()
	{

	   if (!GetComponent<AudioSource> ().isPlaying )
		{
		
		  GetComponent<AudioSource>().PlayOneShot(damageAudioClip);

		}
	}

	public PhotonView view{ get{ return GetComponent<PhotonView>(); }}
		
}//END_CLASS
}//END_NAMESPACE
