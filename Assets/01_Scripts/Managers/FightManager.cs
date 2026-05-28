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
    [SerializeField] private float emptyRoomDelay = 0.2f;
    [SerializeField] private GameObject emptyRoomWarning;

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
    private readonly List<GameObject> spawnedAllyObjects = new();
    private readonly List<GameObject> spawnedEnemyObjects = new();

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

        foreach (var adv in currentParty)
        {
            StatCalculator.RecalculateWithHealthPreserved(adv);
        }

        StartCoroutine(AdvanceRoomsCoroutine(true));
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

    IEnumerator AdvanceRoomsCoroutine(bool isStart)
    {
        bool wasEmpty = false;

        while (true)
        {
            if (!waveManager.EnterNextRoom())
            {
                if (wasEmpty)
                {
                    FadeImage.Instance.Ocultar();
                    if (emptyRoomWarning != null) emptyRoomWarning.SetActive(false);
                    yield return new WaitForSeconds(0.2f);
                }
                OnDungeonComplete();
                yield break;
            }

            if (waveManager.IsCurrentRoomEmpty())
            {
                wasEmpty = true;
                FadeImage.Instance.Mostrar();
                if (emptyRoomWarning != null) emptyRoomWarning.SetActive(true);
                
                yield return new WaitForSeconds(emptyRoomDelay);
            }
            else
            {
                break;
            }
        }

        if (wasEmpty)
        {
            FadeImage.Instance.Ocultar();
            if (emptyRoomWarning != null) emptyRoomWarning.SetActive(false);
            yield return new WaitForSeconds(0.2f);
        }

        TryStartNextWave();

        if (isStart)
        {
            turnManager.StartTurns(
                GetLivingUnits(allySlots),
                GetLivingUnits(aliveEnemies)
            );
        }
    }

    void TryStartNextWave()
    {
        if(!isFightActive)
            return;

        while(true)
        {
            List<AdventurerSetup> waveToSpawn =
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
        spawnedAllyObjects.Clear();

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
            spawnedAllyObjects.Add(obj);

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

    void SpawnWave(List<AdventurerSetup> wave)
    {
        // SAFETY: Asegurarse de quitar el fog y el mensaje al iniciar una pelea real
        FadeImage.Instance.OcultarInstante();
        if (emptyRoomWarning != null) emptyRoomWarning.SetActive(false);

        aliveEnemies.Clear();
        spawnedEnemyObjects.Clear();

        for(int i = 0; i < wave.Count; i++)
        {
            if(i >= currentMap.enemyPoints.Count)
                break;

            AdventurerSetup setup = wave[i];

            if(setup == null || setup.prefab == null)
                continue;

            SpawnPoint point = currentMap.enemyPoints[i];

            GameObject obj = Instantiate(
                setup.prefab,
                point.transform.position,
                point.transform.rotation
            );

            TryAssignFightTag(obj);

            spawnedEnemyObjects.Add(obj);

            UnitStats stats =
                obj.GetComponent<UnitStats>();

            if(stats == null)
                continue;

            stats.sourceEnemyPrefab =
                setup.prefab;

            stats.lineNumber =
                point.lineNumber;

            stats.faction =
                EnumFigthList.Faction.Enemy;

            //--------------------------------
            // aplicar nivel enemigo
            //--------------------------------

            AdventurerData temp = new AdventurerData
            {
                prefab = setup.prefab,
                level = setup.level,

                maxHealth = stats.maxHealth,
                currentHealth = stats.maxHealth,
                strength = stats.strength,
                speed = stats.speed,
                isAlive = true
            };

            StatCalculator.Recalculate(temp);

            // Recalculate solo actualiza maxHealth; igualamos currentHealth
            // para que el enemigo arranque con la vida completa a su nivel.
            temp.currentHealth = temp.maxHealth;

            stats.ApplyFromData(temp);

            //--------------------------------

            UnitMovement movement =
                obj.GetComponent<UnitMovement>();

            if(movement != null)
            {
                obj.transform.rotation *=
                    Quaternion.Euler(
                        movement.modelRotationOffset
                    );
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
            // Eliminar la unidad aliada muerta del orden de turno
            // y reconstruir la lista sin ella.
            turnManager.RemoveUnit(unit);
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
            StartCoroutine(RoomTransitionCoroutine());   
        }
    }

    IEnumerator RoomTransitionCoroutine()
    {
        isTransitioning = true;
        turnManager.StopCombat();

        SyncPartyFromFight();

        if (!waveManager.HasRemainingRoomsWithEnemies())
        {
            isTransitioning = false;
            OnDungeonComplete();
            yield break;
        }

        Debug.Log("GANASTE");
        FadeImage.Instance.Mostrar();
        yield return new WaitForSeconds(roomTransitionDelay);
        FadeImage.Instance.Ocultar();

        isTransitioning = false;

        if(!isFightActive)
            yield break;

        yield return StartCoroutine(AdvanceRoomsCoroutine(false));
    }

    void OnDungeonComplete()
    {
        SyncPartyFromFight();

        ApplyAldeaResult(win: true);
        adventurerManager?.OnFightWon(currentParty);

        DeactivateFight();

        Debug.Log("Venciste completamente a la mazmorra");
        LosePanelUI.Instance?.InvokeLoseByWave();
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
        for(int i = spawnedEnemyObjects.Count - 1; i >= 0; i--)
        {
            GameObject obj = spawnedEnemyObjects[i];
            if(obj != null)
                Destroy(obj);
        }
        for(int i = spawnedAllyObjects.Count - 1; i >= 0; i--)
        {
            GameObject obj = spawnedAllyObjects[i];
            if(obj != null)
                Destroy(obj);
        }

        spawnedAllyObjects.Clear();
        spawnedEnemyObjects.Clear();
        allySlots.Clear();
        aliveEnemies.Clear();
    }
    public void HandleLevels(bool isWave)
    {
        SyncPartyFromFight();

        float chance = waveManager.GetNextRoomDrop();

        int num=0;

        foreach(var adv in currentParty)
        {
            if(Random.Range(0f,100f)<=chance)
            {
                adv.level++;

                if(!isWave)
                    adv.level++;

                StatCalculator.RecalculateWithHealthPreserved(adv);

                ApplyStatsToUnit(num,adv);
            }

            num++;
        }
    }
    void ApplyStatsToUnit(int num, AdventurerData adv)
    {
        if(num >= allySlots.Count)
            return;

        UnitStats unit=allySlots[num];

        if(unit==null)
            return;

        unit.ApplyFromData(adv);
    }
}
