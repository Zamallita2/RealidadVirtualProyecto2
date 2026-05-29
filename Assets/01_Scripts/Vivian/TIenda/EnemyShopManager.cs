using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EnemyShopManager : MonoBehaviour
{
    [Header("Inventario")]
    public GachaInventoryManager inventory;

    [Header("Panel tienda")]
    public GameObject shopPanel;

    [Header("Cartas que aparecerán")]
    public List<EnemyGachaData> shopCards = new List<EnemyGachaData>();

    [Header("Cards pequeñas")]
    public List<EnemyShopItemUI> cardSlots = new List<EnemyShopItemUI>();

    [Header("Textos")]
    public TMP_Text shopCoinsText;
    public TMP_Text crystalText;
    public TMP_Text messageText;

    [Header("Carta grande simple")]
    public GameObject bigImagePanel;
    public Image bigCardImage;

    [Header("Animación")]
    public float delayBetweenCards = 0.08f;

    [Header("Sonidos")]
    public AudioSource audioSource;
    public AudioClip openSound;
    public AudioClip closeSound;
    public AudioClip hoverSound;
    public AudioClip clickSound;
    public AudioClip buySound;
    public AudioClip errorSound;

    [Header("Pruebas")]
    public int addBattleCoinsAmount = 100;

    private EnemyGachaData selectedEnemy;
    private bool shopBuilt;

    private void Start()
    {
        if (inventory == null)
            inventory = GachaInventoryManager.Instance;

        if (bigImagePanel != null)
            bigImagePanel.SetActive(false);

        RefreshCurrencyTexts();

        if (shopPanel != null)
            shopPanel.SetActive(false);
    }

    public void OpenShop()
    {
        if (shopPanel != null)
            shopPanel.SetActive(true);

        BuildShop();
        RefreshAll();
        Play(openSound);
    }

    public void CloseShop()
    {
        if (bigImagePanel != null)
            bigImagePanel.SetActive(false);

        if (shopPanel != null)
            shopPanel.SetActive(false);

        Play(closeSound);
    }

    public void ToggleShop()
    {
        if (shopPanel == null)
            return;

        if (shopPanel.activeSelf)
            CloseShop();
        else
            OpenShop();
    }

    public void BuildShop()
    {
        for (int i = 0; i < cardSlots.Count; i++)
        {
            if (cardSlots[i] == null)
                continue;

            if (i < shopCards.Count && shopCards[i] != null)
            {
                cardSlots[i].gameObject.SetActive(true);
                cardSlots[i].Setup(shopCards[i], this, i * delayBetweenCards);
                cardSlots[i].PlayAppear(i * delayBetweenCards);
            }
            else
            {
                cardSlots[i].gameObject.SetActive(false);
            }
        }

        shopBuilt = true;
    }

    public void ShowBigCardImage(EnemyGachaData enemy)
    {
        if (enemy == null || inventory == null)
            return;

        selectedEnemy = enemy;

        bool unlocked = inventory.HasEnemy(enemy.enemyId);

        if (bigImagePanel != null)
            bigImagePanel.SetActive(true);

        if (bigCardImage != null)
            bigCardImage.sprite = unlocked ? enemy.shopBigUnlockedCardSprite : enemy.shopBigLockedCardSprite;

        Play(clickSound);
    }

    public void HideBigCardImage()
    {
        if (bigImagePanel != null)
            bigImagePanel.SetActive(false);

        selectedEnemy = null;
        Play(closeSound);
    }

    public void BuyEnemy(EnemyGachaData enemy)
    {
        if (enemy == null || inventory == null)
        {
            Debug.LogWarning("No se puede comprar: enemy o inventory es null.");
            return;
        }

        int copiesBefore = inventory.GetCopies(enemy.enemyId);
        int coinsBefore = inventory.GetShopCoins();

        if (!inventory.HasEnemy(enemy.enemyId))
        {
            ShowMessage("Primero debes obtenerlo en el gacha.");
            Play(errorSound);
            RefreshAll();
            return;
        }

        if (!inventory.BuyEnemyCopy(enemy))
        {
            ShowMessage("No tienes monedas suficientes.");
            Play(errorSound);
            RefreshAll();
            return;
        }

        int copiesAfter = inventory.GetCopies(enemy.enemyId);
        int coinsAfter = inventory.GetShopCoins();

        ShowMessage("Compraste una copia de " + enemy.enemyName + ". Copias: " + copiesBefore + " -> " + copiesAfter);
        Debug.Log("Compra tienda: " + enemy.enemyName + " | Copias " + copiesBefore + " -> " + copiesAfter + " | Monedas " + coinsBefore + " -> " + coinsAfter);

        Play(buySound);
        RefreshAll();

        if (selectedEnemy == enemy)
            ShowBigCardImage(enemy);
    }

    public void AddBattleCoinsForTest()
    {
        if (inventory == null)
            return;

        inventory.AddShopCoinsFromBattle(addBattleCoinsAmount);
        RefreshAll();
        ShowMessage("Ganaste " + addBattleCoinsAmount + " monedas de batalla.");
    }

    public void RefreshAll()
    {
        RefreshCurrencyTexts();

        if (!shopBuilt)
            return;

        foreach (EnemyShopItemUI card in cardSlots)
        {
            if (card != null)
                card.Refresh();
        }
    }

    private void RefreshCurrencyTexts()
    {
        if (inventory == null)
            return;

        if (shopCoinsText != null)
            shopCoinsText.text = inventory.GetShopCoins().ToString();

        if (crystalText != null)
            crystalText.text = inventory.GetEssence().ToString();
    }

    public void PlayHoverSound()
    {
        Play(hoverSound);
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
