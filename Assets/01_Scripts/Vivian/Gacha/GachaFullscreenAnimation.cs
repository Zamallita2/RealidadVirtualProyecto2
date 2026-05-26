using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class GachaFullscreenAnimation : MonoBehaviour
{
    [Header("Root")]
    public GameObject animationRoot;

    [Header("Carta principal")]
    public RectTransform mainCard;
    public Image mainCardImage;

    [Header("Cartas extra")]
    public RectTransform[] extraCards;
    public Image[] extraCardImages;

    [Header("Efectos")]
    public CanvasGroup darkFade;
    public CanvasGroup flash;

    [Header("Flash sin sprite")]
    public Graphic flashGraphic;

    public Image glowImage;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip startSound;
    public AudioClip spinSound;
    public AudioClip flashSound;

    [Header("Tiempos")]
    public float introTime = 0.45f;
    public float spinTime = 1.35f;
    public float revealTime = 0.45f;

    [Header("Movimiento")]
    public float rotationSpeed = 780f;
    public float floatAmount = 18f;
    public float shakeAmount = 10f;

    [Header("Colores por rareza")]
    public Color commonColor = new Color(0.04f, 0.35f, 0.12f, 1f);
    public Color rareColor = new Color(0.04f, 0.18f, 0.75f, 1f);
    public Color epicColor = new Color(0.34f, 0.05f, 0.65f, 1f);
    public Color legendaryColor = new Color(0.85f, 0.04f, 0.04f, 1f);

    private Vector2 mainCardStartPos;

    private void Awake()
    {
        if (mainCard != null)
            mainCardStartPos = mainCard.anchoredPosition;

        HideInstant();
    }

    public IEnumerator PlayAnimation(Sprite cardBackSprite, GachaRarity rarity)
    {
        Color rarityColor = GetRarityColor(rarity);

        if (animationRoot != null)
            animationRoot.SetActive(true);

        ApplyRarityColor(rarityColor);

        SetAlpha(darkFade, 0f);
        SetAlpha(flash, 0f);

        if (glowImage != null)
        {
            glowImage.gameObject.SetActive(true);
            glowImage.transform.localScale = Vector3.one * 0.35f;
        }

        SetupCard(mainCard, mainCardImage, cardBackSprite, true);

        for (int i = 0; i < extraCards.Length; i++)
        {
            if (i < extraCardImages.Length)
                SetupCard(extraCards[i], extraCardImages[i], cardBackSprite, false);
        }

        Play(startSound);

        yield return StartCoroutine(FadeDarkness());
        yield return StartCoroutine(IntroCards());
        yield return StartCoroutine(OrbitAndCharge(rarityColor));
        yield return StartCoroutine(PremiumRevealFlash(rarityColor));

        HideInstant();
    }

    private void SetupCard(RectTransform card, Image image, Sprite sprite, bool main)
    {
        if (card == null) return;

        card.gameObject.SetActive(true);
        card.localScale = Vector3.zero;
        card.rotation = Quaternion.identity;

        if (main)
            card.anchoredPosition = mainCardStartPos;

        if (image != null)
        {
            image.sprite = sprite;
            image.color = Color.white;
            image.preserveAspect = true;
        }
    }

    private IEnumerator FadeDarkness()
    {
        float t = 0f;

        while (t < introTime)
        {
            t += Time.deltaTime;
            float p = Mathf.SmoothStep(0f, 1f, t / introTime);

            SetAlpha(darkFade, Mathf.Lerp(0f, 0.82f, p));

            if (glowImage != null)
                glowImage.transform.localScale =
                    Vector3.Lerp(Vector3.one * 0.35f, Vector3.one * 1.15f, p);

            yield return null;
        }
    }

    private IEnumerator IntroCards()
    {
        float t = 0f;

        Vector2[] endPositions = new Vector2[extraCards.Length];

        for (int i = 0; i < extraCards.Length; i++)
        {
            if (extraCards[i] == null) continue;

            float angle = i * Mathf.PI * 2f / Mathf.Max(1, extraCards.Length);
            endPositions[i] = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * 170f;
            extraCards[i].anchoredPosition = Vector2.zero;
        }

        while (t < introTime)
        {
            t += Time.deltaTime;
            float p = Mathf.SmoothStep(0f, 1f, t / introTime);

            if (mainCard != null)
                mainCard.localScale =
                    Vector3.Lerp(Vector3.zero, Vector3.one * 0.95f, p);

            for (int i = 0; i < extraCards.Length; i++)
            {
                if (extraCards[i] == null) continue;

                extraCards[i].localScale =
                    Vector3.Lerp(Vector3.zero, Vector3.one * 0.52f, p);

                extraCards[i].anchoredPosition =
                    Vector2.Lerp(Vector2.zero, endPositions[i], p);
            }

            yield return null;
        }
    }

    private IEnumerator OrbitAndCharge(Color rarityColor)
    {
        Play(spinSound);

        float t = 0f;

        while (t < spinTime)
        {
            t += Time.deltaTime;
            float progress = t / spinTime;

            float pulse = 1f + Mathf.Sin(Time.time * 8f) * 0.08f;
            float floating = Mathf.Sin(Time.time * 5f) * floatAmount;
            float shake = Mathf.Sin(Time.time * 35f) * shakeAmount * progress;

            if (glowImage != null)
            {
                glowImage.color = new Color(rarityColor.r, rarityColor.g, rarityColor.b, 0.65f);
                glowImage.transform.localScale =
                    Vector3.one * Mathf.Lerp(1.1f, 1.55f, progress) * pulse;
            }

            if (mainCard != null)
            {
                mainCard.Rotate(0f, 0f, rotationSpeed * Time.deltaTime);
                mainCard.anchoredPosition = mainCardStartPos + new Vector2(shake, floating);
                mainCard.localScale =
                    Vector3.Lerp(Vector3.one * 0.95f, Vector3.one * 1.15f, progress);
            }

            for (int i = 0; i < extraCards.Length; i++)
            {
                if (extraCards[i] == null) continue;

                float angle =
                    Time.time * 1.7f +
                    i * Mathf.PI * 2f / Mathf.Max(1, extraCards.Length);

                Vector2 orbit =
                    new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) *
                    Mathf.Lerp(170f, 95f, progress);

                extraCards[i].anchoredPosition = orbit;
                extraCards[i].Rotate(0f, 0f, -rotationSpeed * 0.45f * Time.deltaTime);
                extraCards[i].localScale =
                    Vector3.Lerp(Vector3.one * 0.52f, Vector3.one * 0.25f, progress);
            }

            yield return null;
        }
    }

    private IEnumerator PremiumRevealFlash(Color rarityColor)
    {
        Play(flashSound);

        if (flashGraphic != null)
        {
            Color c = rarityColor;
            c.a = 1f;
            flashGraphic.color = c;
        }

        yield return StartCoroutine(FlashPulse(0f, 1f, 0.16f));
        yield return StartCoroutine(FlashPulse(1f, 0.25f, 0.12f));
        yield return StartCoroutine(FlashPulse(0.25f, 1f, 0.10f));
        yield return StartCoroutine(FlashPulse(1f, 0f, revealTime));

        if (mainCard != null)
            mainCard.localScale = Vector3.one * 1.5f;
    }

    private IEnumerator FlashPulse(float from, float to, float time)
    {
        float t = 0f;

        while (t < time)
        {
            t += Time.deltaTime;
            float p = Mathf.SmoothStep(0f, 1f, t / time);

            SetAlpha(flash, Mathf.Lerp(from, to, p));

            if (mainCard != null)
            {
                float scale = Mathf.Lerp(1.15f, 1.55f, p);
                mainCard.localScale = Vector3.one * scale;
            }

            yield return null;
        }
    }

    private void ApplyRarityColor(Color color)
    {
        if (glowImage != null)
        {
            Color glowColor = color;
            glowColor.a = 0.85f;
            glowImage.color = glowColor;
        }

        if (flashGraphic != null)
        {
            Color flashColor = color;
            flashColor.a = 1f;
            flashGraphic.color = flashColor;
        }

        if (darkFade != null)
        {
            Image darkImage = darkFade.GetComponent<Image>();

            if (darkImage != null)
            {
                Color dark = color;
                dark.r *= 0.18f;
                dark.g *= 0.18f;
                dark.b *= 0.18f;
                dark.a = 1f;
                darkImage.color = dark;
            }
        }
    }

    private Color GetRarityColor(GachaRarity rarity)
    {
        switch (rarity)
        {
            case GachaRarity.Comun:
                return commonColor;
            case GachaRarity.Raro:
                return rareColor;
            case GachaRarity.Epico:
                return epicColor;
            case GachaRarity.Legendario:
                return legendaryColor;
            default:
                return Color.white;
        }
    }

    private void HideInstant()
    {
        SetAlpha(darkFade, 0f);
        SetAlpha(flash, 0f);

        if (glowImage != null)
            glowImage.gameObject.SetActive(false);

        if (mainCard != null)
            mainCard.gameObject.SetActive(false);

        foreach (RectTransform card in extraCards)
        {
            if (card != null)
                card.gameObject.SetActive(false);
        }

        if (animationRoot != null)
            animationRoot.SetActive(false);
    }

    private void SetAlpha(CanvasGroup group, float alpha)
    {
        if (group != null)
            group.alpha = alpha;
    }

    private void Play(AudioClip clip)
    {
        if (audioSource != null && clip != null)
            audioSource.PlayOneShot(clip);
    }
}