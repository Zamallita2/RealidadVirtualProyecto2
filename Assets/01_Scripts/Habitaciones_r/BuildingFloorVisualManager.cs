using UnityEngine;

public class BuildingFloorVisualManager : MonoBehaviour
{
    [Header("9 habitaciones del edificio actual")]
    public BuildingRoomVisual[] roomVisuals; // localRoomNumber 1..9

    private int currentFloor = 1;

    private void Start()
    {
        if (RoomManager.Instance != null)
        {
            RoomManager.Instance.OnFloorSelected += OnFloorSelected;
            RoomManager.Instance.OnRoomSelected += OnRoomSelected;
        }

        if (RoomConfigManager.Instance != null)
        {
            RoomConfigManager.Instance.OnRoomConfigChanged += OnRoomConfigChanged;
        }

        currentFloor = RoomManager.Instance != null ? RoomManager.Instance.pisoActual : 1;
        RefreshFloor(currentFloor);
    }

    private void OnDestroy()
    {
        if (RoomManager.Instance != null)
        {
            RoomManager.Instance.OnFloorSelected -= OnFloorSelected;
            RoomManager.Instance.OnRoomSelected -= OnRoomSelected;
        }

        if (RoomConfigManager.Instance != null)
        {
            RoomConfigManager.Instance.OnRoomConfigChanged -= OnRoomConfigChanged;
        }
    }

    private void OnFloorSelected(int floor)
    {
        currentFloor = floor;
        RefreshFloor(currentFloor);
    }

    private void OnRoomSelected(int roomID)
    {
        // No hace falta cambiar nada extra aquí, pero lo dejamos por si luego quieres resaltar la sala.
    }

    private void OnRoomConfigChanged(int roomID)
    {
        int roomFloor = Mathf.CeilToInt(roomID / 9f);
        if (roomFloor == currentFloor)
        {
            RefreshFloor(currentFloor);
        }
    }

    public void RefreshFloor(int floor)
    {
        currentFloor = floor;

        for (int i = 0; i < roomVisuals.Length; i++)
        {
            BuildingRoomVisual roomVisual = roomVisuals[i];
            if (roomVisual == null)
                continue;

            int roomID = ((currentFloor - 1) * 9) + roomVisual.localRoomNumber;
            RoomConfigData config = RoomConfigManager.Instance.GetRoomConfig(roomID);

            roomVisual.RenderRoom(config);
        }
    }
}