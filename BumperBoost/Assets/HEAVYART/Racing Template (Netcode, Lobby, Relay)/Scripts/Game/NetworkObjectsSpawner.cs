using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace HEAVYART.Racing.Netcode
{
    public class NetworkObjectsSpawner : NetworkBehaviour
    {
        public List<Transform> spawnPoints;

        private NetworkObjectsControl userControl;

        private IEnumerator Start()
        {
            userControl = GetComponent<NetworkObjectsControl>();

            //Wait for game to start
            while (GameManager.Instance.gameState != GameState.WaitingForCountdown) yield return 0;

            //Spawn bots after countdown
            if (IsServer)
                for (int i = 0; i < GameManager.Instance.botsCount; i++)
                {
                    yield return 0;

                    int randomBotIndex = Random.Range(0, SettingsManager.Instance.ai.configs.Count);

                    //Spawn bot
                    SpawnAIServerRpc(randomBotIndex);
                }
        }
        public void RespawnLocalPlayer()
        {
            userControl.localPlayer.GetComponent<RespawnController>().Respawn(true);
        }

        [ServerRpc(RequireOwnership = false)]
        public void SpawnPlayerServerRpc(CharacterSpawnParameters characterSpawnParameters)
        {
            Vector3 spawnPoint = GetRandomPlayerSpawnPoint();
            GameObject player = Instantiate(SettingsManager.Instance.player.configs[characterSpawnParameters.modelIndex].playerPrefab, spawnPoint, Quaternion.identity);

            player.GetComponent<CharacterIdentityControl>().SetSpawnParameters(characterSpawnParameters);

            //Spawn player with server ownership
            player.GetComponent<NetworkObject>().Spawn(true);

            //Change ownership
            //It's possible to spawn through SpawnWithOwnership() but it always spawns in (0,0,0)
            player.GetComponent<NetworkObject>().ChangeOwnership(characterSpawnParameters.ownerID);
        }

        [ServerRpc(RequireOwnership = false)]
        public void SpawnAIServerRpc(int modelIndex)
        {
            Vector3 spawnPoint = GetRandomAISpawnPoint();
            GameObject ai = Instantiate(SettingsManager.Instance.ai.configs[modelIndex].botPrefab, spawnPoint, Quaternion.identity);

            //Custom spawn parameters
            ai.GetComponent<CharacterIdentityControl>().SetSpawnParameters(new CharacterSpawnParameters()
            {
                ownerID = NetworkManager.Singleton.LocalClientId
            });

            //Spawn bot
            ai.GetComponent<NetworkObject>().Spawn(true);
        }

        public Vector3 GetRandomPlayerSpawnPoint()
        {
            return GetFreeSpawnPoint(userControl.playerSceneObjects);
        }

        public Vector3 GetRandomAISpawnPoint()
        {
            return GetFreeSpawnPoint(userControl.allCharacters);
        }

        private Vector3 GetFreeSpawnPoint(List<NetworkObject> characters)
        {
            if (characters.Count == 0)
            {
                //Randomly choose one of first 3 spawn points to start
                int range = 3;
                return spawnPoints[Random.Range(0, range)].position;
            }

            //Check all spawn points
            for (int i = 0; i < spawnPoints.Count; i++)
            {
                bool isFree = true;

                //Check all characters
                for (int j = 0; j < characters.Count; j++)
                {
                    if (characters[j] == null) continue;

                    float distanceToCharacter = (characters[j].transform.position - spawnPoints[i].position).magnitude;

                    //Point is occupied
                    if (distanceToCharacter < 1f)
                    {
                        isFree = false;
                        break;
                    }
                }

                if (isFree == true)
                {
                    return spawnPoints[i].position;
                }
            }

            Debug.LogError("No free spawn points found.");
            return Vector3.zero;
        }
    }
}
