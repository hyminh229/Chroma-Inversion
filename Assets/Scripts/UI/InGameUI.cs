using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Manages HUD and in-game UI displays for Life, Bullet Level, Score, Energy Sliders, and Mega Beam status.
/// Subscribes to events from PlayerLife, PlayerEnergy, PlayerShooting, and ScoreManager.
/// UI element references are serialized for manual configuration in the Unity Inspector.
/// </summary>
public class InGameUI : MonoBehaviour
{
    [Header("Life")]
    [SerializeField] private TMP_Text lifeText;

    [Header("Bullet Upgrade")]
    [SerializeField] private TMP_Text bulletLevelText;

    [Header("Score")]
    [SerializeField] private TMP_Text scoreText;

    [Header("Energy")]
    [SerializeField] private Slider blueEnergySlider;
    [SerializeField] private Slider redEnergySlider;

    [Header("Mega Beam")]
    [SerializeField] private GameObject megaBeamReadyObject;

    [Header("Gameplay References (Optional - auto-found if unassigned)")]
    [SerializeField] private PlayerLife playerLife;
    [SerializeField] private PlayerEnergy playerEnergy;
    [SerializeField] private PlayerShooting playerShooting;
    [SerializeField] private ScoreManager scoreManager;

    private PlayerLife subscribedLife;
    private PlayerEnergy subscribedEnergy;
    private PlayerShooting subscribedShooting;
    private ScoreManager subscribedScoreManager;

    private void Awake()
    {
        FindReferences();
    }

    private void OnEnable()
    {
        FindReferences();
        SubscribeEvents();
        RefreshAll();
    }

    private void Start()
    {
        FindReferences();
        SubscribeEvents();
        RefreshAll();
    }

    private void OnDisable()
    {
        UnsubscribeEvents();
    }

    private void OnDestroy()
    {
        UnsubscribeEvents();
    }

    /// <summary>
    /// Finds system references if they were not assigned in the Inspector.
    /// Only runs during initialization/lifecycle hooks, never per frame.
    /// </summary>
    public void FindReferences()
    {
        if (scoreManager == null)
        {
            scoreManager = FindAnyObjectByType<ScoreManager>();
            if (scoreManager == null)
            {
                scoreManager = ScoreManager.Instance;
            }
        }

        if (playerEnergy == null)
        {
            playerEnergy = PlayerEnergy.Instance;
        }

        if (playerLife == null || playerShooting == null || playerEnergy == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                if (playerLife == null) playerLife = playerObj.GetComponent<PlayerLife>();
                if (playerShooting == null) playerShooting = playerObj.GetComponent<PlayerShooting>();
                if (playerEnergy == null) playerEnergy = playerObj.GetComponent<PlayerEnergy>();
            }
        }

