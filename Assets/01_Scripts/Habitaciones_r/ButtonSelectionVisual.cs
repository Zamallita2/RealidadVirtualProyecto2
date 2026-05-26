using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ButtonSelectionVisual :
    MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler
{
    [Header("Sprites")]
    public Sprite normalSprite;
    public Sprite selectedSprite;

    Image img;

    bool isSelected = false;

    void Awake()
    {
        img = GetComponent<Image>();
    }

    public void SetSelected(bool state)
    {
        isSelected = state;

        if (isSelected)
        {
            img.sprite = selectedSprite;
        }
        else
        {
            img.sprite = normalSprite;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        transform.localScale =
            Vector3.one * 1.08f;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        transform.localScale =
            Vector3.one;
    }
}