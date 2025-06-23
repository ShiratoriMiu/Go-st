using UnityEngine;
using UnityEngine.Video;
using System.IO;
using System.Collections;

public class TutorialVideoController : MonoBehaviour
{
    public VideoPlayer videoPlayer;
    public string videoFileName = "Tutorial.mp4";

    void Start()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        StartCoroutine(LoadVideoAndroid());
#else
        string path = Path.Combine(Application.streamingAssetsPath, videoFileName);
        videoPlayer.url = path;
        videoPlayer.Play();
#endif
    }

#if UNITY_ANDROID && !UNITY_EDITOR
    private IEnumerator LoadVideoAndroid()
    {
        string sourcePath = Path.Combine(Application.streamingAssetsPath, videoFileName);
        var request = UnityEngine.Networking.UnityWebRequest.Get(sourcePath);
        yield return request.SendWebRequest();

        if (request.result != UnityEngine.Networking.UnityWebRequest.Result.Success)
        {
            Debug.LogError("ìÆâÊÇÃì«Ç›çûÇ›Ç…é∏îs: " + request.error);
            yield break;
        }

        string tempPath = Path.Combine(Application.persistentDataPath, videoFileName);
        File.WriteAllBytes(tempPath, request.downloadHandler.data);

        videoPlayer.url = tempPath;
        videoPlayer.Play();
    }
#endif
}
