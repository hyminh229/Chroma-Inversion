using System.Collections;
using UnityEngine;

[RequireComponent(typeof(BossHealth))]
public class BossController : MonoBehaviour
{
    [Header("Spin (xoay tại chỗ quanh tâm — giống bánh xe, KHÔNG di chuyển trong lúc này)")]
    [SerializeField] private float spinSpeed = 120f; // độ/giây — dấu (+/-) cố định 1 chiều, không đổi ngẫu nhiên nữa
    [SerializeField] private float spinDuration = 2f;

    [Header("Move (di chuyển sang vị trí X mới — KHÔNG xoay trong lúc này)")]
    [SerializeField] private float moveDuration = 1.2f;

    [Header("Spread Fire (bắn xuyên suốt cả 2 pha)")]
    [SerializeField] private Transform[] firePoints;
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private float fireInterval = 0.8f;
    [SerializeField][Range(0f, 1f)] private float fireChance = 0.8f;

    [Header("Enrage (tự tăng tốc khi HP xuống thấp)")]
    [SerializeField][Range(0f, 1f)] private float enrageHpThreshold = 0.35f;
    [SerializeField] private float enrageSpinMultiplier = 1.6f;
    [SerializeField] private float enrageFireIntervalMultiplier = 0.5f;

    public bool IsEnraged { get; private set; }

    private BossHealth bossHealth;
    private float fireTimer;
    private float minX, maxX;

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
        ScreenBoundsUtil.GetWorldBounds(out minX, out maxX, out _, out _);
        StartCoroutine(BehaviorLoop());
    }

    private void Update()
    {
        HandleFiring();
    }

    // Spin tại chỗ xong HẲN mới chuyển qua Move — 2 pha tách biệt hoàn toàn,
    // không còn chạy song song trong Update() như bản trước.
    private IEnumerator BehaviorLoop()
    {
        while (bossHealth.IsAlive)
        {
            yield return StartCoroutine(SpinPhase());
            if (!bossHealth.IsAlive) yield break;

            yield return StartCoroutine(MovePhase());
        }
    }

    private IEnumerator SpinPhase()
    {
        float t = 0f;

        while (t < spinDuration)
        {
            t += Time.deltaTime;
            float speed = IsEnraged ? spinSpeed * enrageSpinMultiplier : spinSpeed;
            transform.Rotate(0f, 0f, speed * Time.deltaTime);
            yield return null;
        }

        // Snap về đúng bội số 180° gần nhất — đảm bảo LUÔN dừng ở trạng thái lộ
        // rõ 1 màu trên/1 màu dưới, bất kể spinSpeed/spinDuration/enrage được
        // tune thế nào sau này, không cần tự nhẩm cho chia hết 180.
        float snappedZ = Mathf.Round(transform.eulerAngles.z / 180f) * 180f;
        transform.rotation = Quaternion.Euler(0f, 0f, snappedZ);
    }

    private IEnumerator MovePhase()
    {
        float targetX = Random.Range(minX, maxX);
        float startX = transform.position.x;
        float t = 0f;

        while (t < moveDuration)
        {
            t += Time.deltaTime;
            Vector3 pos = transform.position;
            pos.x = Mathf.Lerp(startX, targetX, t / moveDuration);
            transform.position = pos;
            yield return null;
        }

        Vector3 finalPos = transform.position;
        finalPos.x = targetX;
        transform.position = finalPos;
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