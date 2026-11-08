using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PauseMenuSettings : MonoBehaviour
{
    [Header("Settings Sliders")]
    [SerializeField] private Slider sensitivitySlider;
    [SerializeField] private Slider volumeSlider;

    private PlayerCamera playerCamera;

    private const string SensitivityKey = "MouseSensitivity";
    private const string VolumeKey = "AudioVolume";

    private const float DefaultSensitivity = 60f;
    private const float DefaultVolume = 1f;

    private void Awake()
    {
        FindCameraController();

        // Restore previously saved settings.
        float sensitivity =
            PlayerPrefs.GetFloat(SensitivityKey, DefaultSensitivity);
        float volume =
            PlayerPrefs.GetFloat(VolumeKey, DefaultVolume);

        // Update slider positions without triggering their callbacks.
        sensitivitySlider.SetValueWithoutNotify(sensitivity);
        volumeSlider.SetValueWithoutNotify(volume);

        ApplySensitivity(sensitivity);
        ApplyVolume(volume);
    }

    private void OnEnable()
    {
        sensitivitySlider.onValueChanged.AddListener(ApplySensitivity);
        volumeSlider.onValueChanged.AddListener(ApplyVolume);

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        sensitivitySlider.onValueChanged.RemoveListener(ApplySensitivity);
        volumeSlider.onValueChanged.RemoveListener(ApplyVolume);

        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void FindCameraController()
    {
        // cameraController = FindFirstObjectByType<CameraController>();
        playerCamera = FindFirstObjectByType<PlayerCamera>();
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

    private void ApplyVolume(float value)
    {
        AudioListener.volume = value;

        PlayerPrefs.SetFloat(VolumeKey, value);
        PlayerPrefs.Save();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        FindCameraController();

        float sensitivity =
            PlayerPrefs.GetFloat(SensitivityKey, DefaultSensitivity);
        float volume =
            PlayerPrefs.GetFloat(VolumeKey, DefaultVolume);

        if (playerCamera != null)
            playerCamera.SetSensitivity(sensitivity);

        AudioListener.volume = volume;
    }
}