using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace MultiplayerShooter
{
public class MapManager : MonoBehaviour
{
    //stores the spawn points 
   public Transform[] spawnPoints;

    [Header("Current Map Name")]
   public string levelName;
   
   [Header("Map Description")]
   public string levelDescription;

}
}
