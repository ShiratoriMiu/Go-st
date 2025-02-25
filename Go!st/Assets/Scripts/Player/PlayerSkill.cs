using System.Collections.Generic;
using UnityEngine;

public class PlayerSkill : MonoBehaviour
{
    public LineRenderer lineRenderer; // LineRendererをアタッチ
    private List<Vector3> points = new List<Vector3>(); // 線の頂点を記録するリスト
    private List<GameObject> detectedEnemies = new List<GameObject>(); // 検知したEnemyを格納するリスト

    [SerializeField]
    int attack = 0;

    void Start()
    {
        if (lineRenderer == null)
        {
            lineRenderer = GetComponent<LineRenderer>();
            if (lineRenderer == null)
            {
                Debug.LogError("LineRendererがアタッチされていません！");
            }
        }

        // 線の幅を設定
        lineRenderer.startWidth = 0.1f;
        lineRenderer.endWidth = 0.1f;

        // マテリアルを自動設定
        if (lineRenderer.material == null)
        {
            lineRenderer.material = new Material(Shader.Find("Unlit/Color"));
            lineRenderer.material.color = Color.white;
        }
    }

    public void SkillTouchMove()
    {
        AddPoint();
    }

    public void SkillTouchEnded()
    {
        DetectEnemies(); // 範囲内のEnemyを検知
        points.Clear(); // 軌跡をクリア
        UpdateLineRenderer();
    }

    private void AddPoint()
    {
        Vector3 worldPosition = transform.position;

        if (points.Count == 0 || Vector3.Distance(points[points.Count - 1], worldPosition) > 0.1f)
        {
            worldPosition.y = 0.5f; // 固定高さに設定
            points.Add(worldPosition);
            UpdateLineRenderer();
        }
    }

    private void UpdateLineRenderer()
    {
        if (lineRenderer == null)
            return;

        lineRenderer.positionCount = points.Count;
        lineRenderer.SetPositions(points.ToArray());
    }


    private void DetectEnemies()
    {
        // 前回検知したEnemyをクリア
        detectedEnemies.Clear();

        // 頂点が3つ以上ないと多角形を作れないので、検知を行わない
        if (points.Count < 3)
        {
            Debug.Log("線の頂点が少なすぎます。");
            return;
        }

        // すべてのEnemyTagを持つオブジェクトを検索
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");

        foreach (GameObject enemy in enemies)
        {
            // オブジェクトのコライダーを取得
            Collider enemyCollider = enemy.GetComponent<Collider>();

            if (enemyCollider != null)
            {
                Vector3 enemyPosition = enemyCollider.bounds.center;

                // X-Z平面上でのポリゴン内判定を行う
                if (IsPointInsidePolygonXZ(enemyPosition))
                {
                    // 範囲内にあるので、リストに追加
                    detectedEnemies.Add(enemy);

                    // 検知したオブジェクトに対して処理
                    Debug.Log("Enemy Detected: " + enemy.name);

                    // ここに、検知後の処理を記述(例：ダメージを与えるなど)
                    enemy.GetComponent<EnemyController>().Damage(attack);
                }
            }
        }
    }

    private bool IsPointInsidePolygonXZ(Vector3 point)
    {
        int nvert = points.Count;
        int i, j;
        bool c = false;

        for (i = 0, j = nvert - 1; i < nvert; j = i++)
        {
            // X-Z平面上でのポリゴン内判定
            if (((points[i].z > point.z) != (points[j].z > point.z)) &&
                 (point.x < (points[j].x - points[i].x) * (point.z - points[i].z) / (points[j].z - points[i].z) + points[i].x))
            {
                c = !c;
            }
        }
        return c;
    }
}
