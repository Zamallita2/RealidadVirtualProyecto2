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
    public int minCoins=20;
    public int maxCoins=50;
    public int minEssence=20;
    public int maxEssence=50;
    void Start()
    {
        shop=FindFirstObjectByType<GachaInventoryManager>();
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
    }

    public List<GameObject> StartNextWave()
    {
        currentWave++;
        if (currentRoomIndex != 0)
        {
            shop.AddEssence(Random.Range(minEssence,maxEssence));
            shop.AddShopCoinsFromBattle(Random.Range(minCoins,maxCoins));
        }

        if(currentRoomIndex < 0 || currentRoomIndex >= runtimeRooms.Count)
            return null;

        if(currentWave > 3)
            return null;

        return runtimeRooms[currentRoomIndex].GetWave(currentWave);
    }

    public void RemoveEnemyFromCurrentWave(GameObject prefab)
    {
        if(prefab == null)
            return;

        if(currentRoomIndex < 0 || currentRoomIndex >= runtimeRooms.Count)
            return;

        if(currentWave < 1 || currentWave > 3)
            return;

        List<GameObject> wave =
            runtimeRooms[currentRoomIndex].GetWave(currentWave);

        wave?.Remove(prefab);
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
    float GetNextRoomDrop()
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
        return rooms.Count > 0;
    }
}
