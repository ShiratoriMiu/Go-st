using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSelect : MonoBehaviour
{
    [SerializeField]
    GameObject[] players;//”í—Şİ’è
    [SerializeField]
    float space = 5f;

    int count = 0;

    int oldCount = 0;

    GameManager gameManager;

    // Start is called before the first frame update
    void Start()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();

        for (int i = 0; i < players.Length; i++)
        {
            players[i].transform.position = new Vector3(-i * space, 0, 0);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(oldCount != count)
        {
            for (int i = 0; i < players.Length; i++)
            {
                players[i].transform.position = new Vector3((-i + count) * space, 0, 0);
            }
            oldCount = count;
        }
    }

    public void NextCount()
    {
        count++;
        if(count >= players.Length)
        {
            count = 0;
        }
    }

    public void BeforeCount()
    {
        count--;
        if(count < 0)
        {
            count = players.Length - 1;
        }
    }

    public void GameStart()
    {
        for (int i = 0; i < players.Length; i++)
        {
            if(i != count)
            {
                players[i].gameObject.SetActive(false);
            }
        }
        gameManager.ChangeGameState();
    }
}
