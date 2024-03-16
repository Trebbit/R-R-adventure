using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace HEAVYART.Racing.Netcode
{
    public class CarColorController : NetworkBehaviour
    {
        public List<MeshColorAdjuster> meshColorAdjusters = new List<MeshColorAdjuster>();

        private NetworkVariable<Color> synchronizedBotColor = new NetworkVariable<Color>(writePerm: NetworkVariableWritePermission.Owner);
        private CharacterIdentityControl identityControl;

        private void Awake()
        {
            for (int i = 0; i < meshColorAdjusters.Count; i++)
            {
                meshColorAdjusters[i].Initialize();
            }

            identityControl = GetComponent<CharacterIdentityControl>();
        }

        public override void OnNetworkSpawn()
        {
            //Apply player color
            if (identityControl.isPlayer == true)
            {
                //Get color from spawn parameters
                Color playerColor = identityControl.spawnParameters.Value.color;
                ApplyColor(playerColor);
            }

            //Apply bot color
            if (identityControl.isBot == true)
            {
                if (identityControl.IsOwner == true)
                {
                    //Bot owner side

                    //Get and apply random color
                    Color botColor = SettingsManager.Instance.ai.GetRandomColor();
                    ApplyColor(botColor);

                    //Synchronize selected color
                    synchronizedBotColor.Value = botColor;
                }
                else
                {
                    //Get and apply synchronized color
                    Color botColor = synchronizedBotColor.Value;
                    ApplyColor(botColor);
                }
            }
        }

        private void ApplyColor(Color color)
        {
            for (int i = 0; i < meshColorAdjusters.Count; i++)
            {
                meshColorAdjusters[i].ApplyColor(color);
            }
        }
    }
}
