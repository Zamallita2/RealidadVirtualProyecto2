using System.Collections.Generic;
using UnityEngine;

public class FightManager : MonoBehaviour
{
    [Header("Map")]
    public FightMap currentMap;

    [Header("Allies")]
    public List<GameObject> adventurerPrefabs;

    [Header("Enemy Waves")]
    public List<GameObject> wave1;
    public List<GameObject> wave2;
    public List<GameObject> wave3;

    private int currentWave = 0;

    private List<UnitStats> aliveEnemies = new();
    private List<UnitStats> aliveAllies = new();
    private List<UnitStats> attackOrder = new();
    private bool waitingForNextTurn = false;
    private UnitStats currentUnit;
    public void NextTurn()
    {
        waitingForNextTurn = false;

        StartTurn();
    }

    public void StartFight()
    {
        SpawnAllies();
        StartNextWave();

        BuildAttackOrder();
        StartTurn();
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
        currentWave++;

        List<GameObject> waveToSpawn = GetCurrentWave();

        if(waveToSpawn == null)
        {
            WinFight();
            return;
        }

        // Si la wave está vacía, pasa automáticamente
        if(waveToSpawn.Count == 0)
        {
            StartNextWave();
            return;
        }

        SpawnWave(waveToSpawn);
    }

    List<GameObject> GetCurrentWave()
    {
        switch(currentWave)
        {
            case 1:
                return wave1;

            case 2:
                return wave2;

            case 3:
                return wave3;
        }

        return null;
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
    void BuildAttackOrder()
    {
        attackOrder.Clear();

        attackOrder.AddRange(aliveAllies);
        attackOrder.AddRange(aliveEnemies);

        attackOrder.Sort((a, b) =>
        {
            // Más velocidad primero
            int speedCompare =
                b.speed.CompareTo(a.speed);

            if(speedCompare != 0)
                return speedCompare;

            // Prioridad aliados
            if(a.faction == EnumFigthList.Faction.Ally &&
            b.faction == EnumFigthList.Faction.Enemy)
            {
                return -1;
            }

            if(a.faction == EnumFigthList.Faction.Enemy &&
            b.faction == EnumFigthList.Faction.Ally)
            {
                return 1;
            }

            return 0;
        });
    }

    public void UnitDied(UnitStats unit)
    {
        aliveEnemies.Remove(unit);
        aliveAllies.Remove(unit);

        attackOrder.Remove(unit);

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
    void StartTurn()
    {
        BuildAttackOrder();

        currentUnit = GetNextUnit();

        // Todos actuaron
        if(currentUnit == null)
        {
            StartNewRound();

            currentUnit = GetNextUnit();

            if(currentUnit == null)
                return;
        }

        Debug.Log("Turno de: " + currentUnit.name);

        currentUnit.hasActedThisRound = true;

        currentUnit.TakeTurn();
    }
    UnitStats GetNextUnit()
    {
        foreach(UnitStats unit in attackOrder)
        {
            if(unit == null)
                continue;

            if(unit.hasActedThisRound)
                continue;

            return unit;
        }

        return null;
    }
    void StartNewRound()
    {
        Debug.Log("NUEVA RONDA");

        foreach(UnitStats unit in attackOrder)
        {
            if(unit == null)
                continue;

            unit.hasActedThisRound = false;
        }

        BuildAttackOrder();
    }
    public List<UnitStats> GetTargets(
    UnitStats caster,
    EnumFigthList.TargetType targetType)
    {
        List<UnitStats> result = new();

        List<UnitStats> allies =
            caster.faction == EnumFigthList.Faction.Ally
            ? aliveAllies
            : aliveEnemies;

        List<UnitStats> enemies =
            caster.faction == EnumFigthList.Faction.Ally
            ? aliveEnemies
            : aliveAllies;

        switch(targetType)
        {
            case EnumFigthList.TargetType.RandomEnemy:

                result.Add(GetFrontPriorityRandom(enemies));
                break;

            case EnumFigthList.TargetType.LowestHealthEnemy:

                result.Add(GetLowestHealth(enemies));
                break;

            case EnumFigthList.TargetType.LowestHealthAlly:

                result.Add(GetLowestHealth(allies));
                break;

            case EnumFigthList.TargetType.AllEnemies:

                result.AddRange(enemies);
                break;
            case EnumFigthList.TargetType.RandomEnemyWithoutStatus:

                result.Add(GetEnemyWithoutStatus(enemies));
                break;
        }

        result.RemoveAll(x => x == null);

        return result;
    }
    UnitStats GetEnemyWithoutStatus(List<UnitStats> units)
    {
        List<UnitStats> possibleTargets = new();

        foreach(UnitStats unit in units)
        {
            if(unit == null)
                continue;

            if(unit.currentStatus != EnumFigthList.StatusEffect.None)
                continue;

            possibleTargets.Add(unit);
        }

        if(possibleTargets.Count == 0)
            return null;

        possibleTargets.Sort((a, b) =>
            a.lineNumber.CompareTo(b.lineNumber));

        int maxIndex =
            Mathf.Min(2, possibleTargets.Count);

        return possibleTargets[
            Random.Range(0, maxIndex)
        ];
    }
    UnitStats GetFrontPriorityRandom(List<UnitStats> units)
    {
        if(units.Count == 0)
            return null;

        units.Sort((a, b) =>
            a.lineNumber.CompareTo(b.lineNumber));

        // Más chance a frontliners
        int maxIndex =
            Mathf.Min(2, units.Count);

        return units[Random.Range(0, maxIndex)];
    }
    UnitStats GetLowestHealth(List<UnitStats> units)
    {
        UnitStats lowest = units[0];

        foreach(UnitStats u in units)
        {
            if(u.currentHealth < lowest.currentHealth)
            {
                lowest = u;
            }
        }

        return lowest;
    }
    public void EndTurn()
    {
        waitingForNextTurn = true;
        NextTurn();
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