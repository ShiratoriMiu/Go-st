using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class URLOpen : MonoBehaviour
{
    // �C�ӂ�URL���w��
    [SerializeField] private string url = "https://example.com";

    public void OpenURL()
    {
        Application.OpenURL(url);
    }
}
