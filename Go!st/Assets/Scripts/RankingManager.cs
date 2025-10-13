using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Threading.Tasks;

public class RankingManager : MonoBehaviour
{
    [SerializeField] private FirebaseController firebaseController;

    [Header("�����L���OUI�ݒ�")]
    [SerializeField] private Transform rankingContainer; // VerticalLayoutGroup / GridLayoutGroup ����
    [SerializeField] private GameObject rankingTextBGPrefab;
    [SerializeField] private int maxDisplayCount = 100;
    [SerializeField] private ScrollRect rankingScrollRect;

    private List<Text> rankingTextList = new List<Text>();
    private Task initializationTask;

    private bool _isFetchingTopRankings = false; // �A�Ŗh�~�p�t���O

    private async void Start()
    {
        Debug.Log("[RankingManager] Firebase���������ҋ@��...");
        await LoginController.Instance.WaitForFirebaseReadyAsync();
        Debug.Log("[RankingManager] Firebase���������B�����L���O�G���g���������J�n...");

        initializationTask = firebaseController.EnsureRankingEntryAsync();
        await initializationTask;

        Debug.Log("[RankingManager] �����L���O�G���g�������������B");

        GenerateRankingTextUI();
    }

    private void GenerateRankingTextUI()
    {
        if (rankingContainer == null) return;

        // ���łɐ����ς݂�UI���폜
        foreach (Transform child in rankingContainer)
        {
            if (Application.isPlaying)
            {
                Destroy(child.gameObject);
            }
            else
            {
                // �G�f�B�^���[�h���ɌĂ΂ꂽ�ꍇ�͂�������g�p
                DestroyImmediate(child.gameObject);
            }
        }
        rankingTextList.Clear();

        for (int i = 0; i < maxDisplayCount; i++)
        {
            GameObject bg = Instantiate(rankingTextBGPrefab, rankingContainer);
            Text rankingText = bg.GetComponentInChildren<Text>();
            rankingText.text = $"{i + 1}.  --- : ---";
            rankingTextList.Add(rankingText);
        }
    }

    public async void OnRankingButtonClicked()
    {
        if (_isFetchingTopRankings) return; // �A�Ŗh�~
        // �X�N���[���ʒu����ԏ�ɖ߂�
        if (rankingScrollRect != null)
            rankingScrollRect.verticalNormalizedPosition = 1f;
        _isFetchingTopRankings = true;

        Debug.Log("[RankingManager] �����L���O�{�^�������B�����������ҋ@�J�n...");

        if (initializationTask != null)
        {
            await initializationTask;
            initializationTask = null;
        }

        Debug.Log("[RankingManager] �����������B�����L���O�擾�J�n...");

        const int MaxRetryCount = 10; // �ő�10��Ď擾�i��5�b�j
        int retryCount = 0;
        List<(int rank, string name, int score, List<PlayerIconData> icons)> topRanks = null;

        while (retryCount < MaxRetryCount)
        {
            topRanks = await firebaseController.GetTopRankingsFromServerAsync(maxDisplayCount);

            if (topRanks.Count > 0)
            {
                Debug.Log($"[RankingManager] �����L���O�擾�����i���s��: {retryCount + 1}�j ����: {topRanks.Count}");
                break;
            }
            else
            {
                Debug.LogWarning($"[RankingManager] �����L���O�擾���ʂ�0���A�Ď擾���s�� ({retryCount + 1}/{MaxRetryCount})...");
                await Task.Delay(500); // 0.5�b�ҋ@
            }

            retryCount++;
        }

        if (topRanks == null || topRanks.Count == 0)
        {
            Debug.LogError("[RankingManager] �����L���O�擾���s: �f�[�^�����݂��܂���B");

            for (int i = 0; i < rankingTextList.Count; i++)
            {
                rankingTextList[i].text = $"{i + 1}. --- : ---";
            }
        }
        else
        {
            DisplayRanking(topRanks);
        }

        Debug.Log("[RankingManager] �����L���O�\�����������B");
        _isFetchingTopRankings = false; // �A�Ŗh�~�t���O����
    }

    private void DisplayRanking(List<(int rank, string name, int score, List<PlayerIconData> icons)> rankings)
    {
        for (int i = 0; i < rankingTextList.Count; i++)
        {
            if (i < rankings.Count)
            {
                var entry = rankings[i];
                rankingTextList[i].text = $"{entry.rank}. {entry.name} : {entry.score}";

                // --- �A�C�R�����f���� ---
                Transform bgTransform = rankingTextList[i].transform.parent; // �e��RankingTextBGPrefab
                Image iconImage = bgTransform.Find("IconBG/IconImage")?.GetComponent<Image>();
                Image iconBG = bgTransform.Find("IconBG")?.GetComponent<Image>();

                if (iconImage != null && entry.icons != null && entry.icons.Count > 0)
                {
                    // �܂���\��
                    iconImage.enabled = false;
                    if (iconBG != null) iconBG.enabled = false;

                    // �e�A�C�R���f�[�^��ǂݍ���
                    foreach (var iconData in entry.icons)
                    {
                        Sprite sprite = Resources.Load<Sprite>($"PlayerIcon/{iconData.name}");
                        Debug.Log(iconData.name);
                        if (sprite == null)
                        {
                            Debug.LogWarning($"[RankingManager] �A�C�R�� '{iconData.name}' �� Resources/PlayerIcon �Ɍ�����܂���B");
                            continue;
                        }

                        if (iconData.style == PlayerIconStyle.Chara)
                        {
                            if (iconImage != null)
                            {
                                iconImage.sprite = sprite;
                                iconImage.enabled = true;
                            }
                        }
                        else
                        {
                            if (iconBG != null)
                            {
                                iconBG.sprite = sprite;
                                iconBG.enabled = true;
                            }
                        }
                    }
                }
            }
            else
            {
                rankingTextList[i].text = $"{i + 1}. --- : ---";
            }
        }
    }
}
