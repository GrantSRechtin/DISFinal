using UnityEngine;
using UnityEngine.Audio;

public class SoundFXManager : MonoBehaviour
{
    public static SoundFXManager instance;

    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private string masterVolumeParameter = "MasterVolume";
    [SerializeField] private string musicVolumeParameter = "MusicVolume";
    [SerializeField] private string sfxVolumeParameter = "SFXVolume";

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    public void SetMasterVolume(float volume)
    {
        SetMixerVolume(masterVolumeParameter, volume);
    }

    public void SetMusicVolume(float volume)
    {
        SetMixerVolume(musicVolumeParameter, volume);
    }

    public void SetSFXVolume(float volume)
    {
        SetMixerVolume(sfxVolumeParameter, volume);
    }

    private void SetMixerVolume(string parameter, float volume)
    {
        if (audioMixer == null)
        {
            Debug.LogError("Audio mixer is not assigned.", this);
            return;
        }

        float volumeDb = Mathf.Log10(Mathf.Clamp(volume, 0.0001f, 1f)) * 20f;
        if (!audioMixer.SetFloat(parameter, volumeDb))
        {
            Debug.LogError($"Audio mixer parameter '{parameter}' is not exposed.", this);
        }
    }
}
