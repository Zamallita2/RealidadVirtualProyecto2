using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UICardIdleAnimator : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Animación segura")]
    public float pulseSpeed = 1.5f;
    public float pulseAmount = 0.025f;
    public float hoverScale = 1.05f;

    [Header("Glow opcional")]
    public Image glowImage;
    public float glowMin = 0.04f;
    public float glowMax = 0.18f;

    private Vector3 baseScale;
    private Vector3 targetScale;
    private CanvasGroup canvasGroup;

    private void Awake()
    {
        baseScale = transform.localScale;
        targetScale = baseScale;

        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        canvasGroup.alpha = 0f;
    }

    private void Start()
    {
        StartCoroutine(Appear());
    }

    private System.Collections.IEnumerator Appear()
    {
        float time = 0f;
        float duration = 0.45f + transform.GetSiblingIndex() * 0.08f;

        while (time < duration)
        {
            time += Time.deltaTime;
            canvasGroup.alpha = Mathf.Clamp01(time / duration);
            yield return null;
        }

        canvasGroup.alpha = 1f;
    }

    private void Update()
    {
        float pulse = Mathf.Sin(Time.time * pulseSpeed) * pulseAmount;
        Vector3 idleScale = targetScale + Vector3.one * pulse;

        transform.localScale = Vector3.Lerp(transform.localScale, idleScale, Time.deltaTime * 7f);

        if (glowImage != null)
        {
            glowImage.raycastTarget = false;

            float g = (Mathf.Sin(Time.time * pulseSpeed) + 1f) * 0.5f;
            Color c = glowImage.color;
            c.a = Mathf.Lerp(glowMin, glowMax, g);
            glowImage.color = c;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        targetScale = baseScale * hoverScale;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        targetScale = baseScale;
    }
}