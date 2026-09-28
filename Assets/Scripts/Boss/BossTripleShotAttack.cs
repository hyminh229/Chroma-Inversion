using UnityEngine;

// Bắn 1 cụm 3 viên (hoặc N viên tuỳ Inspector) song song, cùng hướng tới vị trí
// Player TẠI THỜI ĐIỂM BẮN. Không homing — sau khi spawn, hướng giữ nguyên dù
// Player có di chuyển.
[RequireComponent(typeof(BossHealth))]
public class BossTripleShotAttack : MonoBehaviour
{
    [Header("Bullet")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private int bulletCount = 3;
    [SerializeField] private float bulletSpacing = 0.4f;
    [SerializeField] private float bulletSpeed = 6f;
    [SerializeField] private ElementColor bulletColor = ElementColor.BLUE;

    [Header("Timing")]
    [SerializeField] private float cooldown = 3f;

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
        else Debug.LogWarning("BossTripleShotAttack could not find Player.");
    }

    private void Update()
    {
        if (!bossHealth.IsAlive || player == null) return;

        timer += Time.deltaTime;

        if (timer >= cooldown)
        {
            timer = 0f;
            FireTripleShot();
        }
    }

    private void FireTripleShot()
    {
        if (bulletPrefab == null || bulletCount <= 0) return;

        // Chốt hướng bắn NGAY LÚC NÀY, không đọc lại vị trí Player sau đó nữa.
        Vector2 origin = transform.position;
        Vector2 direction = ((Vector2)player.position - origin).normalized;

        // Trục vuông góc với direction — dùng để dàn các viên cạnh nhau thành cụm.
        Vector2 perpendicular = new Vector2(-direction.y, direction.x);

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
        Quaternion rotation = Quaternion.Euler(0f, 0f, angle);

        float startOffset = -(bulletCount - 1) / 2f * bulletSpacing;

        for (int i = 0; i < bulletCount; i++)
        {
            float offset = startOffset + i * bulletSpacing;
            Vector2 spawnPos = origin + perpendicular * offset;
            SpawnBullet(spawnPos, rotation);
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