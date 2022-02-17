using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    [SerializeField] List<AudioClip> sfxClips = new List<AudioClip>();

    private AudioSource musicSource;

    private Dictionary<string, AudioSource> audioSources = new Dictionary<string, AudioSource>();

    private static SoundManager instance;
    public static SoundManager Instance => instance;

    private void Awake()
    {
        if (instance != null && instance != this)
            Destroy(gameObject);

        instance = this;
    }

    private void Start()
    {
        musicSource = GetComponent<AudioSource>();

        musicSource.Play();

        AudioSource audioSource;

        foreach (AudioClip clip in sfxClips)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.clip = clip;

            audioSources.Add(clip.name, audioSource);
        }
    }

    public void PlaySFX(string clipName, bool isLooping = false)
    {
        audioSources[clipName].loop = isLooping;
        audioSources[clipName].Play();
    }

    public void PlayMultipleSFX(string clipName, bool isLooping = false)
    {
        audioSources[clipName].loop = isLooping;
        audioSources[clipName].PlayOneShot(audioSources[clipName].clip);
    }

    private void OnDestroy()
    {
        instance = null;
    }
}
