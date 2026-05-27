using System.Collections.Generic;
using UnityEngine;

public class RoomToFightConverter : MonoBehaviour
{
    [SerializeField]
    private List<EnemyGachaData> enemyDatabase;

    private Dictionary<string, EnemyGachaData> enemyLookup;

    private void Awake()
    {
        enemyLookup = new();

        foreach(var enemy in enemyDatabase)
        {
            if(enemy == null)
                continue;

            enemyLookup[enemy.enemyId] = enemy;
        }
    }

    public List<FightRoom> ConvertRooms(RoomConfigSaveData saveData)
    {
        List<FightRoom> result = new();

        foreach(var roomData in saveData.rooms)
        {
            result.Add(ConvertRoom(roomData));
        }

        return result;
    }

    private FightRoom ConvertRoom(RoomConfigData data)
    {
        FightRoom room = new();

        room.loot = data.rewardDrop;

        AddEnemy(room.wave1, data.enemyIds[0], data.difficulty, 2);
        AddEnemy(room.wave1, data.enemyIds[1], data.difficulty, 2);

        AddEnemy(room.wave2, data.enemyIds[2], data.difficulty, 2);
        AddEnemy(room.wave2, data.enemyIds[3], data.difficulty, 2);

        AddEnemy(room.wave3, data.enemyIds[4], data.difficulty, 3);

        return room;
    }

    private void AddEnemy(
        List<AdventurerSetup> wave,
        string enemyId,
        int difficulty,
        int amount)
    {
        if(string.IsNullOrEmpty(enemyId))
            return;

        if(!enemyLookup.TryGetValue(
            enemyId,
            out EnemyGachaData enemy))
            return;

        bool isBoss =
            enemyId == "juguetero_demonio";

        int count = isBoss ? 1 : amount;

        int level = difficulty * 5;

        for(int i = 0; i < count; i++)
        {
            wave.Add(new AdventurerSetup
            {
                prefab = enemy.enemyPrefab,
                level = level
            });
        }
    }
}