using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class RoomEditorSlotUI :
    MonoBehaviour,
    IDropHandler
{
    [Header("UI")]
    public Image iconImage;
    public TMP_Text labelText;
    public Button clearButton;
    public Sprite emptyPlusSprite;

    [HideInInspector]
    public int slotIndex;

    [HideInInspector]
    public RoomEditorPanelController controller;

    public void Setup(
        RoomEditorPanelController editorController,
        int index
    )
    {
        controller =
            editorController;

        slotIndex =
            index;

        if (clearButton != null)
        {
            clearButton.onClick
            .AddListener(
                ClearSlot
            );
        }
    }

    public void SetEmpty()
    {
        iconImage.sprite =
            emptyPlusSprite;

        labelText.text =
            "+";

        clearButton.gameObject
        .SetActive(false);
    }

    public void SetEnemy(
        EnemyGachaData data
    )
    {
        iconImage.sprite =
            data.cardFrontSprite;

        labelText.text =
            data.enemyName;

        clearButton.gameObject
        .SetActive(true);
    }

    public void OnDrop(
        PointerEventData eventData
    )
    {
        if (controller == null)
            return;

        EnemyPaletteItemUI dragged =
        eventData.pointerDrag
        ?
        eventData.pointerDrag
        .GetComponent<
        EnemyPaletteItemUI>()
        :
        null;

        if (dragged == null)
            return;

        EnemyGachaData enemy =
        GachaInventoryManager.Instance
        .GetEnemyData(
            dragged.enemyId
        );

        if (enemy == null)
            return;


        // Boss solo slot 5
        if (
            enemy.isBoss &&
            slotIndex != 4
        )
        {
            Debug.Log(
            "Boss solo puede ir en Slot 5"
            );

            return;
        }

        controller.TryAssignEnemyToSlot(
            dragged.enemyId,
            slotIndex
        );
    }

    private void ClearSlot()
    {
        if (controller != null)
        {
            controller
            .RemoveEnemyFromSlot(
                slotIndex
            );
        }
    }
}