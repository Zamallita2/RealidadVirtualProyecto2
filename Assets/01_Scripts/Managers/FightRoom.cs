using System.Collections.Generic;
using UnityEngine;

public class FightRoom
{
    public List<AdventurerSetup> wave1 = new();
    public List<AdventurerSetup> wave2 = new();
    public List<AdventurerSetup> wave3 = new();

    public float loot = 50;
    public bool isBeingAttacked;

    public FightRoom Clone()
    {
        return new FightRoom
        {
            wave1 = new List<AdventurerSetup>(wave1),
            wave2 = new List<AdventurerSetup>(wave2),
            wave3 = new List<AdventurerSetup>(wave3),
            loot = loot,
            isBeingAttacked = isBeingAttacked
        };
    }

    public List<AdventurerSetup> GetWave(int wave)
    {
        return wave switch
        {
            1 => wave1,
            2 => wave2,
            3 => wave3,
            _ => null
        };
    }

    public void ClearAllWaves()
    {
        wave1.Clear();
        wave2.Clear();
        wave3.Clear();
    }

    public bool IsEmpty()
    {
        return wave1.Count == 0
            && wave2.Count == 0
            && wave3.Count == 0;
    }
}
