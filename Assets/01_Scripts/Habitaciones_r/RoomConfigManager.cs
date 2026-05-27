using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class RoomConfigData
{
    public int roomID;
    public int difficulty = 1;     // 1 a 5
    public int rewardDrop = 75;    // 0 a 100
    public List<string> enemyIds = new List<string>(); // 5 slots
}

[Serializable]
public class RoomConfigSaveData
{
    public List<RoomConfigData> rooms = new List<RoomConfigData>();
}

public class RoomConfigManager : MonoBehaviour
{
    public static RoomConfigManager Instance;

    private const string SaveKey = "ROOM_CONFIG_SAVE";

    public RoomConfigSaveData SaveData { get; private set; }

    public Action<int> OnRoomConfigChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        Load();
    }

    private void Load()
    {
        if (!PlayerPrefs.HasKey(SaveKey))
        {
            CreateNewSave();
            return;
        }

        SaveData = JsonUtility.FromJson<RoomConfigSaveData>(
            PlayerPrefs.GetString(SaveKey)
        );

        if (SaveData == null)
        {
            CreateNewSave();
            return;
        }

        NormalizeAllRooms();
    }

    private void CreateNewSave()
    {
        SaveData = new RoomConfigSaveData();

        for (int i = 1; i <= 99; i++)
        {
            RoomConfigData room = new RoomConfigData
            {
                roomID = i,
                difficulty = 1,
                rewardDrop = 75,
                enemyIds = new List<string> { "", "", "", "", "" }
            };

            SaveData.rooms.Add(room);
        }

        Save();
    }

    private void NormalizeAllRooms()
    {
        foreach (RoomConfigData room in SaveData.rooms)
        {
            NormalizeRoom(room);
        }

        for (int i = 1; i <= 99; i++)
        {
            if (GetRoomConfigOrNull(i) == null)
            {
                RoomConfigData room = new RoomConfigData
                {
                    roomID = i,
                    difficulty = 1,
                    rewardDrop = 75,
                    enemyIds = new List<string> { "", "", "", "", "" }
                };

                SaveData.rooms.Add(room);
            }
        }

        Save();
    }

    private void NormalizeRoom(RoomConfigData room)
    {
        if (room == null)
            return;

        room.difficulty = Mathf.Clamp(room.difficulty, 1, 5);
        room.rewardDrop = Mathf.Clamp(room.rewardDrop, 0, 100);

        if (room.enemyIds == null)
            room.enemyIds = new List<string>();

        while (room.enemyIds.Count < 5)
            room.enemyIds.Add("");

        while (room.enemyIds.Count > 5)
            room.enemyIds.RemoveAt(room.enemyIds.Count - 1);
    }

    public void Save()
    {
        PlayerPrefs.SetString(SaveKey, JsonUtility.ToJson(SaveData, true));
        PlayerPrefs.Save();
    }

    private RoomConfigData GetRoomConfigOrNull(int roomID)
    {
        return SaveData.rooms.Find(x => x.roomID == roomID);
    }

    public RoomConfigData GetRoomConfig(int roomID)
    {
        RoomConfigData room = GetRoomConfigOrNull(roomID);

        if (room == null)
        {
            room = new RoomConfigData
            {
                roomID = roomID,
                difficulty = 1,
                rewardDrop = 75,
                enemyIds = new List<string> { "", "", "", "", "" }
            };

            SaveData.rooms.Add(room);
            Save();
        }

        NormalizeRoom(room);
        return room;
    }

    public bool IsBossAllowedInRoom(int roomID)
    {
        return roomID % 5 == 0;
    }

    public bool SetDifficulty(int roomID, int difficulty)
    {
        RoomConfigData room = GetRoomConfig(roomID);
        int newDifficulty = Mathf.Clamp(difficulty, 1, 5);

        if (room.difficulty == newDifficulty)
            return true;

        room.difficulty = newDifficulty;
        Save();
        OnRoomConfigChanged?.Invoke(roomID);
        return true;
    }

    public bool SetRewardDrop(int roomID, int drop)
    {
        RoomConfigData room = GetRoomConfig(roomID);
        int newDrop = Mathf.Clamp(drop, 0, 100);

        if (room.rewardDrop == newDrop)
            return true;

        room.rewardDrop = newDrop;
        Save();
        OnRoomConfigChanged?.Invoke(roomID);
        return true;
    }

    public bool TryPlaceEnemy(int roomID, int slotIndex, string enemyId)
    {
        if (slotIndex < 0 || slotIndex >= 5)
            return false;

        if (string.IsNullOrEmpty(enemyId))
            return false;

        if (GachaInventoryManager.Instance == null)
            return false;

        EnemyGachaData enemyData = GachaInventoryManager.Instance.GetEnemyData(enemyId);
        if (enemyData == null)
            return false;

        if (enemyData.isBoss && !IsBossAllowedInRoom(roomID))
            return false;

        RoomConfigData room = GetRoomConfig(roomID);
        string currentEnemy = room.enemyIds[slotIndex];

        if (currentEnemy == enemyId)
            return true;

        if (!GachaInventoryManager.Instance.UseEnemyCopy(enemyId))
            return false;

        if (!string.IsNullOrEmpty(currentEnemy))
        {
            GachaInventoryManager.Instance.ReturnEnemyCopy(currentEnemy);
        }

        room.enemyIds[slotIndex] = enemyId;
        Save();
        OnRoomConfigChanged?.Invoke(roomID);
        return true;
    }

    public bool RemoveEnemyFromSlot(int roomID, int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= 5)
            return false;

        RoomConfigData room = GetRoomConfig(roomID);
        string currentEnemy = room.enemyIds[slotIndex];

        if (string.IsNullOrEmpty(currentEnemy))
            return true;

        if (GachaInventoryManager.Instance != null)
        {
            GachaInventoryManager.Instance.ReturnEnemyCopy(currentEnemy);
        }

        room.enemyIds[slotIndex] = "";
        Save();
        OnRoomConfigChanged?.Invoke(roomID);
        return true;
    }
}