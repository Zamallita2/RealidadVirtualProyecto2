using System.Collections;
using UnityEngine;

public enum VillageEvolutionStage
{
    Campamento,
    Ciudad,
    Metropolis
}

public class VillageEvolution : MonoBehaviour
{
    [Header("Prefabs 3D")]
    public GameObject campamentoPrefab;
    public GameObject ciudadPrefab;
    public GameObject metropolisPrefab;

    [Header("Contenedor solo para MODELOS 3D")]
    public Transform modelContainer;

    [Header("Progreso")]
    public int defeatedRooms = 0;
    public int roomsPerFloor = 9;
    public int cityAfterFloors = 3;
    public int metropolisAfterFloors = 6;

    [Header("Estado")]
    public VillageEvolutionStage currentStage = VillageEvolutionStage.Campamento;

    [Header("Cámara")]
    public VillageCameraController villageCamera;

    [Header("Efecto WOW sin partículas")]
    public Color evolutionColor = new Color(1f, 0.1f, 0.05f, 1f);
    public float effectDuration = 2.3f;
    public float scalePunch = 1.18f;
    public float shakeAmount = 0.04f;
    public float lightMaxIntensity = 9f;

    private GameObject currentModel;
    private Transform effectsContainer;
    private Light evolutionLight;
    private LineRenderer ring1;
    private LineRenderer ring2;
    private LineRenderer beam1;
    private LineRenderer beam2;
    private Coroutine effectRoutine;

    private void Awake()
    {
        CreateContainers();
        CreateWowEffectObjects();
        SpawnStage(CalculateStage(defeatedRooms), false);
    }

    public void SetProgress(int roomsWon, bool playEffect)
    {
        defeatedRooms = Mathf.Max(0, roomsWon);

        VillageEvolutionStage newStage = CalculateStage(defeatedRooms);

        if (currentModel == null)
        {
            SpawnStage(newStage, false);
            return;
        }

        if (newStage == currentStage)
            return;

        SpawnStage(newStage, playEffect);
    }

    private VillageEvolutionStage CalculateStage(int roomsWon)
    {
        int floorsWon = roomsWon / roomsPerFloor;

        if (floorsWon >= metropolisAfterFloors)
            return VillageEvolutionStage.Metropolis;

        if (floorsWon >= cityAfterFloors)
            return VillageEvolutionStage.Ciudad;

        return VillageEvolutionStage.Campamento;
    }

    private void SpawnStage(VillageEvolutionStage stage, bool playEffect)
    {
        currentStage = stage;

        ClearOldModel();

        GameObject prefab = GetPrefab(stage);

        if (prefab == null)
        {
            Debug.LogWarning(name + " no tiene prefab asignado para " + stage);
            return;
        }

        if (prefab.GetComponentInChildren<Canvas>() != null ||
            prefab.GetComponent<RectTransform>() != null)
        {
            Debug.LogError("MAL PREFAB EN " + name + ": pusiste un prefab UI/Canvas/Card en vez de un modelo 3D. Revisa el campo " + stage);
            return;
        }

        currentModel = Instantiate(prefab, modelContainer);
        currentModel.name = "ACTIVE_MODEL_" + stage;
        currentModel.transform.localPosition = Vector3.zero;
        currentModel.transform.localRotation = Quaternion.identity;
        currentModel.transform.localScale = Vector3.one;

        if (villageCamera != null)
            villageCamera.SetTarget(currentModel.transform);

        if (playEffect)
            PlayEvolutionEffect();
    }

    private void ClearOldModel()
    {
        if (currentModel != null)
        {
            Destroy(currentModel);
            currentModel = null;
        }

        for (int i = modelContainer.childCount - 1; i >= 0; i--)
        {
            Transform child = modelContainer.GetChild(i);

            if (child.name.StartsWith("ACTIVE_MODEL_"))
                Destroy(child.gameObject);
        }
    }

    private GameObject GetPrefab(VillageEvolutionStage stage)
    {
        switch (stage)
        {
            case VillageEvolutionStage.Campamento:
                return campamentoPrefab;
            case VillageEvolutionStage.Ciudad:
                return ciudadPrefab;
            case VillageEvolutionStage.Metropolis:
                return metropolisPrefab;
            default:
                return campamentoPrefab;
        }
    }

    private void CreateContainers()
    {
        if (modelContainer == null)
        {
            GameObject modelObj = new GameObject("ModelContainer_SOLO_MODELOS_3D");
            modelObj.transform.SetParent(transform, false);
            modelContainer = modelObj.transform;
        }

        GameObject fxObj = new GameObject("EffectsContainer_NO_MODELOS");
        fxObj.transform.SetParent(transform, false);
        effectsContainer = fxObj.transform;
    }

