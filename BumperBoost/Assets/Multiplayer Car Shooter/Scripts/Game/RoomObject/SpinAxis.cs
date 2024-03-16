using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpinAxis : MonoBehaviour {

	public float rotateSpeed = 900f;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {

		transform.RotateAround (transform.position,Vector3.up,rotateSpeed*Time.deltaTime);
		
	}
}
