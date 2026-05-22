using System.Collections.Generic;
using UnityEngine;

public class NodeManager : MonoBehaviour
{
    private readonly List<AdventurerData> partyState = new();

    public void SavePartyState(List<AdventurerData> party)
    {
        partyState.Clear();

        foreach(AdventurerData member in party)
        {
            partyState.Add(member.Clone());
        }
    }

    public void OnPartyDefeated()
    {
        partyState.Clear();
    }

    public IReadOnlyList<AdventurerData> GetPartyState()
    {
        return partyState;
    }
}
