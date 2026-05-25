using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GachaSystem : MonoBehaviour
{
    [Header("Inventario")]
    public GachaInventoryManager inventory;

    [Header("Costos")]
    public int summonOneCost = 100;
    public int summonTenCost = 900;

    [Header("Probabilidades")]
    [Range(0, 100)] public float commonChance = 60f;
    [Range(0, 100)] public float rareChance = 25f;
    [Range(0, 100)] public float epicChance = 12f;
    [Range(0, 100)] public float legendaryChance = 3f;

    [Header("Sistema de suerte")]
    public bool usePity = true;
    public int pityLimit = 20;
    public bool guaranteeEpicInTen = true;

    [Header("Pools")]
    public List<EnemyGachaData> commonPool = new();
    public List<EnemyGachaData> rarePool = new();
    public List<EnemyGachaData> epicPool = new();
    public List<EnemyGachaData> legendaryPool = new();

    [Header("UI que ya tienes")]
    public TMP_Text essenceText;
    public GachaButtonVisual summonOneButton;
    public GachaButtonVisual summonTenButton;

    [Header("Animación fullscreen")]
    public GachaFullscreenAnimation fullscreenAnimation;

    [Header("Popup fullscreen")]
    public GachaRewardPopup rewardPopup;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip clickSound;
    public AudioClip errorSound;

    [Header("Pruebas")]
    public bool forceLegendary = false;
    public bool forceEpic = false;
    public int addEssenceTestAmount = 1000;

    private bool isSummoning;

    private void Start()
    {
        if (inventory == null)
            inventory = GachaInventoryManager.Instance;

        UpdateUI();
    }

    private void Update()
    {
        UpdateButtonStates();
    }

    public void SummonOne()
    {
        if (isSummoning) return;

        Play(clickSound);

        if (!inventory.SpendEssence(summonOneCost))
        {
            Play(errorSound);
            UpdateUI();
            return;
        }

        UpdateUI();
        StartCoroutine(SummonRoutine(1));
    }

    public void SummonTen()
    {
        if (isSummoning) return;

        Play(clickSound);

        if (!inventory.SpendEssence(summonTenCost))
        {
            Play(errorSound);
            UpdateUI();
            return;
        }

        UpdateUI();
        StartCoroutine(SummonRoutine(10));
    }

    private IEnumerator SummonRoutine(int amount)
    {
        isSummoning = true;
        UpdateButtonStates();

        bool hasEpicOrBetter = false;

        for (int i = 0; i < amount; i++)
        {
            bool forceEpicLast =
                amount == 10 &&
                guaranteeEpicInTen &&
                i == amount - 1 &&
                !hasEpicOrBetter;

            EnemyGachaData reward = RollEnemy(forceEpicLast);

            if (reward == null)
                continue;

            if (reward.rarity == GachaRarity.Epico ||
                reward.rarity == GachaRarity.Legendario)
            {
                hasEpicOrBetter = true;
            }

            if (fullscreenAnimation != null)
            {
                yield return StartCoroutine(
                    fullscreenAnimation.PlayAnimation(
                        reward.cardBackSprite,
                        reward.rarity
                    )
                );
            }
            bool isNew = inventory.AddEnemy(reward, out int fragments);

            if (!isNew && fragments > 0)
            {
                inventory.AddEssence(fragments);
                UpdateUI();
            }

            if (rewardPopup != null)
            {
                rewardPopup.Show(reward, isNew);

                yield return rewardPopup.WaitUntilClosed();

                yield return new WaitForSeconds(0.2f);
            }
        }

        isSummoning = false;
        UpdateUI();
    }

    private EnemyGachaData RollEnemy(bool forceEpicOrBetter)
    {
        if (forceLegendary)
            return GetRandomFromPool(legendaryPool);

        if (forceEpic)
            return GetRandomFromPool(epicPool);

        if (usePity)
        {
            inventory.SaveData.pityCounter++;

            if (inventory.SaveData.pityCounter >= pityLimit)
            {
                inventory.SaveData.pityCounter = 0;
                inventory.Save();
                return GetRandomFromPool(legendaryPool);
            }
        }

        GachaRarity rarity = RollRarity(forceEpicOrBetter);

        if (rarity == GachaRarity.Legendario)
            inventory.SaveData.pityCounter = 0;

        inventory.Save();

        switch (rarity)
        {
            case GachaRarity.Comun:
                return GetRandomFromPool(commonPool);

            case GachaRarity.Raro:
                return GetRandomFromPool(rarePool);

            case GachaRarity.Epico:
                return GetRandomFromPool(epicPool);

            case GachaRarity.Legendario:
                return GetRandomFromPool(legendaryPool);

            default:
                return GetRandomFromPool(commonPool);
        }
    }

    private GachaRarity RollRarity(bool forceEpicOrBetter)
    {
        if (forceEpicOrBetter)
        {
            float specialRoll = Random.Range(0f, 100f);

            if (specialRoll <= legendaryChance)
                return GachaRarity.Legendario;

            return GachaRarity.Epico;
        }

        float total = commonChance + rareChance + epicChance + legendaryChance;
        float roll = Random.Range(0f, total);

        if (roll <= legendaryChance)
            return GachaRarity.Legendario;

        roll -= legendaryChance;

        if (roll <= epicChance)
            return GachaRarity.Epico;

        roll -= epicChance;

        if (roll <= rareChance)
            return GachaRarity.Raro;

        return GachaRarity.Comun;
    }

    private EnemyGachaData GetRandomFromPool(List<EnemyGachaData> pool)
    {
        if (pool == null || pool.Count == 0)
        {
            Debug.LogError("Pool vacío en GachaSystem.");
            return null;
        }

        return pool[Random.Range(0, pool.Count)];
    }

    private void UpdateUI()
    {
        if (essenceText != null && inventory != null)
            essenceText.text = inventory.GetEssence().ToString();

        UpdateButtonStates();
    }

    private void UpdateButtonStates()
    {
        if (inventory == null) return;

        bool canOne =
            !isSummoning &&
            inventory.GetEssence() >= summonOneCost;

        bool canTen =
            !isSummoning &&
            inventory.GetEssence() >= summonTenCost;

        if (summonOneButton != null)
            summonOneButton.SetEnabledState(canOne);

        if (summonTenButton != null)
            summonTenButton.SetEnabledState(canTen);
    }

    public void AddEssenceForTest()
    {
        inventory.AddEssence(addEssenceTestAmount);
        UpdateUI();
    }

    private void Play(AudioClip clip)
    {
        if (audioSource != null && clip != null)
            audioSource.PlayOneShot(clip);
    }
}