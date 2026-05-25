using UnityEngine;
using UnityEngine.UI;

public class FloorScrollController : MonoBehaviour
{
    [Header("Referencias")]
    public ScrollRect scrollRect;

    [Header("Botones Piso")]
    public RectTransform[] botonesPiso;


    void Start()
    {
        RoomManager.Instance.OnFloorSelected += ScrollToFloor;
    }


    void ScrollToFloor(int piso)
    {
        if (piso < 1 || piso > botonesPiso.Length)
            return;

        Canvas.ForceUpdateCanvases();

        RectTransform boton =
            botonesPiso[piso - 1];

        RectTransform content =
            scrollRect.content;

        RectTransform viewport =
            scrollRect.viewport;


        float contentHeight =
            content.rect.height;

        float viewportHeight =
            viewport.rect.height;


        //posición Y del botón
        float targetY =
            Mathf.Abs(
                boton.anchoredPosition.y
            );

        //centrarlo
        targetY -=
            (viewportHeight / 2);

        targetY +=
            (boton.rect.height / 2);


        //evitar salir de límites
        targetY =
            Mathf.Clamp(
                targetY,
                0,
                contentHeight -
                viewportHeight
            );

        float normalized =
            1 -
            (
                targetY /
                (
                    contentHeight -
                    viewportHeight
                )
            );

        scrollRect.verticalNormalizedPosition =
            normalized;
    }
}