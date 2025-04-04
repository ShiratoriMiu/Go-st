using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelManager : MonoBehaviour
{
    [SerializeField,Header("初期レベル")]
    int level = 1;
    [SerializeField]
    Text levelText;

    int enemyNum = 0;//現在のレベルで倒した敵の数

    // Start is called before the first frame update
    void Start()
    {
        levelText.text = "Level : " + level;
    }

    // Update is called once per frame
    void Update()
    {
        if(enemyNum >= level * 5)
        {
            level++;
            enemyNum = 0;
            levelText.text = "Level : " + level;
        }
    }

    public void AddEnemyNum()
    {
        enemyNum++;
    }

    public void LevelInit()
    {
        level = 1;
        enemyNum = 0;
        levelText.text = "Level : " + level;
    }

    public int GetCurrentLevel()
    {
        return level;
    }
}
