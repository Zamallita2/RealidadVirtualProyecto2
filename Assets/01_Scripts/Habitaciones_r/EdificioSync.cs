using UnityEngine;

public class EdificioSync : MonoBehaviour
{
    public BotonPiso[] botonesPiso;

    public ClickHabitacion[] habitaciones;

    void Start()
    {
        RoomManager.Instance.OnRoomSelected +=
            ActualizarEdificio;
    }

    void ActualizarEdificio(int roomID)
    {
        int piso =
        Mathf.CeilToInt(
            roomID / 9f
        );

        int habitacionLocal =
        roomID -
        ((piso - 1) * 9);

        Debug.Log(
            "PISO " + piso +
            " HABITACION " +
            habitacionLocal
        );

        RoomManager.Instance
        .SeleccionarPiso(
            piso
        );
    }
}