using System.Collections.Generic;
using UnityEngine;

public class WaveManager : MonoBehaviour
{
    // -------------------------------------------------------------------------
    // Inspector
    // -------------------------------------------------------------------------
    [Header("Dungeon Rooms (estáticas, opcional)")]
    [SerializeField] private List<FightRoom> rooms = new();

    [Header("Rewards")]
    public int minCoins    = 20;
    public int maxCoins    = 50;
    public int minEssence  = 20;
    public int maxEssence  = 50;

    [SerializeField] private RoomToFightConverter converter;

    // -------------------------------------------------------------------------
    // Dependencias en runtime
    // -------------------------------------------------------------------------
    public GachaInventoryManager shop;
    public FightManager FM;

    // -------------------------------------------------------------------------
    // Estado de la mazmorra (DINÁMICO)
    // -------------------------------------------------------------------------

    /// <summary>
    /// Lista de roomIDs que existen en el RoomConfigManager,
    /// construida solo al inicio de cada pelea.
    /// </summary>
    private readonly List<int> roomIdSequence = new();

    /// <summary>
    /// Sala activa en memoria (cargada justo al entrar a la sala).
    /// </summary>
    private FightRoom currentRoomData;
    private int currentRoomIndex = -1;
    private int currentWave;

    // -------------------------------------------------------------------------
    // Unity
    // -------------------------------------------------------------------------
    void Start()
    {
        shop = FindFirstObjectByType<GachaInventoryManager>();
        FM   = gameObject.GetComponent<FightManager>();
    }

    // -------------------------------------------------------------------------
    // API pública — compatibilidad con FightManager
    // -------------------------------------------------------------------------

    public void AddRoom(FightRoom room)
    {
        if (room == null) return;
        rooms.Add(room);
    }

    public void ClearRooms()
    {
        rooms.Clear();
    }

    public IReadOnlyList<FightRoom> GetConfiguredRooms()
    {
        return rooms;
    }

    /// <summary>
    /// Prepara la secuencia de roomIDs disponibles.
    /// NO carga los datos de cada sala todavía.
    /// </summary>
    public void PrepareForFight()
    {
        roomIdSequence.Clear();
        currentRoomData  = null;
        currentRoomIndex = -1;
        currentWave      = 0;

        BuildRoomIdSequence();
    }

    /// <summary>
    /// Avanza al siguiente cuarto y lo carga en tiempo real desde RoomConfigManager.
    /// Retorna false si no quedan más salas con enemigos.
    /// </summary>
    public bool EnterNextRoom()
    {
        currentRoomIndex++;
        currentWave = 0;

        // Saltar salas vacías (cargándolas on-demand para comprobarlo)
        while (currentRoomIndex < roomIdSequence.Count)
        {
            FightRoom candidate = LoadRoomByIndex(currentRoomIndex);

            if (candidate != null && !candidate.IsEmpty())
            {
                currentRoomData = candidate;
                currentRoomData.isBeingAttacked = true;
                return true;
            }

            currentRoomIndex++;
        }

        currentRoomData = null;
        return false;
    }

    public void ClearCurrentWave()
    {
        if (currentRoomData == null) return;
        if (currentWave < 1 || currentWave > 3) return;

        currentRoomData.GetWave(currentWave)?.Clear();
    }

    /// <summary>
    /// Se llama cuando se derrota a todos los enemigos de una sala.
    /// Vacía los slots de esa sala en RoomConfigManager y devuelve las copias
    /// al inventario del jugador para que tenga que volver a colocarlos.
    /// </summary>
    public void FinishCurrentRoom()
    {
        if (currentRoomData == null) return;

        int clearedRoomID = CurrentRoomID();

        currentRoomData.ClearAllWaves();
        currentRoomData.isBeingAttacked = false;

        // Recompensas
        shop.AddEssence(Random.Range(minEssence * 2, maxEssence * 2));
        shop.AddShopCoinsFromBattle(Random.Range(minCoins * 2, maxCoins * 2));
        FM.HandleLevels(false);

        // ---- NUEVA LÓGICA: vaciar la sala en el save data y devolver copias ----
        if (clearedRoomID >= 0)
            ClearRoomSaveData(clearedRoomID);
    }

    public List<AdventurerSetup> StartNextWave()
    {
        if (currentWave != 0)
        {
            FM.HandleLevels(true);
            shop.AddEssence(Random.Range(minEssence, maxEssence));
            shop.AddShopCoinsFromBattle(Random.Range(minCoins, maxCoins));
        }

        currentWave++;

        if (currentRoomData == null) return null;
        if (currentWave > 3)        return null;

        return currentRoomData.GetWave(currentWave);
    }

    public void RemoveEnemyFromCurrentWave(GameObject prefab)
    {
        if (prefab == null)             return;
        if (currentRoomData == null)    return;
        if (currentWave < 1 || currentWave > 3) return;

        List<AdventurerSetup> wave = currentRoomData.GetWave(currentWave);
        if (wave == null) return;

        AdventurerSetup target = wave.Find(x => x.prefab == prefab);
        if (target != null)
            wave.Remove(target);
    }

    // -------------------------------------------------------------------------
    // Lógica de retirada y recompensas
    // -------------------------------------------------------------------------

    public float GetPartyHealthPercent(List<AdventurerData> party)
    {
        if (party == null || party.Count == 0) return 0;

        float totalCurrent = 0;
        float totalMax     = 0;

        foreach (var p in party)
        {
            totalCurrent += p.currentHealth;
            totalMax     += p.maxHealth;
        }

        return totalMax <= 0 ? 0 : (totalCurrent / totalMax) * 100f;
    }

