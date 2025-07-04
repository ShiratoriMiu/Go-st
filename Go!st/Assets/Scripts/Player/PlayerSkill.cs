using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class PlayerSkill : MonoBehaviour
{
    public LineRenderer lineRenderer; // LineRendererをアタッチ
    private List<Vector3> points = new List<Vector3>(); // 線の頂点を記録するリスト
    private List<GameObject> detectedEnemies = new List<GameObject>(); // 検知したEnemyを格納するリスト

    [SerializeField] int attack = 0;
    [SerializeField] float maxLineLength = 0;
    [SerializeField] float maxSkillCoolTime = 7f; // 必殺技のクールタイム（秒）

    [SerializeField]
    ParticleSystem skillEffect;

    Text skillEnemyNumText;

    public float coolTime { get; private set; }

    public bool isOneHand = false; // true: プレイヤー基準(片手モード), false: タッチ位置基準(両手モード)

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

    public void SkillTouchMove(Vector2 screenPosition)
    {
        if (isOneHand)
        {
            AddPoint();
        }
        else
        {
            AddPoint(screenPosition);
        }
    }

    public int SkillTouchEnded()
    {
        skillEffect.transform.position = GetPolygonCenter();
        int enemyNum = DetectEnemies(); // 範囲内のEnemyを検知かつその数を返す
        //範囲内の敵が1体以上いたら
        if(enemyNum > 0)
        {
            //エフェクトを出す
            skillEffect.Play();
        }
        points.Clear(); // 軌跡をクリア
        UpdateLineRenderer();
        skillEnemyNumText.gameObject.SetActive(true);

        return enemyNum;
    }

    //両手モード
    private void AddPoint(Vector2 screenPosition)
    {
        Ray ray = Camera.main.ScreenPointToRay(screenPosition);
        Plane groundPlane = new Plane(Vector3.up, new Vector3(0, this.transform.position.y + 0.5f, 0)); //y = プレイヤーの足元の位置 + プレイヤーの中心までの距離

        if (groundPlane.Raycast(ray, out float enter))
        {
            Vector3 worldPosition = ray.GetPoint(enter);

            if (points.Count == 0 || Vector3.Distance(points[points.Count - 1], worldPosition) > 0.1f)
            {
                points.Add(worldPosition);
                UpdateLineRenderer();

                UpdateCoolTime();//クールタイムのUI更新
            }
        }
    }

    //片手モード
    private void AddPoint()
    {
        Vector3 worldPosition = transform.position;

        if (points.Count == 0 || Vector3.Distance(points[points.Count - 1], worldPosition) > 0.1f)
        {
            worldPosition.y = 0.5f; // 固定高さに設定
            points.Add(worldPosition);
            UpdateLineRenderer();

            UpdateCoolTime();
        }
    }


    private void UpdateLineRenderer()
    {
        if (lineRenderer == null)
            return;

        lineRenderer.positionCount = points.Count;
        lineRenderer.SetPositions(points.ToArray());
    }


    private int DetectEnemies()
    {
        // 前回検知したEnemyをクリア
        Debug.Log("DetectEnemies called. Clearing detectedEnemies.");
        detectedEnemies.Clear();

        // 頂点が3つ以上ないと多角形を作れないので、検知を行わない
        if (points.Count < 3)
        {
            skillEnemyNumText.text = detectedEnemies.Count.ToString() + "COMBO!";
            Invoke("StopSkillEnemyNumText", 3);
            return 0;
        }

        // すべてのEnemyTagを持つオブジェクトを検索
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");

        foreach (GameObject enemy in enemies)
        {
            Collider enemyCollider = enemy.GetComponent<Collider>();

            if (enemyCollider != null)
            {
                Vector3 enemyPosition = enemyCollider.bounds.center;

                // X-Z平面上でのポリゴン内判定を行う
                if (IsPointInsidePolygonXZ(enemyPosition))
                {
                    EnemyBase enemyBase = enemy.GetComponent<EnemyBase>();

                    // isActiveな敵だけを対象にする
                    if (enemyBase != null && enemyBase.isActive && !detectedEnemies.Contains(enemy))
                    {
                        detectedEnemies.Add(enemy);
                        enemyBase.Damage(attack);
                    }
                }
            }
        }
        skillEnemyNumText.text = detectedEnemies.Count.ToString() + "COMBO!";
        Invoke("StopSkillEnemyNumText", 3);

        return detectedEnemies.Count;
    }

    void StopSkillEnemyNumText()
    {
        skillEnemyNumText.gameObject.SetActive(false);
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

    public void SetSkillEnemyNumText(Text _skillEnemyNumText)
    {
        skillEnemyNumText = _skillEnemyNumText;
    }

    //囲った範囲の中心を取得
    private Vector3 GetPolygonCenter()
    {
        if (points == null || points.Count == 0)
        {
            Debug.LogWarning("GetPolygonCenter(): pointsが空です。Vector3.zeroを返します。");
            return Vector3.zero;
        }

        Vector3 center = Vector3.zero;
        foreach (Vector3 point in points)
        {
            center += point;
        }
        center /= points.Count;

        if (float.IsNaN(center.x) || float.IsNaN(center.y) || float.IsNaN(center.z))
        {
            Debug.LogError("GetPolygonCenter(): centerがNaNになっています！");
        }

        return center;
    }

    //引いた線の長さを取得
    private float GetLineLength()
    {
        float length = 0f;

        for (int i = 1; i < points.Count; i++)
        {
            length += Vector3.Distance(points[i - 1], points[i]);
        }

        return length;
    }

    //スキル中に線の長さに合わせてクールタイムを減少
    private void UpdateCoolTime()
    {
        float length = GetLineLength();
        coolTime = 1 - Mathf.Clamp01(length / maxLineLength);
    }

    public void AddSkillCoolTime()
    {
        coolTime+= Mathf.Clamp01(Time.deltaTime / maxSkillCoolTime);
    }

    public void ResetSkillCoolTime()
    {
        coolTime = 1;
    }
}
