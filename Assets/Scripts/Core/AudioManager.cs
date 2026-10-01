using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    private static AudioManager instance;
    public static AudioManager Instance
    {
        get
        {
            if (instance == null)
            {
#if UNITY_2023_1_OR_NEWER
                instance = FindAnyObjectByType<AudioManager>();
#else
                instance = FindObjectOfType<AudioManager>();
#endif
                if (instance == null)
                {
                    GameObject go = new GameObject("AudioManager");
                    instance = go.AddComponent<AudioManager>();
                }
            }
            return instance;
        }
        private set => instance = value;
    }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("Default BGM Clips")]
    [SerializeField] private AudioClip menuBgm;
    [SerializeField] private AudioClip gameplayBgm;
    [SerializeField] private AudioClip bossBgm;

    [Header("Settings")]
    [Tooltip("Tự động phát nhạc nền tương ứng khi chuyển Scene")]
    [SerializeField] private bool autoPlaySceneBGM = true;

    private Coroutine fadeMusicCoroutine;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        EnsureAudioSources();
        UpdateVolumes();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        // Kiểm tra và phát nhạc cho scene khởi đầu
        if (autoPlaySceneBGM && musicSource != null && !musicSource.isPlaying)
        {
            PlayBGMForScene(SceneManager.GetActiveScene().name);
        }
    }

    private void EnsureAudioSources()
    {
        if (musicSource == null)
        {
            GameObject musicObj = new GameObject("MusicSource");
            musicObj.transform.SetParent(transform);
            musicSource = musicObj.AddComponent<AudioSource>();
            musicSource.loop = true;
            musicSource.playOnAwake = false;
        }

        if (sfxSource == null)
        {
            GameObject sfxObj = new GameObject("SFXSource");
            sfxObj.transform.SetParent(transform);
            sfxSource = sfxObj.AddComponent<AudioSource>();
            sfxSource.loop = false;
            sfxSource.playOnAwake = false;
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        UpdateVolumes();

        if (autoPlaySceneBGM)
        {
            PlayBGMForScene(scene.name);
        }
    }

    private void PlayBGMForScene(string sceneName)
    {
        if (sceneName == "MainMenu")
        {
            PlayMenuBGM();
        }
        else if (sceneName == "SampleScene")
        {
            PlayGameplayBGM();
        }
    }

    /// <summary>
    /// Đồng bộ âm lượng từ SettingsManager
    /// </summary>
    public void UpdateVolumes()
    {
        SettingsManager.EnsureInitialized();

        AudioListener.volume = SettingsManager.GetMasterVolume();

        if (musicSource != null)
        {
            musicSource.volume = SettingsManager.GetMusicVolume();
        }

        if (sfxSource != null)
        {
            sfxSource.volume = SettingsManager.GetSFXVolume();
        }
    }

    // ========================================================
    // BGM Methods
    // ========================================================

    public void PlayMenuBGM(float fadeDuration = 0.5f)
    {
        if (menuBgm != null) PlayMusic(menuBgm, true, fadeDuration);
    }

    public void PlayGameplayBGM(float fadeDuration = 0.5f)
    {
        if (gameplayBgm != null) PlayMusic(gameplayBgm, true, fadeDuration);
    }

    public void PlayBossBGM(float fadeDuration = 0.5f)
    {
        if (bossBgm != null) PlayMusic(bossBgm, true, fadeDuration);
    }

    /// <summary>
    /// Phát nhạc nền có hiệu ứng mờ dần chuyển bài (Fade transition)
    /// </summary>
    public void PlayMusic(AudioClip clip, bool loop = true, float fadeDuration = 0.5f)
    {
        if (clip == null) return;
        EnsureAudioSources();

        // Nếu bài nhạc đang phát chính là clip này và vẫn đang chạy thì bỏ qua
        if (musicSource.clip == clip && musicSource.isPlaying) return;

        if (fadeMusicCoroutine != null)
        {
            StopCoroutine(fadeMusicCoroutine);
        }

        fadeMusicCoroutine = StartCoroutine(FadeMusicRoutine(clip, loop, fadeDuration));
    }

    public void StopMusic(float fadeDuration = 0.5f)
    {
        if (musicSource == null || !musicSource.isPlaying) return;

        if (fadeMusicCoroutine != null)
        {
            StopCoroutine(fadeMusicCoroutine);
        }

        fadeMusicCoroutine = StartCoroutine(FadeMusicRoutine(null, false, fadeDuration));
    }

    private IEnumerator FadeMusicRoutine(AudioClip newClip, bool loop, float duration)
    {
        float targetVolume = SettingsManager.GetMusicVolume();

        // 1. Fade out bản nhạc hiện tại
        if (musicSource.isPlaying && duration > 0f)
        {
            float startVol = musicSource.volume;
            float t = 0f;
            while (t < duration)
            {
                t += Time.unscaledDeltaTime;
                musicSource.volume = Mathf.Lerp(startVol, 0f, t / duration);
                yield return null;
            }
            musicSource.Stop();
        }

        if (newClip == null)
        {
            musicSource.volume = targetVolume;
            yield break;
        }

        // 2. Chuyển sang clip mới
        musicSource.clip = newClip;
        musicSource.loop = loop;
        musicSource.volume = 0f;
        musicSource.Play();

        // 3. Fade in bản nhạc mới
        if (duration > 0f)
        {
            float t = 0f;
            while (t < duration)
            {
                t += Time.unscaledDeltaTime;
                musicSource.volume = Mathf.Lerp(0f, targetVolume, t / duration);
                yield return null;
            }
        }

        musicSource.volume = targetVolume;
        fadeMusicCoroutine = null;
    }

    // ========================================================
    // SFX Methods
    // ========================================================

    /// <summary>
    /// Phát âm thanh hiệu ứng (SFX) 2D
    /// </summary>
    public void PlaySFX(AudioClip clip, float volumeScale = 1f)
    {
        if (clip == null) return;
        EnsureAudioSources();

        sfxSource.volume = SettingsManager.GetSFXVolume();
        sfxSource.PlayOneShot(clip, Mathf.Clamp01(volumeScale));
    }

    /// <summary>
    /// Phát âm thanh tại một vị trí trong không gian 3D/2D
    /// </summary>
    public void PlaySFXAtPosition(AudioClip clip, Vector3 position, float volumeScale = 1f)
    {
        if (clip == null) return;
        float finalVolume = SettingsManager.GetSFXVolume() * Mathf.Clamp01(volumeScale);
        AudioSource.PlayClipAtPoint(clip, position, finalVolume);
    }
}
