using System;
using System.Collections;
using UnityEngine;

// Wave 3 — vài Enemy Shooter bay tự do (random trajectory), NGẮM và BẮN vào
// Player. Ít địch, mỗi con bắn lệch pha ngẫu nhiên (EnemyShooting vốn đã
// random timer + fireChance riêng từng con) nên không đồng loạt bắn cùng lúc.
public class ShooterFlutterWaveSpawner : MonoBehaviour, IWaveSpawner
{
    [Header("Formation")]
    [SerializeField] private GameObject enemyPrefab; // PHẢI có EnemyShooting với aimAtPlayer = true
    [SerializeField] private int enemyCount = 4;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 3f;

    [Header("Firing (mỗi con lệch pha quanh giá trị này)")]
    [SerializeField] private float fireCheckInterval = 1.5f;
    [SerializeField][Range(0f, 1f)] private float fireChance = 0.35f;

    [Header("Layout (tự tính theo Camera lúc runtime)")]
    [SerializeField] private float horizontalMargin = 1f;
    [SerializeField] private float topSpawnMargin = 1.5f;
    [SerializeField] private float spawnStagger = 0.4f;

    public event Action OnWaveCleared;

    private int aliveCount;

    public void StartWave()
    {
        StartCoroutine(RunWave());
    }

    private IEnumerator RunWave()
    {
        aliveCount = enemyCount;

        ScreenBoundsUtil.GetWorldBounds(out float rawMinX, out float rawMaxX, out _, out float topY);
        float minX = rawMinX + horizontalMargin;
        float maxX = rawMaxX - horizontalMargin;
        float spawnY = topY + topSpawnMargin;

        for (int i = 0; i < enemyCount; i++)
        {
            float x = UnityEngine.Random.Range(minX, maxX);
            SpawnOne(new Vector3(x, spawnY, 0f), minX, maxX);

            if (spawnStagger > 0f)
            {
                yield return new WaitForSeconds(spawnStagger);
            }
        }
    }

    private void SpawnOne(Vector3 position, float minX, float maxX)
    {
        if (enemyPrefab == null)
        {
            Debug.LogWarning("ShooterFlutterWaveSpawner is missing enemyPrefab.");
            return;
        }

        GameObject instance = Instantiate(enemyPrefab, position, Quaternion.identity);

        ElementColor color = UnityEngine.Random.value < 0.5f ? ElementColor.BLUE : ElementColor.RED;
        if (instance.TryGetComponent(out ChromaPolarityBase polarity))
        {
            polarity.SetColor(color);
        }

        if (instance.TryGetComponent(out EnemyController controller))
        {
            controller.ConfigureMovement(MovementPattern.RandomFlutter, moveSpeed, minX, maxX);
        }

        if (instance.TryGetComponent(out EnemyShooting shooting))
        {
            shooting.enabled = true;
            shooting.ConfigureFiring(fireCheckInterval, fireChance);
        }
        else
        {
            Debug.LogWarning("ShooterFlutterWaveSpawner: enemyPrefab thiếu EnemyShooting.");
        }

        if (instance.TryGetComponent(out EnemyHealth health))
        {
            health.OnDeath += HandleEnemyDeath;
        }
    }

    private void HandleEnemyDeath()
    {
        aliveCount--;

        if (aliveCount <= 0)
        {
            Debug.Log("Wave 3 cleared!");
            OnWaveCleared?.Invoke();
        }
    }
}