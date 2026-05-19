using System.Collections.Generic;
using UnityEngine;

public class AdventurerManager : MonoBehaviour
{
    [Header("Managers")]
    [SerializeField] private FightManager fightManager;
    [SerializeField] private NodeManager nodeManager;

    private readonly List<AdventurerData> party = new();
    private bool isFightRunning;

    const float HealthPerLevel = 0.12f;
    const float StrengthPerLevel = 0.10f;
    const float SpeedPerLevel = 0.05f;

    public bool IsFightRunning => isFightRunning;
    public bool CanStartFight => !isFightRunning && party.Count > 0;

    void Awake()
    {
        if(fightManager == null)
            fightManager = FindFirstObjectByType<FightManager>();

        if(nodeManager == null)
            nodeManager = FindFirstObjectByType<NodeManager>();
    }

    public void Initialize(List<AdventurerSetup> adventurers)
    {
        party.Clear();
        isFightRunning = false;

        foreach(AdventurerSetup setup in adventurers)
        {
            if(setup.prefab == null)
                continue;

            party.Add(CreateAdventurerData(setup));
        }
    }

    public void StartFight()
    {
        if(isFightRunning)
        {
            Debug.LogWarning("Ya hay una pelea en curso.");
            return;
        }

        if(party.Count == 0)
        {
            Debug.LogWarning("No hay aventureros para iniciar la pelea.");
            return;
        }

        isFightRunning = true;
        fightManager.StartFight(party);
    }

    public IReadOnlyList<AdventurerData> GetParty()
    {
        return party;
    }

    public void OnFightWon(List<AdventurerData> updatedParty)
    {
        List<AdventurerData> snapshot = new();

        foreach(AdventurerData member in updatedParty)
        {
            snapshot.Add(member.Clone());
        }

        party.Clear();
        party.AddRange(snapshot);
        isFightRunning = false;

        if(nodeManager != null)
            nodeManager.SavePartyState(party);
    }

    public void OnFightLost()
    {
        party.Clear();
        isFightRunning = false;

        if(nodeManager != null)
            nodeManager.OnPartyDefeated();
    }

    static AdventurerData CreateAdventurerData(AdventurerSetup setup)
    {
        UnitStats prefabStats = setup.prefab.GetComponent<UnitStats>();

        int maxHealth = prefabStats != null ? prefabStats.maxHealth : 10;
        int strength = prefabStats != null ? prefabStats.strength : 2;
        int speed = prefabStats != null ? prefabStats.speed : 5;

        ApplyLevelScaling(setup.level, ref maxHealth, ref strength, ref speed);

        return new AdventurerData
        {
            prefab = setup.prefab,
            level = setup.level,
            maxHealth = maxHealth,
            currentHealth = maxHealth,
            strength = strength,
            speed = speed,
            isAlive = true
        };
    }

    static void ApplyLevelScaling(
        int level,
        ref int maxHealth,
        ref int strength,
        ref int speed)
    {
        if(level <= 1)
            return;

        int bonusLevels = level - 1;

        maxHealth = Mathf.RoundToInt(
            maxHealth * (1f + HealthPerLevel * bonusLevels)
        );

        strength = Mathf.RoundToInt(
            strength * (1f + StrengthPerLevel * bonusLevels)
        );

        speed = Mathf.Max(
            1,
            Mathf.RoundToInt(speed * (1f + SpeedPerLevel * bonusLevels))
        );
    }
}
