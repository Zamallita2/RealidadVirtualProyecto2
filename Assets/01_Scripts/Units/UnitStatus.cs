using UnityEngine;

public class UnitStatus : MonoBehaviour
{
    [Header("Current Status")]
    public EnumFigthList.StatusEffect currentStatus;

    [Header("Turns")]
    public int statusTurnsRemaining = 0;

    [Header("Status Visuals")]
    public GameObject blindIcon;
    public GameObject paralysisIcon;
    public GameObject weaknessIcon;

    private FightManager fightManager;

    void Awake()
    {
        fightManager = FindFirstObjectByType<FightManager>();
        UpdateStatusVisuals();
    }

    public bool CanAct()
    {
        bool skippedTurn = false;

        switch (currentStatus)
        {
            case EnumFigthList.StatusEffect.Paralysis:
            {
                if (Random.value <= 0.5f)
                {
                    Debug.Log(name + " está paralizado");
                    skippedTurn = true;
                }
                break;
            }

            case EnumFigthList.StatusEffect.Blind:
            case EnumFigthList.StatusEffect.Weakness:
            {
                Debug.Log(name + " recibe efecto pasivo del estado");
                break;
            }
        }

        ProcessStatusTurn();
        return !skippedTurn;
    }

    public void ApplyStatus(EnumFigthList.StatusEffect status, int duration)
    {
        if (status == EnumFigthList.StatusEffect.None)
            return;

        currentStatus = status;
        statusTurnsRemaining = duration;

        Debug.Log(name + " recibió el estado: " + status);

        UpdateStatusVisuals(); // 💥 aquí magia uwu
    }

    public void ProcessStatusTurn()
    {
        if (currentStatus == EnumFigthList.StatusEffect.None)
            return;

        statusTurnsRemaining--;

        if (statusTurnsRemaining <= 0)
        {
            Debug.Log(name + " ya no tiene estado");

            currentStatus = EnumFigthList.StatusEffect.None;
            statusTurnsRemaining = 0;

            UpdateStatusVisuals(); // 💥 también aquí
        }
    }

    public bool HasStatus()
    {
        return currentStatus != EnumFigthList.StatusEffect.None;
    }

    public bool HasStatus(EnumFigthList.StatusEffect status)
    {
        return currentStatus == status;
    }

    public void ClearStatus()
    {
        currentStatus = EnumFigthList.StatusEffect.None;
        statusTurnsRemaining = 0;

        UpdateStatusVisuals(); // 💥 y aquí también nwn
    }

    private void UpdateStatusVisuals()
    {
        // 🧼 apagar con seguridad
        if (blindIcon != null) blindIcon.SetActive(false);
        if (paralysisIcon != null) paralysisIcon.SetActive(false);
        if (weaknessIcon != null) weaknessIcon.SetActive(false);

        // 💡 encender el correcto
        switch (currentStatus)
        {
            case EnumFigthList.StatusEffect.Blind:
                if (blindIcon != null)
                    blindIcon.SetActive(true);
                else
                    Debug.LogWarning($"{name}: Blind icon no asignado owo");
                break;

            case EnumFigthList.StatusEffect.Paralysis:
                if (paralysisIcon != null)
                    paralysisIcon.SetActive(true);
                else
                    Debug.LogWarning($"{name}: Paralysis icon no asignado owo");
                break;

            case EnumFigthList.StatusEffect.Weakness:
                if (weaknessIcon != null)
                    weaknessIcon.SetActive(true);
                else
                    Debug.LogWarning($"{name}: Weakness icon no asignado owo");
                break;

            case EnumFigthList.StatusEffect.None:
            default:
                break;
        }
    }
}