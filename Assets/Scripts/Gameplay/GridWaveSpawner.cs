using System;
using System.Collections;
using UnityEngine;

// Script DUY NHẤT cho 1 wave dạng lưới kiểu "Chicken Invasion". Giờ implement
// IWaveSpawner để WaveSequencer điều khiển thời điểm bắt đầu, không tự chạy
// trong Start() nữa (vì giờ nó là Wave 1 trong chuỗi, không đứng riêng lẻ).
public class GridWaveSpawner : MonoBehaviour, IWaveSpawner
{
    [Header("Formation")]
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private int rows = 5;
    [SerializeField] private int columns = 8;

    [Header("Layout (tự tính theo Camera lúc runtime — không phải toạ độ tuyệt đối)")]
    [SerializeField] private float horizontalMargin = 1f;
    [SerializeField] private float topMargin = 1.2f;
    [SerializeField] private float rowSpacing = 1f;

    [Header("Timing")]
    [Tooltip("Khoảng nghỉ giữa lúc 1 hàng xuất hiện xong và hàng kế tiếp bắt đầu.")]
    [SerializeField] private float rowRevealDelay = 0.35f;

    [Header("Shooting (bỏ qua nếu enemyPrefab không có EnemyShooting)")]
    [Range(0f, 1f)][SerializeField] private float shooterChance = 0.25f;

    public event Action OnWaveCleared;

    private int aliveCount;

    public void StartWave()
    {
        StartCoroutine(SpawnGrid());
    }

    private IEnumerator SpawnGrid()
    {
        aliveCount = rows * columns;

        ComputeLayout(out float startX, out float colSpacing, out float topRowY);

        for (int row = 0; row < rows; row++)
        {
            ElementColor rowColor = GetRandomColor();
            float y = topRowY - row * rowSpacing;

            for (int col = 0; col < columns; col++)
            {
                float x = startX + col * colSpacing;
                SpawnOne(new Vector3(x, y, 0f), rowColor);
            }

            if (row < rows - 1 && rowRevealDelay > 0f)
            {
                yield return new WaitForSeconds(rowRevealDelay);
            }
        }
    }

    private void ComputeLayout(out float startX, out float colSpacing, out float topRowY)
    {
        ScreenBoundsUtil.GetWorldBounds(out float minX, out float maxX, out _, out float topY);

        float usableWidth = (maxX - minX) - 2f * horizontalMargin;
        colSpacing = columns > 1 ? usableWidth / (columns - 1) : 0f;
        startX = minX + horizontalMargin;
        topRowY = topY - topMargin;
    }

    private void SpawnOne(Vector3 position, ElementColor color)
    {
        if (enemyPrefab == null)
        {
            Debug.LogWarning("GridWaveSpawner is missing enemyPrefab.");
            return;
        }

        GameObject instance = Instantiate(enemyPrefab, position, Quaternion.identity);

        if (instance.TryGetComponent(out ChromaPolarityBase polarity))
        {
            polarity.SetColor(color);
        }

        if (instance.TryGetComponent(out EnemyController controller))
        {
            controller.ConfigureMovement(MovementPattern.Stationary, 0f, float.NegativeInfinity, float.PositiveInfinity);
        }

        if (instance.TryGetComponent(out EnemyShooting shooting))
        {
            shooting.enabled = UnityEngine.Random.value < shooterChance;
        }

        if (instance.TryGetComponent(out EnemyHealth health))
        {
            health.OnDeath += HandleEnemyDeath;
        }
        else
        {
            Debug.LogWarning("GridWaveSpawner: enemyPrefab thiếu EnemyHealth, không đếm được khi chết.");
        }
    }

    private void HandleEnemyDeath()
    {
        aliveCount--;

        if (aliveCount <= 0)
        {
            Debug.Log("Wave 1 cleared!");
            OnWaveCleared?.Invoke();
        }
    }

    private ElementColor GetRandomColor()
    {
        return UnityEngine.Random.value < 0.5f ? ElementColor.BLUE : ElementColor.RED;
    }
}