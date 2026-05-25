using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RoomScrollUI : MonoBehaviour
{
    public Transform content;
    public GameObject roomButtonPrefab;

    public Button btnTodos;
    public Button btnDesbloqueadas;
    public Button btnJefe;
    public Button btnFavoritas;

    List<RoomButtonData> roomButtons =
        new List<RoomButtonData>();


    void Start()
    {
        CreateRooms();

        btnTodos.onClick.AddListener(FilterTodos);

        btnDesbloqueadas.onClick.AddListener(
            FilterDesbloqueadas);

        btnJefe.onClick.AddListener(
            FilterJefe);

        btnFavoritas.onClick.AddListener(
            FilterFavoritas);

        RoomManager.Instance.OnRoomSelected +=
            ActualizarSeleccion;
    }


    void CreateRooms()
    {
        for (int i = 1; i <= 99; i++)
        {
            GameObject obj =
            Instantiate(
                roomButtonPrefab,
                content
            );

            RoomButtonData data =
            obj.GetComponent<RoomButtonData>();

            data.Setup(i);

            roomButtons.Add(data);
        }
    }


    void ActualizarSeleccion(int roomID)
    {
        foreach (var room in roomButtons)
        {
            room.SetSelected(
                room.roomID == roomID
            );
        }
    }


    void FilterTodos()
    {
        foreach (var room in roomButtons)
        {
            room.gameObject.SetActive(true);
        }
    }

    void FilterDesbloqueadas()
    {
        foreach (var room in roomButtons)
        {
            room.gameObject.SetActive(
                room.unlocked
            );
        }
    }

    void FilterJefe()
    {
        foreach (var room in roomButtons)
        {
            room.gameObject.SetActive(
                room.hasBoss
            );
        }
    }

    void FilterFavoritas()
    {
        foreach (var room in roomButtons)
        {
            room.gameObject.SetActive(
                room.favorite
            );
        }
    }
}