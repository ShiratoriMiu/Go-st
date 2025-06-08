using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerSkill : MonoBehaviour
{
    public LineRenderer lineRenderer; // LineRenderer���A�^�b�`
    private List<Vector3> points = new List<Vector3>(); // ���̒��_���L�^���郊�X�g
    private List<GameObject> detectedEnemies = new List<GameObject>(); // ���m����Enemy���i�[���郊�X�g

    [SerializeField]
    int attack = 0;

    [SerializeField]
    ParticleSystem skillEffect;

    Text skillEnemyNumText;

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
    }

    public void SkillTouchMove(Vector2 screenPosition)
    {
        AddPoint(screenPosition);
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

    private void AddPoint(Vector2 screenPosition)
    {
        Ray ray = Camera.main.ScreenPointToRay(screenPosition);
        Plane groundPlane = new Plane(Vector3.up, new Vector3(0, 0.5f, 0)); // Y = 0.5 �Œ�̕���

        if (groundPlane.Raycast(ray, out float enter))
        {
            Vector3 worldPosition = ray.GetPoint(enter);

            if (points.Count == 0 || Vector3.Distance(points[points.Count - 1], worldPosition) > 0.1f)
            {
                points.Add(worldPosition);
                UpdateLineRenderer();
            }
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


}
