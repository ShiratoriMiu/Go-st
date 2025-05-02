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

        // �S�v���C���[���A�N�e�B�u���d��OFF
        foreach (var p in playerSelect.GetPlayers())
        {
            p.SetActive(false);
            p.GetComponent<Rigidbody>().useGravity = false;
        }
        CurrentPlayer.GetComponent<PlayerController>().Init();
        // �X���C�v�I����Ԃ�������
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
