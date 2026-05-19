using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FightManager : MonoBehaviour
{
    [Header("Map")]
    public FightMap currentMap;

    [Header("Transition")]
    [SerializeField] private float waveTransitionDelay = 5f;
    [SerializeField] private float roomTransitionDelay = 10f;

    [Header("Managers")]
    private WaveManager waveManager;
    private TurnManager turnManager;
    private AdventurerManager adventurerManager;

    private List<AdventurerData> currentParty = new();
    private List<UnitStats> allySlots = new();
    private List<UnitStats> aliveEnemies = new();
    private bool isFightActive;
    private bool isTransitioning;

    public bool IsFightActive => isFightActive;

    public List<UnitStats> GetAllies(EnumFigthList.Faction faction)
    {
        return GetLivingUnits(
            faction == EnumFigthList.Faction.Ally
                ? allySlots
                : aliveEnemies
        );
    }

    public List<UnitStats> GetEnemies(EnumFigthList.Faction faction)
    {
        return GetLivingUnits(
            faction == EnumFigthList.Faction.Ally
                ? aliveEnemies
                : allySlots
        );
    }

    void Awake()
    {
        waveManager = GetComponent<WaveManager>();
        turnManager = GetComponent<TurnManager>();
        adventurerManager = FindFirstObjectByType<AdventurerManager>();
    }

    public void StartFight(List<AdventurerData> party)
    {
        if(waveManager == null || !waveManager.HasConfiguredRooms())
        {
            Debug.LogError("FightManager: no hay salas configuradas en WaveManager.");
            return;
        }

        StopAllCoroutines();
        DeactivateFight();

        currentParty = party;
        isFightActive = true;
        isTransitioning = false;

        waveManager.PrepareForFight();

        SpawnAllies();

        if(!BeginCurrentRoom())
        {
            OnDungeonComplete();
            return;
        }

        turnManager.StartTurns(
            GetLivingUnits(allySlots),
            GetLivingUnits(aliveEnemies)
        );
    }

    public void DeactivateFight()
    {
        isFightActive = false;
        isTransitioning = false;
        StopAllCoroutines();
        turnManager?.StopCombat();
    }

    public bool OnTurnEnded()
    {
        if(!isFightActive || isTransitioning)
            return false;

        if(GetLivingUnits(aliveEnemies).Count <= 0)
        {
            StartCoroutine(WaveTransitionCoroutine());
            return false;
        }

        if(GetLivingUnits(allySlots).Count <= 0)
        {
            LoseFight();
            return false;
        }

        return true;
    }

    bool BeginCurrentRoom()
    {
        if(!waveManager.EnterNextRoom())
            return false;

        TryStartNextWave();
        return true;
    }

    void TryStartNextWave()
    {
        if(!isFightActive)
            return;

        while(true)
        {
            List<GameObject> waveToSpawn =
                waveManager.StartNextWave();

            if(waveToSpawn == null)
            {
                OnRoomCleared();
                return;
            }

            if(waveToSpawn.Count > 0)
            {
                SpawnWave(waveToSpawn);
                return;
            }
        }
    }

    void SpawnAllies()
    {
        allySlots.Clear();

        for(int i = 0; i < currentParty.Count; i++)
        {
            if(i >= currentMap.allyPoints.Count)
                break;

            AdventurerData data = currentParty[i];

            if(!data.isAlive)
            {
                allySlots.Add(null);
                continue;
            }

            SpawnPoint point = currentMap.allyPoints[i];

            GameObject obj = Instantiate(
                data.prefab,
                point.transform.position,
                point.transform.rotation
            );

            UnitStats stats = obj.GetComponent<UnitStats>();

            stats.ApplyFromData(data);
            stats.lineNumber = point.lineNumber;
            stats.faction = EnumFigthList.Faction.Ally;
            stats.partyIndex = i;

            UnitMovement movement = obj.GetComponent<UnitMovement>();

            if(movement != null)
            {
                obj.transform.rotation *= Quaternion.Euler(movement.modelRotationOffset);
            }

            allySlots.Add(stats);
        }
    }

    void SpawnWave(List<GameObject> wave)
    {
        aliveEnemies.Clear();

        for(int i = 0; i < wave.Count; i++)
        {
            if(i >= currentMap.enemyPoints.Count)
                break;

            GameObject prefab = wave[i];

            if(prefab == null)
                continue;

            SpawnPoint point = currentMap.enemyPoints[i];

            GameObject obj = Instantiate(
                prefab,
                point.transform.position,
                point.transform.rotation
            );

            UnitStats stats = obj.GetComponent<UnitStats>();

            stats.sourceEnemyPrefab = prefab;
            stats.lineNumber = point.lineNumber;
            stats.faction = EnumFigthList.Faction.Enemy;

            UnitMovement movement = obj.GetComponent<UnitMovement>();

            if(movement != null)
            {
                obj.transform.rotation *= Quaternion.Euler(movement.modelRotationOffset);
            }

            aliveEnemies.Add(stats);
        }

        if(!isFightActive)
            return;

        turnManager.BeginNewWave(
            GetLivingUnits(allySlots),
            GetLivingUnits(aliveEnemies)
        );
    }

    public void UnitDied(UnitStats unit)
    {
        if(!isFightActive || isTransitioning)
            return;

        if(unit.faction == EnumFigthList.Faction.Enemy)
        {
            waveManager.RemoveEnemyFromCurrentWave(unit.sourceEnemyPrefab);
            aliveEnemies.Remove(unit);
            turnManager.RemoveUnit(unit);
        }
        else
        {
            turnManager.RefreshTurnOrder();
        }
    }

    IEnumerator WaveTransitionCoroutine()
    {
        isTransitioning = true;
        turnManager.StopCombat();

        waveManager.ClearCurrentWave();

        yield return new WaitForSeconds(waveTransitionDelay);

        isTransitioning = false;

        if(!isFightActive)
            yield break;

        TryStartNextWave();
    }

    void OnRoomCleared()
    {
        if(!isFightActive)
            return;

        SyncPartyFromFight();
        waveManager.FinishCurrentRoom();

        if(!waveManager.EnterNextRoom())
        {
            OnDungeonComplete();
            return;
        }

        StartCoroutine(RoomTransitionCoroutine());
    }

    IEnumerator RoomTransitionCoroutine()
    {
        isTransitioning = true;
        turnManager.StopCombat();

        SyncPartyFromFight();

        Debug.Log("GANASTE");

        yield return new WaitForSeconds(roomTransitionDelay);

        isTransitioning = false;

        if(!isFightActive)
            yield break;

        TryStartNextWave();
    }

    void OnDungeonComplete()
    {
        SyncPartyFromFight();
        HealAllAllies();
        SyncPartyFromFight();

        adventurerManager?.OnFightWon(currentParty);

        DeactivateFight();

        Debug.Log("Venciste completamente a la mazmorra");
    }

    void LoseFight()
    {
        SyncPartyFromFight();

        DeactivateFight();

        adventurerManager?.OnFightLost();

        Debug.Log("PERDISTE");
    }

    void HealAllAllies()
    {
        foreach(UnitStats stats in allySlots)
        {
            if(!UnitStats.IsCombatReady(stats))
                continue;

            stats.currentHealth = stats.maxHealth;

            if(stats.partyIndex < 0 || stats.partyIndex >= currentParty.Count)
                continue;

            currentParty[stats.partyIndex].currentHealth = stats.maxHealth;
        }
    }

    void SyncPartyFromFight()
    {
        foreach(UnitStats stats in allySlots)
        {
            if(stats == null || stats.partyIndex < 0)
                continue;

            if(stats.partyIndex >= currentParty.Count)
                continue;

            stats.CopyToData(currentParty[stats.partyIndex]);
        }
    }

    static List<UnitStats> GetLivingUnits(List<UnitStats> units)
    {
        List<UnitStats> living = new();

        foreach(UnitStats unit in units)
        {
            if(UnitStats.IsCombatReady(unit))
                living.Add(unit);
        }

        return living;
    }
}
