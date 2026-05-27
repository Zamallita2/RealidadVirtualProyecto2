using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Iniciar : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private VillageManager villageManager;
    [SerializeField] private AdventurerManager adventurerManager;

    [Header("UI contador")]
    public TMP_Text contadorAtaqueText;

    [Header("Intervals (seconds)")]
    [SerializeField] private float minIntervalSeconds = 15f;
    [SerializeField] private float maxIntervalSeconds = 20f;
    [SerializeField] private float retryDelaySeconds = 1f;

    private Coroutine loopCoroutine;
    private float tiempoRestante;
    private bool esperandoSiguienteAtaque;

    void Start()
    {
        iniciar();
    }

    public void iniciar()
    {
        if (loopCoroutine != null)
            return;

        if (villageManager == null)
            villageManager = FindFirstObjectByType<VillageManager>();

        if (adventurerManager == null)
            adventurerManager = FindFirstObjectByType<AdventurerManager>();

        if (villageManager == null || adventurerManager == null)
        {
            Debug.LogError("Iniciar: faltan referencias (VillageManager/AdventurerManager).");
            return;
        }

        loopCoroutine = StartCoroutine(VillageFightLoop());
    }

    IEnumerator VillageFightLoop()
    {
        while (true)
        {
            if (!adventurerManager.IsFightRunning)
            {
                tiempoRestante = Random.Range(minIntervalSeconds, maxIntervalSeconds);
                esperandoSiguienteAtaque = true;

                while (tiempoRestante > 0f && !adventurerManager.IsFightRunning)
                {
                    tiempoRestante -= Time.deltaTime;
                    ActualizarTextoContador();
                    yield return null;
                }

                esperandoSiguienteAtaque = false;
                ActualizarTextoContador();

                while (!TryStartRandomVillageFight())
                {
                    if (contadorAtaqueText != null)
                        contadorAtaqueText.text = "Buscando aldea...";

                    yield return new WaitForSeconds(retryDelaySeconds);
                }
            }

            yield return null;
        }
    }

    private void ActualizarTextoContador()
    {
        if (contadorAtaqueText == null)
            return;

        if (adventurerManager != null && adventurerManager.IsFightRunning)
        {
            contadorAtaqueText.text = "Aldea atacando";
            return;
        }

        if (esperandoSiguienteAtaque)
        {
            int segundos = Mathf.CeilToInt(tiempoRestante);
            contadorAtaqueText.text = "Siguiente aldea en: " + segundos + "s";
        }
        else
        {
            contadorAtaqueText.text = "";
        }
    }

    bool TryStartRandomVillageFight()
    {
        if (villageManager.aldeas == null ||
            villageManager.aldeas.Length == 0)
            return false;

        Aldea aldea = villageManager.ObtenerAldeaAleatoria();

        if (aldea == null ||
            aldea.aventureros == null ||
            aldea.aventureros.Count == 0)
        {
            return false;
        }

        List<AdventurerSetup> setups = new();

        foreach (AventureroData a in aldea.aventureros)
        {
            if (a == null || a.prefab == null)
                continue;

            setups.Add(new AdventurerSetup
            {
                prefab = a.prefab,
                level = a.nivel
            });
        }

        if (setups.Count == 0)
            return false;

        adventurerManager.Initialize(setups);
        adventurerManager.StartFight(aldea);
        return true;
    }
}