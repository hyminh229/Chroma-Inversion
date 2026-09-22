using UnityEngine;

/// <summary>
/// Static utility managing persistent user settings via PlayerPrefs.
/// Completely independent from the gameplay Save/Load system (SaveSystem + GameSaveData).
/// Mirrors the static utility pattern used by SaveSystem.
/// </summary>
public static class SettingsManager
{
    // --- PlayerPrefs Keys ---
    private const string KEY_MASTER_VOLUME = "Settings_MasterVolume";
    private const string KEY_MUSIC_VOLUME  = "Settings_MusicVolume";
    private const string KEY_SFX_VOLUME    = "Settings_SFXVolume";
    private const string KEY_FULLSCREEN    = "Settings_Fullscreen";

    // --- Defaults ---
    private const float DEFAULT_MASTER_VOLUME = 1.0f;
    private const float DEFAULT_MUSIC_VOLUME  = 0.8f;
    private const float DEFAULT_SFX_VOLUME    = 1.0f;
    private const bool  DEFAULT_FULLSCREEN    = true;

    // --- Runtime Cache ---
    private static float masterVolume;
    private static float musicVolume;
    private static float sfxVolume;
    private static bool  fullscreen;

    private static bool initialized = false;

    // ========================================================
    // Initialization
    // ========================================================

    /// <summary>
    /// Ensures settings are loaded from PlayerPrefs on first access.
    /// Called automatically by getters; safe to call multiple times.
    /// </summary>
    public static void EnsureInitialized()
    {
        if (!initialized)
        {
            LoadSettings();
        }
    }

    // ========================================================
    // Getters
    // ========================================================

    public static float GetMasterVolume()
    {
        EnsureInitialized();
        return masterVolume;
    }

    public static float GetMusicVolume()
    {
        EnsureInitialized();
        return musicVolume;
    }

    public static float GetSFXVolume()
    {
        EnsureInitialized();
        return sfxVolume;
    }

    public static bool GetFullscreen()
    {
        EnsureInitialized();
        return fullscreen;
    }

    // ========================================================
    // Setters (update runtime cache only — call ApplySettings + SaveSettings separately)
    // ========================================================

    public static void SetMasterVolume(float value)
    {
        EnsureInitialized();
        masterVolume = Mathf.Clamp01(value);
    }

    public static void SetMusicVolume(float value)
    {
        EnsureInitialized();
        musicVolume = Mathf.Clamp01(value);
    }

    public static void SetSFXVolume(float value)
    {
        EnsureInitialized();
        sfxVolume = Mathf.Clamp01(value);
    }

    public static void SetFullscreen(bool value)
    {
        EnsureInitialized();
        fullscreen = value;
    }

    // ========================================================
    // Load / Save / Apply
    // ========================================================

    /// <summary>
    /// Loads all settings from PlayerPrefs into runtime cache.
    /// Uses default values if keys do not exist (first launch).
    /// </summary>
    public static void LoadSettings()
    {
        masterVolume = PlayerPrefs.GetFloat(KEY_MASTER_VOLUME, DEFAULT_MASTER_VOLUME);
        musicVolume  = PlayerPrefs.GetFloat(KEY_MUSIC_VOLUME, DEFAULT_MUSIC_VOLUME);
        sfxVolume    = PlayerPrefs.GetFloat(KEY_SFX_VOLUME, DEFAULT_SFX_VOLUME);
        fullscreen   = PlayerPrefs.GetInt(KEY_FULLSCREEN, DEFAULT_FULLSCREEN ? 1 : 0) == 1;

        initialized = true;

        Debug.Log($"[SettingsManager] Settings loaded — Master:{masterVolume:F2} Music:{musicVolume:F2} SFX:{sfxVolume:F2} Fullscreen:{fullscreen}");
    }

    /// <summary>
    /// Persists current runtime settings to PlayerPrefs.
    /// </summary>
    public static void SaveSettings()
    {
        EnsureInitialized();

        PlayerPrefs.SetFloat(KEY_MASTER_VOLUME, masterVolume);
        PlayerPrefs.SetFloat(KEY_MUSIC_VOLUME, musicVolume);
        PlayerPrefs.SetFloat(KEY_SFX_VOLUME, sfxVolume);
        PlayerPrefs.SetInt(KEY_FULLSCREEN, fullscreen ? 1 : 0);
        PlayerPrefs.Save();

        Debug.Log("[SettingsManager] Settings saved to PlayerPrefs.");
    }

    /// <summary>
    /// Applies all current settings to the engine immediately.
    /// 
    /// Audio:
    ///   - Master Volume → AudioListener.volume (global audio level)
    ///   - Music/SFX Volume → stored and ready for future AudioSource integration
    ///     (no AudioSources exist in the project yet)
    /// 
    /// Display:
    ///   - Fullscreen → Screen.fullScreen
    /// </summary>
    public static void ApplySettings()
    {
        EnsureInitialized();

        // Master volume controls the global AudioListener
        AudioListener.volume = masterVolume;

        // Music and SFX volumes are stored in the runtime cache.
        // When AudioSources are added to the project, they can query
        // SettingsManager.GetMusicVolume() / GetSFXVolume() to set their volume.
        // No AudioSources exist yet, so no additional application is needed.

        // Fullscreen
        Screen.fullScreen = fullscreen;
    }
}
