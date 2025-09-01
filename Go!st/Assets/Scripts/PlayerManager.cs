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

        List<ItemData> allItems = data.allItems ?? new List<ItemData>();

        foreach (var slot in skinItemTarget.ItemSlots)
        {
            if (slot == null) continue;

            // �Z�[�u�f�[�^����Y������ ItemData ��T��
            ItemData savedItem = allItems.Find(i => i.name == slot.itemName);
            if (savedItem == null) continue;

            // �����E�����E�F�ς����𕜌�
            slot.isOwned = savedItem.isOwned;
            slot.isEquipped = savedItem.isEquipped;

            if (slot.isEquipped)
            {
                // ��������
                skinItemTarget.EquippedSkinItem(slot);

                // Renderer ������ꍇ�͐F�𔽉f
                if (slot.itemObjectRenderers != null)
                {
                    Color colorToApply = savedItem.ToColor();
                    foreach (var renderer in slot.itemObjectRenderers)
                    {
                        if (renderer != null && renderer.material != null)
                        {
                            renderer.material.color = colorToApply;
                        }
                    }
                }
            }

            slot.currentColorChange = savedItem.isColorChangeOn;
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

            Texture2D tex = !string.IsNullOrEmpty(matData.textureName)
                ? Resources.Load<Texture2D>($"Textures/{matData.textureName}")
                : defaultTexture;

            if (tex == null)
            {
                Debug.LogWarning($"�e�N�X�`�� {matData.textureName} ��������܂���BdefaultTexture���g�p");
                tex = defaultTexture;
            }

            mat.mainTexture = tex;
            mat.color = matData.ToColor();
            mats.Add(mat);
        }

        // 2�ڈȍ~�̃}�e���A���� Fade �ɂ���
        for (int i = 1; i < mats.Count; i++)
        {
            SetMaterialRenderingMode(mats[i], "Fade");
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

    private void SetMaterialRenderingMode(Material material, string mode)
    {
        switch (mode)
        {
            case "Opaque":
                material.SetFloat("_Mode", 0);
                material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                material.SetInt("_ZWrite", 1);
                material.DisableKeyword("_ALPHATEST_ON");
                material.DisableKeyword("_ALPHABLEND_ON");
                material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                material.renderQueue = -1;
                break;

            case "Fade": // ������
                material.SetFloat("_Mode", 2);
                material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                material.SetInt("_ZWrite", 0);
                material.DisableKeyword("_ALPHATEST_ON");
                material.EnableKeyword("_ALPHABLEND_ON");
                material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                material.renderQueue = 3000;
                break;

            case "Transparent": // ����
                material.SetFloat("_Mode", 3);
                material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                material.SetInt("_ZWrite", 0);
                material.DisableKeyword("_ALPHATEST_ON");
                material.DisableKeyword("_ALPHABLEND_ON");
                material.EnableKeyword("_ALPHAPREMULTIPLY_ON");
                material.renderQueue = 3000;
                break;
        }
    }
}
