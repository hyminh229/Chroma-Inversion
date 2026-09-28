using UnityEngine;

// Bắn 1 loạt đạn cùng lúc, dàn thành hình quạt xoay quanh hướng chính (thẳng
// tới Player). Hướng của cả cụm được chốt NGAY LÚC BẮN — không homing.
[RequireComponent(typeof(BossHealth))]
public class BossFanShotAttack : MonoBehaviour
{
    [Header("Bullet")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private int bulletCount = 7;
    [SerializeField] private float spreadAngle = 60f;
    [SerializeField] private float bulletSpeed = 6f;
    [SerializeField] private ElementColor bulletColor = ElementColor.RED;

    [Header("Timing")]
    [SerializeField] private float cooldown = 4f;

    private BossHealth bossHealth;
    private Transform player;
    private float timer;

    private void Awake()
    {
        bossHealth = GetComponent<BossHealth>();
    }

    private void Start()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null) player = playerObject.transform;
        else Debug.LogWarning("BossFanShotAttack could not find Player.");
    }

    private void Update()
    {
        if (!bossHealth.IsAlive || player == null) return;

        timer += Time.deltaTime;

        if (timer >= cooldown)
        {
            timer = 0f;
            FireFan();
        }
    }

    private void FireFan()
    {
        if (bulletPrefab == null || bulletCount <= 0) return;

        // Lấy Player.position 1 lần duy nhất tại thời điểm bắn để tính góc trung tâm.
        Vector2 origin = transform.position;
        Vector2 toPlayer = (Vector2)player.position - origin;
        float centerAngle = Mathf.Atan2(toPlayer.y, toPlayer.x) * Mathf.Rad2Deg;

        float startAngle = bulletCount > 1 ? -spreadAngle / 2f : 0f;
        float angleStep = bulletCount > 1 ? spreadAngle / (bulletCount - 1) : 0f;

        for (int i = 0; i < bulletCount; i++)
        {
            float bulletAngle = centerAngle + startAngle + i * angleStep;
            Quaternion rotation = Quaternion.Euler(0f, 0f, bulletAngle - 90f);
            SpawnBullet(origin, rotation);
        }
    }

    private void SpawnBullet(Vector2 position, Quaternion rotation)
    {
        GameObject bulletObject = ObjectPooler.Instance != null
            ? ObjectPooler.Instance.Spawn(bulletPrefab, position, rotation)
            : Instantiate(bulletPrefab, position, rotation);

        if (bulletObject == null) return;
        if (!bulletObject.TryGetComponent(out EnemyBullet enemyBullet)) return;

        enemyBullet.SetColor(bulletColor);
        enemyBullet.SetSpeed(bulletSpeed);
    }
}