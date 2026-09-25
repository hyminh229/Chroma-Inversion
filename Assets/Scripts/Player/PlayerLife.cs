using System.Collections;
using UnityEngine;

public class PlayerLife : MonoBehaviour
{
    [SerializeField] private int startingLives = 1;
    [SerializeField] private float invulnerabilityDuration = 2f;

    public int CurrentLives { get; private set; }
    public bool IsAlive { get; private set; }
    public bool IsInvulnerable { get; private set; }

    private void Awake()
    {
        CurrentLives = startingLives;
        IsAlive = true;
    }

    // Giữ tên TakeDamage để không phải sửa lại mọi nơi đang gọi (EnemyBullet,
    // MeteorController, PlayerContactDamage...) — nhưng bản chất không còn
    // trừ dần theo "damage", bất kỳ hit nào lọt qua (không bị Shield chặn,
    // không đang bất tử) đều tính là mất NGAY 1 mạng.
    public void TakeDamage(int damage)
    {
        if (!IsAlive) return;
        if (IsInvulnerable) return;
        if (damage <= 0) return;

        if (CurrentLives > 0)
        {
            CurrentLives--;
            Debug.Log("Player mất 1 mạng do va chạm/trúng đạn. Còn lại: " + CurrentLives);
            StartCoroutine(RespawnInvulnerability());
        }
        else
        {
            Die();
        }
    }

    public void AddLife(int amount)
    {
        if (amount <= 0) return;

        CurrentLives += amount;
        Debug.Log("Nhặt Life! Còn lại: " + CurrentLives);
    }
    public void RestoreLives(int lives)
    {
        CurrentLives = Mathf.Max(0, lives);
        IsAlive = true;
        IsInvulnerable = false;
        Debug.Log("Player lives restored: " + CurrentLives);
    }
    private IEnumerator RespawnInvulnerability()
    {
        IsInvulnerable = true;
        yield return new WaitForSeconds(invulnerabilityDuration);
        IsInvulnerable = false;
    }

    private void Die()
    {
        IsAlive = false;

        Debug.Log("Player Died! Game Over.");
        // Game Over UI xử lý ở Phase 8.
    }
}