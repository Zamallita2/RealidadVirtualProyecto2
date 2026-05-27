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

    [Header("Cleanup")]
    [SerializeField] private string fightUnitTag = "FightUnit";

    [Header("Managers")]
    private WaveManager waveManager;
    private TurnManager turnManager;
    private AdventurerManager adventurerManager;

    // Aldea actual que inició la pelea (para actualizar vidas/victorias y equipo).
    private Aldea currentAldea;
    private readonly List<AventureroData> currentAldeaTeamSnapshot = new();

    public List<AdventurerData> currentParty = new();
    private List<UnitStats> allySlots = new();
    private List<UnitStats> aliveEnemies = new();
    private bool isFightActive;
    private bool isTransitioning;
    private readonly List<GameObject> spawnedFightObjects = new();

    public bool IsFightActive => isFightActive;

    // Para que, en el futuro, se pueda condicionar el comportamiento por tipo de aldea.
    public TipoAldea CurrentAldeaType =>
        currentAldea != null ? currentAldea.tipoAldea : default;

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
        StartFight(party, null);
    }

    public void StartFight(List<AdventurerData> party, Aldea aldea)
    {
        if(waveManager == null || !waveManager.HasConfiguredRooms())
        {
            Debug.LogError("FightManager: no hay salas configuradas en WaveManager.");
            return;
        }

        currentAldea = aldea;
        currentAldeaTeamSnapshot.Clear();
        if(currentAldea != null &&
            currentAldea.aventureros != null)
        {
            foreach(AventureroData member in
                currentAldea.aventureros)
            {
                currentAldeaTeamSnapshot.Add(CloneMember(member));
            }
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
        CleanupFightObjects();
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
        spawnedFightObjects.Clear();

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
            TryAssignFightTag(obj);
            spawnedFightObjects.Add(obj);

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
            TryAssignFightTag(obj);
            spawnedFightObjects.Add(obj);

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
        FadeImage.Instance.Mostrar();
        yield return new WaitForSeconds(waveTransitionDelay);
        FadeImage.Instance.Ocultar();

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
        if(waveManager.ShouldPartyRetreat(currentAldea, currentParty))
        {
            RetreatFromDungeon();
        }
        else
        {
            if(!waveManager.EnterNextRoom())
            {
                OnDungeonComplete();
                return;
            }

            StartCoroutine(RoomTransitionCoroutine());   
        }
    }

    IEnumerator RoomTransitionCoroutine()
    {
        isTransitioning = true;
        turnManager.StopCombat();

        SyncPartyFromFight();

        Debug.Log("GANASTE");
        FadeImage.Instance.Mostrar();
        yield return new WaitForSeconds(roomTransitionDelay);
        FadeImage.Instance.Ocultar();

        isTransitioning = false;

        if(!isFightActive)
            yield break;

        TryStartNextWave();
    }

    void OnDungeonComplete()
    {
        SyncPartyFromFight();

        ApplyAldeaResult(win: true);
        adventurerManager?.OnFightWon(currentParty);

        DeactivateFight();

        Debug.Log("Venciste completamente a la mazmorra");
    }
    void RetreatFromDungeon()
    {
        SyncPartyFromFight();

        ApplyAldeaResult(win: true);
        adventurerManager?.OnFightWon(currentParty);

        DeactivateFight();

        Debug.Log("Se retiraron");
    }
    void LoseFight()
    {
        SyncPartyFromFight();

        ApplyAldeaResult(win: false);
        DeactivateFight();

        adventurerManager?.OnFightLost();

        Debug.Log("PERDISTE");
    }

    void ApplyAldeaResult(bool win)
    {
        if(currentAldea == null)
            return;

        if(win)
        {
            List<AventureroData> survivors =
                BuildSurvivorTeam();

            // Ganar implica actualizar victorias y reemplazar el equipo.
            currentAldea.ActualizarEquipo(
                survivors,
                ganaron: true
            );

            return;
        }

        // En la pérdida, asumimos que todos han muerto.
        currentAldea.TodosMurieron();
    }

    List<AventureroData> BuildSurvivorTeam()
    {
        List<AventureroData> result = new();

        int count = Mathf.Min(
            currentAldeaTeamSnapshot.Count,
            currentParty.Count
        );

        for(int i = 0; i < count; i++)
        {
            AdventurerData fightMember =
                currentParty[i];

            if(!fightMember.isAlive)
                continue;

            AventureroData snapshot =
                currentAldeaTeamSnapshot[i];

            AventureroData cloned = CloneMember(snapshot);
            cloned.prefab = fightMember.prefab;
            cloned.nivel = fightMember.level;

            result.Add(cloned);
        }

        return result;
    }

    static AventureroData CloneMember(AventureroData member)
    {
        if(member == null)
            return new AventureroData();

        return new AventureroData
        {
            nombre = member.nombre,
            rol = member.rol,
            prefab = member.prefab,
            nivel = member.nivel
        };
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

    void TryAssignFightTag(GameObject obj)
    {
        if(obj == null)
            return;

        if(string.IsNullOrWhiteSpace(fightUnitTag))
            return;

        try
        {
            obj.tag = fightUnitTag;
        }
        catch
        {
            // Tag doesn't exist in the project yet; ignore.
        }
    }

    void CleanupFightObjects()
    {
        // Destroy only what this FightManager spawned to avoid deleting other scene objects.
        for(int i = spawnedFightObjects.Count - 1; i >= 0; i--)
        {
            GameObject obj = spawnedFightObjects[i];
            if(obj != null)
                Destroy(obj);
        }

        spawnedFightObjects.Clear();
        allySlots.Clear();
        aliveEnemies.Clear();
    }
    public void HandleLevels(bool isWave)
    {
        float chance = waveManager.GetNextRoomDrop(); // 0-100

        foreach (var adv in currentParty)
        {
            for (int i = 0; i < 3; i++)
            {
                if (Random.Range(0f, 100f) <= chance)
                {
                    adv.level++;

                    if (!isWave)
                        adv.level++;

                    ApplyMidFightLevelUp(adv, isWave);
                }
            }
        }
    }
    void ApplyMidFightLevelUp(AdventurerData adv, bool isWave)
    {
        float oldMax = adv.maxHealth;
        float hpPercent = adv.currentHealth / oldMax;

        AdventurerManager.ApplyLevelScaling(
            adv.level,
            ref adv.maxHealth,
            ref adv.strength,
            ref adv.speed
        );

        adv.currentHealth = adv.maxHealth * hpPercent;
    }
}
