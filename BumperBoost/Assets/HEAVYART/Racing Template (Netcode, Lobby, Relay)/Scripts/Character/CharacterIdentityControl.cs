using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace HEAVYART.Racing.Netcode
{
    public class CharacterIdentityControl : NetworkBehaviour
    {
        //Since we use scene objects as players, we need some tool to recognize if its player or bot 

        public bool isPlayer { get; private set; }
        public bool isBot { get; private set; }

        new public bool IsLocalPlayer => isPlayer && IsOwner;
        new public bool IsOwner => spawnParameters.Value.ownerID == NetworkManager.Singleton.LocalClientId;
        new public ulong OwnerClientId => spawnParameters.Value.ownerID;

        [HideInInspector]
        public NetworkVariable<CharacterSpawnParameters> spawnParameters = new NetworkVariable<CharacterSpawnParameters>();

        private CharacterSpawnParameters serverBufferedSpawnParameters;

        public override void OnNetworkSpawn()
        {
            if (IsServer)
                spawnParameters.Value = serverBufferedSpawnParameters;
        }

        public void SetSpawnParameters(CharacterSpawnParameters spawnParameters)
        {
            //It's server side
            //Prepare local copy of spawn parameters, before network spawn
            //Server will use it to initialize spawnParameters at OnNetworkSpawn()  
            //After OnNetworkSpawn() server will change this object's owner to OwnerClientId
            serverBufferedSpawnParameters = spawnParameters;
        }

        private void Awake()
        {
            AICarBehaviour carAIBehaviour = GetComponent<AICarBehaviour>();
            PlayerCarBehaviour carPlayerBehaviour = GetComponent<PlayerCarBehaviour>();

            if (carAIBehaviour != null && carAIBehaviour.enabled == true)
                isBot = true;

            if (carPlayerBehaviour != null && carPlayerBehaviour.enabled == true)
            {
                isPlayer = true;
                isBot = false;
            }
        }
    }
}