using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ButtonSEController : MonoBehaviour
{
    [SerializeField] private string seName = "ButtonSE";

    private void Awake()
    {
        var button = GetComponent<Button>();
        button.onClick.AddListener(() => SoundManager.Instance.PlaySE(seName));
    }
}
