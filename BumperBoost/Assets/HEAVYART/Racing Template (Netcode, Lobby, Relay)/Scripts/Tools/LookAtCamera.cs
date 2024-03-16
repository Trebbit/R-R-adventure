using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HEAVYART.Racing.Netcode
{
    public class LookAtCamera : MonoBehaviour
    {
        private Transform mainCamera;

        private void Start()
        {
            mainCamera = Camera.main.transform;
        }

        void FixedUpdate()
        {
            //Reversed
            Vector3 lookDirection = transform.position - mainCamera.position;

            transform.rotation = Quaternion.LookRotation(lookDirection, Vector3.up);
        }
    }
}
