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

    [Header("SFX Clips - Player")]
    [SerializeField] private AudioClip playerShootSFX;
    [SerializeField] private AudioClip megaBeamSFX;
    [SerializeField] private AudioClip megaBeamReadySFX;
    [SerializeField] private AudioClip playerDeadSFX;

    [Header("SFX Clips - Enemy & Boss")]
    [SerializeField] private AudioClip enemyShootSFX;
    [SerializeField] private AudioClip enemyHitSFX;
    [SerializeField] private AudioClip enemyDeadSFX;
    [SerializeField] private AudioClip bossShootSFX;
    [SerializeField] private AudioClip bossExplosionSFX;

    [Header("SFX Clips - Meteor & Pickups")]
    [SerializeField] private AudioClip meteorExplosionSFX;
    [SerializeField] private AudioClip energyCollectSFX;
    [SerializeField] private AudioClip lifePickupSFX;
    [SerializeField] private AudioClip bulletUpgradeSFX;

    [Header("SFX Clips - UI")]
    [SerializeField] private AudioClip uiClickSFX;
    [SerializeField] private AudioClip gameOverSFX;
    [SerializeField] private AudioClip gameWinSFX;

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
    public void PlaySFX(AudioClip clip, float volumeScale = 1f, bool randomizePitch = false)
    {
        if (clip == null) return;
        EnsureAudioSources();

        sfxSource.volume = SettingsManager.GetSFXVolume();
        sfxSource.pitch = randomizePitch ? Random.Range(0.92f, 1.08f) : 1f;
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

    // --- Quick Helpers: Player ---
    public void PlayPlayerShoot() => PlaySFX(playerShootSFX, 0.8f, true);
    public void PlayMegaBeam() => PlaySFX(megaBeamSFX, 1.0f);
    public void PlayMegaBeamReady() => PlaySFX(megaBeamReadySFX, 1.0f);
    public void PlayPlayerDead() => PlaySFX(playerDeadSFX, 1.0f);

    // --- Quick Helpers: Enemy & Boss ---
    public void PlayEnemyShoot() => PlaySFX(enemyShootSFX, 0.7f, true);
    public void PlayEnemyHit() => PlaySFX(enemyHitSFX, 0.7f, true);
    public void PlayEnemyDead() => PlaySFX(enemyDeadSFX, 0.9f, true);
    public void PlayBossShoot() => PlaySFX(bossShootSFX, 0.85f, true);
    public void PlayBossExplosion() => PlaySFX(bossExplosionSFX, 1.0f);

    // --- Quick Helpers: Meteor & Pickups ---
    public void PlayMeteorExplosion() => PlaySFX(meteorExplosionSFX, 0.9f, true);
    public void PlayEnergyCollect() => PlaySFX(energyCollectSFX, 0.8f, true);
    public void PlayLifePickup() => PlaySFX(lifePickupSFX, 1.0f);
    public void PlayBulletUpgrade() => PlaySFX(bulletUpgradeSFX, 1.0f);

    // --- Quick Helpers: UI ---
    public void PlayUIClick() => PlaySFX(uiClickSFX, 0.9f);
    public void PlayGameOver()
    {
        StopMusic(0.2f);
        PlaySFX(gameOverSFX, 1.0f);
    }
    public void PlayGameWin()
    {
        StopMusic(0.2f);
        PlaySFX(gameWinSFX, 1.0f);
    }

#if UNITY_EDITOR
    [ContextMenu("Auto Assign All Audio Clips")]
    public void AutoAssignClips()
    {
        menuBgm = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/BGM/menu_bgm.wav");
        gameplayBgm = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/BGM/gameplay_bgm.wav");
        bossBgm = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/BGM/boss_bgm.wav");

        playerShootSFX = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/SFX/Player/player_shoot2.wav");
        megaBeamSFX = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/SFX/Player/mega_beam.wav");
        megaBeamReadySFX = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/SFX/Player/megabeam_ready.wav");
        playerDeadSFX = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/SFX/Player/player_dead.wav");

        enemyShootSFX = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/SFX/Enemy/enemy_shoot.wav");
        enemyHitSFX = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/SFX/Enemy/enemy_hit.wav");
        enemyDeadSFX = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/SFX/Enemy/enemy_dead.wav");
        bossShootSFX = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/SFX/Boss/boss_shoot.wav");
        bossExplosionSFX = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/SFX/Boss/boss_explosion.wav");

        meteorExplosionSFX = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/SFX/Mateor/explosion.wav");
        energyCollectSFX = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/SFX/Pickup/energy_collect.wav");
        lifePickupSFX = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/SFX/Pickup/life_pickup.wav");
        bulletUpgradeSFX = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/SFX/Pickup/bullet_upgrade1.wav");

        uiClickSFX = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/SFX/UI/ui_click.wav");
        gameOverSFX = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/SFX/UI/game_over.wav");
        gameWinSFX = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/SFX/UI/game_win.wav");

        UnityEditor.EditorUtility.SetDirty(this);
        Debug.Log("[AudioManager] Đã tự động gán toàn bộ file âm thanh thành công!");
    }

    private void Reset()
    {
        AutoAssignClips();
    }
#endif
}
