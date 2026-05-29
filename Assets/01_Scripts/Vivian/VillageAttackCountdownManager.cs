using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class VillageAttackCountdownManager : MonoBehaviour
{
    [Header("Managers")]
    public VillageManager villageManager;
    public AdventurerManager adventurerManager;

    [Header("UI")]
    public TMP_Text countdownText;

    [Header("Tiempo para el siguiente ataque")]
    public float minTimeToAttack = 60f;
    public float maxTimeToAttack = 190f;

    [Header("Reintento si no encuentra aldea")]
    public float retryDelay = 1f;

    private float currentTime;
    private bool isCounting = false;
    private Coroutine countdownCoroutine;

    private void Start()
    {
        FindManagersIfNeeded();
        StartCountdown();
    }

    private void FindManagersIfNeeded()
    {
        if (villageManager == null)
            villageManager = FindFirstObjectByType<VillageManager>();

        if (adventurerManager == null)
            adventurerManager = FindFirstObjectByType<AdventurerManager>();
    }

    public void StartCountdown()
    {
        if (countdownCoroutine != null)
            StopCoroutine(countdownCoroutine);

        countdownCoroutine = StartCoroutine(CountdownLoop());
    }

    private IEnumerator CountdownLoop()
    {
        while (true)
        {
            FindManagersIfNeeded();

            if (villageManager == null || adventurerManager == null)
            {
                SetText("Faltan managers");
                yield return new WaitForSeconds(1f);
                continue;
            }

            if (adventurerManager.IsFightRunning)
            {
                SetText("Aldea atacando...");
                yield return null;
                continue;
            }

            currentTime = Random.Range(minTimeToAttack, maxTimeToAttack);
            isCounting = true;

            while (currentTime > 0f)
            {
                if (adventurerManager.IsFightRunning)
                    break;

                currentTime -= Time.deltaTime;
                UpdateCountdownText();

                yield return null;
            }

            isCounting = false;

            if (!adventurerManager.IsFightRunning)
            {
                SetText("Iniciando ataque...");

                while (!TryStartRandomVillageAttack())
                {
                    SetText("Buscando aldea...");
                    yield return new WaitForSeconds(retryDelay);
                }
            }

            yield return null;
        }
    }

    private void UpdateCountdownText()
    {
        if (countdownText == null)
            return;

        int seconds = Mathf.CeilToInt(currentTime);

        if (seconds < 0)
            seconds = 0;

        countdownText.text = "Atacan en: " + seconds + "s";
    }

    private void SetText(string message)
    {
        if (countdownText != null)
            countdownText.text = message;
    }

    private bool TryStartRandomVillageAttack()
    {
        if (villageManager == null || adventurerManager == null)
            return false;

        if (villageManager.aldeas == null || villageManager.aldeas.Length == 0)
            return false;

        Aldea aldea = villageManager.ObtenerAldeaAleatoria();

        if (aldea == null)
            return false;

        if (aldea.aventureros == null || aldea.aventureros.Count == 0)
            return false;

        List<AdventurerSetup> setups = new List<AdventurerSetup>();

        foreach (AventureroData aventurero in aldea.aventureros)
        {
            if (aventurero == null)
                continue;

            if (aventurero.prefab == null)
                continue;

            setups.Add(new AdventurerSetup
            {
                prefab = aventurero.prefab,
                level = aventurero.nivel
            });
        }

        if (setups.Count == 0)
            return false;

        adventurerManager.Initialize(setups);
        adventurerManager.StartFight(aldea);

        SetText("Aldea atacando...");

        return true;
    }
}