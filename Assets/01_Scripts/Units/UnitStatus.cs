using UnityEngine;

public class UnitStatus : MonoBehaviour
{
    [Header("Current Status")]
    public EnumFigthList.StatusEffect currentStatus;

    [Header("Turns")]
    public int statusTurnsRemaining = 0;

    private FightManager fightManager;

    void Awake()
    {
        fightManager =
            FindFirstObjectByType<FightManager>();
    }

    public bool CanAct()
    {
        bool skippedTurn = false;

        switch(currentStatus)
        {
            case EnumFigthList.StatusEffect.Paralysis:

                if(Random.value <= 0.5f)
                {
                    Debug.Log(
                        name + " está paralizado"
                    );

                    skippedTurn = true;
                }

                break;
        }

        ProcessStatusTurn();

        return !skippedTurn;
    }

    public void ApplyStatus(
        EnumFigthList.StatusEffect status,
        int duration
    )
    {
        if(status ==
        EnumFigthList.StatusEffect.None)
        {
            return;
        }

        currentStatus = status;

        statusTurnsRemaining = duration;

        Debug.Log(
            name +
            " recibió el estado: " +
            status
        );
    }

    public void ProcessStatusTurn()
    {
        if(currentStatus ==
        EnumFigthList.StatusEffect.None)
        {
            return;
        }

        statusTurnsRemaining--;

        if(statusTurnsRemaining <= 0)
        {
            Debug.Log(
                name +
                " ya no tiene estado"
            );

            currentStatus =
                EnumFigthList.StatusEffect.None;

            statusTurnsRemaining = 0;
        }
    }

    public bool HasStatus()
    {
        return currentStatus !=
        EnumFigthList.StatusEffect.None;
    }

    public bool HasStatus(
        EnumFigthList.StatusEffect status
    )
    {
        return currentStatus == status;
    }

    public void ClearStatus()
    {
        currentStatus =
            EnumFigthList.StatusEffect.None;

        statusTurnsRemaining = 0;
    }
}