        if (playerLife == null) playerLife = FindAnyObjectByType<PlayerLife>();
        if (playerShooting == null) playerShooting = FindAnyObjectByType<PlayerShooting>();
        if (playerEnergy == null) playerEnergy = FindAnyObjectByType<PlayerEnergy>();
    }

    /// <summary>
    /// Explicitly initialize gameplay references and immediately refresh the UI.
    /// </summary>
    public void Initialize(PlayerLife life, PlayerEnergy energy, PlayerShooting shooting, ScoreManager score = null)
    {
        UnsubscribeEvents();

        if (life != null) playerLife = life;
        if (energy != null) playerEnergy = energy;
        if (shooting != null) playerShooting = shooting;
        if (score != null) scoreManager = score;

        SubscribeEvents();
        RefreshAll();
    }

    /// <summary>
    /// Subscribes to gameplay events without creating duplicate subscriptions.
    /// </summary>
    public void SubscribeEvents()
    {
        if (playerLife != null && subscribedLife != playerLife)
        {
            if (subscribedLife != null) subscribedLife.OnLifeChanged -= UpdateLife;
            playerLife.OnLifeChanged += UpdateLife;
            subscribedLife = playerLife;
        }

        if (playerShooting != null && subscribedShooting != playerShooting)
        {
            if (subscribedShooting != null) subscribedShooting.OnShotLevelChanged -= UpdateBulletLevel;
            playerShooting.OnShotLevelChanged += UpdateBulletLevel;
            subscribedShooting = playerShooting;
        }

        if (playerEnergy != null && subscribedEnergy != playerEnergy)
        {
            if (subscribedEnergy != null) subscribedEnergy.OnEnergyChanged -= UpdateEnergy;
            playerEnergy.OnEnergyChanged += UpdateEnergy;
            subscribedEnergy = playerEnergy;
        }

        if (scoreManager != null && subscribedScoreManager != scoreManager)
        {
            if (subscribedScoreManager != null) subscribedScoreManager.OnScoreChanged -= UpdateScore;
            scoreManager.OnScoreChanged += UpdateScore;
            subscribedScoreManager = scoreManager;
        }
    }

    /// <summary>
    /// Unsubscribes from all gameplay events safely.
    /// </summary>
    public void UnsubscribeEvents()
    {
        if (subscribedLife != null)
        {
            subscribedLife.OnLifeChanged -= UpdateLife;
            subscribedLife = null;
        }

        if (subscribedShooting != null)
        {
            subscribedShooting.OnShotLevelChanged -= UpdateBulletLevel;
            subscribedShooting = null;
        }

        if (subscribedEnergy != null)
        {
            subscribedEnergy.OnEnergyChanged -= UpdateEnergy;
            subscribedEnergy = null;
        }

        if (subscribedScoreManager != null)
        {
            subscribedScoreManager.OnScoreChanged -= UpdateScore;
            subscribedScoreManager = null;
        }
    }

    /// <summary>
    /// Forces a full UI refresh based on current gameplay state.
    /// </summary>
    public void RefreshAll()
    {
        if (playerLife != null)
        {
            UpdateLife(playerLife.CurrentLives);
        }

        if (playerShooting != null)
        {
            UpdateBulletLevel(playerShooting.ShotLevel);
        }

        if (scoreManager != null)
        {
            UpdateScore(scoreManager.TotalScore);
        }

        if (playerEnergy != null)
        {
            UpdateEnergy(playerEnergy.CurrentBlueEnergy, playerEnergy.CurrentRedEnergy);
        }
        else
        {
            SetMegaBeamReady(false);
        }
    }

    /// <summary>
    /// Updates the Life display text (e.g., "3").
    /// </summary>
    public void UpdateLife(int currentLives)
    {
        if (lifeText != null)
        {
            lifeText.text = currentLives.ToString();
        }
    }

    /// <summary>
    /// Updates the Bullet Level display text (e.g., "Lv. 2").
    /// </summary>
    public void UpdateBulletLevel(int level)
    {
        if (bulletLevelText != null)
        {
            bulletLevelText.text = $"Lv. {level}";
        }
    }

    /// <summary>
    /// Updates the Score display text (e.g., "1250").
    /// </summary>
    public void UpdateScore(int score)
    {
        if (scoreText != null)
        {
            scoreText.text = score.ToString();
        }
    }

    /// <summary>
    /// Updates the Blue and Red energy sliders (current energy / max energy)
    /// and updates the Mega Beam readiness state.
    /// </summary>
    public void UpdateEnergy(int blueEnergy, int redEnergy)
    {
        int maxEnergy = playerEnergy != null ? playerEnergy.MaxEnergy : 100;
        float blueRatio = maxEnergy > 0 ? Mathf.Clamp01((float)blueEnergy / maxEnergy) : 0f;
        float redRatio = maxEnergy > 0 ? Mathf.Clamp01((float)redEnergy / maxEnergy) : 0f;

        if (blueEnergySlider != null)
        {
            blueEnergySlider.value = blueRatio;
        }

        if (redEnergySlider != null)
        {
            redEnergySlider.value = redRatio;
        }

        bool isReady = playerEnergy != null
            ? playerEnergy.IsFull
            : (maxEnergy > 0 && blueEnergy >= maxEnergy && redEnergy >= maxEnergy);

        SetMegaBeamReady(isReady);
    }

    /// <summary>
    /// Shows or hides the Mega Beam ready object.
    /// </summary>
    public void SetMegaBeamReady(bool isReady)
    {
        if (megaBeamReadyObject != null)
        {
            megaBeamReadyObject.SetActive(isReady);
        }
    }
}
