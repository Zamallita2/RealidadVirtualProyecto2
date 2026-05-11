using System.Collections.Generic;
using UnityEngine;

public class WaveManager : MonoBehaviour
{
    [Header("Enemy Waves")]
    public List<GameObject> wave1;
    public List<GameObject> wave2;
    public List<GameObject> wave3;

    private int currentWave = 0;

    public List<GameObject> StartNextWave()
    {
        currentWave++;

        return GetCurrentWave();
    }

    List<GameObject> GetCurrentWave()
    {
        switch(currentWave)
        {
            case 1:
                return wave1;

            case 2:
                return wave2;

            case 3:
                return wave3;
        }

        return null;
    }

    public void ResetWaves()
    {
        currentWave = 0;
    }

    public int GetCurrentWaveNumber()
    {
        return currentWave;
    }
}