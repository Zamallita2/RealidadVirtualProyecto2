using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RoomButtonData : MonoBehaviour
{
    public int roomID;

    public bool unlocked;
    public bool hasBoss;
    public bool favorite;

    public TMP_Text roomText;

    Button btn;

    ButtonSelectionVisual visual;

    void Awake()
    {
        btn = GetComponent<Button>();

        visual =
        GetComponent<ButtonSelectionVisual>();

        btn.onClick.AddListener(
            OnClickRoom
        );
    }

    public void Setup(int id)
    {
        roomID = id;

        roomText.text = id.ToString();

        unlocked = id <= 30;

        hasBoss = id % 10 == 0;

        favorite =
        id == 23 ||
        id == 50;
    }

    void OnClickRoom()
    {
        RoomManager.Instance
        .SeleccionarHabitacion(
            roomID
        );

        // SONIDO 2
        UISoundManager.Instance
        .PlayRoomButtonSound();
    }

    public void SetSelected(bool state)
    {
        visual.SetSelected(state);
    }
}