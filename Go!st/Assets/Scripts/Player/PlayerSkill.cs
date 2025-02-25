using System.Collections.Generic;
using UnityEngine;

public class PlayerSkill : MonoBehaviour
{
    public LineRenderer lineRenderer; // LineRenderer���A�^�b�`
    private List<Vector3> points = new List<Vector3>(); // ���̒��_���L�^���郊�X�g
    private List<GameObject> detectedEnemies = new List<GameObject>(); // ���m����Enemy���i�[���郊�X�g

    [SerializeField]
    int attack = 0;

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

    public void SkillTouchMove()
    {
        AddPoint();
    }

    public void SkillTouchEnded()
    {
        DetectEnemies(); // �͈͓���Enemy�����m
        points.Clear(); // �O�Ղ��N���A
        UpdateLineRenderer();
    }

    private void AddPoint()
    {
        Vector3 worldPosition = transform.position;

        if (points.Count == 0 || Vector3.Distance(points[points.Count - 1], worldPosition) > 0.1f)
        {
            worldPosition.y = 0.5f; // �Œ荂���ɐݒ�
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
        // �O�񌟒m����Enemy���N���A
        detectedEnemies.Clear();

        // ���_��3�ȏ�Ȃ��Ƒ��p�`�����Ȃ��̂ŁA���m���s��Ȃ�
        if (points.Count < 3)
        {
            Debug.Log("���̒��_�����Ȃ����܂��B");
            return;
        }

        // ���ׂĂ�EnemyTag�����I�u�W�F�N�g������
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");

        foreach (GameObject enemy in enemies)
        {
            // �I�u�W�F�N�g�̃R���C�_�[���擾
            Collider enemyCollider = enemy.GetComponent<Collider>();

            if (enemyCollider != null)
            {
                Vector3 enemyPosition = enemyCollider.bounds.center;

                // X-Z���ʏ�ł̃|���S����������s��
                if (IsPointInsidePolygonXZ(enemyPosition))
                {
                    // �͈͓��ɂ���̂ŁA���X�g�ɒǉ�
                    detectedEnemies.Add(enemy);

                    // ���m�����I�u�W�F�N�g�ɑ΂��ď���
                    Debug.Log("Enemy Detected: " + enemy.name);

                    // �����ɁA���m��̏������L�q(��F�_���[�W��^����Ȃ�)
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
            // X-Z���ʏ�ł̃|���S��������
            if (((points[i].z > point.z) != (points[j].z > point.z)) &&
                 (point.x < (points[j].x - points[i].x) * (point.z - points[i].z) / (points[j].z - points[i].z) + points[i].x))
            {
                c = !c;
            }
        }
        return c;
    }
}
