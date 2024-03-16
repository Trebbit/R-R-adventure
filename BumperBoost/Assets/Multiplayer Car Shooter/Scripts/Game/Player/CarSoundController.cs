using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace MultiplayerShooter
{
public class CarSoundController : MonoBehaviour
{
    public AudioSource audioSource;
    public float minPitch;
    private float pitchFromCar;

    public PlayerManager playerManager;

    public float offset ;

    public float reverseOffset;
     
    // Start is called before the first frame update
    void Start()
    {
      
        
    }

    public void SetCarSoundEffects()
    {
      audioSource.clip = GetComponentInParent<PlayerManager>().currentCar.GetComponent<ModelManager>().carEngineAudioclip;
      audioSource.pitch = GetComponentInParent<PlayerManager>().currentCar.GetComponent<ModelManager>().minPitch;
      offset =  GetComponentInParent<PlayerManager>().currentCar.GetComponent<ModelManager>().offset;
      reverseOffset =  GetComponentInParent<PlayerManager>().currentCar.GetComponent<ModelManager>().reverseOffset;
      audioSource.Play();
     
    }
 
    // Update is called once per frame
    void Update()
    {
        if(playerManager.GetState().Equals("gamming"))
        {
          if(!playerManager.GetComponent<PlayerHealth>().isDead )
        {

            var acceleration = GetComponentInParent<PlayerManager>().acceleration;
            var regularAcceleration = GetComponentInParent<PlayerManager>().currentCar.GetComponent<ModelManager>().regularAcceleration;
            
            pitchFromCar = acceleration*offset/regularAcceleration;
           
            if(playerManager.acceleration >= 0)
            {
              if(pitchFromCar < minPitch)
                audioSource.pitch = minPitch;
              else
                audioSource.pitch = pitchFromCar;
            }
            else
            {
              audioSource.pitch = reverseOffset;
             
            }

        }
        else
        {
            audioSource.Stop();
        }

        }   
    }

}//END_CLASS
}//END_NAMESPACE
