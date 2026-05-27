using System.Collections.Generic;
using UnityEngine;

public class AdventurerManager : MonoBehaviour
{
    [Header("Managers")]
    [SerializeField] private FightManager fightManager;
    [SerializeField] private NodeManager nodeManager;

    private readonly List<AdventurerData> party = new();
    private bool isFightRunning;

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
        StartFight(null);
    }

    public void StartFight(Aldea aldea)
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
        fightManager.StartFight(party, aldea);
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
            if(member == null || !member.isAlive)
                continue;

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

        AdventurerData temp = new AdventurerData
        {
            prefab = setup.prefab,
            level = setup.level
        };

        StatCalculator.Recalculate(temp);

        return new AdventurerData
        {
            prefab = setup.prefab,
            level = setup.level,
            maxHealth = temp.maxHealth,
            currentHealth = temp.maxHealth,
            strength = temp.strength,
            speed = temp.speed,
            isAlive = true
        };
    }
}
