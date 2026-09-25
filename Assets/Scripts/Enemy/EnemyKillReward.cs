using UnityEngine;

[RequireComponent(typeof(EnemyHealth))]
public class EnemyKillReward : MonoBehaviour
{
    [Header("Thưởng trực tiếp khi tiêu diệt (không cần nhặt gì thêm)")]
    [SerializeField] private int killScore = 50;
    [SerializeField] private int killEnergy = 5;

    [Header("EXP Orb rơi thêm (bonus — phải tự bay tới nhặt)")]
    [SerializeField] private GameObject energyOrbPrefab;

    private EnemyHealth enemyHealth;
    private EnemyPolarity enemyPolarity;

    private void Awake()
    {
        enemyHealth = GetComponent<EnemyHealth>();
        enemyPolarity = GetComponent<EnemyPolarity>();
    }

    private void OnEnable()
    {
        enemyHealth.OnDeath += HandleDeath;
    }

    private void OnDisable()
    {
        enemyHealth.OnDeath -= HandleDeath;
    }

    private void HandleDeath()
    {
        if (enemyPolarity == null)
        {
            Debug.LogWarning("EnemyKillReward requires EnemyPolarity to know reward color.");
            return;
        }

        ElementColor color = enemyPolarity.CurrentColor;

        ScoreManager.Instance?.AddScore(killScore);
        PlayerEnergy.Instance?.AddEnergy(color, killEnergy);

        DropOrb(color);
    }

    private void DropOrb(ElementColor color)
    {
        if (energyOrbPrefab == null) return;

        GameObject orbObject = Instantiate(energyOrbPrefab, transform.position, Quaternion.identity);

        if (orbObject.TryGetComponent(out EnergyOrb orb))
        {
            orb.SetColor(color);
        }
    }
}