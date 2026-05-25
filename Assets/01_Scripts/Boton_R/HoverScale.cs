using UnityEngine;
using UnityEngine.EventSystems;

public class HoverScale : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler,
    IPointerDownHandler
{
    [Header("Escala")]
    public float hoverScale = 1.08f;
    public float smoothSpeed = 10f;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip hoverSound;
    public AudioClip clickSound;

    private Vector3 originalScale;
    private Vector3 targetScale;

    void Start()
    {
        originalScale = transform.localScale;
        targetScale = originalScale;
    }

    void Update()
    {
        transform.localScale = Vector3.Lerp(
            transform.localScale,
            targetScale,
            Time.deltaTime * smoothSpeed
        );
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        targetScale = originalScale * hoverScale;

        if (audioSource != null && hoverSound != null)
        {
            audioSource.PlayOneShot(hoverSound);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        targetScale = originalScale;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (audioSource != null && clickSound != null)
        {
            audioSource.PlayOneShot(clickSound);
        }
    }
}