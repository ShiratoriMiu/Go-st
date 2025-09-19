using UnityEngine;
using System.Collections.Generic;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource bgmSource;          // BGM用
    [SerializeField] private int seSourcePoolSize = 10;      // SEプール数
    private List<AudioSource> seSources = new List<AudioSource>();

    [Header("Audio Clips")]
    [SerializeField] private AudioClip[] bgmClips;
    [SerializeField] private AudioClip[] seClips;

    private Dictionary<string, AudioClip> bgmDict = new Dictionary<string, AudioClip>();
    private Dictionary<string, AudioClip> seDict = new Dictionary<string, AudioClip>();

    // 間引き用
    private Dictionary<string, float> lastPlayTime = new Dictionary<string, float>();
    private float minInterval = 0.1f; // 同じSEを鳴らす最小間隔(秒)

    // === 音量設定 ===
    [Header("Volume Settings")]
    [Range(0f, 1f)] public float masterVolume = 1f;
    [Range(0f, 1f)] public float bgmVolume = 1f;
    [Range(0f, 1f)] public float seVolume = 1f;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            foreach (var clip in bgmClips)
                bgmDict[clip.name] = clip;

            foreach (var clip in seClips)
                seDict[clip.name] = clip;

            // SE用AudioSourceプール作成
            for (int i = 0; i < seSourcePoolSize; i++)
            {
                var source = gameObject.AddComponent<AudioSource>();
                seSources.Add(source);
            }
        }
        else
        {
            Destroy(gameObject);
        }

        PlayBGM("HomeBGM", true);
    }

    private void Update()
    {
        // 毎フレーム音量を反映
        ApplyVolume();
    }

    private void ApplyVolume()
    {
        // BGM
        bgmSource.volume = masterVolume * bgmVolume;

        // SE
        foreach (var source in seSources)
        {
            // 再生中のみに反映してもよい
            source.volume = masterVolume * seVolume;
        }
    }

    // ==== BGM ====
    public void PlayBGM(string bgmName, bool loop = true)
    {
        if (bgmDict.TryGetValue(bgmName, out var clip))
        {
            bgmSource.clip = clip;
            bgmSource.loop = loop;
            bgmSource.volume = masterVolume * bgmVolume;
            bgmSource.Play();
        }
    }

    public void StopBGM()
    {
        bgmSource.Stop();
    }

    // ==== SE ====
    public void PlaySE(string seName, float volume = 1f)
    {
        if (!seDict.TryGetValue(seName, out var clip)) return;

        // 間引きチェック
        if (lastPlayTime.TryGetValue(seName, out float lastTime))
        {
            if (Time.time - lastTime < minInterval)
                return; // 前回からの間隔が短ければ再生しない
        }
        lastPlayTime[seName] = Time.time;

        // 空いている AudioSource を探す
        var source = seSources.Find(s => !s.isPlaying);
        if (source != null)
        {
            source.clip = clip;
            source.volume = masterVolume * seVolume * volume;
            source.Play();
        }
    }
}
