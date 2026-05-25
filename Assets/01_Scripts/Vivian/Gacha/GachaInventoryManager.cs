using System.Collections.Generic;
using UnityEngine;

public class GachaInventoryManager : MonoBehaviour
{
    public static GachaInventoryManager Instance;

    [Header("Todos los enemigos")]
    public List<EnemyGachaData> allEnemies = new List<EnemyGachaData>();

    [Header("Pruebas")]
    public int startEssence = 450;
    public bool resetSaveOnStart = false;

    private const string SaveKey = "GACHA_SAVE_DATA";

    public GachaSaveData SaveData { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (resetSaveOnStart)
            PlayerPrefs.DeleteKey(SaveKey);

        Load();
    }

    public void Load()
    {
        if (!PlayerPrefs.HasKey(SaveKey))
        {
            SaveData = new GachaSaveData();
            SaveData.essence = startEssence;
            SaveData.pityCounter = 0;
            Save();
            return;
        }

        SaveData = JsonUtility.FromJson<GachaSaveData>(
            PlayerPrefs.GetString(SaveKey)
        );

        if (SaveData == null)
        {
            SaveData = new GachaSaveData();
            SaveData.essence = startEssence;
            SaveData.pityCounter = 0;
            Save();
        }
    }

    public void Save()
    {
        PlayerPrefs.SetString(SaveKey, JsonUtility.ToJson(SaveData, true));
        PlayerPrefs.Save();
    }

    public int GetEssence()
    {
        return SaveData.essence;
    }

    public void AddEssence(int amount)
    {
        SaveData.essence += amount;
        Save();
    }

    public bool SpendEssence(int amount)
    {
        if (SaveData.essence < amount)
            return false;

        SaveData.essence -= amount;
        Save();
        return true;
    }

    public GachaOwnedEnemy GetOwned(string enemyId)
    {
        return SaveData.ownedEnemies.Find(x => x.enemyId == enemyId);
    }

    public bool HasEnemy(string enemyId)
    {
        GachaOwnedEnemy owned = GetOwned(enemyId);
        return owned != null && owned.unlocked;
    }

    public bool AddEnemy(EnemyGachaData enemy, out int fragments)
    {
        fragments = 0;

        GachaOwnedEnemy owned = GetOwned(enemy.enemyId);

        if (owned == null)
        {
            owned = new GachaOwnedEnemy
            {
                enemyId = enemy.enemyId,
                copies = 1,
                fragments = 0,
                unlocked = true
            };

            SaveData.ownedEnemies.Add(owned);
            Save();
            return true;
        }

        owned.copies++;
        owned.fragments += enemy.duplicateFragments;
        fragments = enemy.duplicateFragments;

        Save();
        return false;
    }

    public List<GameObject> GetUnlockedEnemyPrefabs()
    {
        List<GameObject> prefabs = new List<GameObject>();

        foreach (EnemyGachaData enemy in allEnemies)
        {
            if (enemy != null && HasEnemy(enemy.enemyId) && enemy.enemyPrefab != null)
                prefabs.Add(enemy.enemyPrefab);
        }

        return prefabs;
    }
}