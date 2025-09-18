using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GameManager;

public class TutorialController : MonoBehaviour
{
    [SerializeField] private GameObject tutorialUI;

    [SerializeField] private TutorialVideoController videoController;

    void Start()
    {
        InputManager.Instance.OnAnyTouchDown += HandleTutorialTouch;

        if (!PlayerPrefs.HasKey("Tutorial"))
        {
            // èââÒãNìÆ
            tutorialUI.SetActive(true);
            videoController.PlayVideo();

            // ÉtÉâÉOÇï€ë∂
            PlayerPrefs.SetInt("Tutorial", 1);
            PlayerPrefs.Save();
        }
        else
        {
            // 2âÒñ⁄à»ç~
            tutorialUI.SetActive(false);
        }
    }

    private void OnDestroy()
    {
        if (InputManager.Instance != null)
            InputManager.Instance.OnAnyTouchDown -= HandleTutorialTouch;
    }

    private void HandleTutorialTouch()
    {
        videoController.StopVideo();
        tutorialUI.SetActive(false);
    }

    public void ActiveTutorial()
    {
        InputManager.Instance.OnAnyTouchDown += HandleTutorialTouch;
        videoController.PlayVideo();
        tutorialUI.SetActive(true);
    }
}
