using UnityEngine;
using System.Collections;

public class FadeImage : MonoBehaviour
{
    public static FadeImage Instance;

    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private float fadeTime = 0.2f;

    private Coroutine currentFade;

    private void Awake()
    {
        Instance = this;
        Ocultar();
    }

    public void Mostrar()
    {
        if(currentFade != null)
            StopCoroutine(currentFade);

        currentFade = StartCoroutine(Fade(1));
    }

    public void Ocultar()
    {
        if(currentFade != null)
            StopCoroutine(currentFade);

        currentFade = StartCoroutine(Fade(0));
    }

    public void OcultarInstante()
    {
        if(currentFade != null)
            StopCoroutine(currentFade);
            
        canvasGroup.alpha = 0;
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