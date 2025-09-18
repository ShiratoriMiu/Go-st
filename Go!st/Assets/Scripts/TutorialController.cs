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
            // ����N��
            tutorialUI.SetActive(true);
            videoController.PlayVideo();

            // �t���O��ۑ�
            PlayerPrefs.SetInt("Tutorial", 1);
            PlayerPrefs.Save();
        }
        else
        {
            // 2��ڈȍ~
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
