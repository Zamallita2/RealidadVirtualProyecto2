using TMPro;
using UnityEngine;

public class FightWaveUI : MonoBehaviour
{
    [Header("Manager")]
    public WaveManager waveManager;

    [Header("UI")]
    public TMP_Text salaText;
    public TMP_Text oleadaText;

    private void Update()
    {
        if (waveManager == null)
            return;

        int sala = waveManager.GetCurrentRoomIndex() + 1;
        int oleada = waveManager.GetCurrentWaveNumber();

        if (salaText != null)
            salaText.text = "Sala: " + sala;

        if (oleadaText != null)
            oleadaText.text = "Oleada: " + oleada;
    }
}