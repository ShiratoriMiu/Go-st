using UnityEngine;
using System.Collections;

public class CameraLerpSwitcher : MonoBehaviour
{
    public Transform[] cameraPositions; // カメラの位置リスト
    private Transform targetPosition;
    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;
        targetPosition = cameraPositions[0];
    }

    public void SwitchCamera()
    {
        int nextIndex = (System.Array.IndexOf(cameraPositions, targetPosition) + 1) % cameraPositions.Length;
        StartCoroutine(MoveCamera(cameraPositions[nextIndex]));
    }

    IEnumerator MoveCamera(Transform newTarget)
    {
        targetPosition = newTarget;
        float duration = 1f;
        float elapsedTime = 0f;
        Vector3 startPos = mainCamera.transform.position;
        Quaternion startRot = mainCamera.transform.rotation;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / duration;
            mainCamera.transform.position = Vector3.Lerp(startPos, targetPosition.position, t);
            mainCamera.transform.rotation = Quaternion.Slerp(startRot, targetPosition.rotation, t);
            yield return null;
        }
    }
}
