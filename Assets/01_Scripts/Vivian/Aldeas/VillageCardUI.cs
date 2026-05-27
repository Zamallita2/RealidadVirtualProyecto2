using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class VillageCardUI : MonoBehaviour
{
    [Header("Render")]
    public RawImage rawImageVillage;

    [Header("Fondo")]
    public Image backgroundImage;

    [Header("Marco")]
    public Image frameImage;

    [Header("Personajes")]
    public List<Image> slotImages = new();

    [Header("Textos")]
    public TMP_Text attackText;
    public TMP_Text defenseText;
    public TMP_Text hpText;

    [Header("Estados")]
    public Sprite enEsperaSprite;
    public Sprite enCaminoSprite;
    public Sprite enCombateSprite;
    public Sprite avanzandoSprite;
    public Sprite retirandoseSprite;
    public Sprite derrotadoSprite;

    [Header("Animación segura")]
    public float fadeDuration = 0.45f;

    private VillagePanelEntry currentEntry;
    private CanvasGroup canvasGroup;

    public void Setup(VillagePanelEntry entry)
    {
        currentEntry = entry;

        if (canvasGroup == null)
        {
            canvasGroup = GetComponent<CanvasGroup>();

            if (canvasGroup == null)
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        SetupVillage();

        StopAllCoroutines();
        StartCoroutine(FadeIn());
    }

    private void Update()
    {
        if (currentEntry != null)
            UpdateStates();
    }

    private void SetupVillage()
    {
        VillageData data = currentEntry.villageData;

        if (rawImageVillage != null)
            rawImageVillage.texture = currentEntry.renderTexture;

        if (backgroundImage != null)
        {
            backgroundImage.sprite = currentEntry.backgroundSprite;
            backgroundImage.color = Color.white;
        }

        if (frameImage != null)
        {
            frameImage.sprite = currentEntry.frameSprite;
            frameImage.color = Color.white;
        }

        for (int i = 0; i < slotImages.Count; i++)
        {
            if (i >= currentEntry.portraits.Count || currentEntry.portraits[i] == null)
            {
                slotImages[i].enabled = false;
                continue;
            }

            slotImages[i].enabled = true;
            slotImages[i].sprite = currentEntry.portraits[i];
            slotImages[i].color = Color.white;
        }

        if (attackText != null)
            attackText.text = data.attack.ToString();

        if (defenseText != null)
            defenseText.text = data.defense.ToString();

        if (hpText != null)
            hpText.text = data.health.ToString();

        UpdateStates();
    }

    private IEnumerator FadeIn()
    {
        canvasGroup.alpha = 0f;

        float time = 0f;

        while (time < fadeDuration)
        {
            time += Time.deltaTime;
            canvasGroup.alpha = Mathf.SmoothStep(0f, 1f, time / fadeDuration);
            yield return null;
        }

        canvasGroup.alpha = 1f;
    }

    private void UpdateStates()
    {
        VillageData data = currentEntry.villageData;

        bool replacePortraits =
            data.currentState == VillageState.EnCombate ||
            data.currentState == VillageState.Derrotado ||
            data.currentState == VillageState.EnCamino ||
            data.currentState == VillageState.Avanzando ||
            data.currentState == VillageState.Retirandose;

        Sprite stateSprite = GetStateSprite(data.currentState);

        for (int i = 0; i < slotImages.Count; i++)
        {
            if (slotImages[i] == null)
                continue;

            if (replacePortraits)
            {
                slotImages[i].enabled = true;
                slotImages[i].sprite = stateSprite;
            }
            else
            {
                if (i < currentEntry.portraits.Count && currentEntry.portraits[i] != null)
                {
                    slotImages[i].enabled = true;
                    slotImages[i].sprite = currentEntry.portraits[i];
                }
                else
                {
                    slotImages[i].enabled = false;
                }
            }
        }
    }

    private Sprite GetStateSprite(VillageState state)
    {
        switch (state)
        {
            case VillageState.EnEspera: return enEsperaSprite;
            case VillageState.EnCamino: return enCaminoSprite;
            case VillageState.EnCombate: return enCombateSprite;
            case VillageState.Avanzando: return avanzandoSprite;
            case VillageState.Retirandose: return retirandoseSprite;
            case VillageState.Derrotado: return derrotadoSprite;
            default: return enEsperaSprite;
        }
    }
}