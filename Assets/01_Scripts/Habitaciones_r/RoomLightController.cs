using UnityEngine;

public class RoomLightController :
    MonoBehaviour
{
    [Header("Habitación local")]
    public int numeroHabitacion;

    [Header("Objeto luces")]
    public GameObject luces;


    void Start()
    {
        RoomManager.Instance
        .OnRoomSelected +=
        ActualizarLuces;

        luces.SetActive(false);
    }


    void ActualizarLuces(
        int roomID
    )
    {
        int piso =
        RoomManager.Instance
        .pisoActual;

        int roomLocal =
        roomID -
        ((piso - 1) * 9);

        bool activa =
            roomLocal ==
            numeroHabitacion;

        luces.SetActive(activa);
    }
}