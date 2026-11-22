//AI Generated Code file with Copilot in VSCode.

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PauseMenuSettings : MonoBehaviour
{
    [Header("Settings Sliders")]
    [SerializeField] private Slider sensitivitySlider;
    [SerializeField] private Slider masterVolumeSlider;
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private Slider sfxVolumeSlider;
    [SerializeField] private Slider voiceoverVolumeSlider;

    private PlayerCamera playerCamera;

    private const string SensitivityKey = "MouseSensitivity";
    private const string MasterVolumeKey = "MasterVolume";
    private const string MusicVolumeKey = "MusicVolume";
    private const string SFXVolumeKey = "SFXVolume";
    private const string VoiceoverVolumeKey = "VoiceoverVolume";

    private const float DefaultSensitivity = 20f;
    private const float DefaultMasterVolume = 1f;
    private const float DefaultMusicVolume = 1f;
    private const float DefaultSFXVolume = 1f;
    private const float DefaultVoiceoverVolume = 1f;

    private void Awake()
    {
        FindCameraController();
        ApplySavedSettings();
    }

    private void Start()
    {
        ApplySavedSettings();
    }

    private void OnEnable()
    {
        sensitivitySlider.onValueChanged.AddListener(ApplySensitivity);
        masterVolumeSlider.onValueChanged.AddListener(ApplyMasterVolume);
        musicVolumeSlider.onValueChanged.AddListener(ApplyMusicVolume);
        sfxVolumeSlider.onValueChanged.AddListener(ApplySFXVolume);
        voiceoverVolumeSlider.onValueChanged.AddListener(ApplyVoiceoverVolume);

        SceneManager.sceneLoaded += OnSceneLoaded;
        ApplySavedSettings();
    }

    private void OnDisable()
    {
        sensitivitySlider.onValueChanged.RemoveListener(ApplySensitivity);
        masterVolumeSlider.onValueChanged.RemoveListener(ApplyMasterVolume);
        musicVolumeSlider.onValueChanged.RemoveListener(ApplyMusicVolume);
        sfxVolumeSlider.onValueChanged.RemoveListener(ApplySFXVolume);
        voiceoverVolumeSlider.onValueChanged.RemoveListener(ApplyVoiceoverVolume);

        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void FindCameraController()
    {
        // cameraController = FindFirstObjectByType<CameraController>();
        playerCamera = FindFirstObjectByType<PlayerCamera>();
    }

    private void ApplySavedSettings()
    {
        float sensitivity =
            PlayerPrefs.GetFloat(SensitivityKey, DefaultSensitivity);
        float masterVolume =
            PlayerPrefs.GetFloat(MasterVolumeKey, DefaultMasterVolume);
        float musicVolume =
            PlayerPrefs.GetFloat(MusicVolumeKey, DefaultMusicVolume);
        float sfxVolume =
            PlayerPrefs.GetFloat(SFXVolumeKey, DefaultSFXVolume);
        float voiceoverVolume =
            PlayerPrefs.GetFloat(VoiceoverVolumeKey, DefaultVoiceoverVolume);

        if (sensitivitySlider != null)
            sensitivitySlider.SetValueWithoutNotify(sensitivity);
        if (masterVolumeSlider != null)
            masterVolumeSlider.SetValueWithoutNotify(masterVolume);
        if (musicVolumeSlider != null)
            musicVolumeSlider.SetValueWithoutNotify(musicVolume);
        if (sfxVolumeSlider != null)
            sfxVolumeSlider.SetValueWithoutNotify(sfxVolume);
        if (voiceoverVolumeSlider != null)
            voiceoverVolumeSlider.SetValueWithoutNotify(voiceoverVolume);

        ApplySensitivity(sensitivity);
        ApplyMasterVolume(masterVolume);
        ApplyMusicVolume(musicVolume);
        ApplySFXVolume(sfxVolume);
        ApplyVoiceoverVolume(voiceoverVolume);
    }

    private void ApplySensitivity(float value)
    {
        if (playerCamera == null)
            FindCameraController();

        if (playerCamera != null)
            playerCamera.SetSensitivity(value);

        PlayerPrefs.SetFloat(SensitivityKey, value);
        PlayerPrefs.Save();
    }

    private void ApplyMasterVolume(float value)
    {
        if (SoundFXManager.instance != null)
            SoundFXManager.instance.SetMasterVolume(value);

        PlayerPrefs.SetFloat(MasterVolumeKey, value);
        PlayerPrefs.Save();
    }

    private void ApplyMusicVolume(float value)
    {
        if (SoundFXManager.instance != null)
            SoundFXManager.instance.SetMusicVolume(value);

        PlayerPrefs.SetFloat(MusicVolumeKey, value);
        PlayerPrefs.Save();
    }

    private void ApplySFXVolume(float value)
    {
        if (SoundFXManager.instance != null)
            SoundFXManager.instance.SetSFXVolume(value);

        PlayerPrefs.SetFloat(SFXVolumeKey, value);
        PlayerPrefs.Save();
    }

    private void ApplyVoiceoverVolume(float value)
    {
        if (SoundFXManager.instance != null)
            SoundFXManager.instance.SetVoiceoverVolume(value);

        PlayerPrefs.SetFloat(VoiceoverVolumeKey, value);
        PlayerPrefs.Save();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        FindCameraController();
        ApplySavedSettings();
    }
}