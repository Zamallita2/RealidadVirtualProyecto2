using System.Collections.Generic;
using UnityEngine;

public class WaveManager : MonoBehaviour
{
    [Header("Dungeon Rooms")]
    [SerializeField] private List<FightRoom> rooms = new();

    private readonly List<FightRoom> runtimeRooms = new();
    private int currentRoomIndex = -1;
    private int currentWave;

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
    }

    public List<GameObject> StartNextWave()
    {
        currentWave++;

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
