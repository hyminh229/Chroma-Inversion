using UnityEngine;

[RequireComponent(typeof(EnemyHealth))]
public class EnemyPowerUpDrop : MonoBehaviour
{
    [SerializeField][Range(0f, 1f)] private float dropChance = 0.08f;
    [SerializeField] private GameObject[] powerUpPrefabs;

    private EnemyHealth enemyHealth;

    private void Awake()
    {
        enemyHealth = GetComponent<EnemyHealth>();
    }

    private void OnEnable()
    {
        enemyHealth.OnDeath += TryDropPowerUp;
    }

    private void OnDisable()
    {
        enemyHealth.OnDeath -= TryDropPowerUp;
    }

    private void TryDropPowerUp()
    {
        if (powerUpPrefabs == null || powerUpPrefabs.Length == 0) return;
        if (Random.value > dropChance) return;

        GameObject prefab = powerUpPrefabs[Random.Range(0, powerUpPrefabs.Length)];
        Instantiate(prefab, transform.position, Quaternion.identity);
    }
}