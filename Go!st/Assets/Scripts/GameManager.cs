using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [SerializeField] private int minute;
    [SerializeField] private float seconds;
    [SerializeField] GameObject gameUI;
    [SerializeField] GameObject titleUI;

    Text timeText;
    Text resultText;

    //　前のUpdateの時の秒数
    private float oldSeconds;

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
        gameUI.SetActive(false);
        titleUI.SetActive(true);
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
            state = GameState.Result;
            resultText.enabled = true;
        }
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
}
