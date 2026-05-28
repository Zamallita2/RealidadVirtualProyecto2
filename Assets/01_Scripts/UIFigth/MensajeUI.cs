using UnityEngine;
using TMPro;
using System.Collections;

public class MensajeUI : MonoBehaviour
{
    public static MensajeUI Instance;

    [Header("Referencias")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private TMP_Text texto;

    [Header("Configuración")]
    [SerializeField] private float fadeTime = 0.2f;
    [SerializeField] private float tiempoVisible = 2f;
    [SerializeField] private float parpadeoTime = 0.08f;

    private Coroutine rutinaActual;

    private void Awake()
    {
        Instance = this;

        canvasGroup.alpha = 0;
    }

    public void Mostrar(string mensaje)
    {
        texto.text = mensaje;

        if(rutinaActual != null)
        {
            StopCoroutine(rutinaActual);
            rutinaActual = StartCoroutine(ActualizarMensaje());
            return;
        }

        rutinaActual = StartCoroutine(MostrarRutina());
    }

    IEnumerator MostrarRutina()
    {
        yield return Fade(1);

        yield return new WaitForSeconds(tiempoVisible);

        yield return Fade(0);

        rutinaActual = null;
    }

    IEnumerator ActualizarMensaje()
    {
        // Mini parpadeo
        canvasGroup.alpha = 0.4f;

        yield return new WaitForSeconds(parpadeoTime);

        canvasGroup.alpha = 1;

        // Reinicia tiempo
        yield return new WaitForSeconds(tiempoVisible);

        yield return Fade(0);

        rutinaActual = null;
    }

    IEnumerator Fade(float objetivo)
    {
        float inicio = canvasGroup.alpha;
        float tiempo = 0;

        while(tiempo < fadeTime)
        {
            tiempo += Time.deltaTime;

            canvasGroup.alpha = Mathf.Lerp(
                inicio,
                objetivo,
                tiempo / fadeTime
            );

            yield return null;
        }

        canvasGroup.alpha = objetivo;
    }
}