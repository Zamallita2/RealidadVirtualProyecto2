using UnityEngine;

public class ClickHabitacion : MonoBehaviour
{
    [Header("Número local de habitación")]
    public int numeroHabitacion;


    public void Click()
    {
        //Calcula la habitación real
        //Ejemplo:
        //Piso 1 + Habitación 5 = 5
        //Piso 2 + Habitación 5 = 14
        //Piso 8 + Habitación 3 = 66

        int roomID =
            ((RoomManager.Instance.pisoActual - 1) * 9)
            + numeroHabitacion;


        RoomManager.Instance
            .SeleccionarHabitacion(roomID);


        Debug.Log(
            "PISO: "
            + RoomManager.Instance.pisoActual
            + " | HABITACION LOCAL: "
            + numeroHabitacion
            + " | ROOM ID: "
            + roomID
        );
    }
}