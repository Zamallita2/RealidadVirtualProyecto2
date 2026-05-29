using System.Collections.Generic;
using UnityEngine;

public class GachaInventoryManager : MonoBehaviour
{
    public static GachaInventoryManager Instance;

    [Header("Todos los enemigos del juego")]
    public List<EnemyGachaData> allEnemies = new List<EnemyGachaData>();

    [Header("Valores iniciales")]
    public int startEssence = 450;
    public int startShopCoins = 300;

    [Header("Pruebas desde Inspector")]
    public bool resetSaveOnStart = false;
    public bool useInspectorCurrencyOnStart = true;
    public int inspectorEssence = 450;
    public int inspectorShopCoins = 300;

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

        if (useInspectorCurrencyOnStart)
        {
            SaveData.essence = inspectorEssence;
            SaveData.shopCoins = inspectorShopCoins;
            Save();
        }
    }

    public void Load()
    {
        if (!PlayerPrefs.HasKey(SaveKey))
        {
            CreateNewSave();
            return;
        }

        SaveData = JsonUtility.FromJson<GachaSaveData>(
            PlayerPrefs.GetString(SaveKey)
        );

        if (SaveData == null)
            CreateNewSave();

        if (SaveData.ownedEnemies == null)
            SaveData.ownedEnemies = new List<GachaOwnedEnemy>();
    }

    private void CreateNewSave()
    {
        SaveData = new GachaSaveData
        {
            essence = startEssence,
            shopCoins = startShopCoins,
            pityCounter = 0,
            ownedEnemies = new List<GachaOwnedEnemy>()
        };

        Save();
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

    public int GetShopCoins()
    {
        return SaveData.shopCoins;
    }

    public void AddEssence(int amount)
    {
        if (amount <= 0)
            return;

        Debug.Log("Ganaste " + amount + " cristales/esencia");
        SaveData.essence += amount;
        Save();
    }

    public void AddShopCoinsFromBattle(int amount)
    {
        if (amount <= 0)
            return;

        Debug.Log("Ganaste " + amount + " monedas");
        SaveData.shopCoins += amount;
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

    public bool SpendShopCoins(int amount)
    {
        if (SaveData.shopCoins < amount)
            return false;

        SaveData.shopCoins -= amount;
        Save();
        return true;
    }

    public GachaOwnedEnemy GetOwned(string enemyId)
    {
        if (SaveData == null || SaveData.ownedEnemies == null)
            return null;

        return SaveData.ownedEnemies.Find(x => x.enemyId == enemyId);
    }

    public bool HasEnemy(string enemyId)
    {
        GachaOwnedEnemy owned = GetOwned(enemyId);
        return owned != null && owned.unlocked;
    }

    public int GetCopies(string enemyId)
    {
        GachaOwnedEnemy owned = GetOwned(enemyId);
        return owned != null ? owned.copies : 0;
    }

    public bool AddEnemyFromGacha(EnemyGachaData enemy)
    {
        if (enemy == null)
            return false;

        GachaOwnedEnemy owned = GetOwned(enemy.enemyId);

        if (owned == null)
        {
            owned = new GachaOwnedEnemy
            {
                enemyId = enemy.enemyId,
                copies = 1,
                unlocked = true
            };

            SaveData.ownedEnemies.Add(owned);
            Save();
            Debug.Log("Nuevo enemigo desbloqueado: " + enemy.enemyName + " | Copias: " + owned.copies);
            return true;
        }

        owned.unlocked = true;
        owned.copies++;
        Save();
        Debug.Log("Enemigo repetido: " + enemy.enemyName + " | Copias: " + owned.copies);
        return false;
    }

    public bool BuyEnemyCopy(EnemyGachaData enemy)
    {
        if (enemy == null)
            return false;

        GachaOwnedEnemy owned = GetOwned(enemy.enemyId);

        if (owned == null || !owned.unlocked)
        {
            Debug.LogWarning("No puedes comprar copia porque no está desbloqueado: " + enemy.enemyName);
            return false;
        }

        if (SaveData.shopCoins < enemy.shopPrice)
        {
            Debug.LogWarning("No tienes monedas suficientes para comprar: " + enemy.enemyName);
            return false;
        }

        SaveData.shopCoins -= enemy.shopPrice;
        owned.copies++;
        Save();

        Debug.Log("Compra correcta: " + enemy.enemyName + " | Copias ahora: " + owned.copies + " | Monedas ahora: " + SaveData.shopCoins);
        return true;
    }

    public EnemyGachaData GetEnemyData(string enemyId)
    {
        return allEnemies.Find(x => x.enemyId == enemyId);
    }

    public GameObject GetEnemyPrefab(string enemyId)
    {
        EnemyGachaData data = GetEnemyData(enemyId);
        return data != null ? data.enemyPrefab : null;
    }

    public bool IsBoss(string enemyId)
    {
        EnemyGachaData data = GetEnemyData(enemyId);
        return data != null && data.isBoss;
    }

    public bool UseEnemyCopy(string enemyId)
    {
        GachaOwnedEnemy owned = GetOwned(enemyId);

        if (owned == null || !owned.unlocked || owned.copies <= 0)
            return false;

        owned.copies--;
        Save();
        return true;
    }

    public void ReturnEnemyCopy(string enemyId)
    {
        GachaOwnedEnemy owned = GetOwned(enemyId);

        if (owned == null)
        {
            owned = new GachaOwnedEnemy
            {
                enemyId = enemyId,
                copies = 0,
                unlocked = true
            };

            SaveData.ownedEnemies.Add(owned);
        }

        owned.copies++;
        Save();
    }
}
