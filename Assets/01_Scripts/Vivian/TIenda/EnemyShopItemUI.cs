using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EnemyShopItemUI : MonoBehaviour
{
    [Header("UI")]
    public Image iconImage;
    public TMP_Text nameText;
    public TMP_Text priceText;
    public TMP_Text copiesText;
    public TMP_Text statusText;
    public Button buyButton;

    [Header("Visual")]
    public CanvasGroup canvasGroup;
    public Image lockOverlay;

    private EnemyGachaData enemyData;
    private EnemyShopManager shopManager;

    public void Setup(EnemyGachaData data, EnemyShopManager manager)
    {
        enemyData = data;
        shopManager = manager;

        if (buyButton != null)
        {
            buyButton.onClick.RemoveAllListeners();
            buyButton.onClick.AddListener(Buy);
        }

        Refresh();
    }

    public void Refresh()
    {
        if (enemyData == null || GachaInventoryManager.Instance == null)
            return;

        GachaInventoryManager inventory = GachaInventoryManager.Instance;

        bool unlocked = inventory.HasEnemy(enemyData.enemyId);
        int copies = inventory.GetCopies(enemyData.enemyId);
        bool canBuy = unlocked && inventory.GetShopCoins() >= enemyData.shopPrice;

        if (iconImage != null)
            iconImage.sprite = enemyData.shopIcon != null ? enemyData.shopIcon : enemyData.cardFrontSprite;

        if (nameText != null)
            nameText.text = enemyData.enemyName;

        if (priceText != null)
            priceText.text = enemyData.shopPrice.ToString();

        if (copiesText != null)
            copiesText.text = "Copias: " + copies;

        if (statusText != null)
            statusText.text = unlocked ? "Disponible" : "Bloqueado";

        if (buyButton != null)
            buyButton.interactable = canBuy;

        if (canvasGroup != null)
            canvasGroup.alpha = unlocked ? 1f : 0.45f;

        if (lockOverlay != null)
            lockOverlay.gameObject.SetActive(!unlocked);
    }

    private void Buy()
    {
        if (enemyData == null || shopManager == null)
            return;

        shopManager.BuyEnemy(enemyData);
    }
}