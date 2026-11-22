//AI Generated Code file with Copilot in VSCode.

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class SoundFXManager : MonoBehaviour
{
    public static SoundFXManager instance;

    [Header("Sound Effect Pool")]
    [SerializeField, Min(1)] private int initialPoolSize = 8;
    [SerializeField] private AudioMixerGroup sfxMixerGroup;
    [SerializeField, Min(0.01f)] private float minDistance = 1f;
    [SerializeField, Min(0.01f)] private float maxDistance = 20f;

    [Header("Music")]
    [SerializeField] private AudioMixerGroup musicMixerGroup;
    [SerializeField] private AudioClip startingMusicClip;
    [SerializeField, Range(0f, 1f)] private float startingMusicVolume = 1f;

    [SerializeField] private AudioMixerGroup voiceoverMixerGroup;

    [Header("Audio Mixer Parameters")]
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private string masterVolumeParameter = "MasterVolume";
    [SerializeField] private string musicVolumeParameter = "MusicVolume";
    [SerializeField] private string sfxVolumeParameter = "SFXVolume";
    [SerializeField] private string voiceoverVolumeParameter = "VoiceoverVolume";

    private readonly List<PooledVoice> voices = new List<PooledVoice>();
    private AudioSource musicSource;

    private sealed class PooledVoice
    {
        public AudioSource source;
        public Transform followTarget;
        public Vector3 localOffset;
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        for (int i = 0; i < Mathf.Max(1, initialPoolSize); i++)
        {
            CreateVoice();
        }

        CreateMusicSource();
        if (startingMusicClip != null)
        {
            PlayMusic(startingMusicClip, startingMusicVolume);
        }

    }

    private void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }

    private void LateUpdate()
    {
        foreach (PooledVoice voice in voices)
        {
            if (voice.source == null || voice.followTarget == null)
            {
                continue;
            }

            voice.source.transform.position =
                voice.followTarget.TransformPoint(voice.localOffset);
        }
    }

    private void OnValidate()
    {
        initialPoolSize = Mathf.Max(1, initialPoolSize);
        minDistance = Mathf.Max(0.01f, minDistance);
        maxDistance = Mathf.Max(minDistance, maxDistance);
    }

    public void PlayAt(AudioClip clip, Vector3 position, float volume = 1f)
    {
        PooledVoice voice = PrepareVoice(clip, volume, true, sfxMixerGroup);
        if (voice == null)
        {
            return;
        }

        voice.followTarget = null;
        voice.source.transform.position = position;
        voice.source.Play();
    }

    public void PlayAttached(
        AudioClip clip,
        Transform target,
        float volume = 1f,
        Vector3 localOffset = default)
    {
        if (target == null)
        {
            Debug.LogError("A target transform is required for an attached sound.", this);
            return;
        }

        PooledVoice voice = PrepareVoice(clip, volume, true, sfxMixerGroup);
        if (voice == null)
        {
            return;
        }

        voice.followTarget = target;
        voice.localOffset = localOffset;
        voice.source.transform.position = target.TransformPoint(localOffset);
        voice.source.Play();
    }

    public void Play2D(AudioClip clip, float volume = 1f)
    {
        PooledVoice voice = PrepareVoice(clip, volume, false, sfxMixerGroup);
        if (voice == null)
        {
            return;
        }

        voice.followTarget = null;
        voice.source.transform.localPosition = Vector3.zero;
        voice.source.Play();
    }

    public void PlayVoiceover2D(AudioClip clip, float volume = 1f)
    {
        PooledVoice voice = PrepareVoice(clip, volume, false, voiceoverMixerGroup);
        if (voice == null)
        {
            return;
        }

        voice.followTarget = null;
        voice.source.transform.localPosition = Vector3.zero;
        voice.source.Play();
    }

    private PooledVoice PrepareVoice(
        AudioClip clip,
        float volume,
        bool spatial,
        AudioMixerGroup mixerGroup)
    {
        if (clip == null)
        {
            Debug.LogError("An audio clip is required to play a sound.", this);
            return null;
        }

        PooledVoice voice = GetAvailableVoice();
        AudioSource source = voice.source;

        voice.followTarget = null;
        source.clip = clip;
        source.volume = Mathf.Clamp01(volume);
        source.spatialBlend = spatial ? 1f : 0f;
        source.outputAudioMixerGroup = mixerGroup;

        return voice;
    }

    private PooledVoice GetAvailableVoice()
    {
        foreach (PooledVoice voice in voices)
        {
            if (!voice.source.isPlaying)
            {
                return voice;
            }
        }

        return CreateVoice();
    }

    private PooledVoice CreateVoice()
    {
        GameObject sourceObject = new GameObject("Pooled SFX Source");
        sourceObject.transform.SetParent(transform, false);

        AudioSource source = sourceObject.AddComponent<AudioSource>();
        source.playOnAwake = false;
        source.loop = false;
        source.rolloffMode = AudioRolloffMode.Logarithmic;
        source.minDistance = minDistance;
        source.maxDistance = maxDistance;
        source.outputAudioMixerGroup = sfxMixerGroup;

        PooledVoice voice = new PooledVoice { source = source };
        voices.Add(voice);
        return voice;
    }

    private void CreateMusicSource()
    {
        if (musicSource != null)
        {
            return;
        }

        GameObject musicObject = new GameObject("Music Source");
        musicObject.transform.SetParent(transform, false);

        musicSource = musicObject.AddComponent<AudioSource>();
        musicSource.playOnAwake = false;
        musicSource.loop = true;
        musicSource.spatialBlend = 0f;
        musicSource.outputAudioMixerGroup = musicMixerGroup;
    }

    public void PlayMusic(AudioClip clip, float volume = 1f)
    {
        if (clip == null)
        {
            Debug.LogError("An audio clip is required to play music.", this);
            return;
        }

        CreateMusicSource();
        musicSource.clip = clip;
        musicSource.volume = Mathf.Clamp01(volume);
        if (!musicSource.isPlaying)
        {
            musicSource.Play();
        }
    }

    public void StopMusic()
    {
        if (musicSource != null)
        {
            musicSource.Stop();
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

    public void SetVoiceoverVolume(float volume)
    {
        SetMixerVolume(voiceoverVolumeParameter, volume);
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
