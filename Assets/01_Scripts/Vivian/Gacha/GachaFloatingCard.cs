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

    private RectTransform rect;
    private Vector2 startPos;
    private Vector3 startScale;
    private float offset;
    private bool hovering;

    private void Awake()
    {
        rect = GetComponent<RectTransform>();
        startPos = rect.anchoredPosition;
        startScale = transform.localScale;
        offset = Random.Range(0f, 100f);
    }

    private void Update()
    {
        float wave = Mathf.Sin(Time.time * floatSpeed + offset);

        Vector2 targetPos = startPos + new Vector2(0f, wave * floatAmount);
        Vector3 targetScale = startScale * (1f + wave * pulseAmount);
        Quaternion targetRot = Quaternion.Euler(0f, 0f, wave * rotateAmount);

        if (hovering)
        {
            targetPos += new Vector2(0f, hoverLift);
            targetScale = startScale * hoverScale;
            targetRot = Quaternion.Euler(0f, 0f, wave * rotateAmount * 0.4f);
        }

        rect.anchoredPosition = Vector2.Lerp(rect.anchoredPosition, targetPos, Time.deltaTime * smooth);
        rect.localScale = Vector3.Lerp(rect.localScale, targetScale, Time.deltaTime * smooth);
        rect.localRotation = Quaternion.Lerp(rect.localRotation, targetRot, Time.deltaTime * smooth);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        hovering = true;
        transform.SetAsLastSibling();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        hovering = false;
    }
}