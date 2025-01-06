using UnityEngine;

public class CameraEdgeIntersections : MonoBehaviour
{
    public Camera mainCamera; // �g�p����J����
    public LayerMask layerMask; // �`�F�b�N����I�u�W�F�N�g�̃��C���[

    Vector3[] limitPos = new Vector3[4];

    private void Start()
    {
        // �r���[�|�[�g�̏㉺���E�̍��W���`
        Vector3[] viewportPoints = new Vector3[]
        {
            new Vector3(0.5f, 1f, mainCamera.nearClipPlane), // ��
            new Vector3(0.5f, 0f, mainCamera.nearClipPlane), // ��
            new Vector3(0f, 0.5f, mainCamera.nearClipPlane), // ��
            new Vector3(1f, 0.5f, mainCamera.nearClipPlane)  // �E
        };

        string[] directions = { "��", "��", "��", "�E" };

        for (int i = 0; i < viewportPoints.Length; i++)
        {
            // �r���[�|�[�g���W�����[���h���W�ɕϊ�
            Vector3 edgePoint = mainCamera.ViewportToWorldPoint(viewportPoints[i]);

            // �J�����ʒu����r���[�|�[�g���W�Ɍ����ă��C���쐬
            Vector3 direction = (edgePoint - mainCamera.transform.position).normalized;
            Ray ray = new Ray(mainCamera.transform.position, direction);

            // ���C�L���X�g�Ō�_���擾
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, layerMask))
            {
                //Debug.DrawLine(ray.origin, hit.point, Color.red); // �f�o�b�O�p�ɉ���
                Debug.Log($"����: {directions[i]} ��_: {hit.point}, �I�u�W�F�N�g: {hit.collider.name}");
                limitPos[i] = hit.point;
            }
            else
            {
                //Debug.DrawLine(ray.origin, ray.origin + direction * 100f, Color.blue); // ���C��������Ȃ��ꍇ�̃f�o�b�O
                Debug.Log($"����: {directions[i]} �I�u�W�F�N�g�Ƀq�b�g���܂���ł���");
            }
        }
    }

    void Update()
    {
        
    }
}
