using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerShooter{
public class ModelManager : MonoBehaviour
{

   [Header("Max Acceleration")]
	public float maxAcceleration;
	
	public float regularAcceleration;

   public NitroManager nitroManager;

   public GameObject[] truckSmokeParticles;

   [Header("Weapons Transform")]
	public Transform[] spawnBulletTransf;

   public float minPitch = 0.05f;

   private float pitchFromCar;

   public float offset =.5f;

   public float reverseOffset = 1.2f;

   public AudioClip carEngineAudioclip;


 
}//END_CLASS
}//END_NAMESPACE
