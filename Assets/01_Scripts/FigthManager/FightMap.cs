using System.Collections.Generic;
using UnityEngine;

public class FightMap : MonoBehaviour
{
    [Header("Allies")]
    public List<SpawnPoint> allyPoints;

    [Header("Enemies")]
    public List<SpawnPoint> enemyPoints;
}