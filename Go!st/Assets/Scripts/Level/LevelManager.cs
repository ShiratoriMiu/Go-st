using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class LevelManager : MonoBehaviour
{
    [SerializeField] private Text levelText;
    [SerializeField] private PlayerManager playerManager;
    [SerializeField] private EnemyManager enemyManager;

    public Dictionary<int, LevelData> levelDataDict = new Dictionary<int, LevelData>();

    private int level = 1;
    private int enemyKillCount = 0;

    public int CurrentLevel => level;

    private int nextLevelEnemyNum = 1;//次のレベルに必要な敵を倒す数

    public void LoadLevelData()
    {
        TextAsset csvFile = Resources.Load<TextAsset>("LevelData");
        StringReader reader = new StringReader(csvFile.text);

        bool isFirstLine = true;
        while (reader.Peek() > -1)
        {
            string line = reader.ReadLine();
            if (isFirstLine)
            {
                isFirstLine = false;
                continue;
            }

            string[] values = line.Split(',');

            LevelData data = new LevelData
            {
                Level = int.Parse(values[0]),
                BulletCount = int.Parse(values[1]),
                BulletSpeed = float.Parse(values[2]),
                AttackInterval = float.Parse(values[3]),
                EnemyTypes = new List<string>(values[4].Split('/')),
                YellowEnemyHP = int.Parse(values[5]),
                RedEnemyHP = int.Parse(values[6]),
                BlackEnemyHP = int.Parse(values[7]),
                RedBulletHP = int.Parse(values[8]),
                BlackBulletHP = int.Parse(values[9]),
                NextLevelEnemyNum = int.Parse(values[10]),
                BossSpawn = int.Parse(values[11]),
            };

            levelDataDict[data.Level] = data;
        }
    }

    public void InitializeLevel()
    {
        level = 1;
        enemyKillCount = 0;
        UpdateLevelUI();
        ApplyLevelParameters(level);
    }

    public void AddEnemyKill()
    {
        enemyKillCount++;

        // レベルアップ条件（例：level * 5体倒す）
        if (enemyKillCount >= nextLevelEnemyNum)
        {
            level++;
            enemyKillCount -= nextLevelEnemyNum;
            UpdateLevelUI();
            ApplyLevelParameters(level);
        }
    }

    private void UpdateLevelUI()
    {
        if (levelText != null)
            levelText.text = "Level : " + level;
    }

    public void ApplyLevelParameters(int level)
    {
        if (!levelDataDict.TryGetValue(level, out var data))
        {
            Debug.LogWarning("このレベルのデータが見つかりません: " + level);
            return;
        }
        print("levelup");
        // プレイヤー設定
        PlayerController playerController = playerManager.Player.GetComponent<PlayerController>();
        playerController.SetAttackParameters(data.BulletCount, data.BulletSpeed, data.AttackInterval);
        if (level != 1) playerController.LevelUpText();

        // 敵設定
        enemyManager.SetEnemyTypesByLevelData(data);

        nextLevelEnemyNum = data.NextLevelEnemyNum;
    }

    public void ResetLevel()
    {
        level = 1;
        enemyKillCount = 0;
        UpdateLevelUI();
    }
}
