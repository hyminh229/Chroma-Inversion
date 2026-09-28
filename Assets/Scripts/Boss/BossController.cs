using System.Collections;
using UnityEngine;

[RequireComponent(typeof(BossHealth))]
public class BossController : MonoBehaviour
{
    [Header("Spin (xoay tại chỗ quanh tâm — giống bánh xe, KHÔNG di chuyển trong lúc này)")]
    [SerializeField] private float spinSpeed = 120f; // độ/giây — cũng quyết định luôn thời gian xoay (180°/spinSpeed)

    [Header("Move (di chuyển sang vị trí X mới — KHÔNG xoay trong lúc này)")]
    [SerializeField] private float moveDuration = 1.2f;
    [SerializeField] private float horizontalMargin = 2.5f; // chừa lề theo bề rộng sprite Boss, tránh lòi ra ngoài màn hình

    [Header("Spread Fire (bắn xuyên suốt cả 2 pha)")]
    [SerializeField] private Transform[] firePoints;
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private float fireInterval = 0.8f;
    [SerializeField][Range(0f, 1f)] private float fireChance = 0.8f;
    [SerializeField] private float aimRotationOffset = -90f; // giống EnemyShooting: bù trừ vì sprite đạn hướng "up" mặc định

    [Header("Enrage (tự tăng tốc khi HP xuống thấp)")]
    [SerializeField][Range(0f, 1f)] private float enrageHpThreshold = 0.35f;
    [SerializeField] private float enrageSpinMultiplier = 1.6f;
    [SerializeField] private float enrageFireIntervalMultiplier = 0.5f;

    public bool IsEnraged { get; private set; }

    private BossHealth bossHealth;
    private Transform player;
    private float fireTimer;
    private float minX, maxX;
    private bool isSpinning;

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
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null) player = playerObject.transform;
        else Debug.LogWarning("BossController could not find Player. Make sure Player has the 'Player' tag.");

        ScreenBoundsUtil.GetWorldBounds(out float rawMinX, out float rawMaxX, out _, out _);
        minX = rawMinX + horizontalMargin;
        maxX = rawMaxX - horizontalMargin;

        StartCoroutine(BehaviorLoop());
    }

    private void Update()
    {
        HandleFiring();
    }

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
        isSpinning = true;

        float speed = IsEnraged ? spinSpeed * enrageSpinMultiplier : spinSpeed;
        float duration = 180f / speed;

        float startZ = transform.eulerAngles.z;
        float targetZ = startZ + 180f;

        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float z = Mathf.Lerp(startZ, targetZ, Mathf.Clamp01(t / duration));
            transform.rotation = Quaternion.Euler(0f, 0f, z);
            yield return null;
        }

        transform.rotation = Quaternion.Euler(0f, 0f, targetZ);

        isSpinning = false;
        fireTimer = 0f;
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
        if (isSpinning) return;
        if (player == null) return;

        fireTimer += Time.deltaTime;
        float interval = IsEnraged ? fireInterval * enrageFireIntervalMultiplier : fireInterval;

        if (fireTimer >= interval)
        {
            fireTimer = 0f;
            FireFromLowerPoint();
        }
    }

    private void FireFromLowerPoint()
    {
        Transform lowerPoint = GetLowerFirePoint();
        if (lowerPoint == null) return;
        if (Random.value > fireChance) return;

        FireOne(lowerPoint);
    }

    private Transform GetLowerFirePoint()
    {
        if (firePoints == null) return null;

        Transform lowest = null;
        float lowestY = float.PositiveInfinity;

        foreach (Transform point in firePoints)
        {
            if (point == null) continue;

            if (point.position.y < lowestY)
            {
                lowestY = point.position.y;
                lowest = point;
            }
        }

        return lowest;
    }

    // Bắn TỪ vị trí firePoint (để đúng "nửa trên/nửa dưới" theo yêu cầu),
    // nhưng HƯỚNG bắn tính riêng từ firePoint -> Player, hoàn toàn độc lập
    // với rotation hiện tại của Boss (khác point.rotation cũ — cái đó chỉ
    // phản ánh góc xoay của Boss, không liên quan Player).
    private void FireOne(Transform point)
    {
        if (bulletPrefab == null) return;

        Vector2 direction = (Vector2)player.position - (Vector2)point.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        Quaternion aimRotation = Quaternion.Euler(0f, 0f, angle + aimRotationOffset);

        GameObject bulletObject = ObjectPooler.Instance != null
            ? ObjectPooler.Instance.Spawn(bulletPrefab, point.position, aimRotation)
            : Instantiate(bulletPrefab, point.position, aimRotation);

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