using UnityEngine;

public class EdificioSync : MonoBehaviour
{
    public BotonPiso[] botonesPiso;

    public ClickHabitacion[] habitaciones;


    void Start()
    {
        RoomManager.Instance
        .OnRoomSelected +=
        ActualizarEdificio;
    }


    void ActualizarEdificio(
        int roomID
    )
    {
        //nada seleccionado
        if (roomID == -1)
        {
            return;
        }

        int piso =
        Mathf.CeilToInt(
            roomID / 9f
        );

        int habitacionLocal =
        roomID -
        (
            (piso - 1) * 9
        );

        Debug.Log(
            "PISO " +
            piso +
            " HABITACION " +
            habitacionLocal
        );

        // SOLO visual
        // NO volver a llamar:
        // SeleccionarPiso()
    }


    void OnDestroy()
    {
        if (RoomManager.Instance != null)
        {
            RoomManager.Instance
            .OnRoomSelected -=
            ActualizarEdificio;
        }
    }
}