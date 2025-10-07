using UnityEngine;

public class GachaBGPatternScrollUI : MonoBehaviour
{
    [SerializeField] private float scrollSpeed = 100f; // ������X�N���[�����x�i�s�N�Z���P�ʁj
    [SerializeField] private RectTransform[] backgrounds; // ���[�v������w�i

    private float bgHeight;

    void Start()
    {
        if (backgrounds.Length == 0)
        {
            Debug.LogError("�w�i���ݒ肳��Ă��܂���I");
            return;
        }

        // �w�i�̍����i�����T�C�Y��z��j
        bgHeight = backgrounds[0].rect.height;

        // �����z�u�F1���ڂ����_�ɁA2���ڈȍ~�͏��Ԃɉ��ɔz�u
        for (int i = 0; i < backgrounds.Length; i++)
        {
            backgrounds[i].localPosition = new Vector3(
                0,
                -i * bgHeight,  // �������ɏ��Ԃɔz�u
                backgrounds[i].localPosition.z
            );
        }
    }

    void Update()
    {
        // �X�N���[����[�iCanvas Pivot �������̏ꍇ�j
        float screenTopY = Screen.height / 2f;

        foreach (var bg in backgrounds)
        {
            // ������ɃX�N���[��
            bg.localPosition += Vector3.up * scrollSpeed * Time.deltaTime;

            // �w�i���X�N���[����[�𒴂����牺�Ƀ��[�v
            if (bg.localPosition.y - bgHeight / 2 > screenTopY)
            {
                // ��ԉ��̔w�i�̉��Ɉړ�
                float minY = float.MaxValue;
                foreach (var b in backgrounds)
                {
                    if (b.localPosition.y < minY) minY = b.localPosition.y;
                }
                bg.localPosition = new Vector3(bg.localPosition.x, minY - bgHeight, bg.localPosition.z);
            }
        }
    }
}
