using UnityEngine;
using System.Collections;
namespace MultiplayerShooter
{
public class spin : MonoBehaviour {

	public Vector3 spinXYZ;

	public bool offSpin;

	public bool isFront;

    public Transform spawnDirtPoint;

	bool waitingDirt;

    //bool  waitingDirtDelay;

    //public float dirtDelay =7;

    public GameObject dirtParticlePref;

	public   LayerMask hitLayer;

	private  string cTag;

	public bool onHit;
	public float offset;





	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () 
	{
        if(!offSpin && GetComponentInParent<PlayerManager>().GetState().Equals("gamming"))
		{
			transform.Rotate(spinXYZ*Time.deltaTime);

			RaycastHit hit;
       
		    onHit = Physics.Raycast(transform.position + new Vector3(0, offset,0), -Vector3.up,out hit,
			 Mathf.Infinity, hitLayer);

		    if(onHit)
		    {
			  // Debug.Log("colison with: "+hit.collider.gameObject.name);
			  
			   cTag = hit.collider.tag.ToLower(); 
			   //Debug.Log("cTag: "+cTag);
				 
			   if(cTag == "ground")
		        {
			     // StartCoroutine ("SpawnDirtParticles");
				 if(GetComponentInParent<PlayerManager>().h!=0 && !waitingDirt)
					{
						waitingDirt = true;
						SpawnDirtParticles();
					}
		        }
		    }
       
		    if(waitingDirt && GetComponentInParent<PlayerManager>().h.Equals(0))
			{
				waitingDirt = false;
			}

			
	
		}
		
	}

	 void SpawnDirtParticles()
	{
		if(GetComponentInParent<PlayerManager>().acceleration >50 )
		{
			Debug.Log("Spawn particles");
			GameObject particle = Instantiate (dirtParticlePref, spawnDirtPoint.position,
			   dirtParticlePref.transform.rotation);
			  
			particle.transform.parent =  spawnDirtPoint;

		}
		
	}

/*
	 private IEnumerator SpawnDirtParticles()
	{

 
		if (waitingDirtDelay)
		{
			yield break;
		}

		waitingDirtDelay = true;

		if(GetComponentInParent<PlayerManager>().acceleration >50 && GetComponentInParent<PlayerManager>().h!=0)
		{
			 
			  GameObject particle = Instantiate (dirtParticlePref, spawnDirtPoint.position,
			   dirtParticlePref.transform.rotation);

			  particle.transform.parent =  spawnDirtPoint;

		}
		

		// wait 1 seconds and continue
		yield return new WaitForSeconds(dirtDelay);

		waitingDirtDelay = false;

	}
	*/
}
}
