using System.Collections;
using UnityEngine;

[RequireComponent(typeof(BossHealth))]
public class BossController : MonoBehaviour
{
    [Header("Flip (đổi phe màu Top/Bottom bằng cách xoay 180°)")]
    [SerializeField] private float holdDuration = 2.5f;
    [SerializeField] private float flipDuration = 0.5f;

    [Header("Spread Fire (đạn màu random, độc lập hoàn toàn với việc đang xoay tới đâu)")]
    [SerializeField] private Transform[] firePoints;
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private float fireInterval = 0.8f;
    [SerializeField][Range(0f, 1f)] private float fireChance = 0.8f;

    [Header("Enrage (tự tăng tốc khi HP xuống thấp)")]
    [SerializeField][Range(0f, 1f)] private float enrageHpThreshold = 0.35f;
    [SerializeField] private float enrageHoldMultiplier = 0.5f;
    [SerializeField] private float enrageFireIntervalMultiplier = 0.5f;

    public bool IsEnraged { get; private set; }

    private BossHealth bossHealth;
    private float fireTimer;
    private float currentZ;

    private void Awake()
    {
        bossHealth = GetComponent<BossHealth>();
    }

    private void OnEnable()
    {
        bossHealth.OnDamaged += CheckEnrage;
    }

    private void OnDisable()
    {
        bossHealth.OnDamaged -= CheckEnrage;
    }

    private void Start()
    {
        StartCoroutine(FlipLoop());
    }

    private void Update()
    {
        HandleFiring();
    }

    private IEnumerator FlipLoop()
    {
        while (bossHealth.IsAlive)
        {
            float hold = IsEnraged ? holdDuration * enrageHoldMultiplier : holdDuration;
            yield return new WaitForSeconds(hold);

            if (!bossHealth.IsAlive) yield break;

            yield return StartCoroutine(FlipRotation());
        }
    }

    // Xoay đúng 180° mỗi lần, không oscillate liên tục nữa. TopHalf/BottomHalf
    // (2 box con) giữ màu CỐ ĐỊNH trên chính nó — chỉ có VỊ TRÍ trên màn hình
    // đổi chỗ cho nhau sau mỗi lần flip. Đó là toàn bộ cơ chế "đổi phe".
    private IEnumerator FlipRotation()
    {
        float startZ = currentZ;
        float targetZ = currentZ + 180f;
        float t = 0f;

        while (t < flipDuration)
        {
            t += Time.deltaTime;
            float z = Mathf.Lerp(startZ, targetZ, t / flipDuration);
            transform.rotation = Quaternion.Euler(0f, 0f, z);
            yield return null;
        }

        currentZ = targetZ;
        transform.rotation = Quaternion.Euler(0f, 0f, currentZ);
    }

    private void HandleFiring()
    {
        fireTimer += Time.deltaTime;
        float interval = IsEnraged ? fireInterval * enrageFireIntervalMultiplier : fireInterval;

        if (fireTimer >= interval)
        {
            fireTimer = 0f;
            FireFromAllPoints();
        }
    }

    private void FireFromAllPoints()
    {
        if (firePoints == null) return;

        foreach (Transform point in firePoints)
        {
            if (point == null) continue;
            if (Random.value > fireChance) continue;

            FireOne(point);
        }
    }

    private void FireOne(Transform point)
    {
        if (bulletPrefab == null) return;

        GameObject bulletObject = ObjectPooler.Instance != null
            ? ObjectPooler.Instance.Spawn(bulletPrefab, point.position, point.rotation)
            : Instantiate(bulletPrefab, point.position, point.rotation);

        if (bulletObject == null) return;
        if (!bulletObject.TryGetComponent(out EnemyBullet enemyBullet)) return;

        // Màu đạn RANDOM hoàn toàn mỗi phát — không còn liên quan tới việc thân
        // đang xoay tới đâu (khác hẳn bản trước).
        ElementColor randomColor = Random.value < 0.5f ? ElementColor.BLUE : ElementColor.RED;
        enemyBullet.SetColor(randomColor);
    }

    private void CheckEnrage()
    {
        if (IsEnraged) return;
        if ((float)bossHealth.CurrentHealth / bossHealth.MaxHealth > enrageHpThreshold) return;

        IsEnraged = true;
        Debug.Log("BOSS ENRAGED!");
    }
}