using UnityEngine;
using System;

public class RoomManager : MonoBehaviour
{
    public static RoomManager Instance;

    public int pisoActual = 1;
    public int habitacionSeleccionada = 1;

    public Action<int> OnRoomSelected;
    public Action<int> OnFloorSelected;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        OnFloorSelected?.Invoke(pisoActual);
        OnRoomSelected?.Invoke(habitacionSeleccionada);
    }

    public void SeleccionarPiso(int piso)
    {
        pisoActual = piso;
        OnFloorSelected?.Invoke(piso);
        Debug.Log("PISO: " + piso);
    }

    public void SeleccionarHabitacion(int roomID)
    {
        habitacionSeleccionada = roomID;

        pisoActual = Mathf.CeilToInt(roomID / 9f);

        OnFloorSelected?.Invoke(pisoActual);
        OnRoomSelected?.Invoke(roomID);

        Debug.Log("Habitación: " + roomID + " Piso: " + pisoActual);
    }
}