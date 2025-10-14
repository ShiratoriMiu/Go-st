using UnityEngine;
using System.Collections.Generic;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource bgmSource;          // BGM�p
    [SerializeField] private int seSourcePoolSize = 10;      // SE�v�[����
    private List<AudioSource> seSources = new List<AudioSource>();

    [Header("Audio Clips")]
    [SerializeField] private AudioClip[] bgmClips;
    [SerializeField] private AudioClip[] seClips;

    private Dictionary<string, AudioClip> bgmDict = new Dictionary<string, AudioClip>();
    private Dictionary<string, AudioClip> seDict = new Dictionary<string, AudioClip>();

    // �Ԉ����p
    private Dictionary<string, float> lastPlayTime = new Dictionary<string, float>();
    private float minInterval = 0.1f; // ����SE��炷�ŏ��Ԋu(�b)

    // �C�x���g
    public System.Action<float> OnMasterVolumeChangedEvent;
    public System.Action<float> OnBGMVolumeChangedEvent;
    public System.Action<float> OnSEVolumeChangedEvent;

    private float _masterVolume = 1f;
    public float masterVolume
    {
        get => _masterVolume;
        set
        {
            _masterVolume = value;
            ApplyVolume();
            OnMasterVolumeChangedEvent?.Invoke(_masterVolume);
        }
    }

    private float _bgmVolume = 1f;
    public float bgmVolume
    {
        get => _bgmVolume;
        set
        {
            _bgmVolume = value;
            ApplyVolume();
            OnBGMVolumeChangedEvent?.Invoke(_bgmVolume);
        }
    }

    private float _seVolume = 1f;
    public float seVolume
    {
        get => _seVolume;
        set
        {
            _seVolume = value;
            ApplyVolume();
            OnSEVolumeChangedEvent?.Invoke(_seVolume);
        }
    }

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

            // PlayerPrefs����ǂݍ��ށi���݂��Ȃ����1f�j
            masterVolume = PlayerPrefs.GetFloat("MasterVolume", 1f);
            bgmVolume = PlayerPrefs.GetFloat("BGMVolume", 1f);
            seVolume = PlayerPrefs.GetFloat("SEVolume", 1f);

            // SE�pAudioSource�v�[���쐬
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
        // ���t���[�����ʂ𔽉f
        ApplyVolume();
    }

    private void ApplyVolume()
    {
        // BGM
        bgmSource.volume = masterVolume * bgmVolume;

        // SE
        foreach (var source in seSources)
        {
            // �Đ����݂̂ɔ��f���Ă��悢
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

        // �Ԉ����`�F�b�N
        if (lastPlayTime.TryGetValue(seName, out float lastTime))
        {
            if (Time.time - lastTime < minInterval)
                return; // �O�񂩂�̊Ԋu���Z����΍Đ����Ȃ�
        }
        lastPlayTime[seName] = Time.time;

        // �󂢂Ă��� AudioSource ��T��
        var source = seSources.Find(s => !s.isPlaying);
        if (source != null)
        {
            source.clip = clip;
            source.volume = masterVolume * seVolume * volume;
            source.Play();
        }
    }
}
