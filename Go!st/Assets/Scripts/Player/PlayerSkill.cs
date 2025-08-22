using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerSkill : MonoBehaviour
{
    public LineRenderer lineRenderer; // LineRendererをアタッチ
    public float coolTime { get; private set; }

    public bool isOneHand = false; // true: プレイヤー基準(片手モード), false: タッチ位置基準(両手モード)

    [SerializeField] int skillPower = 0;
    [SerializeField] float maxLineLength = 0;
    [SerializeField] float maxSkillCoolTime = 7f; // 必殺技のクールタイム（秒）
    [SerializeField] float maxSkillTime = 7f; // 必殺技の最大継続時間

    [SerializeField] ParticleSystem skillEffect;
    [SerializeField] CenterToGrayEffect centerToGrayEffect;
    [SerializeField] GameObject skillChargeEffect; //スキル発動可能エフェクト 
    [SerializeField] PlayerSkillAnim playerSkillAnim;
    [SerializeField] private Button skillButton;

    //画面内に移動範囲を制限
    [SerializeField] LayerMask groundLayer; // 地面のレイヤー

    [SerializeField, Range(0f, 1f)] float leftOffset = 0.1f;   // 左端のオフセット
    [SerializeField, Range(0f, 1f)] float rightOffset = 0.9f;  // 右端のオフセット
    [SerializeField, Range(0f, 1f)] float topOffset = 0.9f;    // 上端のオフセット
    [SerializeField, Range(0f, 1f)] float bottomOffset = 0.1f; // 下端のオフセット

    Text skillEnemyNumText;

    Camera mainCamera; // 使用するカメラ

    private Vector3[] corners = new Vector3[4]; // 四角形の頂点

    private List<Vector3> points = new List<Vector3>(); // 線の頂点を記録するリスト
    private List<GameObject> detectedEnemies = new List<GameObject>(); // 検知したEnemyを格納するリスト

    //スキル発動中は敵と当たらなくする
    private int playerLayer;
    private int enemyLayer;

    private bool isSkill = false;//必殺技フラグ
    private bool isSkillEndEffect = false;//必殺技フラグ
    private bool canSkillLine = false;//スキルの線を引ける状態か

    //スキルボタンに登録する用
    GameObject stickController;
    Func<bool> canSkillGetter;
    Action<bool> SetCanSkill;

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

        playerLayer = LayerMask.NameToLayer("Player");
        enemyLayer = LayerMask.NameToLayer("Enemy");

        mainCamera = Camera.main;

        SetSkillButton(skillButton);
    }

    public void SetSkillButton(Button _skillButton)
    {
        if (_skillButton != null)
        {
            _skillButton.onClick.RemoveAllListeners();
        }

        _skillButton.onClick.AddListener(() => OnClickSkillButton());
    }

    //playerControllerのAwake/Startで呼び、値をセット
    public void InitializeSkillDependencies(GameObject _stickController, Func<bool> _canSkillGetter, Action<bool> _SetCanSkill)
    {
        stickController = _stickController;
        canSkillGetter = _canSkillGetter;
        SetCanSkill = _SetCanSkill;
    }


    public void Init(Vector2 _startPosition)
    {
        skillChargeEffect.SetActive(false);
        StopSkill(_startPosition);
        canSkillLine = false;
    }

    void StartLine()
    {
        canSkillLine = true;
    }

    public void SkillUpdate(bool _isInteracting, Vector2 _currentPosition, Action _Attack, Vector2 _startPosition)
    {
        //skill
        if (isSkill)
        {
            if (canSkillLine && _isInteracting)
            {
                SkillTouchMove(_currentPosition);
                // プレイヤーを四角形の中に制限
                ConstrainPlayer();
            }
        }
        else
        {
            _Attack();
        }

        // Skill発動可能時にエフェクト表示
        if (!isSkill)
        {
            if (coolTime >= 1)
            {
                skillChargeEffect.SetActive(true);
                SetCanSkill(true);
            }
            else
            {
                AddSkillCoolTime();
            }
        }
        else
        {
            if (coolTime <= 0)
            {
                StopSkill(_startPosition);
                
            }
        }
    }

    void Skill()
    {
        if (isSkill) return;

        isSkill = true;
        //念のためボタンを押した判定と線を引くための指を離した判定がかぶらないように待つ
        Invoke("StartLine", 1f);
        centerToGrayEffect.Gray(true);
        skillChargeEffect.SetActive(false);
        if (!isOneHand) stickController.SetActive(false);
        SetCanSkill(false);
        //maxSpeed *= skillAddSpeed;
        //moveSpeed *= skillAddSpeed;
        // 衝突無効化
        Physics.IgnoreLayerCollision(playerLayer, enemyLayer, true);
        Invoke("StopSkill", maxSkillTime);
    }

    public void StopSkill(Vector2 _startPosition, Action onSkillEndCallback = null)
    {
        if (!isSkill || isSkillEndEffect)
        {
            onSkillEndCallback?.Invoke(); // スキルでなければ即呼ぶ
            return;
        }

        int enemyNum = SkillTouchEnded();
        canSkillLine = false;
        isSkillEndEffect = true;

        if (enemyNum > 0)
        {
            playerSkillAnim.PlayerSkillAnimPlay(() =>
            {
                StopSkillAnim(_startPosition);
                onSkillEndCallback?.Invoke(); // アニメ後に呼ぶ
            });
        }
        else
        {
            StopSkillAnim(_startPosition);
            onSkillEndCallback?.Invoke(); // 即呼ぶ
        }
    }


    void StopSkillAnim(Vector2 _startPosition)
    {
        // スキル終了処理
        isSkill = false;
        isSkillEndEffect = false;
        //maxSpeed /= skillAddSpeed;
        //moveSpeed /= skillAddSpeed;
        centerToGrayEffect.Gray(false);
        if (!isOneHand)
        {
            stickController.SetActive(true);
            stickController.transform.position = _startPosition;
        }
        // 衝突を再び有効にする
        Physics.IgnoreLayerCollision(playerLayer, enemyLayer, false);
        canSkillLine = true;
    }

    void OnClickSkillButton()
    {
        if (canSkillGetter())
        {
            // カメラの四隅から地面への交点を取得
            CalculateCorners();
            Skill();
            SetCanSkill(false);
        }
    }

    //ここから下はカメラの範囲内に移動範囲を制限
    // カメラの四隅から地面への交点を取得
    void CalculateCorners()
    {
        // カメラのビューの四隅のViewport座標
        Vector3[] viewportPoints = new Vector3[]
        {
            new Vector3(leftOffset, topOffset, mainCamera.nearClipPlane), // 左上
            new Vector3(rightOffset, topOffset, mainCamera.nearClipPlane), // 右上
            new Vector3(rightOffset, bottomOffset, mainCamera.nearClipPlane), // 右下
            new Vector3(leftOffset, bottomOffset, mainCamera.nearClipPlane)  // 左下
        };

        for (int i = 0; i < viewportPoints.Length; i++)
        {
            Vector3 worldPoint = mainCamera.ViewportToWorldPoint(viewportPoints[i]);
            Vector3 direction = (worldPoint - mainCamera.transform.position).normalized;

            // 地面との交点を取得
            if (Physics.Raycast(mainCamera.transform.position, direction, out RaycastHit hit, Mathf.Infinity, groundLayer))
            {
                corners[i] = hit.point;
            }
        }
    }

    public void ConstrainPlayer()
    {
        // プレイヤーの現在位置
        Vector3 playerPosition = transform.position;

        // 四角形の頂点を使って2D平面上で制約をかける
        Vector2 player2D = new Vector2(playerPosition.x, playerPosition.z);

        // 四角形を2D平面上で定義
        Vector2[] polygon = new Vector2[]
        {
            new Vector2(corners[0].x, corners[0].z), // 左上
            new Vector2(corners[1].x, corners[1].z), // 右上
            new Vector2(corners[2].x, corners[2].z), // 右下
            new Vector2(corners[3].x, corners[3].z)  // 左下
        };

        // プレイヤーが四角形の外に出ているか
        if (!IsPointInPolygon(player2D, polygon))
        {
            // プレイヤーが四角形の外に出ていれば最も近い点に制限
            Vector2 closestPoint = FindClosestPointInPolygon(player2D, polygon);
            transform.position = new Vector3(closestPoint.x, transform.position.y, closestPoint.y);
        }
    }

    // プレイヤーが四角形の外に出ているか判定
    bool IsPointInPolygon(Vector2 point, Vector2[] polygon)
    {
        int count = polygon.Length;// 多角形の頂点の数
        bool isInside = false;
        for (int i = 0, j = count - 1; i < count; j = i++)
        {
            // 頂点iと頂点jの辺を考える
            if ((polygon[i].y > point.y) != (polygon[j].y > point.y) &&
                point.x < (polygon[j].x - polygon[i].x) * (point.y - polygon[i].y) / (polygon[j].y - polygon[i].y) + polygon[i].x)
            {
                isInside = !isInside; // 内部判定を反転させる
            }
        }
        return isInside;
    }

    //プレイヤーから一番近い点を取得（各辺で一番近い点を探しその中で一番近い点を返す）
    Vector2 FindClosestPointInPolygon(Vector2 point, Vector2[] polygon)
    {
        Vector2 closestPoint = polygon[0];// 初期値として多角形の最初の頂点を設定
        float minDistance = float.MaxValue;// 最小距離を無限大で初期化

        for (int i = 0; i < polygon.Length; i++)
        {
            Vector2 segmentStart = polygon[i];
            Vector2 segmentEnd = polygon[(i + 1) % polygon.Length];// 次の頂点（ループ処理）

            // 辺上の最も近い点を探す
            Vector2 closestOnSegment = ClosestPointOnSegment(point, segmentStart, segmentEnd);

            // 点との距離を計算
            float distance = Vector2.Distance(point, closestOnSegment);

            // 最小距離を更新
            if (distance < minDistance)
            {
                minDistance = distance;
                closestPoint = closestOnSegment;
            }
        }

        return closestPoint;
    }

    //プレイヤーの位置からsegmentの線分上に垂直に投影された位置を取得
    Vector2 ClosestPointOnSegment(Vector2 point, Vector2 segmentStart, Vector2 segmentEnd)
    {
        Vector2 segment = segmentEnd - segmentStart;// 線分のベクトル
        float t = Vector2.Dot(point - segmentStart, segment) / segment.sqrMagnitude;// 射影の係数

        // tを0～1の範囲にクランプ
        t = Mathf.Clamp01(t);

        // 線分上の最も近い点を返す
        return segmentStart + t * segment;
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
                        enemyBase.Damage(skillPower);
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

    public bool GetIsSkill() => isSkill;
}
