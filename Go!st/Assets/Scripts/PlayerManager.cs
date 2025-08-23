using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections.Generic;

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

    private PlayerSaveData playerData;

    [SerializeField] Texture2D defaultTexture;

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

    private void Start()
    {
        // JSON���烍�[�h
        playerData = SaveManager.Load();

        // �����X�L���E�����X�L���𕜌�
        RestoreSkins(playerData);

        // �}�e���A���𕜌�
        RestoreMaterials(playerData);
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

        // ���ۂɉ�]�i��: ������]�Ȃǂɂ���ꍇ�j
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

    private void RestoreSkins(PlayerSaveData data)
    {
        SkinItemTarget skinItemTarget = player.GetComponent<SkinItemTarget>();
        if (skinItemTarget == null || skinItemTarget.ItemSlots == null) return;

        // equippedSkins �� null �Ȃ�󃊃X�g��
        List<EquippedSkinData> equippedSkins = data.equippedSkins ?? new List<EquippedSkinData>();
        List<string> ownedSkins = data.ownedSkins ?? new List<string>();

        foreach (var slot in skinItemTarget.ItemSlots)
        {
            if (slot == null) continue;

            // ������Ԃ̕���
            slot.isOwned = slot.isOwned || ownedSkins.Contains(slot.itemName);

            // ������Ԃ̕���
            EquippedSkinData equippedData = equippedSkins.Find(e => e.skinId == slot.itemName);
            if (equippedData != null)
            {
                slot.isEquipped = true;
                skinItemTarget.EquippedSkinItem(slot);  // ���������Ăяo��

                // Renderer ������ꍇ�͑S�Ă� Renderer �ɐF�𔽉f
                if (slot.itemObjectRenderers != null)
                {
                    Color colorToApply = equippedData.ToColor();
                    foreach (var renderer in slot.itemObjectRenderers)
                    {
                        if (renderer != null && renderer.material != null)
                        {
                            renderer.material.color = colorToApply;
                        }
                    }
                }
            }
            else
            {
                slot.isEquipped = false;
            }
        }
    }

    private void RestoreMaterials(PlayerSaveData data)
    {
        if (data == null)
        {
            data = new PlayerSaveData();
        }

        Renderer renderer = player.GetComponent<PlayerController>().renderer;
        List<Material> mats = new List<Material>();
        Shader standardShader = Shader.Find("Standard");

        foreach (var matData in data.materials)
        {
            Material mat = new Material(standardShader);

            Texture2D tex = null;
            if (!string.IsNullOrEmpty(matData.textureName))
            {
                tex = Resources.Load<Texture2D>($"Textures/{matData.textureName}");
                if (tex == null)
                {
                    Debug.LogWarning($"�e�N�X�`�� {matData.textureName} ��������܂���BdefaultTexture���g�p");
                    tex = defaultTexture;
                }
            }
            else
            {
                tex = defaultTexture;
            }

            mat.mainTexture = tex;
            mat.color = matData.ToColor();
            mats.Add(mat);
        }

        // �ۑ�����Ă���}�e���A�����Ȃ��ꍇ��1���
        if (mats.Count == 0)
        {
            Material mat = new Material(standardShader);
            mat.mainTexture = defaultTexture;
            mat.color = Color.white;
            mats.Add(mat);
        }

        renderer.materials = mats.ToArray();
    }
}
