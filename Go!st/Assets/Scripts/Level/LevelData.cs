using System.Collections.Generic;

[System.Serializable]
public class LevelData
{
    public int Level;
    public int BulletCount;
    public float BulletSpeed;
    public float AttackInterval;
    public List<string> EnemyTypes;
    public int YellowEnemyHP;
    public int RedEnemyHP;
    public int BlackEnemyHP;
    public int RedBulletHP;
    public int BlackBulletHP;
    public int NextLevelEnemyNum;
    public int BossSpawn;
}
