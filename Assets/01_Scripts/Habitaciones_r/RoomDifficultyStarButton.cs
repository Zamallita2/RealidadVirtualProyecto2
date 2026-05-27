using UnityEngine;
using UnityEngine.EventSystems;

public class RoomDifficultyStarButton : MonoBehaviour, IPointerClickHandler
{
    public int difficultyValue = 1;
    public RoomEditorPanelController editorController;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (editorController != null)
            editorController.SetDifficulty(difficultyValue);
    }
}