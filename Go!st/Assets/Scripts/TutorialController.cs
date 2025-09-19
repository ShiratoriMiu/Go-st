using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialController : MonoBehaviour
{
    [SerializeField] private GameObject tutorialUI;
    [SerializeField] private GameObject SettingUI;

    [SerializeField] private TutorialVideoController videoController;

    [SerializeField] private GameManager gameManager;

    void Start()
    {
        //InputManager.Instance.OnAnyTouchDown += HandleTutorialTouch;

        //if (!PlayerPrefs.HasKey("Tutorial"))
        //{
        //    // èââÒãNìÆ
        //    tutorialUI.SetActive(true);
        //    videoController.PlayVideo();

        //    // ÉtÉâÉOÇï€ë∂
        //    PlayerPrefs.SetInt("Tutorial", 1);
        //    PlayerPrefs.Save();
        //}
        //else
        //{
        //    // 2âÒñ⁄à»ç~
        //    tutorialUI.SetActive(false);
        //}
    }

    private void OnDestroy()
    {
        if (InputManager.Instance != null)
            InputManager.Instance.OnAnyTouchDown -= HandleTutorialTouch;
    }

    private void HandleTutorialTouch()
    {
        if (InputManager.Instance != null)
            InputManager.Instance.OnAnyTouchDown -= HandleTutorialTouch;
        videoController.StopVideo();
        tutorialUI.SetActive(false);
        gameManager.ToTitle(false);
    }

    public void ActiveTutorial()
    {
        videoController.PlayVideo();
        SettingUI.SetActive(false);
        tutorialUI.SetActive(true);
        InputManager.Instance.OnAnyTouchDown += HandleTutorialTouch;
    }
}
