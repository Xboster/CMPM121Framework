using System;
using System.Collections.Generic;

[System.Serializable]
public class EnemyData
{
    public string name;
    public int sprite;
    public int hp;
    public int speed;
    public int damage;
}

[System.Serializable]
public class LevelSpawn
{
    public string enemy;
    public string count;
    public string hp;
    public string speed;
    public string damage;
    public List<int> sequence;
    public int delay;
    public string location;
}

[System.Serializable]
public class LevelData
{
    public string name;
    public int waves;
    public List<LevelSpawn> spawns;
}
