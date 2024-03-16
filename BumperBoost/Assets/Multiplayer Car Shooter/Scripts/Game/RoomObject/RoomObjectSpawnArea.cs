using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerShooter
{
[RequireComponent(typeof (BoxCollider))]
public class RoomObjectSpawnArea : MonoBehaviour
{

	//maximum range of area
	public float areaSize ;
	
    public float maxAreaSize = 100f;

	public BoxCollider areaBoxCollider;

	public float powerUpAmount = 1;

	public float maxAmount = 10;

	float maxX;
	float minX;

	float maxZ;
	float minZ;

	float randX;

	float randZ;


    // Start is called before the first frame update
    void Start()
    {
	 areaBoxCollider = GetComponent<BoxCollider>();
	 areaBoxCollider.isTrigger = true;
	 
	 areaSize = areaBoxCollider.size.x;

    }

    // Update is called once per frame
    void Update()
    {
        
    }


	public Vector3 GetRandomAreaPosition()
	{
		minX =   transform.position.x - areaSize/2;

		maxX =  transform.position.x + areaSize/2;

		minZ =   transform.position.z - areaSize/2;

		maxZ =   transform.position.z + areaSize/2;

		randX = Random.Range (minX, maxX);

		randZ = Random.Range (minZ, maxZ);

		Vector3 position = new Vector3 (randX, transform.position.y,randZ);


		return position;
	}
}//END_CLASS
}//END_NAMESPACE
