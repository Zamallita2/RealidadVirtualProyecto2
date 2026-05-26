using System;
using System.Collections.Generic;

[Serializable]
public class GachaOwnedEnemy
{
    public string enemyId;
    public int copies;
    public bool unlocked;
}

[Serializable]
public class GachaSaveData
{
    public int essence;
    public int shopCoins;
    public int pityCounter;
    public List<GachaOwnedEnemy> ownedEnemies = new List<GachaOwnedEnemy>();
}