using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [SerializeField] private int minute;
    [SerializeField] private float seconds;

    Text timeText;

    //　前のUpdateの時の秒数
    private float oldSeconds;

    // Start is called before the first frame update
    void Start()
    {
        timeText = GameObject.Find("TimeText").GetComponent<Text>();
    }

    // Update is called once per frame
    void Update()
    {
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
    }
}
