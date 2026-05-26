using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Iniciar : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private VillageManager villageManager;
    [SerializeField] private AdventurerManager adventurerManager;

    [Header("Intervals (seconds)")]
    [SerializeField] private float minIntervalSeconds = 15f;
    [SerializeField] private float maxIntervalSeconds = 20f;
    [SerializeField] private float retryDelaySeconds = 1f;

    private Coroutine loopCoroutine;

    void Start()
    {
        iniciar();
    }

    // Mantengo el mismo nombre para que no se rompan calls desde el Inspector/UI.
    public void iniciar()
    {
        if(loopCoroutine != null)
            return;

        if(villageManager == null)
            villageManager = FindFirstObjectByType<VillageManager>();

        if(adventurerManager == null)
            adventurerManager = FindFirstObjectByType<AdventurerManager>();

        if(villageManager == null || adventurerManager == null)
        {
            Debug.LogError("Iniciar: faltan referencias (VillageManager/AdventurerManager).");
            return;
        }

        loopCoroutine = StartCoroutine(VillageFightLoop());
    }

    IEnumerator VillageFightLoop()
    {
        while(true)
        {
            if (!adventurerManager.IsFightRunning)
            {
                float wait = Random.Range(minIntervalSeconds, maxIntervalSeconds);
                yield return new WaitForSeconds(wait);

                while (!TryStartRandomVillageFight())
                    yield return new WaitForSeconds(retryDelaySeconds);
            }

            yield return null; // 👈 CLAVE: evita el spin loop
        }
    }

    bool TryStartRandomVillageFight()
    {

        if(villageManager.aldeas == null ||
            villageManager.aldeas.Length == 0)
            return false;

        Aldea aldea = villageManager.ObtenerAldeaAleatoria();

        if(aldea == null ||
            aldea.aventureros == null ||
            aldea.aventureros.Count == 0)
        {
            return false;
        }

        List<AdventurerSetup> setups = new();

        foreach(AventureroData a in aldea.aventureros)
        {
            if(a == null || a.prefab == null)
                continue;

            setups.Add(new AdventurerSetup
            {
                prefab = a.prefab,
                level = a.nivel
            });
        }

        if(setups.Count == 0)
            return false;

        adventurerManager.Initialize(setups);
        adventurerManager.StartFight(aldea);
        return true;
    }
}