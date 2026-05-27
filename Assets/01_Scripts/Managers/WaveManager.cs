using System.Collections.Generic;
using UnityEngine;

public class WaveManager : MonoBehaviour
{
    [Header("Dungeon Rooms")]
    [SerializeField] private List<FightRoom> rooms = new();

    private readonly List<FightRoom> runtimeRooms = new();
    private int currentRoomIndex = -1;
    private int currentWave;
    public GachaInventoryManager shop;
    public FightManager FM;
    public int minCoins=20;
    public int maxCoins=50;
    public int minEssence=20;
    public int maxEssence=50;
    [SerializeField]
    private RoomToFightConverter converter;
    void Start()
    {
        shop=FindFirstObjectByType<GachaInventoryManager>();
        FM=gameObject.GetComponent<FightManager>();
    }

    public void AddRoom(FightRoom room)
    {
        if(room == null)
            return;

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

    public void PrepareForFight()
    {
        runtimeRooms.Clear();

        LoadRoomsFromConfig();

        foreach(FightRoom room in rooms)
        {
            runtimeRooms.Add(room.Clone());
        }

        currentRoomIndex = -1;
        currentWave = 0;
    }

    public bool EnterNextRoom()
    {
        currentRoomIndex++;
        currentWave = 0;

        while(currentRoomIndex < runtimeRooms.Count
            && runtimeRooms[currentRoomIndex].IsEmpty())
        {
            currentRoomIndex++;
        }

        if(currentRoomIndex < runtimeRooms.Count)
            runtimeRooms[currentRoomIndex].isBeingAttacked = true;

        return currentRoomIndex < runtimeRooms.Count;
    }

    public void ClearCurrentWave()
    {
        if(currentRoomIndex < 0 || currentRoomIndex >= runtimeRooms.Count)
            return;

        if(currentWave < 1 || currentWave > 3)
            return;

        runtimeRooms[currentRoomIndex]
            .GetWave(currentWave)?
            .Clear();
    }

    public void FinishCurrentRoom()
    {
        if(currentRoomIndex < 0 || currentRoomIndex >= runtimeRooms.Count)
            return;

        FightRoom room = runtimeRooms[currentRoomIndex];

        room.ClearAllWaves();
        room.isBeingAttacked = false;
        shop.AddEssence(Random.Range(minEssence*2,maxEssence*2));
        shop.AddShopCoinsFromBattle(Random.Range(minCoins*2,maxCoins*2));
        FM.HandleLevels(false);
    }

    public List<AdventurerSetup> StartNextWave()
    {
        if (currentWave!=0)
        {
            FM.HandleLevels(true);
            shop.AddEssence(Random.Range(minEssence,maxEssence));
            shop.AddShopCoinsFromBattle(Random.Range(minCoins,maxCoins));
        }
        currentWave++;

        if(currentRoomIndex < 0 || currentRoomIndex >= runtimeRooms.Count)
            return null;

        if(currentWave > 3)
            return null;

        return runtimeRooms[currentRoomIndex].GetWave(currentWave);
    }

    public void RemoveEnemyFromCurrentWave(
    GameObject prefab)
    {
        if(prefab == null)
            return;

        if(currentRoomIndex < 0 ||
        currentRoomIndex >= runtimeRooms.Count)
            return;

        if(currentWave < 1 ||
        currentWave > 3)
            return;

        List<AdventurerSetup> wave =
            runtimeRooms[currentRoomIndex]
            .GetWave(currentWave);

        if(wave == null)
            return;

        AdventurerSetup target =
            wave.Find(x => x.prefab == prefab);

        if(target != null)
            wave.Remove(target);
    }
    public float GetPartyHealthPercent(List<AdventurerData> party)
    {
        if(party == null || party.Count == 0)
            return 0;

        float totalCurrent = 0;
        float totalMax = 0;

        foreach(var p in party)
        {
            totalCurrent += p.currentHealth;
            totalMax += p.maxHealth;
        }

        return totalMax <= 0 ? 0 : (totalCurrent / totalMax) * 100f;
    }
    public bool ShouldPartyRetreat(Aldea aldea, List<AdventurerData> party)
    {
        if(aldea == null || party == null)
            return false;

        float hp = GetPartyHealthPercent(party);

        int roomIndex = GetCurrentRoomIndex();
        int wave = GetCurrentWaveNumber();

        bool nextRoomIsHighDrop = GetNextRoomDrop() > 50f;
        bool nextRoomMultiple5 = (roomIndex + 1) % 5 == 0;

        return aldea.tipoAldea switch
        {
            TipoAldea.Temerosos => CheckTemerosos(party, hp),
            TipoAldea.Codiciosos => CheckCodiciosos(hp, nextRoomIsHighDrop),
            TipoAldea.Cautelosos => CheckCautelosos(hp, nextRoomMultiple5),
            TipoAldea.Vengativos => CheckVengativos(hp, party),
            TipoAldea.Eruditos => CheckEruditos(hp, nextRoomMultiple5),
            _ => false
        };
    }
    public float GetNextRoomDrop()
    {
        return(rooms[currentRoomIndex].loot);
    }
    bool CheckTemerosos(List<AdventurerData> party, float hp)
    {
        bool hasDead = party.Exists(p => !p.isAlive);
        return hasDead || hp < 40f;
    }
    bool CheckCodiciosos(float hp, bool nextRoomHighDrop)
    {
        if(hp < 30f)
            return true;

        if(nextRoomHighDrop && hp < 20f)
            return true;

        return false;
    }
    bool CheckCautelosos(float hp, bool nextRoomMultiple5)
    {
        if(hp < 30f)
            return true;

        if(nextRoomMultiple5 && hp < 50f)
            return true;

        return false;
    }
    bool CheckVengativos(float hp, List<AdventurerData> party)
    {
        foreach(var p in party)
        {
            if(p.currentHealth / p.maxHealth * 100f < 15f)
            {
                p.currentHealth = p.maxHealth * 0.5f;
            }
        }

        return hp < 30f;
    }
    bool CheckEruditos(float hp, bool nextRoomMultiple5)
    {
        if(nextRoomMultiple5)
        {
            // buff de curación global
            // (esto idealmente en FightManager)
        }

        return hp < 30f;
    }
    void LoadRoomsFromConfig()
    {
        Debug.Log("Cargando salas");
        rooms.Clear();

        if(RoomConfigManager.Instance == null ||
        RoomConfigManager.Instance.SaveData == null)
        {
            Debug.Log("Son nulas");
            return;
        }

        List<FightRoom> convertedRooms =
            converter.ConvertRooms(
                RoomConfigManager.Instance.SaveData
            );

        rooms.AddRange(convertedRooms);
        // Garantizar que existan salas aunque estén vacías
        if(rooms.Count == 0)
        {
            rooms.Add(new FightRoom());
        }
    }
    public int GetCurrentRoomIndex()
    {
        return currentRoomIndex;
    }

    public int GetCurrentWaveNumber()
    {
        return currentWave;
    }

    public bool HasConfiguredRooms()
    {
        LoadRoomsFromConfig();

        return rooms.Count > 0;
    }
}
