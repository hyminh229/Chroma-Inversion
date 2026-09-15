using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement; // Thêm thư viện chuyển Scene

public class MainMenuUI : MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField] private Button playButton;
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
        // 1. Chuyển sang màn chơi chính (Index 1 trong Build Settings)
        playButton.onClick.AddListener(() => SceneManager.LoadScene(1));

        // 2. Bật các bảng popup tương ứng khi bấm nút
        if (tutorialButton != null && tutorialPanel != null)
            tutorialButton.onClick.AddListener(() => tutorialPanel.SetActive(true));

        if (optionsButton != null && optionsPanel != null)
            optionsButton.onClick.AddListener(() => optionsPanel.SetActive(true));

        if (creditsButton != null && creditsPanel != null)
            creditsButton.onClick.AddListener(() => creditsPanel.SetActive(true));

        // 3. Thoát game
        if (quitButton != null)
            quitButton.onClick.AddListener(QuitGame);
    }

    private void QuitGame()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
