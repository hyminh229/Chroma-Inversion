using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Manages the Game End (Win / Lose) UI screen.
/// Listens to gameplay events from PlayerLife and WaveSequencer to present
/// results without directly coupling gameplay logic to UI.
/// UI elements and button bindings are manually configured in the Inspector.
/// </summary>
public class GameEndUI : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject winPanel;
    [SerializeField] private GameObject losePanel;

    [Header("Win UI Elements")]
    [SerializeField] private TMP_Text winScoreText;
    [SerializeField] private TMP_Text winBulletLevelText;
    [SerializeField] private TMP_Text winLifeText;

    [Header("Lose UI Elements")]
    [SerializeField] private TMP_Text loseScoreText;
    [SerializeField] private TMP_Text loseWaveText;
    [SerializeField] private TMP_Text loseBulletLevelText;

    [Header("Gameplay References (Optional - auto-found if null)")]
    [SerializeField] private PlayerLife playerLife;
    [SerializeField] private PlayerShooting playerShooting;
    [SerializeField] private WaveSequencer waveSequencer;

    [Header("Text Format Prefixes")]
    [SerializeField] private string scorePrefix = "Score: ";
    [SerializeField] private string bulletLevelPrefix = "Bullet Level: ";
    [SerializeField] private string lifePrefix = "Lives: ";
    [SerializeField] private string wavePrefix = "Wave: ";

    private bool gameEnded;
    private bool isSubscribed;

    /// <summary>
    /// Global flag indicating whether the game has concluded (Win or Lose).
    /// Used by PauseMenu to disallow opening the Pause menu after game over or victory.
    /// </summary>
    public static bool IsGameEnded { get; private set; }

    private void Awake()
    {
        gameEnded = false;
        IsGameEnded = false;

        if (winPanel != null) winPanel.SetActive(false);
        if (losePanel != null) losePanel.SetActive(false);

        FindReferences();

        Button[] buttons = GetComponentsInChildren<Button>(true);
        foreach (Button btn in buttons)
        {
            btn.onClick.AddListener(() => AudioManager.Instance?.PlayUIClick());
        }
    }

    private void OnEnable()
    {
        FindReferences();
        SubscribeEvents();
    }

    private void Start()
    {
        FindReferences();
        SubscribeEvents();
    }

    private void OnDisable()
    {
        UnsubscribeEvents();
    }

    private void OnDestroy()
    {
        UnsubscribeEvents();
        IsGameEnded = false;
    }

    private void FindReferences()
    {
        // Auto-find panels if unassigned
        if (winPanel == null || losePanel == null)
        {
            Transform[] allChildren = GetComponentsInChildren<Transform>(true);
            foreach (Transform t in allChildren)
            {
                if (winPanel == null && t.gameObject.name.IndexOf("WinPanel", System.StringComparison.OrdinalIgnoreCase) >= 0)
                    winPanel = t.gameObject;
                if (losePanel == null && t.gameObject.name.IndexOf("LosePanel", System.StringComparison.OrdinalIgnoreCase) >= 0)
                    losePanel = t.gameObject;
            }
        }

        // Auto-find Win texts if unassigned
        if (winPanel != null)
        {
            TMP_Text[] winTexts = winPanel.GetComponentsInChildren<TMP_Text>(true);
            foreach (TMP_Text txt in winTexts)
            {
                string n = txt.gameObject.name;
                if (winScoreText == null && n.IndexOf("Score", System.StringComparison.OrdinalIgnoreCase) >= 0)
                    winScoreText = txt;
                else if (winBulletLevelText == null && (n.IndexOf("Bullet", System.StringComparison.OrdinalIgnoreCase) >= 0 || n.IndexOf("Shot", System.StringComparison.OrdinalIgnoreCase) >= 0))
                    winBulletLevelText = txt;
                else if (winLifeText == null && (n.IndexOf("Life", System.StringComparison.OrdinalIgnoreCase) >= 0 || n.IndexOf("Lives", System.StringComparison.OrdinalIgnoreCase) >= 0))
                    winLifeText = txt;
            }
        }

        // Auto-find Lose texts if unassigned
        if (losePanel != null)
        {
            TMP_Text[] loseTexts = losePanel.GetComponentsInChildren<TMP_Text>(true);
            foreach (TMP_Text txt in loseTexts)
            {
                string n = txt.gameObject.name;
                if (loseScoreText == null && n.IndexOf("Score", System.StringComparison.OrdinalIgnoreCase) >= 0)
                    loseScoreText = txt;
                else if (loseWaveText == null && n.IndexOf("Wave", System.StringComparison.OrdinalIgnoreCase) >= 0)
                    loseWaveText = txt;
                else if (loseBulletLevelText == null && (n.IndexOf("Bullet", System.StringComparison.OrdinalIgnoreCase) >= 0 || n.IndexOf("Shot", System.StringComparison.OrdinalIgnoreCase) >= 0))
                    loseBulletLevelText = txt;
            }
        }

        if (playerLife == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                playerLife = playerObj.GetComponent<PlayerLife>();
                if (playerShooting == null)
                {
                    playerShooting = playerObj.GetComponent<PlayerShooting>();
                }
            }
            else
            {
                playerLife = FindAnyObjectByType<PlayerLife>();
            }
        }

        if (playerShooting == null && playerLife != null)
        {
            playerShooting = playerLife.GetComponent<PlayerShooting>();
        }

        if (waveSequencer == null)
        {
            waveSequencer = FindAnyObjectByType<WaveSequencer>();
        }
    }

    private void SubscribeEvents()
    {
        if (isSubscribed) return;

        if (playerLife != null)
        {
            playerLife.OnGameOver += ShowLose;
        }

        if (waveSequencer != null)
        {
            waveSequencer.OnAllWavesCompleted += ShowWin;
        }

        if (playerLife != null && waveSequencer != null)
        {
            isSubscribed = true;
        }
    }

    private void UnsubscribeEvents()
    {
        if (!isSubscribed) return;

        if (playerLife != null)
        {
            playerLife.OnGameOver -= ShowLose;
        }

        if (waveSequencer != null)
        {
            waveSequencer.OnAllWavesCompleted -= ShowWin;
        }

        isSubscribed = false;
    }

    /// <summary>
    /// Displays the Win panel and freezes gameplay time.
    /// </summary>
    public void ShowWin()
    {
        if (gameEnded) return;
        gameEnded = true;
        IsGameEnded = true;

        if (losePanel != null) losePanel.SetActive(false);
        if (winPanel != null) winPanel.SetActive(true);

        if (playerLife == null || playerShooting == null)
        {
            FindReferences();
        }

        int score = ScoreManager.Instance != null ? ScoreManager.Instance.TotalScore : 0;
        int bulletLevel = playerShooting != null ? playerShooting.ShotLevel : 1;
        int lives = playerLife != null ? playerLife.CurrentLives : 0;

        if (winScoreText != null) winScoreText.text = $"{scorePrefix}{score}";
        else Debug.LogWarning("[GameEndUI] winScoreText is null! Please assign Win Score Text in the Inspector.");
        if (winBulletLevelText != null) winBulletLevelText.text = $"{bulletLevelPrefix}{bulletLevel}";
        if (winLifeText != null) winLifeText.text = $"{lifePrefix}{lives}";

        Debug.Log($"[GameEndUI] ShowWin - Score: {score}, BulletLevel: {bulletLevel}, Lives: {lives}");

        Time.timeScale = 0f;

        AudioManager.Instance?.PlayGameWin();

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    /// <summary>
    /// Displays the Lose panel and freezes gameplay time.
    /// </summary>
    public void ShowLose()
    {
        if (gameEnded) return;
        gameEnded = true;
        IsGameEnded = true;

        if (winPanel != null) winPanel.SetActive(false);
        if (losePanel != null) losePanel.SetActive(true);

        if (playerLife == null || playerShooting == null || waveSequencer == null)
        {
            FindReferences();
        }

        int score = ScoreManager.Instance != null ? ScoreManager.Instance.TotalScore : 0;
        int wave = waveSequencer != null ? waveSequencer.CurrentWave : 1;
        int bulletLevel = playerShooting != null ? playerShooting.ShotLevel : 1;

        if (loseScoreText != null) loseScoreText.text = $"{scorePrefix}{score}";
        else Debug.LogWarning("[GameEndUI] loseScoreText is null! Please assign Lose Score Text in the Inspector.");
        if (loseWaveText != null) loseWaveText.text = $"{wavePrefix}{wave}";
        if (loseBulletLevelText != null) loseBulletLevelText.text = $"{bulletLevelPrefix}{bulletLevel}";

        Debug.Log($"[GameEndUI] ShowLose - Score: {score}, Wave: {wave}, BulletLevel: {bulletLevel}");

        Time.timeScale = 0f;

        AudioManager.Instance?.PlayGameOver();

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    /// <summary>
    /// Deletes existing save checkpoint and reloads the gameplay scene for a fresh run.
    /// </summary>
    public void RestartGame()
    {
        SaveSystem.DeleteSave();
        Time.timeScale = 1f;
        Loader.Load(Loader.Scene.SampleScene);
    }

    /// <summary>
    /// Restores timeScale and returns to the Main Menu scene.
    /// </summary>
    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        Loader.Load(Loader.Scene.MainMenu);
    }
}
