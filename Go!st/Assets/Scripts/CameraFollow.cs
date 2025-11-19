using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] float speed;
    [SerializeField] float toSkinChangrSpeed;

    [SerializeField] Transform titleCameraPos;
    [SerializeField] Transform skinChangeCameraPos;

    [SerializeField] float shakeMagnitude = 0.1f;
    [SerializeField] float shakeFrequency = 20f;
    float shakeDuration = 0f;
    float shakeElapsed = 0f; // フィールドで追加

    GameObject player;
    Vector3 offset;
    Vector3 initAngle;
    Vector3 shakeOffset = Vector3.zero;
    Vector3 shakeDirection;    // ← ランダム方向を保持

    PlayerController playerController;
    PlayerSkill playerSkill;

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
        else if(gameManager.state == GameManager.GameState.Game || gameManager.state == GameManager.GameState.StartCountDown 
            || gameManager.state == GameManager.GameState.GameSetting || gameManager.state == GameManager.GameState.Score)
        {
            if(oldState != gameManager.state)
            {
                Init();
            }

            if (!playerController.GetIsSkill() || !playerSkill.isOneHand)
            {
                transform.position = Vector3.Lerp(transform.position, player.transform.position - offset, speed);
                transform.eulerAngles = Vector3.Lerp(transform.eulerAngles, initAngle, speed);
            }
        }
        else
        {
            transform.position = Vector3.Lerp(transform.position, titleCameraPos.position, toSkinChangrSpeed);
            transform.eulerAngles = Vector3.Lerp(transform.eulerAngles, titleCameraPos.eulerAngles, toSkinChangrSpeed);

            if(shakeDuration != 0)
            {
                shakeDuration = 0;
            }
        }

        if (shakeDuration > 0)
        {
            shakeElapsed += Time.deltaTime;

            // サイン波で揺れの強度を作る（0〜1〜0）
            float shakeValue = Mathf.Sin(shakeElapsed * shakeFrequency) * shakeMagnitude;

            // 毎フレームランダム方向（自然に揺れる）
            Vector3 randomDir = Random.insideUnitSphere.normalized;

            shakeOffset = randomDir * shakeValue;

            shakeDuration -= Time.deltaTime;
        }
        else
        {
            shakeOffset = Vector3.zero;
            shakeElapsed = 0; // 次の揺れのためにリセット
        }

        transform.position += shakeOffset;

        oldState = gameManager.state;
    }

    void Init()
    {
        player = GameObject.FindWithTag("Player");
        playerController = player.GetComponent<PlayerController>();
        playerSkill = player.GetComponent<PlayerSkill>();
    }

    public void Shake(float duration)
    {
        shakeDuration = duration;
    }
}