    public bool ShouldPartyRetreat(Aldea aldea, List<AdventurerData> party)
    {
        if (aldea == null || party == null) return false;

        float hp = GetPartyHealthPercent(party);
        int   roomIndex = GetCurrentRoomIndex();

        bool nextRoomIsHighDrop = GetNextRoomDrop() > 50f;
        bool nextRoomMultiple5  = (roomIndex + 1) % 5 == 0;

        return aldea.tipoAldea switch
        {
            TipoAldea.Temerosos  => CheckTemerosos(party, hp),
            TipoAldea.Codiciosos => CheckCodiciosos(hp, nextRoomIsHighDrop),
            TipoAldea.Cautelosos => CheckCautelosos(hp, nextRoomMultiple5),
            TipoAldea.Vengativos => CheckVengativos(hp, party),
            TipoAldea.Eruditos   => CheckEruditos(hp, nextRoomMultiple5),
            _ => false
        };
    }

    public float GetNextRoomDrop()
    {
        return currentRoomData != null ? currentRoomData.loot : 0f;
    }

    // -------------------------------------------------------------------------
    // Getters de estado
    // -------------------------------------------------------------------------

    public int GetCurrentRoomIndex()  => currentRoomIndex;
    public int GetCurrentWaveNumber() => currentWave;

    public bool HasConfiguredRooms()
    {
        BuildRoomIdSequence();
        return roomIdSequence.Count > 0;
    }

    // -------------------------------------------------------------------------
    // Helpers privados
    // -------------------------------------------------------------------------

    /// <summary>
    /// Construye la secuencia de IDs de sala a partir de RoomConfigManager.
    /// Si no hay datos de configuración, recurre a las salas estáticas.
    /// </summary>
    private void BuildRoomIdSequence()
    {
        roomIdSequence.Clear();

        if (RoomConfigManager.Instance != null &&
            RoomConfigManager.Instance.SaveData != null)
        {
            foreach (var roomData in RoomConfigManager.Instance.SaveData.rooms)
            {
                roomIdSequence.Add(roomData.roomID);
            }

            if (roomIdSequence.Count > 0)
                return;
        }

        // Fallback: salas estáticas del Inspector
        for (int i = 0; i < rooms.Count; i++)
            roomIdSequence.Add(-(i + 1)); // IDs negativos = estáticas
    }

    /// <summary>
    /// Carga la FightRoom correspondiente al índice dado dentro de roomIdSequence.
    /// Si el ID es negativo, usa la sala estática.
    /// </summary>
    private FightRoom LoadRoomByIndex(int index)
    {
        if (index < 0 || index >= roomIdSequence.Count)
            return null;

        int roomID = roomIdSequence[index];

        // ID negativo → sala estática del Inspector
        if (roomID < 0)
        {
            int staticIdx = (-roomID) - 1;
            if (staticIdx < rooms.Count)
                return rooms[staticIdx].Clone();
            return null;
        }

        // ID positivo → cargar en tiempo real desde RoomConfigManager
        FightRoom loaded = converter != null
            ? converter.ConvertSingleRoom(roomID)
            : null;

        return loaded;
    }

    /// <summary>
    /// Retorna el roomID de la sala actualmente activa, o -1 si no hay ninguna.
    /// </summary>
    private int CurrentRoomID()
    {
        if (currentRoomIndex < 0 || currentRoomIndex >= roomIdSequence.Count)
            return -1;

        return roomIdSequence[currentRoomIndex];
    }

    /// <summary>
    /// Vacía todos los slots de enemigos de la sala indicada en RoomConfigManager
    /// y devuelve las copias al inventario del jugador.
    /// </summary>
    private void ClearRoomSaveData(int roomID)
    {
        if (roomID < 0) return; // sala estática, no hay nada que borrar
        if (RoomConfigManager.Instance == null) return;

        RoomConfigData roomData = RoomConfigManager.Instance.SaveData.rooms
            .Find(r => r.roomID == roomID);

        if (roomData == null) return;

        for (int i = 0; i < roomData.enemyIds.Count; i++)
        {
            string enemyId = roomData.enemyIds[i];
            if (string.IsNullOrEmpty(enemyId)) continue;

            // Devolver la copia al inventario
            if (GachaInventoryManager.Instance != null)
                GachaInventoryManager.Instance.ReturnEnemyCopy(enemyId);

            roomData.enemyIds[i] = "";
        }

        RoomConfigManager.Instance.Save();
        RoomConfigManager.Instance.OnRoomConfigChanged?.Invoke(roomID);

        Debug.Log($"[WaveManager] Sala {roomID} vaciada: copias devueltas al inventario.");
    }

    // -------------------------------------------------------------------------
    // Checks de retirada por tipo de aldea
    // -------------------------------------------------------------------------

    bool CheckTemerosos(List<AdventurerData> party, float hp)
    {
        bool hasDead = party.Exists(p => !p.isAlive);
        return hasDead || hp < 40f;
    }

    bool CheckCodiciosos(float hp, bool nextRoomHighDrop)
    {
        if (hp < 30f) return true;
        if (nextRoomHighDrop && hp < 20f) return true;
        return false;
    }

    bool CheckCautelosos(float hp, bool nextRoomMultiple5)
    {
        if (hp < 30f) return true;
        if (nextRoomMultiple5 && hp < 50f) return true;
        return false;
    }

    bool CheckVengativos(float hp, List<AdventurerData> party)
    {
        foreach (var p in party)
        {
            if (p.currentHealth / p.maxHealth * 100f < 15f)
                p.currentHealth = p.maxHealth * 0.5f;
        }
        return hp < 30f;
    }

    bool CheckEruditos(float hp, bool nextRoomMultiple5)
    {
        if (nextRoomMultiple5)
        {
            // buff de curación global
            // (esto idealmente en FightManager)
        }
        return hp < 30f;
    }
}
