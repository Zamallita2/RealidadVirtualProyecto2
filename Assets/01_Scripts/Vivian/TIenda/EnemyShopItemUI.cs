using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class EnemyShopItemUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Imagen principal")]
    public Image cardImage;

    [Header("Textos")]
    public TMP_Text copyText;
    public TMP_Text priceText;

    [Header("Botón para abrir carta grande")]
    public Button cardOpenButton;

    [Header("Botón comprar")]
    public Button buyButton;
    public Image buyButtonImage;
    public Sprite buyEnabledSprite;
    public Sprite buyDisabledSprite;

    [Header("Animación")]
    public float appearDuration = 0.45f;
    public float hoverScale = 1.05f;

    private EnemyGachaData enemyData;
    private EnemyShopManager shopManager;
    private CanvasGroup canvasGroup;
    private Vector3 originalScale;
    private Coroutine appearCoroutine;

    public void Setup(EnemyGachaData data, EnemyShopManager manager, float delay)
    {
        enemyData = data;
        shopManager = manager;
        originalScale = Vector3.one;

        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        canvasGroup.blocksRaycasts = true;
        canvasGroup.interactable = true;

        if (buyButton != null)
        {
            buyButton.onClick.RemoveAllListeners();
            buyButton.onClick.AddListener(Buy);
        }
        else
        {
            Debug.LogWarning("Falta asignar buyButton en " + gameObject.name);
        }

        // IMPORTANTE:
        // Antes el script agregaba un Button automáticamente al objeto completo de la carta.
        // Eso puede tapar o competir con el botón Comprar si el área se sobrepone.
        // Ahora solo usamos cardOpenButton si tú lo asignas en el Inspector.
        if (cardOpenButton != null && cardOpenButton != buyButton)
        {
            cardOpenButton.onClick.RemoveAllListeners();
            cardOpenButton.onClick.AddListener(OpenBigImage);
        }

        Refresh();

        if (gameObject.activeInHierarchy)
        {
            if (appearCoroutine != null)
                StopCoroutine(appearCoroutine);

            appearCoroutine = StartCoroutine(AppearAnimation(delay));
        }
    }

    public void PlayAppear(float delay)
    {
        if (!gameObject.activeInHierarchy)
            return;

        if (appearCoroutine != null)
            StopCoroutine(appearCoroutine);

        appearCoroutine = StartCoroutine(AppearAnimation(delay));
    }

    public void Refresh()
    {
        if (enemyData == null || GachaInventoryManager.Instance == null)
            return;

        GachaInventoryManager inventory = GachaInventoryManager.Instance;

        bool unlocked = inventory.HasEnemy(enemyData.enemyId);
        int copies = inventory.GetCopies(enemyData.enemyId);
        bool canBuy = unlocked && inventory.GetShopCoins() >= enemyData.shopPrice;

        if (cardImage != null)
            cardImage.sprite = unlocked ? enemyData.shopUnlockedCardSprite : enemyData.shopLockedCardSprite;

        if (copyText != null)
            copyText.text = copies.ToString();

        if (priceText != null)
            priceText.text = enemyData.shopPrice.ToString();

        if (buyButton != null)
            buyButton.interactable = canBuy;

        if (buyButtonImage != null)
            buyButtonImage.sprite = canBuy ? buyEnabledSprite : buyDisabledSprite;
    }

    private IEnumerator AppearAnimation(float delay)
    {
        canvasGroup.alpha = 0f;
        transform.localScale = originalScale * 0.8f;

        yield return new WaitForSeconds(delay);

        float time = 0f;

        while (time < appearDuration)
        {
            time += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, time / appearDuration);

            canvasGroup.alpha = t;
            transform.localScale = Vector3.Lerp(originalScale * 0.8f, originalScale, t);

            yield return null;
        }

        canvasGroup.alpha = 1f;
        transform.localScale = originalScale;
    }

    private void OpenBigImage()
    {
        if (shopManager != null)
            shopManager.ShowBigCardImage(enemyData);
    }

    private void Buy()
    {
        Debug.Log("Click en Comprar: " + (enemyData != null ? enemyData.enemyName : "SIN DATA"));

        if (shopManager != null)
            shopManager.BuyEnemy(enemyData);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        transform.localScale = originalScale * hoverScale;

        if (shopManager != null)
            shopManager.PlayHoverSound();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        transform.localScale = originalScale;
    }
}
