using UnityEngine;

[RequireComponent(typeof(BossHealth))]
public class BossSummonAttack : MonoBehaviour
{
    [Header("Timing")]
    [SerializeField] private float summonInterval = 10f;
    [SerializeField] private int minionsPerSummon = 3;

    [Header("Minion Prefab (PHẢI có EnemyController + EnemyHealth + EnemyPolarity + EnemyShooting với aimAtPlayer = true)")]
    [SerializeField] private GameObject minionPrefab;
    [SerializeField] private float minionMoveSpeed = 3f;
    [SerializeField] private float minionFireCheckInterval = 1.5f;
    [SerializeField][Range(0f, 1f)] private float minionFireChance = 0.35f;

    [Header("Spawn Area (tự tính theo Camera lúc runtime)")]
    [SerializeField] private float horizontalMargin = 1f;
    [SerializeField] private float topSpawnMargin = 1.5f;

    private BossHealth bossHealth;
    private float summonTimer;
    private int totalSummoned;

    private void Awake()
    {
        bossHealth = GetComponent<BossHealth>();
    }

    private void Update()
    {
        if (!bossHealth.IsAlive) return;

        summonTimer += Time.deltaTime;

        if (summonTimer >= summonInterval)
        {
            summonTimer = 0f;
            SummonMinions();
        }
    }

    private void SummonMinions()
    {
        if (minionPrefab == null)
        {
            Debug.LogWarning("BossSummonAttack is missing minionPrefab.");
            return;
        }

        ScreenBoundsUtil.GetWorldBounds(out float rawMinX, out float rawMaxX, out _, out float topY);
        float minX = rawMinX + horizontalMargin;
        float maxX = rawMaxX - horizontalMargin;
        float spawnY = topY + topSpawnMargin;

        for (int i = 0; i < minionsPerSummon; i++)
        {
            float x = Random.Range(minX, maxX);
            SpawnOne(new Vector3(x, spawnY, 0f), minX, maxX);
        }

        totalSummoned += minionsPerSummon;
        // Log tổng đã summon từ đầu trận — playtest xong xem số này có tăng
        // đều theo summonInterval hay dừng lại, để xác nhận đúng nguyên nhân.
        Debug.Log("Boss summoned " + minionsPerSummon + " minions. Tổng cộng: " + totalSummoned);
    }

    private void SpawnOne(Vector3 position, float minX, float maxX)
    {
        GameObject instance = Instantiate(minionPrefab, position, Quaternion.identity);

        ElementColor bodyColor = Random.value < 0.5f ? ElementColor.BLUE : ElementColor.RED;
        if (instance.TryGetComponent(out ChromaPolarityBase polarity))
        {
            polarity.SetColor(bodyColor);
        }

        if (instance.TryGetComponent(out EnemyController controller))
        {
            controller.ConfigureMovement(MovementPattern.RandomFlutter, minionMoveSpeed, minX, maxX);
        }

        if (instance.TryGetComponent(out EnemyShooting shooting))
        {
            shooting.enabled = true;
            shooting.ConfigureFiring(minionFireCheckInterval, minionFireChance);

            ElementColor bulletColor = Random.value < 0.5f ? ElementColor.BLUE : ElementColor.RED;
            shooting.SetBulletColor(bulletColor);
        }
    }
}