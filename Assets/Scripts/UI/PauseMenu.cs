using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Controls the in-game Pause Menu overlay.
/// Handles ESC toggle, Time.timeScale, and scene navigation.
/// Attach this MonoBehaviour to the PausePanel GameObject inside the gameplay Canvas.
/// </summary>
public class PauseMenu : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button mainMenuButton;

    [Header("Settings")]
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private SettingsUI settingsUI;

    private bool isPaused;
    private bool isSettingsOpen;

    private void Awake()
    {
        // Auto-fetch SettingsUI if not assigned in Inspector
        if (settingsUI == null && settingsPanel != null)
        {
            settingsUI = settingsPanel.GetComponent<SettingsUI>();
            if (settingsUI == null)
                settingsUI = settingsPanel.GetComponentInChildren<SettingsUI>(true);
        }

        // Wire up button listeners
        if (resumeButton != null)
            resumeButton.onClick.AddListener(ResumeGame);

        if (settingsButton != null)
            settingsButton.onClick.AddListener(OpenSettings);

        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(GoToMainMenu);
    }

    private void Start()
    {
        // Ensure the game always starts unpaused
        isPaused = false;
        isSettingsOpen = false;

        if (pausePanel != null)
            pausePanel.SetActive(false);

        if (settingsPanel != null)
            settingsPanel.SetActive(false);

        Time.timeScale = 1f;
    }

    private void Update()
    {
        // Detect ESC key press (works even when Time.timeScale = 0
        // because Update still runs; only Time.deltaTime becomes 0)
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isSettingsOpen)
            {
                CloseSettings();
            }
            else if (isPaused)
            {
                ResumeGame();
            }
            else
            {
                OpenPause();
            }
        }
    }

    /// <summary>
    /// Opens the pause menu and freezes gameplay.
    /// </summary>
    public void OpenPause()
    {
        isPaused = true;

        if (pausePanel != null)
            pausePanel.SetActive(true);

        Time.timeScale = 0f;

        // Show and unlock cursor so the player can click UI buttons
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    /// <summary>
    /// Closes the pause menu and resumes gameplay.
    /// </summary>
    public void ResumeGame()
    {
        isPaused = false;

        if (pausePanel != null)
            pausePanel.SetActive(false);

        Time.timeScale = 1f;

        // Re-hide and confine cursor for gameplay (matches PlayerMovement.Awake behavior)
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Confined;
    }

    /// <summary>
    /// Opens the Settings panel from the Pause Menu.
    /// Hides PausePanel but keeps Time.timeScale at 0 (game stays paused).
    /// Registers a close callback so pressing Back returns to PausePanel.
    /// </summary>
    public void OpenSettings()
    {
        if (settingsPanel == null)
        {
            Debug.LogWarning("[PauseMenu] Settings panel reference is not assigned.");
            return;
        }

        if (settingsUI == null)
        {
            settingsUI = settingsPanel.GetComponent<SettingsUI>();
            if (settingsUI == null)
                settingsUI = settingsPanel.GetComponentInChildren<SettingsUI>(true);
        }

        isSettingsOpen = true;

        // Hide the Pause Panel while Settings is open
        if (pausePanel != null)
            pausePanel.SetActive(false);

        // Register callback so Back returns to PausePanel
        if (settingsUI != null)
            settingsUI.SetOnCloseCallback(OnSettingsClosed);

        settingsPanel.SetActive(true);

        // Time.timeScale remains 0 — game stays paused
    }

    /// <summary>
    /// Closes the Settings panel and returns to the Pause Menu.
    /// Called by ESC or by the SettingsUI close callback.
    /// </summary>
    private void CloseSettings()
    {
        isSettingsOpen = false;

        if (settingsPanel != null)
            settingsPanel.SetActive(false);

        // Re-show the Pause Panel
        if (pausePanel != null)
            pausePanel.SetActive(true);

        // Time.timeScale remains 0 — game stays paused
    }

    /// <summary>
    /// Callback invoked by SettingsUI when the Back button is pressed.
    /// </summary>
    private void OnSettingsClosed()
    {
        CloseSettings();
    }

    /// <summary>
    /// Returns to the Main Menu scene.
    /// Restores Time.timeScale before leaving so the Main Menu does not inherit a paused state.
    /// Uses the existing Loader system to transition through the LoadingScene.
    /// </summary>
    public void GoToMainMenu()
    {
        isPaused = false;
        Time.timeScale = 1f;

        // Restore cursor for menu navigation
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        // Use the existing Loader to go through the loading screen
        Loader.Load("MainMenu");
    }

    /// <summary>
    /// Returns whether the game is currently paused.
    /// Other scripts can query this if needed.
    /// </summary>
    public bool IsPaused => isPaused;
}
