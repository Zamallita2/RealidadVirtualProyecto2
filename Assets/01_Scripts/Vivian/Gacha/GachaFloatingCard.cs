using UnityEngine;
using UnityEngine.EventSystems;

public class GachaFloatingCard : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Flotación")]
    public float floatSpeed = 1.2f;
    public float floatAmount = 10f;
    public float rotateAmount = 5f;
    public float pulseAmount = 0.035f;

    [Header("Hover")]
    public float hoverScale = 1.08f;
    public float hoverLift = 12f;
    public float smooth = 8f;

    [Header("Audio")]
    public AudioSource audioSource;

    [Tooltip("Hover del oso")]
    public AudioClip bearHover;

    [Tooltip("Hover del conejo")]
    public AudioClip rabbitHover;

    [Tooltip("Hover del tren")]
    public AudioClip trainHover;

    [Tooltip("Hover del jefe")]
    public AudioClip bossHover;

    [Header("Tipo")]
    public CardHoverType cardType;

    private RectTransform rect;
    private Vector2 startPos;
    private Vector3 startScale;
    private float offset;
    private bool hovering;

    public enum CardHoverType
    {
        Bear,
        Rabbit,
        Train,
        Boss
    }

    private void Awake()
    {
        rect = GetComponent<RectTransform>();

        startPos = rect.anchoredPosition;

        startScale = rect.localScale;

        offset = Random.Range(0f, 100f);

        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();

            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0f;
            audioSource.volume = 0.7f;
        }
    }

    private void Update()
    {
        float wave =
            Mathf.Sin(Time.time * floatSpeed + offset);

        Vector2 targetPos =
            startPos +
            new Vector2(0f, wave * floatAmount);

        Vector3 targetScale =
            startScale *
            (1f + wave * pulseAmount);

        Quaternion targetRot =
            Quaternion.Euler(
                0f,
                0f,
                wave * rotateAmount
            );

        if (hovering)
        {
            targetPos +=
                new Vector2(0f, hoverLift);

            targetScale =
                startScale * hoverScale;

            targetRot =
                Quaternion.Euler(
                    0f,
                    0f,
                    wave * rotateAmount * 0.4f
                );
        }

        rect.anchoredPosition =
            Vector2.Lerp(
                rect.anchoredPosition,
                targetPos,
                Time.deltaTime * smooth
            );

        rect.localScale =
            Vector3.Lerp(
                rect.localScale,
                targetScale,
                Time.deltaTime * smooth
            );

        rect.localRotation =
            Quaternion.Lerp(
                rect.localRotation,
                targetRot,
                Time.deltaTime * smooth
            );
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        hovering = true;

        transform.SetAsLastSibling();

        PlayHoverSound();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        hovering = false;
    }

    private void PlayHoverSound()
    {
        AudioClip clip = null;

        switch (cardType)
        {
            case CardHoverType.Bear:
                clip = bearHover;
                break;

            case CardHoverType.Rabbit:
                clip = rabbitHover;
                break;

            case CardHoverType.Train:
                clip = trainHover;
                break;

            case CardHoverType.Boss:
                clip = bossHover;
                break;
        }

        if (clip != null)
        {
            audioSource.pitch =
                Random.Range(0.94f, 1.06f);

            audioSource.PlayOneShot(clip);
        }
    }
    private System.Collections.IEnumerator StopHoverAudio()
    {
        float stopTime = 1f;

        // SOLO el tren dura menos
        if (cardType == CardHoverType.Train)
        {
            stopTime = 0.45f;
        }

        yield return new WaitForSeconds(stopTime);

        audioSource.Stop();
    }
}