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
            Debug.Log(
                $"Registrado: [{enemy.enemyId}]"
            );
        }
    }

    public List<FightRoom> ConvertRooms(
    RoomConfigSaveData saveData)
    {
        EnsureLookup();

        List<FightRoom> result = new();

        foreach(var roomData in saveData.rooms)
        {
            result.Add(ConvertRoom(roomData));
        }

        return result;
    }

    /// <summary>
    /// Convierte UNA sola sala de forma dinámica consultando RoomConfigManager en tiempo real.
    /// Retorna null si no se encuentra la configuración.
    /// </summary>
    public FightRoom ConvertSingleRoom(int roomID)
    {
        EnsureLookup();

        if (RoomConfigManager.Instance == null ||
            RoomConfigManager.Instance.SaveData == null)
            return null;

        RoomConfigData data = RoomConfigManager.Instance.SaveData.rooms
            .Find(r => r.roomID == roomID);

        if (data == null)
            return null;

        return ConvertRoom(data);
    }
    private void EnsureLookup()
    {
        if (enemyLookup == null)
        {
            enemyLookup = new();
        }

        // 1. Cargar desde la base de datos local
        if (enemyDatabase != null)
        {
            foreach(var enemy in enemyDatabase)
            {
                if(enemy == null || string.IsNullOrEmpty(enemy.enemyId))
                    continue;

                if (!enemyLookup.ContainsKey(enemy.enemyId))
                {
                    enemyLookup[enemy.enemyId] = enemy;
                }
            }
        }

        // 2. Cargar desde GachaInventoryManager de forma dinámica
        if (GachaInventoryManager.Instance != null && GachaInventoryManager.Instance.allEnemies != null)
        {
            foreach (var enemy in GachaInventoryManager.Instance.allEnemies)
            {
                if (enemy == null || string.IsNullOrEmpty(enemy.enemyId))
                    continue;

                if (!enemyLookup.ContainsKey(enemy.enemyId))
                {
                    enemyLookup[enemy.enemyId] = enemy;
                }
            }
        }
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
        Debug.Log($"Intentando agregar: [{enemyId}]");

        if(string.IsNullOrEmpty(enemyId))
        {
            Debug.Log("Vacío");
            return;
        }

        if(enemyLookup == null)
        {
            Debug.LogError("enemyLookup es NULL");
            return;
        }

        Debug.Log(
            $"enemyLookup tiene {enemyLookup.Count} enemigos"
        );

        if(!enemyLookup.TryGetValue(
            enemyId,
            out EnemyGachaData enemy))
        {
            Debug.LogError(
                $"No encontrado: [{enemyId}]"
            );
            return;
        }

        Debug.Log(
            $"Encontrado: {enemy.enemyId}"
        );

        // Añadir el enemigo a la oleada de combate
        bool isBoss = enemyId == "juguetero_demonio" || enemy.isBoss;
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