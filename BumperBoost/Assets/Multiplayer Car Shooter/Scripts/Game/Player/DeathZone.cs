using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;

namespace MultiplayerShooter
{
    public class DeathZone : MonoBehaviour
    {
        string shooterID = "deathZone";
        public float damage = 10;
        void OnTriggerEnter(Collider colisor)
        {
            Debug.Log("death");

            if (colisor.gameObject.tag.Equals("Player"))
            {

                Debug.Log("deat2h");
                var playerHealth = colisor.gameObject.GetComponentInParent<PlayerHealth>();
                //sends notification to the server with the shooter ID and target ID
                playerHealth.view.RPC("TakeDamage", RpcTarget.All, shooterID, damage);
                var playerM = colisor.gameObject.GetComponentInParent<PlayerManager>();
                if (playerM.isLocalPlayer)
                {
                    colisor.gameObject.transform.position = Vector3.zero;
                    colisor.gameObject.transform.position = Vector3.zero;
                    playerM.sphere.position= Vector3.zero;
                    //playerM.sphere.constraints = RigidbodyConstraints.FreezeAll;

                }

                //instantiate an explosion effect
                //Instantiate(explosionPref, transform.position, transform.rotation);

                //Destroy(gameObject);

            }//END_IF


        }
    }

}