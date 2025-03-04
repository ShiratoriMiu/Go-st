using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [SerializeField] private int minute;
    [SerializeField] private float seconds;
    [SerializeField] GameObject gameUI;
    [SerializeField] GameObject titleUI;
    [SerializeField] GameObject resultUI;

    Text timeText;
    Text resultText;

    //　前のUpdateの時の秒数
    private float oldSeconds;

    LevelManager levelManager;

    private int minuteInit;
    private float secondsInit;

    //倒した敵の総数
    private int enemiesDefeatedNum = 0;
    public enum GameState
    {
        Title,
        Game,
        Result
    }
    public GameState state = GameState.Title;

    // Start is called before the first frame update
    void Start()
    {
        timeText = GameObject.Find("TimeText").GetComponent<Text>();
        resultText = GameObject.Find("ResultText").GetComponent<Text>();
        levelManager = GameObject.Find("LevelManager").GetComponent<LevelManager>();
        gameUI.SetActive(false);
        titleUI.SetActive(true);
        resultUI.SetActive(false);
        minuteInit = minute;
        secondsInit = seconds;
    }

    // Update is called once per frame
    void Update()
    {
        if (state != GameState.Game) return;

        seconds -= Time.deltaTime;
        if (seconds <= 0f)
        {
            minute--;
            seconds = seconds + 60;
        }

        if (minute >= 0)
        {
            //　値が変わった時だけテキストUIを更新
            if ((int)seconds != (int)oldSeconds)
            {
                timeText.text = minute.ToString("00") + ":" + ((int)seconds).ToString("00");
            }
            oldSeconds = seconds;
        }
        else
        {
            ChangeResultState();
        }
    }

    public void ChangeResultState()
    {
        state = GameState.Result;
        resultUI.SetActive(true);
        resultText.text = "Score : " + ((minuteInit - minute) * 60 + ((int)secondsInit - (int)seconds) + enemiesDefeatedNum).ToString();
    }

    public void AddEnemiesDefeatedNum()
    {
        enemiesDefeatedNum++;
    }

    public void ChangeGameState()
    {
        state = GameState.Game;
        gameUI.SetActive(true);
        titleUI.SetActive(false);
    }

    public void ChangeTitleState()
    {
        state = GameState.Title;
        enemiesDefeatedNum = 0;
        resultUI.SetActive(false);
        gameUI.SetActive(false);
        titleUI.SetActive(true);
        levelManager.LevelInit();
        minute = minuteInit;
        seconds = secondsInit;
    }
}
