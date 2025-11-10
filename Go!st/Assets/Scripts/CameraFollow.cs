using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] float speed;
    [SerializeField] float toSkinChangrSpeed;

    [SerializeField] Transform titleCameraPos;
    [SerializeField] Transform skinChangeCameraPos;

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
        offset = Vector3.zero - this.transform.position;//playerの初期位置を原点にする

        //最初のカメラ位置はタイトルの位置にする
        transform.position = titleCameraPos.position;
        transform.eulerAngles = titleCameraPos.eulerAngles;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (gameManager.state == GameManager.GameState.SkinChange)
        {
            transform.position = Vector3.Lerp(transform.position, skinChangeCameraPos.position, toSkinChangrSpeed);
            transform.eulerAngles = Vector3.Lerp(transform.eulerAngles, skinChangeCameraPos.eulerAngles, toSkinChangrSpeed);
        }
        else if(gameManager.state == GameManager.GameState.Game || gameManager.state == GameManager.GameState.StartCountDown || gameManager.state == GameManager.GameState.GameSetting)
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
        else
        {
            transform.position = Vector3.Lerp(transform.position, titleCameraPos.position, toSkinChangrSpeed);
            transform.eulerAngles = Vector3.Lerp(transform.eulerAngles, titleCameraPos.eulerAngles, toSkinChangrSpeed);
        }
        oldState = gameManager.state;
    }

    void Init()
    {
        player = GameObject.FindWithTag("Player");
        playerController = player.GetComponent<PlayerController>();
    }
}
