using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] float speed;

    [SerializeField] Transform titleCameraPos;

    GameObject player;
    Vector3 offset;
    Vector3 initAngle;

    PlayerController playerController;

    GameManager gameManager;

    GameManager.GameState oldState;

    // Start is called before the first frame update
    void Start()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        initAngle = this.transform.eulerAngles;
        offset = Vector3.zero - this.transform.position;//playerÇÃèâä˙à íuÇå¥ì_Ç…Ç∑ÇÈ
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if(gameManager.state == GameManager.GameState.Title)
        {
            transform.position = Vector3.Lerp(transform.position,titleCameraPos.position, speed);
            transform.eulerAngles = Vector3.Lerp(transform.eulerAngles, titleCameraPos.eulerAngles, speed);
        }
        else
        {
            if(oldState != gameManager.state)
            {
                Init();
            }

            if (!playerController.GetIsSkill())
            {
                transform.position = Vector3.Lerp(transform.position, player.transform.position - offset, speed);
                transform.eulerAngles = Vector3.Lerp(transform.eulerAngles, initAngle, speed);
            }
        }
        oldState = gameManager.state;
    }

    void Init()
    {
        player = GameObject.FindWithTag("Player");
        playerController = player.GetComponent<PlayerController>();
    }
}
