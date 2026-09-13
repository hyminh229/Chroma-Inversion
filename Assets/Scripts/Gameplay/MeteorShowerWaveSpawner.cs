using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Dùng chung cho cả 2 kiểu mưa thiên thạch — chéo (Wave 4) và thẳng đứng
// tốc độ cao (Wave 5) — chỉ khác Direction/Speed/Count khai báo trên từng
// GameObject riêng, không cần 2 script khác nhau.
public class MeteorShowerWaveSpawner : MonoBehaviour, IWaveSpawner
{
    [Header("Formation")]
    [SerializeField] private List<GameObject> meteorPrefabs = new List<GameObject>();
    [SerializeField] private int meteorCount = 20;
    [SerializeField] private float showerDuration = 5f;

    [Header("Trajectory")]
    [SerializeField] private Vector2 direction = new Vector2(0.6f, -1f);
    [SerializeField] private float moveSpeed = 2.5f;
    [SerializeField][Range(0f, 0.5f)] private float speedVariance = 0.25f;

    [Header("Spawn Area (tự tính theo Camera lúc runtime)")]
    [Tooltip("Tỉ lệ bề rộng màn hình dùng làm vùng xuất phát, tính từ mép trái. 1 = phủ toàn màn hình (Wave thẳng đứng); nhỏ hơn (VD 0.6) = dồn về góc trái như Meteor Shower chéo.")]
    [Range(0.1f, 1f)][SerializeField] private float spawnSpreadRatio = 1f;
    [SerializeField] private float topSpawnMargin = 1.5f;

    public event Action OnWaveCleared;

    private int aliveCount;

    public void StartWave()
    {
        StartCoroutine(RunWave());
    }

    private IEnumerator RunWave()
    {
        aliveCount = meteorCount;
        float interval = meteorCount > 0 ? showerDuration / meteorCount : 0f;

        ScreenBoundsUtil.GetWorldBounds(out float rawMinX, out float rawMaxX, out _, out float topY);
        float fullWidth = rawMaxX - rawMinX;
        float minX = rawMinX;
        float maxX = minX + fullWidth * spawnSpreadRatio;
        float spawnY = topY + topSpawnMargin;

        for (int i = 0; i < meteorCount; i++)
        {
            float x = UnityEngine.Random.Range(minX, maxX);
            SpawnOne(new Vector3(x, spawnY, 0f));
            yield return new WaitForSeconds(interval);
        }
    }

    private void SpawnOne(Vector3 position)
    {
        if (meteorPrefabs == null || meteorPrefabs.Count == 0)
        {
            Debug.LogWarning("MeteorShowerWaveSpawner is missing meteorPrefabs.");
            return;
        }

        GameObject prefab = meteorPrefabs[UnityEngine.Random.Range(0, meteorPrefabs.Count)];
        GameObject instance = Instantiate(prefab, position, Quaternion.identity);

        float randomSpeed = moveSpeed * UnityEngine.Random.Range(1f - speedVariance, 1f + speedVariance);

        if (instance.TryGetComponent(out MeteorController controller))
        {
            controller.ConfigureMovement(direction, randomSpeed);
        }

        if (instance.TryGetComponent(out MeteorHealth health))
        {
            health.OnDeath += HandleMeteorCleared;
        }
        else
        {
            HandleMeteorCleared();
        }
    }

    private void HandleMeteorCleared()
    {
        aliveCount--;

        if (aliveCount <= 0)
        {
            Debug.Log(gameObject.name + " cleared!");
            OnWaveCleared?.Invoke();
        }
    }
}