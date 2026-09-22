using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// UI controller for the reusable SettingsPanel prefab.
/// Connects sliders/toggles to SettingsManager and handles the Back button.
/// Attach this to the root SettingsPanel GameObject.
/// </summary>
public class SettingsUI : MonoBehaviour
{
    [Header("Audio Sliders")]
    [SerializeField] private Slider masterVolumeSlider;
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private Slider sfxVolumeSlider;

    [Header("Display")]
    [SerializeField] private Toggle fullscreenToggle;

    [Header("Buttons")]
    [SerializeField] private Button backButton;

    /// <summary>
    /// Optional callback invoked when the Settings panel closes.
    /// The caller (PauseMenu, MainMenuUI, etc.) can register a callback
    /// to react when the user presses Back.
    /// </summary>
    private System.Action onCloseCallback;

    private void Awake()
    {
        // Wire up Back button
        if (backButton != null)
            backButton.onClick.AddListener(OnBackPressed);
    }

    private void OnEnable()
    {
        // Load current values from SettingsManager into UI controls every time the panel opens
        SettingsManager.EnsureInitialized();
        SettingsManager.ApplySettings();

        LoadValuesIntoUI();
        SubscribeListeners();
    }

    private void OnDisable()
    {
        UnsubscribeListeners();
    }

    /// <summary>
    /// Sets a callback that will be invoked when the Back button is pressed.
    /// Call this before activating the SettingsPanel.
    /// </summary>
    public void SetOnCloseCallback(System.Action callback)
    {
        onCloseCallback = callback;
    }

    // ========================================================
    // UI ↔ SettingsManager Binding
    // ========================================================

    private void LoadValuesIntoUI()
    {
        if (masterVolumeSlider != null)
        {
            masterVolumeSlider.minValue = 0f;
            masterVolumeSlider.maxValue = 1f;
            masterVolumeSlider.value = SettingsManager.GetMasterVolume();
        }

        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.minValue = 0f;
            musicVolumeSlider.maxValue = 1f;
            musicVolumeSlider.value = SettingsManager.GetMusicVolume();
        }

        if (sfxVolumeSlider != null)
        {
            sfxVolumeSlider.minValue = 0f;
            sfxVolumeSlider.maxValue = 1f;
            sfxVolumeSlider.value = SettingsManager.GetSFXVolume();
        }

        if (fullscreenToggle != null)
        {
            fullscreenToggle.isOn = SettingsManager.GetFullscreen();
        }
    }

    private void SubscribeListeners()
    {
        if (masterVolumeSlider != null)
            masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);

        if (musicVolumeSlider != null)
            musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);

        if (sfxVolumeSlider != null)
            sfxVolumeSlider.onValueChanged.AddListener(OnSFXVolumeChanged);

        if (fullscreenToggle != null)
            fullscreenToggle.onValueChanged.AddListener(OnFullscreenChanged);
    }

    private void UnsubscribeListeners()
    {
        if (masterVolumeSlider != null)
            masterVolumeSlider.onValueChanged.RemoveListener(OnMasterVolumeChanged);

        if (musicVolumeSlider != null)
            musicVolumeSlider.onValueChanged.RemoveListener(OnMusicVolumeChanged);

        if (sfxVolumeSlider != null)
            sfxVolumeSlider.onValueChanged.RemoveListener(OnSFXVolumeChanged);

        if (fullscreenToggle != null)
            fullscreenToggle.onValueChanged.RemoveListener(OnFullscreenChanged);
    }

    // ========================================================
    // Slider / Toggle Callbacks
    // ========================================================

    private void OnMasterVolumeChanged(float value)
    {
        SettingsManager.SetMasterVolume(value);
        SettingsManager.ApplySettings();
        SettingsManager.SaveSettings();
    }

    private void OnMusicVolumeChanged(float value)
    {
        SettingsManager.SetMusicVolume(value);
        SettingsManager.ApplySettings();
        SettingsManager.SaveSettings();
    }

    private void OnSFXVolumeChanged(float value)
    {
        SettingsManager.SetSFXVolume(value);
        SettingsManager.ApplySettings();
        SettingsManager.SaveSettings();
    }

    private void OnFullscreenChanged(bool value)
    {
        SettingsManager.SetFullscreen(value);
        SettingsManager.ApplySettings();
        SettingsManager.SaveSettings();
    }

    // ========================================================
    // Back Button
    // ========================================================

    private void OnBackPressed()
    {
        SettingsManager.SaveSettings();

        // Deactivate self
        gameObject.SetActive(false);

        // Notify the caller (if any)
        onCloseCallback?.Invoke();
        onCloseCallback = null;
    }
}
