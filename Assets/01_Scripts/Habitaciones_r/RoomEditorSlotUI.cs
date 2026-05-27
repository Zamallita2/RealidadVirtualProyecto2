using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class RoomEditorSlotUI : MonoBehaviour, IDropHandler
{
    [Header("UI")]
    public Image iconImage;
    public TMP_Text labelText;
    public Button clearButton;
    public Sprite emptyPlusSprite;

    [HideInInspector] public int slotIndex;
    [HideInInspector] public RoomEditorPanelController controller;

    public void Setup(RoomEditorPanelController editorController, int index)
    {
        controller = editorController;
        slotIndex = index;

        if (clearButton != null)
            clearButton.onClick.AddListener(ClearSlot);
    }

    public void SetEmpty()
    {
        if (iconImage != null)
            iconImage.sprite = emptyPlusSprite;

        if (labelText != null)
            labelText.text = "+";

        if (clearButton != null)
            clearButton.gameObject.SetActive(false);
    }

    public void SetEnemy(EnemyGachaData data)
    {
        if (iconImage != null)
            iconImage.sprite = data.cardFrontSprite;

        if (labelText != null)
            labelText.text = data.enemyName;

        if (clearButton != null)
            clearButton.gameObject.SetActive(true);
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (controller == null)
            return;

        EnemyPaletteItemUI dragged = eventData.pointerDrag
            ? eventData.pointerDrag.GetComponent<EnemyPaletteItemUI>()
            : null;

        if (dragged != null)
        {
            controller.TryAssignEnemyToSlot(dragged.enemyId, slotIndex);
        }
    }

    private void ClearSlot()
    {
        if (controller != null)
            controller.RemoveEnemyFromSlot(slotIndex);
    }
}