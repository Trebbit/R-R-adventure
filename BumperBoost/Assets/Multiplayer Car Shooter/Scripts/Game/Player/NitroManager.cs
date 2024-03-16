using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace MultiplayerShooter
{
public class NitroManager : MonoBehaviour
{
    public int index;
    
    public float nitro;

    public ParticleSystem[] nitroParticles;

    public bool onNitro;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(onNitro)
	    {
		 
	        foreach(ParticleSystem nitroParticle in nitroParticles)
		      {
		       
				// Setting the particle to enabled
                var emission = nitroParticle.emission;
                emission.enabled = true;
			
		      }
	    }
	    else
	    {
	        foreach(ParticleSystem nitroParticle in nitroParticles)
		      {
		         
				 // Setting the particle to enabled
                var emission = nitroParticle.emission;
                emission.enabled = false;
				
		      }
	  }
        
    }

   
}
}
