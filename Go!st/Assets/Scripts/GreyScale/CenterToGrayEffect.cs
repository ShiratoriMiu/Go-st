using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class CenterToGrayEffect : MonoBehaviour
{
    [SerializeField] Shader shader;
    //Range(0f, 1f)
    [SerializeField] float radius = 0;
    //Range(0.01f, 1f)
    [SerializeField] float smoothness = 0;
    [SerializeField] float duration = 1f;

    private Material material;

    private Coroutine greyCoroutine;

    void Start()
    {
        if (shader == null)
            shader = Shader.Find("Hidden/CenterToGray");

        if (shader != null)
            material = new Material(shader);
    }

    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (material == null)
        {
            Graphics.Blit(source, destination);
            return;
        }

        material.SetFloat("_Radius", radius);
        material.SetFloat("_Smoothness", smoothness);

        Graphics.Blit(source, destination, material);
    }

    IEnumerator GreyScaleAnim(bool _onGray)
    {
        float start = radius;
        float end = 1f;
        float t = 0f;

        if (_onGray)
        {
            end = 1f;
        }
        else
        {
            end = 0f;
        }

        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            radius = Mathf.Lerp(start, end, t);
            yield return null;
        }

        radius = end; // �Ō�ɃL�b�`���l���Z�b�g
    }

    public void Gray(bool _onGray)
    {
        if (greyCoroutine != null)
            StopCoroutine(greyCoroutine); // �O��̃R���[�`���������Ă���~�߂�

        greyCoroutine = StartCoroutine(GreyScaleAnim(_onGray)); // �V�����R���[�`�����J�n
    }

    public void ResetGrey()
    {
        if (greyCoroutine != null)
        {
            StopCoroutine(greyCoroutine);
            radius = 0;
            greyCoroutine = null;
        }
    }
}
