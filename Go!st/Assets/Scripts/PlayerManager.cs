using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    [SerializeField] private PlayerSelect playerSelect;

    public GameObject CurrentPlayer => playerSelect.selectPlayer;

    [SerializeField] bool onAutoAim = false;

    public void InitializePlayer(Vector3 spawnPosition)
    {
        var player = playerSelect.selectPlayer;
        player.transform.position = spawnPosition;
        Rigidbody playerRb = player.GetComponent<Rigidbody>();
        playerRb.velocity = Vector3.zero;
        player.SetActive(true);
        playerRb.useGravity = true;
        SetAutoAim();
    }

    public void ResetPlayer()
    {
        if (playerSelect == null) return;

        // 全プレイヤーを非アクティブ＆重力OFF
        foreach (var p in playerSelect.GetPlayers())
        {
            p.SetActive(false);
            p.GetComponent<Rigidbody>().useGravity = false;
        }
        CurrentPlayer.GetComponent<PlayerController>().Init();
        // スワイプ選択状態を初期化
        playerSelect.ResetSelection();
    }

    public void GameStart()
    {
        playerSelect.GameStart();
    }

    public void SetAutoAim()
    {
        playerSelect.SetAutoAim(onAutoAim);
    }
}
