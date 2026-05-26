using UnityEngine;

public class BotonPiso : MonoBehaviour
{
    public int numeroPiso;

    ButtonSelectionVisual visual;

    void Start()
    {
        visual =
        GetComponent<ButtonSelectionVisual>();

        RoomManager.Instance
        .OnFloorSelected +=
        ActualizarVisual;
    }

    public void SeleccionarPiso()
    {
        RoomManager.Instance
        .SeleccionarPiso(
            numeroPiso
        );
    }

    void ActualizarVisual(
        int piso
    )
    {
        visual.SetSelected(
            numeroPiso == piso
        );
    }
}