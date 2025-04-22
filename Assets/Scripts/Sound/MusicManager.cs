using System.Collections;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    private static MusicManager Instance;
    private AudioSource audioSource;

    [Header("Music Clips")]
    public AudioClip backgroundMusic;
    public AudioClip combatMusic;

    [Header("Settings")]
    public float fadeDuration = 1.5f;

    private float initialVolume;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            audioSource = GetComponent<AudioSource>();
            initialVolume = audioSource.volume;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        if (backgroundMusic != null)
        {
            PlayMusic(backgroundMusic);
        }
    }

    public static void SetVolume(float volume)
    {
        Instance.audioSource.volume = volume;
        Instance.initialVolume = volume;
    }

    public static void PlayMusic(AudioClip clip, bool reset = false)
    {
        if (clip != null)
        {
            Instance.audioSource.clip = clip;
        }

        if (Instance.audioSource.clip != null)
        {
            if (reset)
            {
                Instance.audioSource.Stop();
            }
            Instance.audioSource.Play();
        }
    }

    public static void PauseMusic()
    {
        Instance.audioSource.Pause();
    }

    // Call this to smoothly transition to new music
    public static void TransitionToMusic(AudioClip newClip)
    {
        if (Instance.audioSource.clip == newClip) return;
        Instance.StartCoroutine(Instance.FadeToNewMusic(newClip));
    }

    private IEnumerator FadeToNewMusic(AudioClip newClip)
    {
        float startVolume = initialVolume;

        // Fade out
        while (audioSource.volume > 0f)
        {
            audioSource.volume -= startVolume * Time.deltaTime / fadeDuration;
            yield return null;
        }

        audioSource.Stop();
        audioSource.clip = newClip;
        audioSource.Play();

        // Fade in
        while (audioSource.volume < startVolume)
        {
            audioSource.volume += startVolume * Time.deltaTime / fadeDuration;
            yield return null;
        }

        audioSource.volume = startVolume;
    }

    // Example method you could call when player attacks
    public static void OnPlayerAttack()
    {
        if (Instance.combatMusic != null)
        {
            TransitionToMusic(Instance.combatMusic);
        }
    }

    // return to peaceful music
    public static void ReturnToBackground()
    {
        if (Instance.backgroundMusic != null)
        {
            TransitionToMusic(Instance.backgroundMusic);
        }
    }
}
