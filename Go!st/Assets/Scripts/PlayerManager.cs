using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class PlayerManager : MonoBehaviour
{
    [Header("Player Settings")]
    [SerializeField] private GameObject player;

    [Header("Skin Change Settings")]
    [SerializeField] private float skinChangeRotationSpeed = 0.5f;
    [SerializeField] private float rotationDamping = 5f;

    [Header("UI & References")]
    [SerializeField] private Text skillEnemyNumText;
    [SerializeField] private ColorChanger colorChanger;
    [SerializeField] private SkinItemUIManager skinItemUIManager;
    [SerializeField] private MakeUpManager makeUpManager;
    [SerializeField] private bool onAutoAim = false;

    public GameObject Player => player;
    private PlayerController playerController;
    private Rigidbody playerRb;

    private GameManager gameManager;

    private bool isInitialized = false;
    private float currentRotationSpeed = 0f;

    void Awake()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();

        playerController = player.GetComponent<PlayerController>();
        playerRb = player.GetComponent<Rigidbody>();

        player.transform.position = Vector3.zero;
        player.transform.rotation = Quaternion.identity;
        playerRb.useGravity = false;
        player.GetComponent<PlayerSkill>().SetSkillEnemyNumText(skillEnemyNumText);
        skillEnemyNumText.gameObject.SetActive(false);
    }

    void Update()
    {
        switch (gameManager.state)
        {
            case GameManager.GameState.Title:
                HandleTitleState();
                break;
            case GameManager.GameState.SkinChange:
                HandleSkinChangeState();
                break;
        }
    }

    private void HandleTitleState()
    {
        if (!isInitialized)
        {
            player.transform.position = Vector3.zero;
            isInitialized = true;
        }
    }

    private void HandleSkinChangeState()
    {
        player.transform.position = Vector3.zero;
        UpdateModelRotation();
    }

    private void UpdateModelRotation()
    {
        if (player == null) return;

        // ŽÀÛ‚É‰ñ“]i—á: Ž©“®‰ñ“]‚È‚Ç‚É‚·‚éê‡j
        currentRotationSpeed = Mathf.Lerp(currentRotationSpeed, 0f, Time.deltaTime * rotationDamping);
        if (Mathf.Abs(currentRotationSpeed) > 0.01f)
            player.transform.Rotate(0f, currentRotationSpeed * Time.deltaTime, 0f);
    }

    public void PlayerGameStart()
    {
        playerRb.useGravity = true;
    }

    public void InitializePlayer(Vector3 spawnPosition)
    {
        player.transform.position = spawnPosition;
        playerRb.velocity = Vector3.zero;
        playerRb.useGravity = true;
    }

    public void ResetPlayer()
    {
        playerRb.useGravity = false;
        playerController.Init();
    }

    public void ToSkinChangeSelectedPlayer()
    {
        Renderer renderer = playerController.renderer;
        colorChanger.SetTargetRenderer(renderer);
        skinItemUIManager.SetTargetPlayer(player.GetComponent<SkinItemTarget>());
        makeUpManager.SetTargetRenderer(renderer);
    }
}
