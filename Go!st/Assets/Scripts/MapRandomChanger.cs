using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapRandomChanger : MonoBehaviour
{
    [SerializeField] GameObject[] maps;

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
        foreach (GameObject map in maps)
        {
            if (map != null)
                map.SetActive(false);
        }

        // ランダムで1つ選んで表示
        int randomIndex = Random.Range(0, maps.Length);
        if (maps[randomIndex] != null)
            maps[randomIndex].SetActive(true);
    }
}