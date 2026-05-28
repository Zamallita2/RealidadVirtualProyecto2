using UnityEngine;
using TMPro;
using System.Collections;

public class AldeaTexto : MonoBehaviour
{
    public static AldeaTexto Instance;

    [Header("Referencias")]
    [SerializeField] private GameObject canvas;
    [SerializeField] private TMP_Text texto;
    private bool isActive;

    private void Awake()
    {
        Instance = this;
        canvas.SetActive(false);
    }

    public void Mostrar(string mensaje)
    {
        if (!isActive)
        {
            canvas.SetActive(true);
        }
        texto.text=mensaje;
    }
    public void Quitar()
    {
        canvas.SetActive(false);
    }
}