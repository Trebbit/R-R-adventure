using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HEAVYART.Racing.Netcode
{
    public class RespawnController : MonoBehaviour
    {
        public void Respawn(bool restore)
        {
            //Find last checkpoint
            int checkPointIndex = GetComponent<RaceStatusController>().currentCheckPointIndex;
            Transform currentCheckPoint = GameManager.Instance.checkPointControl.checkpoints[checkPointIndex].GetRespawnPoint();

            //Move car to last checkpoint
            transform.position = currentCheckPoint.position;
            transform.rotation = currentCheckPoint.rotation;

            //Stop velocity
            Rigidbody rigidbodyComponent = transform.GetComponent<Rigidbody>();
            rigidbodyComponent.velocity = Vector3.zero;
            rigidbodyComponent.angularVelocity = Vector3.zero;

            //Fully restore after destroy 
            if (restore == true)
            {
                GetComponent<ModifiersControlSystem>().ClearModifiers();
                GetComponent<HealthController>().Restore();
                GetComponent<NitroController>().Restore();
            }
        }
    }
}