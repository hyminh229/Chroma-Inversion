using System;
using System.Collections;
using UnityEngine;

public class PlayerLife : MonoBehaviour
{
    [SerializeField] private int startingLives = 1;
    [Header("Invulnerability & Flashing")]
    [SerializeField] private float invulnerabilityDuration = 2f;
    [Tooltip("Thời gian mỗi nhịp nhấp nháy (giây)")]
    [SerializeField] private float flashInterval = 0.1f;
    [Tooltip("Độ trong suốt khi nhấp nháy (0 là ẩn hẳn, 0.2 là mờ ảo)")]
    [Range(0f, 0.8f)]
    [SerializeField] private float flashAlpha = 0.2f;

    [Header("Death VFX")]
    [Tooltip("Prefab hiệu ứng vụ nổ khi Player mất mạng / chết")]
    [SerializeField] private GameObject explosionPrefab;

    public int CurrentLives { get; private set; }
    public bool IsAlive { get; private set; }
    public bool IsInvulnerable { get; private set; }

    public event Action<int> OnLifeChanged;
    public event Action OnLifeLost;
    public event Action OnRespawn;
    public event Action OnGameOver;

    private Coroutine invulnerabilityCoroutine;
    private SpriteRenderer[] spriteRenderers;

    private void Awake()
    {
        CurrentLives = startingLives;
        IsAlive = true;
        CacheRenderers();
    }

    private void CacheRenderers()
    {
        if (spriteRenderers == null || spriteRenderers.Length == 0)
        {
            spriteRenderers = GetComponentsInChildren<SpriteRenderer>(true);
        }
    }

    // Bất kỳ va chạm/trúng đòn nào lọt qua Shield và không đang bất tử đều
    // trừ NGAY 1 mạng — không còn khái niệm "damage amount".
    public void LoseLife()
    {
        if (!IsAlive) return;
        if (IsInvulnerable) return;

        if (explosionPrefab != null)
        {
            Instantiate(explosionPrefab, transform.position, Quaternion.identity);
        }

        if (CurrentLives > 1)
        {
            CurrentLives--;
            AudioManager.Instance?.PlayPlayerDead();
            Debug.Log("Player mất 1 mạng. Còn lại: " + CurrentLives);
            OnLifeChanged?.Invoke(CurrentLives);
            OnLifeLost?.Invoke();

            if (invulnerabilityCoroutine != null)
            {
                StopCoroutine(invulnerabilityCoroutine);
            }
            invulnerabilityCoroutine = StartCoroutine(RespawnInvulnerability());
        }
        else
        {
            CurrentLives = 0;
            AudioManager.Instance?.PlayPlayerDead();
            Debug.Log("Player mất mạng cuối cùng. Còn lại: 0");
            OnLifeChanged?.Invoke(CurrentLives);
            OnLifeLost?.Invoke();
            Die();
        }
    }

    public void AddLife(int amount)
    {
        if (amount <= 0) return;

        CurrentLives += amount;
        AudioManager.Instance?.PlayLifePickup();
        Debug.Log("Nhặt Life! Còn lại: " + CurrentLives);
        OnLifeChanged?.Invoke(CurrentLives);
    }

    public void RestoreLives(int lives)
    {
        CurrentLives = Mathf.Max(0, lives);
        IsAlive = CurrentLives > 0;
        IsInvulnerable = false;
        SetRenderersAlpha(1f);
        Debug.Log("Player lives restored: " + CurrentLives);
        OnLifeChanged?.Invoke(CurrentLives);
    }

    private IEnumerator RespawnInvulnerability()
    {
        IsInvulnerable = true;
        CacheRenderers();

        float elapsed = 0f;
        bool isDimmed = false;

        while (elapsed < invulnerabilityDuration)
        {
            isDimmed = !isDimmed;
            SetRenderersAlpha(isDimmed ? flashAlpha : 1f);

            yield return new WaitForSeconds(flashInterval);
            elapsed += flashInterval;
        }

        // Hồi phục hoàn toàn sau khi hết thời gian bất tử
        SetRenderersAlpha(1f);
        IsInvulnerable = false;
        invulnerabilityCoroutine = null;
        OnRespawn?.Invoke();
    }

    private void SetRenderersAlpha(float alpha)
    {
        if (spriteRenderers == null) return;

        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            if (spriteRenderers[i] != null)
            {
                Color color = spriteRenderers[i].color;
                color.a = alpha;
                spriteRenderers[i].color = color;
            }
        }
    }

    private void Die()
    {
        if (!IsAlive) return;
        IsAlive = false;

        if (invulnerabilityCoroutine != null)
        {
            StopCoroutine(invulnerabilityCoroutine);
            invulnerabilityCoroutine = null;
        }
        IsInvulnerable = false;
        SetRenderersAlpha(1f);

        Debug.Log("Player Died! Game Over.");
        OnGameOver?.Invoke();
    }
}