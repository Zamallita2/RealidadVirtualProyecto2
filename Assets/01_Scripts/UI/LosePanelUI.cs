using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LosePanelUI : MonoBehaviour
{
    public static LosePanelUI Instance;

    [Header("UI")]
    public GameObject losePanel;
    public TMP_Text loseMessageText;

    [Header("Escena menú")]
    public string menuSceneName = "Menu";

    [Header("Aldeas")]
    public List<Aldea> aldeas = new List<Aldea>();

    private bool alreadyLost = false;

    [Header("Mensajes derrota aldeanos")]
    [TextArea(4, 8)]
    public List<string> villagerLoseMessages = new List<string>()
    {
        "Todos los aventureros de las aldeas han muerto...\n\nSin aventureros ya no generas ingresos y el reino cae lentamente en la oscuridad.\n\nLa próxima vez administra mejor tus recursos y protege a tus aldeanos.",

        "Las aldeas han quedado vacías.\n\nSin aventureros vivos ya no es posible mantener tu economía.\n\nPiensa mejor tu estrategia para evitar perder a todos tus habitantes.",

        "El último aventurero ha perecido.\n\nTus ingresos se han detenido por completo y tu dominio se ha derrumbado.\n\nProtege mejor a tus aldeas en la próxima partida."
    };

    [Header("Mensajes derrota oleadas")]
    [TextArea(4, 8)]
    public List<string> waveLoseMessages = new List<string>()
    {
        "Asegúrate de armar tus defensas rápidamente.\n\nSi un grupo de aventureros llega y no tienes al menos un defensor preparado, conquistarán tu sala instantáneamente.\n\nTus enemigos aprovecharon tu debilidad y acabaron contigo.",

        "Debes construir defensas desde el inicio.\n\nUna sala sin defensores puede ser conquistada en segundos por cualquier grupo de aventureros.\n\nTus enemigos atravesaron tus dominios sin resistencia.",

        "Tus defensas fueron insuficientes.\n\nRecuerda que si los aventureros encuentran una sala indefensa, la tomarán al instante.\n\nPrepara unidades cuanto antes para sobrevivir a las oleadas."
    };

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        losePanel.SetActive(false);
    }

    // SOLO para derrota por oleadas
    public void InvokeLoseByWave()
    {
        if(alreadyLost)
            return;

        alreadyLost = true;

        ShowRandomMessage(waveLoseMessages);
    }

    // Las aldeas llaman esto cuando mueren
    public void CheckIfLose()
    {
        if(alreadyLost)
            return;

        bool allDead = true;

        foreach(Aldea aldea in aldeas)
        {
            if(aldea != null && aldea.vidas<=0)
            {
                allDead = false;
                break;
            }
        }

        if(allDead)
        {
            alreadyLost = true;

            ShowRandomMessage(villagerLoseMessages);
        }
    }

    private void ShowRandomMessage(List<string> messageList)
    {
        losePanel.SetActive(true);

        int randomIndex = Random.Range(0, messageList.Count);

        loseMessageText.text = messageList[randomIndex];

        Time.timeScale = 0f;
    }

    public void BackToMenu()
    {
        Time.timeScale = 1f;

        SceneManager.LoadScene(menuSceneName);
    }
}