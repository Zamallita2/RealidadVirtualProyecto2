using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class FightRoom
{
    public List<GameObject> wave1 = new();
    public List<GameObject> wave2 = new();
    public List<GameObject> wave3 = new();
    public float loot=50;
    public bool isBeingAttacked;

    public FightRoom Clone()
    {
        return new FightRoom
        {
            wave1 = new List<GameObject>(wave1),
            wave2 = new List<GameObject>(wave2),
            wave3 = new List<GameObject>(wave3),
            isBeingAttacked = isBeingAttacked
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

    public List<GameObject> GetWave(int waveNumber)
    {
        switch(waveNumber)
        {
            case 1:
                return wave1;
            case 2:
                return wave2;
            case 3:
                return wave3;
            default:
                return null;
        }
    }
}
