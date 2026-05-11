using System.Collections.Generic;
using Unity.Android.Gradle.Manifest;
using UnityEngine;

public class FightManager : MonoBehaviour
{
    [Header("Map")]
    public FightMap currentMap;

    [Header("Allies")]
    public List<GameObject> adventurerPrefabs;

    [Header("Managers")]
    private WaveManager waveManager;
    private TurnManager turnManager;

    private List<UnitStats> aliveEnemies = new();
    private List<UnitStats> aliveAllies = new();
    public List<UnitStats> GetAllies(
    EnumFigthList.Faction faction)
    {
        return faction == EnumFigthList.Faction.Ally
            ? aliveAllies
            : aliveEnemies;
    }

    public List<UnitStats> GetEnemies(
        EnumFigthList.Faction faction)
    {
        return faction == EnumFigthList.Faction.Ally
            ? aliveEnemies
            : aliveAllies;
    }
    void Awake()
    {
        waveManager=GetComponent<WaveManager>();
        turnManager=GetComponent<TurnManager>();
    }

    public void StartFight()
    {
        SpawnAllies();
        StartNextWave();

        turnManager.StartTurns(
            aliveAllies,
            aliveEnemies
        );
        turnManager.RefreshTurnOrder();
    }

    void SpawnAllies()
    {
        aliveAllies.Clear();

        for(int i = 0; i < adventurerPrefabs.Count; i++)
        {
            if(i >= currentMap.allyPoints.Count)
                break;

            SpawnPoint point = currentMap.allyPoints[i];

            GameObject obj =
                Instantiate(
                    adventurerPrefabs[i],
                    point.transform.position,
                    point.transform.rotation
                );

            UnitStats stats = obj.GetComponent<UnitStats>();

            stats.lineNumber = point.lineNumber;

            stats.faction = EnumFigthList.Faction.Ally;

            aliveAllies.Add(stats);
        }
    }

    void StartNextWave()
    {
        List<GameObject> waveToSpawn =
            waveManager.StartNextWave();

        if(waveToSpawn == null)
        {
            WinFight();
            return;
        }

        if(waveToSpawn.Count == 0)
        {
            StartNextWave();
            return;
        }

        SpawnWave(waveToSpawn);
    }
    void SpawnWave(List<GameObject> wave)
    {
        aliveEnemies.Clear();

        for(int i = 0; i < wave.Count; i++)
        {
            if(i >= currentMap.enemyPoints.Count)
                break;

            SpawnPoint point = currentMap.enemyPoints[i];

            GameObject obj =
                Instantiate(
                    wave[i],
                    point.transform.position,
                    point.transform.rotation
                );

            UnitStats stats = obj.GetComponent<UnitStats>();

            stats.lineNumber = point.lineNumber;

            stats.faction = EnumFigthList.Faction.Enemy;

            aliveEnemies.Add(stats);
        }
    }

    public void UnitDied(UnitStats unit)
    {
        aliveEnemies.Remove(unit);
        aliveAllies.Remove(unit);

        turnManager.RemoveUnit(unit);

        if(aliveEnemies.Count <= 0)
        {
            StartNextWave();
            return;
        }

        if(aliveAllies.Count <= 0)
        {
            LoseFight();
            return;
        }
    }

    void WinFight()
    {
        Debug.Log("GANASTE");
    }

    void LoseFight()
    {
        Debug.Log("PERDISTE");
    }
}