    private void CreateWowEffectObjects()
    {
        GameObject lightObj = new GameObject("EvolutionLight_WOW");
        lightObj.transform.SetParent(effectsContainer, false);
        lightObj.transform.localPosition = new Vector3(0f, 2f, 0f);

        evolutionLight = lightObj.AddComponent<Light>();
        evolutionLight.type = LightType.Point;
        evolutionLight.color = evolutionColor;
        evolutionLight.range = 7f;
        evolutionLight.intensity = 0f;

        ring1 = CreateRing("MagicRing_1", 1.1f);
        ring2 = CreateRing("MagicRing_2", 1.6f);

        beam1 = CreateBeam("LightBeam_1", new Vector3(-0.5f, 0f, 0f));
        beam2 = CreateBeam("LightBeam_2", new Vector3(0.5f, 0f, 0f));

        SetLineAlpha(ring1, 0f);
        SetLineAlpha(ring2, 0f);
        SetLineAlpha(beam1, 0f);
        SetLineAlpha(beam2, 0f);
    }

    private LineRenderer CreateRing(string objName, float radius)
    {
        GameObject obj = new GameObject(objName);
        obj.transform.SetParent(effectsContainer, false);

        LineRenderer line = obj.AddComponent<LineRenderer>();
        line.useWorldSpace = false;
        line.loop = true;
        line.positionCount = 96;
        line.widthMultiplier = 0.035f;
        line.material = CreateLineMaterial();
        line.numCapVertices = 6;

        for (int i = 0; i < line.positionCount; i++)
        {
            float angle = i / (float)line.positionCount * Mathf.PI * 2f;
            Vector3 pos = new Vector3(Mathf.Cos(angle) * radius, 0.04f, Mathf.Sin(angle) * radius);
            line.SetPosition(i, pos);
        }

        return line;
    }

    private LineRenderer CreateBeam(string objName, Vector3 offset)
    {
        GameObject obj = new GameObject(objName);
        obj.transform.SetParent(effectsContainer, false);
        obj.transform.localPosition = offset;

        LineRenderer line = obj.AddComponent<LineRenderer>();
        line.useWorldSpace = false;
        line.positionCount = 2;
        line.widthMultiplier = 0.08f;
        line.material = CreateLineMaterial();
        line.numCapVertices = 8;

        line.SetPosition(0, new Vector3(0f, 0f, 0f));
        line.SetPosition(1, new Vector3(0f, 2.5f, 0f));

        return line;
    }

    private Material CreateLineMaterial()
    {
        Shader shader = Shader.Find("Sprites/Default");

        Material mat = new Material(shader);
        mat.color = evolutionColor;
        return mat;
    }

    private void PlayEvolutionEffect()
    {
        if (effectRoutine != null)
            StopCoroutine(effectRoutine);

        effectRoutine = StartCoroutine(EvolutionEffectRoutine());
    }

    private IEnumerator EvolutionEffectRoutine()
    {
        Vector3 baseScale = modelContainer.localScale;
        Vector3 basePos = modelContainer.localPosition;

        float time = 0f;

        while (time < effectDuration)
        {
            time += Time.deltaTime;
            float t = Mathf.Clamp01(time / effectDuration);

            float pulse = Mathf.Sin(t * Mathf.PI);
            float fastPulse = Mathf.Abs(Mathf.Sin(t * Mathf.PI * 5f));

            modelContainer.localScale = Vector3.Lerp(baseScale, baseScale * scalePunch, pulse);
            modelContainer.localPosition = basePos + Random.insideUnitSphere * shakeAmount * pulse;

            if (evolutionLight != null)
                evolutionLight.intensity = lightMaxIntensity * pulse * fastPulse;

            AnimateRing(ring1, t, pulse, 1.1f, 2.3f);
            AnimateRing(ring2, t, pulse, 1.6f, 3.0f);

            AnimateBeam(beam1, pulse);
            AnimateBeam(beam2, pulse);

            yield return null;
        }

        modelContainer.localScale = baseScale;
        modelContainer.localPosition = basePos;

        if (evolutionLight != null)
            evolutionLight.intensity = 0f;

        SetLineAlpha(ring1, 0f);
        SetLineAlpha(ring2, 0f);
        SetLineAlpha(beam1, 0f);
        SetLineAlpha(beam2, 0f);
    }

    private void AnimateRing(LineRenderer line, float t, float pulse, float startScale, float endScale)
    {
        if (line == null) return;

        line.transform.localScale = Vector3.one * Mathf.Lerp(startScale, endScale, t);
        line.transform.localRotation = Quaternion.Euler(0f, t * 360f, 0f);

        SetLineAlpha(line, pulse * 0.9f);
    }

    private void AnimateBeam(LineRenderer line, float pulse)
    {
        if (line == null) return;

        line.widthMultiplier = Mathf.Lerp(0.02f, 0.12f, pulse);
        SetLineAlpha(line, pulse * 0.75f);
    }

    private void SetLineAlpha(LineRenderer line, float alpha)
    {
        if (line == null || line.material == null) return;

        Color c = evolutionColor;
        c.a = alpha;
        line.startColor = c;
        line.endColor = c;
        line.material.color = c;
    }

    [ContextMenu("TEST ciudad")]
    public void TestCity()
    {
        SetProgress(cityAfterFloors * roomsPerFloor, true);
    }

    [ContextMenu("TEST metrópolis")]
    public void TestMetropolis()
    {
        SetProgress(metropolisAfterFloors * roomsPerFloor, true);
    }
}