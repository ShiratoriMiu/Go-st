using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelManager : MonoBehaviour
{
    [SerializeField,Header("‰ŠúƒŒƒxƒ‹")]
    int level = 1;
    [SerializeField]
    Text levelText;

    int enemyNum = 0;//Œ»Ý‚ÌƒŒƒxƒ‹‚Å“|‚µ‚½“G‚Ì”

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
