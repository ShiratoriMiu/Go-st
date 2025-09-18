using UnityEngine;
using UnityEngine.Video;
using System.IO;
using System.Collections;

public class TutorialVideoController : MonoBehaviour
{
    public VideoPlayer videoPlayer;
    public string videoFileName = "Tutorial.mp4";

    private string videoPath;

    void Awake()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        StartCoroutine(LoadVideoAndroid());
#else
        videoPath = Path.Combine(Application.streamingAssetsPath, videoFileName);
        PrepareVideo(videoPath);
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

        videoPath = Path.Combine(Application.persistentDataPath, videoFileName);
        File.WriteAllBytes(videoPath, request.downloadHandler.data);

        PrepareVideo(videoPath);
    }
#endif

    private void PrepareVideo(string path)
    {
        videoPlayer.url = path;
        videoPlayer.Prepare();
        videoPlayer.prepareCompleted += OnVideoPrepared;
    }

    private void OnVideoPrepared(VideoPlayer vp)
    {
        vp.Play();
        vp.prepareCompleted -= OnVideoPrepared;
        Debug.Log("ìÆâÊçƒê∂äJén: " + vp.url);
    }

    // -----------------------------
    // äOïîÇ©ÇÁåƒÇ◊ÇÈçƒê∂ / í‚é~
    // -----------------------------
    public void PlayVideo()
    {
        if (videoPlayer.isPrepared)
        {
            videoPlayer.Play();
        }
        else if (!string.IsNullOrEmpty(videoPath))
        {
            PrepareVideo(videoPath);
        }
    }

    public void StopVideo()
    {
        if (videoPlayer.isPlaying)
        {
            videoPlayer.Stop();
        }
    }
}
