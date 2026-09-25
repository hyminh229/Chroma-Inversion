using UnityEngine;
using UnityEngine.UI;

public class CreditsUI : MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField] private Button backButton;
    private System.Action onCloseCallback;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Awake()
    {
        // Wire up Back button
        if (backButton != null)
            backButton.onClick.AddListener(OnBackPressed);
    }
        public void SetOnCloseCallback(System.Action callback)
    {
        onCloseCallback = callback;
    }

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
