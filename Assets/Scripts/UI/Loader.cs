using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class Loader : MonoBehaviour
{
    public enum Scene
    {
        MainMenu,
        LoadingScene,
        SampleScene
    }

    private static string targetSceneName = "SampleScene";

    [Header("UI References")]
    [SerializeField] private Slider progressBar;
    [SerializeField] private Image progressFillImage;
    [SerializeField] private TMP_Text progressText;

    [Header("Loading Settings")]
    [SerializeField] private float minLoadingTime = 1.0f; // Thời gian hiển thị tối thiểu để animation mượt mà
    [SerializeField] private float progressSmoothSpeed = 2.5f;

    /// <summary>
    /// Chuyển tới LoadingScene và chuẩn bị tải scene mục tiêu.
    /// </summary>
    public static void Load(string sceneName)
    {
        targetSceneName = sceneName;
        SceneManager.LoadScene("LoadingScene");
    }

    public static void Load(Scene scene)
    {
        Load(scene.ToString());
    }

    private void Start()
    {
        StartCoroutine(LoadSceneAsyncRoutine());
    }

    private IEnumerator LoadSceneAsyncRoutine()
    {
        // Chờ 1 frame để UI được render hoàn tất
        yield return null;

        string sceneToLoad = string.IsNullOrEmpty(targetSceneName) ? "SampleScene" : targetSceneName;

        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneToLoad);
        if (operation == null)
        {
            Debug.LogError($"[Loader] Không thể tải scene '{sceneToLoad}'.");
            yield break;
        }

        operation.allowSceneActivation = false;

        float currentProgress = 0f;
        float elapsedTime = 0f;

        while (!operation.isDone)
        {
            elapsedTime += Time.deltaTime;

            // Tiến độ thực của AsyncOperation chạy từ 0 đến 0.9f
            float targetProgress = Mathf.Clamp01(operation.progress / 0.9f);

            // Chạy hiệu ứng tăng thanh tiến trình mượt mà
            currentProgress = Mathf.MoveTowards(currentProgress, targetProgress, progressSmoothSpeed * Time.deltaTime);

            if (progressBar != null)
            {
                progressBar.value = currentProgress;
            }

            if (progressFillImage != null)
            {
                progressFillImage.fillAmount = currentProgress;
            }

            if (progressText != null)
            {
                progressText.text = $"LOADING... {Mathf.RoundToInt(currentProgress * 100)}%";
            }

            // Đạt 100% và vượt qua thời gian tối thiểu -> kích hoạt scene
            if (operation.progress >= 0.9f && currentProgress >= 0.99f && elapsedTime >= minLoadingTime)
            {
                if (progressBar != null) progressBar.value = 1f;
                if (progressFillImage != null) progressFillImage.fillAmount = 1f;
                if (progressText != null) progressText.text = "LOADING... 100%";

                yield return new WaitForSeconds(0.15f);
                operation.allowSceneActivation = true;
            }

            yield return null;
        }
    }
}
