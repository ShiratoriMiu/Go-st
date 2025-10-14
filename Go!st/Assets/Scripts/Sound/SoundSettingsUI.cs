using UnityEngine;
using UnityEngine.UI;

public class SoundSettingsUI : MonoBehaviour
{
    [SerializeField] private Slider masterSlider;
    [SerializeField] private Slider bgmSlider;
    [SerializeField] private Slider seSlider;

    private void Start()
    {
        // スライダーに初期値を反映
        masterSlider.value = SoundManager.Instance.masterVolume;
        bgmSlider.value = SoundManager.Instance.bgmVolume;
        seSlider.value = SoundManager.Instance.seVolume;

        // スライダー変更時にSoundManagerへ反映
        masterSlider.onValueChanged.AddListener(OnMasterSliderChanged);
        bgmSlider.onValueChanged.AddListener(OnBGMSliderChanged);
        seSlider.onValueChanged.AddListener(OnSESliderChanged);

        // SoundManagerのイベントでUIを同期
        SoundManager.Instance.OnMasterVolumeChangedEvent += v => masterSlider.value = v;
        SoundManager.Instance.OnBGMVolumeChangedEvent += v => bgmSlider.value = v;
        SoundManager.Instance.OnSEVolumeChangedEvent += v => seSlider.value = v;
    }

    private void OnMasterSliderChanged(float value)
    {
        SoundManager.Instance.masterVolume = value;
        PlayerPrefs.SetFloat("MasterVolume", value);
    }

    private void OnBGMSliderChanged(float value)
    {
        SoundManager.Instance.bgmVolume = value;
        PlayerPrefs.SetFloat("BGMVolume", value);
    }

    private void OnSESliderChanged(float value)
    {
        SoundManager.Instance.seVolume = value;
        PlayerPrefs.SetFloat("SEVolume", value);
    }
}