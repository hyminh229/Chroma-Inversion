using System.Collections;
using TMPro;
using UnityEngine;

public class WaveBannerUI : MonoBehaviour
{
    [SerializeField] private TMP_Text bannerText;
    [SerializeField] private float fadeDuration = 0.3f;

    private void Awake()
    {
        if (bannerText == null)
        {
            bannerText = GetComponent<TMP_Text>();
        }

        if (bannerText == null)
        {
            Debug.LogWarning("WaveBannerUI is missing bannerText reference.");
            return;
        }

        SetAlpha(0f);
    }

    public IEnumerator ShowBanner(string message, float displayDuration)
    {
        if (bannerText == null) yield break;

        bannerText.text = message;

        yield return StartCoroutine(Fade(0f, 1f));
        yield return new WaitForSeconds(displayDuration);
        yield return StartCoroutine(Fade(1f, 0f));
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
        Color color = bannerText.color;
        color.a = alpha;
        bannerText.color = color;
    }
}