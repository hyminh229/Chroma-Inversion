using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Cung cấp hiệu ứng xúc giác (game juice) khi tương tác với nút UI:
/// - Khi rê chuột vào (Hover): Phóng to nhẹ để nhận biết nút có thể bấm.
/// - Khi nhấn chuột xuống (Pointer Down): Nút lún xuống (scale 0.92x) tạo cảm giác nút vật lý.
/// - Khi thả chuột / Click (Pointer Click): Nảy nhẹ (punch bounce) tạo cảm giác đã kích hoạt thành công.
/// Hoạt động mượt mà bằng Time.unscaledDeltaTime (vẫn mượt ngay cả khi Pause game).
/// </summary>
[RequireComponent(typeof(Selectable))]
public class UIButtonFeedback : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("Scale Animation")]
    [SerializeField] private bool enableScale = true;
    [Tooltip("Tỉ lệ co lại khi nhấn chuột (mặc định: 0.92 = lún 8%)")]
    [SerializeField] private float pressedScale = 0.92f;
    [Tooltip("Tỉ lệ phóng to nhẹ khi rê chuột vào (mặc định: 1.04 = to 4%)")]
    [SerializeField] private float hoverScale = 1.04f;
    [Tooltip("Tốc độ chuyển đổi kích thước")]
    [SerializeField] private float lerpSpeed = 18f;

    [Header("Punch Click Effect")]
    [SerializeField] private bool enableClickPunch = true;
    [Tooltip("Độ nảy bung ra khi click")]
    [SerializeField] private float punchScale = 1.07f;
    [Tooltip("Thời gian nảy (giây)")]
    [SerializeField] private float punchDuration = 0.14f;

    private Selectable selectable;
    private Vector3 originalScale = Vector3.one;
    private Coroutine activeCoroutine;
    private bool isPointerDown = false;
    private bool isPointerOver = false;

    private void Awake()
    {
        selectable = GetComponent<Selectable>();
        CaptureOriginalScale();
    }

    private void Start()
    {
        if (originalScale == Vector3.zero)
        {
            CaptureOriginalScale();
        }
    }

    private void CaptureOriginalScale()
    {
        Vector3 cur = transform.localScale;
        originalScale = (cur != Vector3.zero) ? cur : Vector3.one;
    }

    private void OnDisable()
    {
        if (activeCoroutine != null)
        {
            StopCoroutine(activeCoroutine);
            activeCoroutine = null;
        }

        transform.localScale = originalScale;
        isPointerDown = false;
        isPointerOver = false;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!IsInteractable() || !enableScale) return;
        isPointerDown = true;
        StartLerpScale(originalScale * pressedScale);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!IsInteractable() || !enableScale) return;
        isPointerDown = false;

        // Nếu không có click punch sắp chạy thì trở về hover/original
        if (!enableClickPunch)
        {
            Vector3 target = isPointerOver ? (originalScale * hoverScale) : originalScale;
            StartLerpScale(target);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!IsInteractable() || !enableScale) return;
        isPointerOver = true;

        if (!isPointerDown)
        {
            StartLerpScale(originalScale * hoverScale);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isPointerOver = false;

        if (!isPointerDown)
        {
            StartLerpScale(originalScale);
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!IsInteractable()) return;

        if (enableClickPunch && gameObject.activeInHierarchy)
        {
            if (activeCoroutine != null)
            {
                StopCoroutine(activeCoroutine);
            }
            activeCoroutine = StartCoroutine(PunchBounceRoutine());
        }
    }

    private bool IsInteractable()
    {
        return selectable == null || selectable.interactable;
    }

    private void StartLerpScale(Vector3 targetScale)
    {
        if (activeCoroutine != null)
        {
            StopCoroutine(activeCoroutine);
        }
        if (gameObject.activeInHierarchy)
        {
            activeCoroutine = StartCoroutine(LerpScaleRoutine(targetScale));
        }
    }

    private IEnumerator LerpScaleRoutine(Vector3 targetScale)
    {
        while (Vector3.Distance(transform.localScale, targetScale) > 0.002f)
        {
            transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.unscaledDeltaTime * lerpSpeed);
            yield return null;
        }
        transform.localScale = targetScale;
        activeCoroutine = null;
    }

    private IEnumerator PunchBounceRoutine()
    {
        Vector3 pressed = originalScale * pressedScale;
        Vector3 overshoot = originalScale * punchScale;
        Vector3 returnTarget = isPointerOver ? (originalScale * hoverScale) : originalScale;

        // Giai đoạn 1: Nảy vượt mức (overshoot)
        float elapsed = 0f;
        float halfDuration = punchDuration * 0.5f;
        Vector3 from = transform.localScale;

        while (elapsed < halfDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            transform.localScale = Vector3.Lerp(from, overshoot, elapsed / halfDuration);
            yield return null;
        }

        // Giai đoạn 2: Thu về kích thước đích (hover hoặc original)
        elapsed = 0f;
        while (elapsed < halfDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            transform.localScale = Vector3.Lerp(overshoot, returnTarget, elapsed / halfDuration);
            yield return null;
        }

        transform.localScale = returnTarget;
        activeCoroutine = null;
    }
}
