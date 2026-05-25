using UnityEngine;

public enum VillagePersonality
{
    Osado,
    Temeroso,
    Codicioso,
    Cauteloso,
    Vengativo,
    Erudito
}

public enum VillageState
{
    EnEspera,
    EnCamino,
    EnCombate,
    Avanzando,
    Retirandose,
    Derrotado
}

public class VillageData : MonoBehaviour
{
    [Header("Datos")]
    public string villageName;
    public VillagePersonality personality;
    public VillageState currentState = VillageState.EnEspera;

    [Header("Stats")]
    public int attack = 10;
    public int defense = 10;
    public int health = 100;
    public int members = 6;

    [Header("Luces de la aldea")]
    public Light[] villageLights;

    public void SetState(VillageState newState)
    {
        currentState = newState;
        ApplyColorByState();
    }

    private void Start()
    {
        ApplyColorByState();
    }

    private void ApplyColorByState()
    {
        Color color = Color.white;

        switch (currentState)
        {
            case VillageState.EnEspera:
                color = Color.white;
                break;

            case VillageState.EnCamino:
                color = new Color(0.4f, 0.7f, 1f);
                break;

            case VillageState.EnCombate:
                color = new Color(1f, 0.25f, 0.25f);
                break;

            case VillageState.Avanzando:
                color = new Color(0.4f, 1f, 0.4f);
                break;

            case VillageState.Retirandose:
                color = new Color(1f, 0.8f, 0.25f);
                break;

            case VillageState.Derrotado:
                color = Color.gray;
                break;
        }

        foreach (Light light in villageLights)
        {
            if (light != null)
                light.color = color;
        }
    }
}