using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapRandomChanger : MonoBehaviour
{
    public static MapRandomChanger Instance { get; private set; }

    [System.Serializable]
    public class MapObj
    {
        public GameObject mapObj;
        public GameObject offModel;
        public string bgmName;
    }

    [SerializeField] private MapObj[] maps;

    private void Awake()
    {
        // シングルトン化
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogWarning("MapRandomChanger が複数存在します。破棄します。");
            Destroy(gameObject);
            return;
        }

        // 必要ならシーン切り替え時も破棄しない
        // DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// ランダムで1つをアクティブにし、それ以外を非表示にする
    /// </summary>
    public void ActivateRandomMap()
    {
        if (maps == null || maps.Length == 0)
        {
            Debug.LogWarning("maps が空です。オブジェクトを登録してください。");
            return;
        }

        // まず全部非表示にする
        foreach (MapObj map in maps)
        {
            if (map != null)
                map.mapObj.SetActive(false);
        }

        // ランダムで1つ選んで表示
        int randomIndex = Random.Range(0, maps.Length);
        if (maps[randomIndex] != null)
        {
            maps[randomIndex].mapObj.SetActive(true);
            maps[randomIndex].offModel.SetActive(false);
            //SoundManager.Instance.StopBGM();
            //SoundManager.Instance.PlayBGM(maps[randomIndex].bgmName, true);
        }
    }

    public void OffModelActive()
    {
        // offModelを表示
        foreach (MapObj map in maps)
        {
            if (map != null && map.mapObj.activeSelf)
                map.offModel.SetActive(true);
        }
    }
}
