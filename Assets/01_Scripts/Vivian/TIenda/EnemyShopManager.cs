using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class EnemyShopManager : MonoBehaviour
{
    [Header("Inventario")]
    public GachaInventoryManager inventory;

    [Header("Panel")]
    public GameObject shopPanel;

    [Header("Lista")]
    public Transform contentParent;
    public EnemyShopItemUI itemPrefab;

    [Header("UI")]
    public TMP_Text shopCoinsText;
    public TMP_Text messageText;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip openSound;
    public AudioClip buySound;
    public AudioClip errorSound;

    private readonly List<EnemyShopItemUI> spawnedItems = new List<EnemyShopItemUI>();

    private void Start()
    {
        if (inventory == null)
            inventory = GachaInventoryManager.Instance;

        if (shopPanel != null)
            shopPanel.SetActive(false);

        BuildShop();
        RefreshUI();
    }

    public void ToggleShop()
    {
        if (shopPanel == null) return;

        bool newState = !shopPanel.activeSelf;
        shopPanel.SetActive(newState);

        if (newState)
        {
            Play(openSound);
            BuildShop();
            RefreshUI();
        }
    }

    public void OpenShop()
    {
        if (shopPanel != null)
            shopPanel.SetActive(true);

        Play(openSound);
        BuildShop();
        RefreshUI();
    }

    public void CloseShop()
    {
        if (shopPanel != null)
            shopPanel.SetActive(false);
    }

    public void BuildShop()
    {
        if (inventory == null || contentParent == null || itemPrefab == null)
            return;

        ClearShop();

        foreach (EnemyGachaData enemy in inventory.allEnemies)
        {
            if (enemy == null)
                continue;

            EnemyShopItemUI item = Instantiate(itemPrefab, contentParent);
            item.Setup(enemy, this);
            spawnedItems.Add(item);
        }
    }

    private void ClearShop()
    {
        for (int i = contentParent.childCount - 1; i >= 0; i--)
            Destroy(contentParent.GetChild(i).gameObject);

        spawnedItems.Clear();
    }

    public void BuyEnemy(EnemyGachaData enemy)
    {
        if (inventory == null || enemy == null)
            return;

        if (!inventory.HasEnemy(enemy.enemyId))
        {
            ShowMessage("Primero debes desbloquearlo en el gacha.");
            Play(errorSound);
            RefreshUI();
            return;
        }

        bool bought = inventory.BuyEnemyCopy(enemy);

        if (!bought)
        {
            ShowMessage("No tienes suficientes monedas de tienda.");
            Play(errorSound);
            RefreshUI();
            return;
        }

        ShowMessage("Compraste una copia de " + enemy.enemyName + ".");
        Play(buySound);
        RefreshUI();
    }

    public void RefreshUI()
    {
        if (inventory == null)
            return;

        if (shopCoinsText != null)
            shopCoinsText.text = inventory.GetShopCoins().ToString();

        foreach (EnemyShopItemUI item in spawnedItems)
            item.Refresh();
    }

    private void ShowMessage(string message)
    {
        if (messageText != null)
            messageText.text = message;

        Debug.Log(message);
    }

    private void Play(AudioClip clip)
    {
        if (audioSource != null && clip != null)
            audioSource.PlayOneShot(clip);
    }
}