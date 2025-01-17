using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerSkill : MonoBehaviour
{
    public LineRenderer lineRenderer; // LineRenderer���A�^�b�`
    private List<Vector3> points = new List<Vector3>(); // ���̒��_���L�^���郊�X�g

    PlayerInputAction action;

    private bool isInteracting = false;//���͒��t���O

    void Awake()
    {
        action = new PlayerInputAction();
    }

    void Start()
    {
        action.Player.Touch.performed += OnTouchMoved;
        action.Player.TouchClick.started += OnTouchStarted;
        action.Player.TouchClick.canceled += OnTouchEnded;

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

    private void OnEnable()
    {
        action.Enable();
    }

    private void OnDisable()
    {
        action.Disable();
    }

    public void OnTouchStarted(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            isInteracting = true;
        }
    }

    public void OnTouchMoved(InputAction.CallbackContext context)
    {
        if (context.performed && isInteracting)
        {
            Vector2 screenPosition = context.ReadValue<Vector2>();
            AddPoint(screenPosition);
        }
    }

    public void OnTouchEnded(InputAction.CallbackContext context)
    {
        if (context.canceled && isInteracting)
        {
            //points.Clear(); // �O�Ղ��N���A
            isInteracting = false;
            //UpdateLineRenderer();
        }
    }

    private void AddPoint(Vector2 screenPosition)
    {
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, Mathf.Abs(Camera.main.transform.position.z)));

        Debug.Log($"Screen Position: {screenPosition}, World Position: {worldPosition}");

        if (points.Count == 0 || Vector3.Distance(points[points.Count - 1], worldPosition) > 0.1f)
        {
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
}
