using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerSkill : MonoBehaviour
{
    public LineRenderer lineRenderer; // LineRenderer���A�^�b�`
    public float coolTime { get; private set; }

    public bool isOneHand = false; // true: �v���C���[�(�Ў胂�[�h), false: �^�b�`�ʒu�(���胂�[�h)

    [SerializeField] int skillPower = 0;
    [SerializeField] float maxLineLength = 0;
    [SerializeField] float maxSkillCoolTime = 7f; // �K�E�Z�̃N�[���^�C���i�b�j
    [SerializeField] float maxSkillTime = 7f; // �K�E�Z�̍ő�p������

    [SerializeField] ParticleSystem skillEffect;
    [SerializeField] CenterToGrayEffect centerToGrayEffect;
    [SerializeField] GameObject skillChargeEffect; //�X�L�������\�G�t�F�N�g 
    [SerializeField] PlayerSkillAnim playerSkillAnim;
    [SerializeField] private Button skillButton;

    //��ʓ��Ɉړ��͈͂𐧌�
    [SerializeField] LayerMask groundLayer; // �n�ʂ̃��C���[

    [SerializeField, Range(0f, 1f)] float leftOffset = 0.1f;   // ���[�̃I�t�Z�b�g
    [SerializeField, Range(0f, 1f)] float rightOffset = 0.9f;  // �E�[�̃I�t�Z�b�g
    [SerializeField, Range(0f, 1f)] float topOffset = 0.9f;    // ��[�̃I�t�Z�b�g
    [SerializeField, Range(0f, 1f)] float bottomOffset = 0.1f; // ���[�̃I�t�Z�b�g

    Text skillEnemyNumText;

    Camera mainCamera; // �g�p����J����

    private Vector3[] corners = new Vector3[4]; // �l�p�`�̒��_

    private List<Vector3> points = new List<Vector3>(); // ���̒��_���L�^���郊�X�g
    private List<GameObject> detectedEnemies = new List<GameObject>(); // ���m����Enemy���i�[���郊�X�g

    //�X�L���������͓G�Ɠ�����Ȃ�����
    private int playerLayer;
    private int enemyLayer;

    private bool isSkill = false;//�K�E�Z�t���O
    private bool isSkillEndEffect = false;//�K�E�Z�t���O
    private bool canSkillLine = false;//�X�L���̐����������Ԃ�

    //�X�L���{�^���ɓo�^����p
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
                Debug.LogError("LineRenderer���A�^�b�`����Ă��܂���I");
            }
        }

        // ���̕���ݒ�
        lineRenderer.startWidth = 0.1f;
        lineRenderer.endWidth = 0.1f;

        // �}�e���A���������ݒ�
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

    //playerController��Awake/Start�ŌĂсA�l���Z�b�g
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
                // �v���C���[���l�p�`�̒��ɐ���
                ConstrainPlayer();
            }
        }
        else
        {
            _Attack();
        }

        // Skill�����\���ɃG�t�F�N�g�\��
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
        //�O�̂��߃{�^��������������Ɛ����������߂̎w�𗣂������肪���Ԃ�Ȃ��悤�ɑ҂�
        Invoke("StartLine", 1f);
        centerToGrayEffect.Gray(true);
        skillChargeEffect.SetActive(false);
        if (!isOneHand) stickController.SetActive(false);
        SetCanSkill(false);
        //maxSpeed *= skillAddSpeed;
        //moveSpeed *= skillAddSpeed;
        // �Փ˖�����
        Physics.IgnoreLayerCollision(playerLayer, enemyLayer, true);
        Invoke("StopSkill", maxSkillTime);
    }

    public void StopSkill(Vector2 _startPosition, Action onSkillEndCallback = null)
    {
        if (!isSkill || isSkillEndEffect)
        {
            onSkillEndCallback?.Invoke(); // �X�L���łȂ���Α��Ă�
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
                onSkillEndCallback?.Invoke(); // �A�j����ɌĂ�
            });
        }
        else
        {
            StopSkillAnim(_startPosition);
            onSkillEndCallback?.Invoke(); // ���Ă�
        }
    }


    void StopSkillAnim(Vector2 _startPosition)
    {
        // �X�L���I������
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
        // �Փ˂��ĂїL���ɂ���
        Physics.IgnoreLayerCollision(playerLayer, enemyLayer, false);
        canSkillLine = true;
    }

    void OnClickSkillButton()
    {
        if (canSkillGetter())
        {
            // �J�����̎l������n�ʂւ̌�_���擾
            CalculateCorners();
            Skill();
            SetCanSkill(false);
        }
    }

    //�������牺�̓J�����͈͓̔��Ɉړ��͈͂𐧌�
    // �J�����̎l������n�ʂւ̌�_���擾
    void CalculateCorners()
    {
        // �J�����̃r���[�̎l����Viewport���W
        Vector3[] viewportPoints = new Vector3[]
        {
            new Vector3(leftOffset, topOffset, mainCamera.nearClipPlane), // ����
            new Vector3(rightOffset, topOffset, mainCamera.nearClipPlane), // �E��
            new Vector3(rightOffset, bottomOffset, mainCamera.nearClipPlane), // �E��
            new Vector3(leftOffset, bottomOffset, mainCamera.nearClipPlane)  // ����
        };

        for (int i = 0; i < viewportPoints.Length; i++)
        {
            Vector3 worldPoint = mainCamera.ViewportToWorldPoint(viewportPoints[i]);
            Vector3 direction = (worldPoint - mainCamera.transform.position).normalized;

            // �n�ʂƂ̌�_���擾
            if (Physics.Raycast(mainCamera.transform.position, direction, out RaycastHit hit, Mathf.Infinity, groundLayer))
            {
                corners[i] = hit.point;
            }
        }
    }

    public void ConstrainPlayer()
    {
        // �v���C���[�̌��݈ʒu
        Vector3 playerPosition = transform.position;

        // �l�p�`�̒��_���g����2D���ʏ�Ő����������
        Vector2 player2D = new Vector2(playerPosition.x, playerPosition.z);

        // �l�p�`��2D���ʏ�Œ�`
        Vector2[] polygon = new Vector2[]
        {
            new Vector2(corners[0].x, corners[0].z), // ����
            new Vector2(corners[1].x, corners[1].z), // �E��
            new Vector2(corners[2].x, corners[2].z), // �E��
            new Vector2(corners[3].x, corners[3].z)  // ����
        };

        // �v���C���[���l�p�`�̊O�ɏo�Ă��邩
        if (!IsPointInPolygon(player2D, polygon))
        {
            // �v���C���[���l�p�`�̊O�ɏo�Ă���΍ł��߂��_�ɐ���
            Vector2 closestPoint = FindClosestPointInPolygon(player2D, polygon);
            transform.position = new Vector3(closestPoint.x, transform.position.y, closestPoint.y);
        }
    }

    // �v���C���[���l�p�`�̊O�ɏo�Ă��邩����
    bool IsPointInPolygon(Vector2 point, Vector2[] polygon)
    {
        int count = polygon.Length;// ���p�`�̒��_�̐�
        bool isInside = false;
        for (int i = 0, j = count - 1; i < count; j = i++)
        {
            // ���_i�ƒ��_j�̕ӂ��l����
            if ((polygon[i].y > point.y) != (polygon[j].y > point.y) &&
                point.x < (polygon[j].x - polygon[i].x) * (point.y - polygon[i].y) / (polygon[j].y - polygon[i].y) + polygon[i].x)
            {
                isInside = !isInside; // ��������𔽓]������
            }
        }
        return isInside;
    }

    //�v���C���[�����ԋ߂��_���擾�i�e�ӂň�ԋ߂��_��T�����̒��ň�ԋ߂��_��Ԃ��j
    Vector2 FindClosestPointInPolygon(Vector2 point, Vector2[] polygon)
    {
        Vector2 closestPoint = polygon[0];// �����l�Ƃ��đ��p�`�̍ŏ��̒��_��ݒ�
        float minDistance = float.MaxValue;// �ŏ������𖳌���ŏ�����

        for (int i = 0; i < polygon.Length; i++)
        {
            Vector2 segmentStart = polygon[i];
            Vector2 segmentEnd = polygon[(i + 1) % polygon.Length];// ���̒��_�i���[�v�����j

            // �ӏ�̍ł��߂��_��T��
            Vector2 closestOnSegment = ClosestPointOnSegment(point, segmentStart, segmentEnd);

            // �_�Ƃ̋������v�Z
            float distance = Vector2.Distance(point, closestOnSegment);

            // �ŏ��������X�V
            if (distance < minDistance)
            {
                minDistance = distance;
                closestPoint = closestOnSegment;
            }
        }

        return closestPoint;
    }

    //�v���C���[�̈ʒu����segment�̐�����ɐ����ɓ��e���ꂽ�ʒu���擾
    Vector2 ClosestPointOnSegment(Vector2 point, Vector2 segmentStart, Vector2 segmentEnd)
    {
        Vector2 segment = segmentEnd - segmentStart;// �����̃x�N�g��
        float t = Vector2.Dot(point - segmentStart, segment) / segment.sqrMagnitude;// �ˉe�̌W��

        // t��0�`1�͈̔͂ɃN�����v
        t = Mathf.Clamp01(t);

        // ������̍ł��߂��_��Ԃ�
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
        int enemyNum = DetectEnemies(); // �͈͓���Enemy�����m�����̐���Ԃ�
        //�͈͓��̓G��1�̈ȏア����
        if(enemyNum > 0)
        {
            //�G�t�F�N�g���o��
            skillEffect.Play();
        }
        points.Clear(); // �O�Ղ��N���A
        UpdateLineRenderer();
        skillEnemyNumText.gameObject.SetActive(true);

        return enemyNum;
    }

    //���胂�[�h
    private void AddPoint(Vector2 screenPosition)
    {
        Ray ray = Camera.main.ScreenPointToRay(screenPosition);
        Plane groundPlane = new Plane(Vector3.up, new Vector3(0, this.transform.position.y + 0.5f, 0)); //y = �v���C���[�̑����̈ʒu + �v���C���[�̒��S�܂ł̋���

        if (groundPlane.Raycast(ray, out float enter))
        {
            Vector3 worldPosition = ray.GetPoint(enter);

            if (points.Count == 0 || Vector3.Distance(points[points.Count - 1], worldPosition) > 0.1f)
            {
                points.Add(worldPosition);
                UpdateLineRenderer();

                UpdateCoolTime();//�N�[���^�C����UI�X�V
            }
        }
    }

    //�Ў胂�[�h
    private void AddPoint()
    {
        Vector3 worldPosition = transform.position;

        if (points.Count == 0 || Vector3.Distance(points[points.Count - 1], worldPosition) > 0.1f)
        {
            worldPosition.y = 0.5f; // �Œ荂���ɐݒ�
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
        // �O�񌟒m����Enemy���N���A
        Debug.Log("DetectEnemies called. Clearing detectedEnemies.");
        detectedEnemies.Clear();

        // ���_��3�ȏ�Ȃ��Ƒ��p�`�����Ȃ��̂ŁA���m���s��Ȃ�
        if (points.Count < 3)
        {
            skillEnemyNumText.text = detectedEnemies.Count.ToString() + "COMBO!";
            Invoke("StopSkillEnemyNumText", 3);
            return 0;
        }

        // ���ׂĂ�EnemyTag�����I�u�W�F�N�g������
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");

        foreach (GameObject enemy in enemies)
        {
            Collider enemyCollider = enemy.GetComponent<Collider>();

            if (enemyCollider != null)
            {
                Vector3 enemyPosition = enemyCollider.bounds.center;

                // X-Z���ʏ�ł̃|���S����������s��
                if (IsPointInsidePolygonXZ(enemyPosition))
                {
                    EnemyBase enemyBase = enemy.GetComponent<EnemyBase>();

                    // isActive�ȓG������Ώۂɂ���
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
            // X-Z���ʏ�ł̃|���S��������
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

    //�͂����͈͂̒��S���擾
    private Vector3 GetPolygonCenter()
    {
        if (points == null || points.Count == 0)
        {
            Debug.LogWarning("GetPolygonCenter(): points����ł��BVector3.zero��Ԃ��܂��B");
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
            Debug.LogError("GetPolygonCenter(): center��NaN�ɂȂ��Ă��܂��I");
        }

        return center;
    }

    //���������̒������擾
    private float GetLineLength()
    {
        float length = 0f;

        for (int i = 1; i < points.Count; i++)
        {
            length += Vector3.Distance(points[i - 1], points[i]);
        }

        return length;
    }

    //�X�L�����ɐ��̒����ɍ��킹�ăN�[���^�C��������
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
