using System;
using System.Collections;
using UnityEngine;

// Wave 2 — vài con bay tự do khắp màn hình (RandomFlutter có sẵn trên
// EnemyController), số lượng tăng dần theo thời gian, KHÔNG bắn — chỉ đe doạ
// bằng va chạm trực tiếp (PlayerContactDamage đã xử lý sẵn).
public class RandomFlutterWaveSpawner : MonoBehaviour, IWaveSpawner
{
    [Header("Formation")]
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private int startCount = 5;
    [SerializeField] private int maxCount = 8;
    [SerializeField] private float growInterval = 4f;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 3f;

    [Header("Layout (tự tính theo Camera lúc runtime)")]
    [SerializeField] private float horizontalMargin = 1f;
    [SerializeField] private float topSpawnMargin = 1.5f;

    public event Action OnWaveCleared;

    private int aliveCount;
    private int spawnedCount;

    public void StartWave()
    {
        StartCoroutine(RunWave());
    }

    private IEnumerator RunWave()
    {
        SpawnBatch(startCount);

        while (spawnedCount < maxCount)
        {
            yield return new WaitForSeconds(growInterval);
            SpawnBatch(1);
        }
    }

    private void SpawnBatch(int count)
    {
        ScreenBoundsUtil.GetWorldBounds(out float rawMinX, out float rawMaxX, out _, out float topY);
        float minX = rawMinX + horizontalMargin;
        float maxX = rawMaxX - horizontalMargin;
        float spawnY = topY + topSpawnMargin;

        for (int i = 0; i < count; i++)
        {
            float x = UnityEngine.Random.Range(minX, maxX);
            SpawnOne(new Vector3(x, spawnY, 0f), minX, maxX);
        }
    }

    private void SpawnOne(Vector3 position, float minX, float maxX)
    {
        if (enemyPrefab == null)
        {
            Debug.LogWarning("RandomFlutterWaveSpawner is missing enemyPrefab.");
            return;
        }

        GameObject instance = Instantiate(enemyPrefab, position, Quaternion.identity);
        spawnedCount++;
        aliveCount++;

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
            shooting.enabled = false;
        }

        if (instance.TryGetComponent(out EnemyHealth health))
        {
            health.OnDeath += HandleEnemyDeath;
        }
    }

    private void HandleEnemyDeath()
    {
        aliveCount--;

        if (aliveCount <= 0 && spawnedCount >= maxCount)
        {
            Debug.Log("Wave 2 cleared!");
            OnWaveCleared?.Invoke();
        }
    }
}