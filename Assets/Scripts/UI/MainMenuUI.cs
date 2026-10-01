using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class MainMenuUI : MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField] private Button playButton;      // Nút New Game
    [SerializeField] private Button continueButton;  // Nút Continue
    [Tooltip("Độ mờ của nút Continue khi chưa có save / chưa bấm được (mờ cả khung và chữ)")]
    [Range(0f, 1f)]
    [SerializeField] private float disabledContinueAlpha = 0.35f;
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
            Transform continueTransform = transform.Find("ButtonsContainer/ContinueButton") ?? transform.Find("ContinueButton");
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

        // Gắn âm thanh click và hiệu ứng xúc giác (co lún / nảy) cho tất cả các nút trong menu
        Button[] allButtons = GetComponentsInChildren<Button>(true);
        foreach (Button btn in allButtons)
        {
            btn.onClick.AddListener(() => AudioManager.Instance?.PlayUIClick());
            if (!btn.TryGetComponent<UIButtonFeedback>(out _))
            {
                btn.gameObject.AddComponent<UIButtonFeedback>();
            }
        }
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
    /// Làm mờ đồng bộ cả khung (Image) và chữ (TMP_Text) khi chưa có file save.
    /// </summary>
    private void UpdateContinueButtonState()
    {
        bool hasSave = SaveSystem.HasSave();

        if (continueButton != null)
        {
            continueButton.interactable = hasSave;

            // Sử dụng CanvasGroup để làm mờ đồng bộ toàn bộ nút (khung viền Image, chữ TMP_Text, bóng shadow)
            CanvasGroup canvasGroup = continueButton.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = continueButton.gameObject.AddComponent<CanvasGroup>();
            }

            canvasGroup.alpha = hasSave ? 1f : disabledContinueAlpha;

            // Reset alpha của TMP_Text về 1 để CanvasGroup quản lý đồng đều, tránh bị nhân đôi độ mờ
            TMP_Text tmpText = continueButton.GetComponentInChildren<TMP_Text>();
            if (tmpText != null)
            {
                Color color = tmpText.color;
                color.a = 1f;
                tmpText.color = color;
            }

            // Đảm bảo alpha của Image cũng là 1 để CanvasGroup quản lý
            Image frameImage = continueButton.GetComponent<Image>();
            if (frameImage != null)
            {
                Color color = frameImage.color;
                color.a = 1f;
                frameImage.color = color;
            }
        }
    }

    private void OnNewGameClicked()
    {
        Debug.Log("[MainMenuUI] New Game clicked. Resetting progression and starting fresh.");
        SaveSystem.DeleteSave();
        SaveSystem.IsContinuing = false;
        Loader.Load("SampleScene");
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
        Loader.Load("SampleScene");
    }

    private void QuitGame()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
