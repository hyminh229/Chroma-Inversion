using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WaveBannerUI : MonoBehaviour
{
    [Header("Wave Text UI References")]
    [Tooltip("Kéo thả GameObject WaveTest (TextMeshPro) vào đây")]
    [SerializeField] private TMP_Text waveTest;

    [Tooltip("Dự phòng: Tham chiếu TextMeshPro mặc định")]
    [SerializeField] private TMP_Text bannerText;

    [Tooltip("Dự phòng: Hỗ trợ nếu bạn dùng UI Text thường thay vì TextMeshPro")]
    [SerializeField] private Text legacyBannerText;

    [Header("Animation Settings")]
    [Tooltip("Thời gian mờ dần vào / ra của banner")]
    [SerializeField] private float fadeDuration = 0.4f;
    [SerializeField] private CanvasGroup canvasGroup;

    private void Awake()
    {
        ResolveReferences();
        SetAlpha(0f);
    }

    private void ResolveReferences()
    {
        // 1. Ưu tiên biến waveTest nếu đã được gán trong Inspector
        if (waveTest != null)
        {
            bannerText = waveTest;
        }

        // 2. Nếu cả hai biến TMP_Text đều trống, thử tìm tự động trong hierarchy
        if (bannerText == null && legacyBannerText == null)
        {
            // Tìm GameObject con tên "WaveTest" hoặc "WaveText"
            Transform child = transform.Find("WaveTest");
            if (child == null) child = transform.Find("WaveText");

            if (child != null)
            {
                bannerText = child.GetComponent<TMP_Text>();
                if (bannerText == null)
                {
                    legacyBannerText = child.GetComponent<Text>();
                }
            }

            // Nếu vẫn chưa thấy, tìm bất kỳ TMP_Text nào trên chính nó hoặc con cháu
            if (bannerText == null && legacyBannerText == null)
            {
                bannerText = GetComponentInChildren<TMP_Text>(true);
            }

            // Hoặc UI Text thường
            if (bannerText == null && legacyBannerText == null)
            {
                legacyBannerText = GetComponentInChildren<Text>(true);
            }
        }

        if (canvasGroup == null)
        {
            canvasGroup = GetComponent<CanvasGroup>();
        }
    }

    public IEnumerator ShowBanner(string message, float displayDuration)
    {
        ResolveReferences();

        if (bannerText == null && legacyBannerText == null && canvasGroup == null)
        {
            Debug.LogWarning("[WaveBannerUI] Chưa gán WaveTest hoặc TMP_Text!");
            yield break;
        }

        // Kích hoạt GameObject để hiển thị
        gameObject.SetActive(true);

        if (bannerText != null)
        {
            bannerText.text = message;
        }
        else if (legacyBannerText != null)
        {
            legacyBannerText.text = message;
        }

        // 1. Hiện dần lên (Fade In)
        yield return Fade(0f, 1f);

        // 2. Giữ banner hiển thị trong thời gian quy định (2 - 4 giây)
        yield return new WaitForSeconds(displayDuration);

        // 3. Mờ dần đi (Fade Out)
        yield return Fade(1f, 0f);

        // Ẩn GameObject sau khi banner tắt
        gameObject.SetActive(false);
    }

    private IEnumerator Fade(float from, float to)
    {
        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            SetAlpha(Mathf.Lerp(from, to, t / fadeDuration));
            yield return null;
        }

        SetAlpha(to);
    }

    private void SetAlpha(float alpha)
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = alpha;
            return;
        }

        if (bannerText != null)
        {
            Color color = bannerText.color;
            color.a = alpha;
            bannerText.color = color;
        }

        if (legacyBannerText != null)
        {
            Color color = legacyBannerText.color;
            color.a = alpha;
            legacyBannerText.color = color;
        }
    }
}