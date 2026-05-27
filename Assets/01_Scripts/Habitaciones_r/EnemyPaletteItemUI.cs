using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
public class EnemyPaletteItemUI :
    MonoBehaviour,
    IBeginDragHandler,
    IDragHandler,
    IEndDragHandler
{
    [Header("UI")]
    public Image iconImage;
    public TMP_Text nameText;
    public TMP_Text copiesText;

    [HideInInspector]
    public string enemyId;

    [HideInInspector]
    public bool canDrag = true;

    private Canvas rootCanvas;
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Transform originalParent;
    private Vector3 originalScale;

    private EnemyGachaData enemyData;

    private void Awake()
    {
        rectTransform =
            GetComponent<RectTransform>();

        canvasGroup =
            GetComponent<CanvasGroup>();

        rootCanvas =
            GetComponentInParent<Canvas>();

        originalScale =
            transform.localScale;
    }

    public void Setup(
        EnemyGachaData data,
        int copies,
        bool draggable
    )
    {
        enemyData = data;

        enemyId =
            data.enemyId;

        canDrag =
            draggable;

        bool roomAllowsBoss =
        RoomConfigManager.Instance
        .IsBossAllowedInRoom(
            RoomManager.Instance
            .habitacionSeleccionada
        );

        bool blocked =
        data.isBoss &&
        !roomAllowsBoss;

        if (iconImage != null)
        {
            if (
                blocked &&
                data.roomBlockedSprite != null
            )
            {
                iconImage.sprite =
                    data.roomBlockedSprite;
            }
            else
            {
                iconImage.sprite =
                    data.cardFrontSprite;
            }
        }

        if (nameText != null)
        {
            nameText.text =
                data.enemyName;
        }

        if (copiesText != null)
        {
            copiesText.text =
                "x" + copies;
        }

        canDrag =
        !blocked &&
        draggable;

        canvasGroup.alpha =
        canDrag
        ? 1f
        : 0.45f;

        canvasGroup.blocksRaycasts =
            canDrag;
    }

    public void OnBeginDrag(
        PointerEventData eventData
    )
    {
        if (!canDrag)
            return;

        originalParent =
            transform.parent;

        transform.SetParent(
            rootCanvas.transform,
            true
        );

        transform.SetAsLastSibling();

        canvasGroup.alpha =
            0.85f;

        canvasGroup.blocksRaycasts =
            false;

        transform.localScale =
            originalScale * 1.03f;
    }

    public void OnDrag(
        PointerEventData eventData
    )
    {
        if (!canDrag)
            return;

        rectTransform.position =
            eventData.position;
    }

    public void OnEndDrag(
        PointerEventData eventData
    )
    {
        if (!canDrag)
            return;

        transform.SetParent(
            originalParent,
            true
        );

        transform.localScale =
            originalScale;

        canvasGroup.alpha =
            1f;

        canvasGroup.blocksRaycasts =
            true;
    }
}