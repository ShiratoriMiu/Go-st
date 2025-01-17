using System.Collections.Generic;
using UnityEngine;

public class PlayerSkill01 : MonoBehaviour
{
    public LineRenderer lineRenderer; // LineRenderer���A�^�b�`
    private List<Vector3> points = new List<Vector3>(); // ���̒��_���L�^���郊�X�g

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

    public void SkillTouchMove(Vector2 _context)
    {
        Vector2 screenPosition = _context;
        AddPoint(screenPosition);
    }

    public void SkillTouchEnded()
    {
        points.Clear(); // �O�Ղ��N���A
        UpdateLineRenderer();
    }

    private void AddPoint(Vector2 screenPosition)
    {
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, Mathf.Abs(Camera.main.transform.position.z)));

        Debug.Log($"Screen Position: {screenPosition}, World Position: {worldPosition}");

        if (points.Count == 0 || Vector3.Distance(points[points.Count - 1], worldPosition) > 0.1f)
        {
            points.Add(transform.position);
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
}
