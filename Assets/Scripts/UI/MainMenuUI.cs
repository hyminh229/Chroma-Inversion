using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class MainMenuUI : MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField] private Button playButton;      // Nút New Game
    [SerializeField] private Button continueButton;  // Nút Continue
    [SerializeField] private Button tutorialButton;
    [SerializeField] private Button optionsButton;
    [SerializeField] private Button creditsButton;
    [SerializeField] private Button quitButton;

    [Header("Panels Popup (Khung giao diện)")]
    [SerializeField] private GameObject tutorialPanel;
    [SerializeField] private GameObject optionsPanel;
    [SerializeField] private GameObject creditsPanel;

    private void Awake()
    {
        // Tự động tìm ContinueButton nếu chưa được kéo vào Inspector
        if (continueButton == null)
        {
            Transform continueTransform = transform.Find("ContinueButton");
            if (continueTransform != null)
            {
                continueButton = continueTransform.GetComponent<Button>();
            }
        }

        // 1. Nút New Game: Xóa save cũ, reset tiến trình, vào màn chơi mới
        if (playButton != null)
        {
            playButton.onClick.AddListener(OnNewGameClicked);
        }

        // 2. Nút Continue: Tải checkpoint và vào màn chơi
        if (continueButton != null)
        {
            continueButton.onClick.AddListener(OnContinueClicked);
        }

        // 3. Bật các bảng popup tương ứng khi bấm nút
        if (tutorialButton != null && tutorialPanel != null)
            tutorialButton.onClick.AddListener(() => tutorialPanel.SetActive(true));

        if (optionsButton != null && optionsPanel != null)
            optionsButton.onClick.AddListener(() => optionsPanel.SetActive(true));

        if (creditsButton != null && creditsPanel != null)
            creditsButton.onClick.AddListener(() => creditsPanel.SetActive(true));

        // 4. Thoát game
        if (quitButton != null)
            quitButton.onClick.AddListener(QuitGame);
    }

    private void Start()
    {
        UpdateContinueButtonState();
    }

    private void OnEnable()
    {
        UpdateContinueButtonState();
    }

    /// <summary>
    /// Cập nhật trạng thái tương tác của nút Continue dựa trên sự tồn tại của file save.
    /// </summary>
    private void UpdateContinueButtonState()
    {
        bool hasSave = SaveSystem.HasSave();

        if (continueButton != null)
        {
            continueButton.interactable = hasSave;

            TMP_Text tmpText = continueButton.GetComponentInChildren<TMP_Text>();
            if (tmpText != null)
            {
                Color color = tmpText.color;
                color.a = hasSave ? 1f : 0.4f;
                tmpText.color = color;
            }
        }
    }

    private void OnNewGameClicked()
    {
        Debug.Log("[MainMenuUI] New Game clicked. Resetting progression and starting fresh.");
        SaveSystem.DeleteSave();
        SaveSystem.IsContinuing = false;
        SceneManager.LoadScene(1);
    }

    private void OnContinueClicked()
    {
        if (!SaveSystem.HasSave())
        {
            Debug.LogWarning("[MainMenuUI] Continue clicked, but no valid save file exists.");
            UpdateContinueButtonState();
            return;
        }

        Debug.Log("[MainMenuUI] Continue clicked. Loading latest checkpoint.");
        SaveSystem.IsContinuing = true;
        SceneManager.LoadScene(1);
    }

    private void QuitGame()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
