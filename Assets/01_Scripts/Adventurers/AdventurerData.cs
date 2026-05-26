using UnityEngine;

[System.Serializable]
public class AdventurerSetup
{
    public GameObject prefab;
    public int level = 1;
}

[System.Serializable]
public class AdventurerData
{
    public GameObject prefab;
    public int level = 1;

    public float maxHealth;
    public float currentHealth;
    public int strength;
    public int speed;
    public bool isAlive = true;

    public AdventurerData Clone()
    {
        return new AdventurerData
        {
            prefab = prefab,
            level = level,
            maxHealth = maxHealth,
            currentHealth = currentHealth,
            strength = strength,
            speed = speed,
            isAlive = isAlive
        };
    }
}
