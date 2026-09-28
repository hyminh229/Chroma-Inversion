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

    // Bất kỳ va chạm/trúng đòn nào lọt qua Shield và không đang bất tử đều
    // trừ NGAY 1 mạng — không còn khái niệm "damage amount".
    public void LoseLife()
    {
        if (!IsAlive) return;
        if (IsInvulnerable) return;

        if (CurrentLives > 0)
        {
            CurrentLives--;
            Debug.Log("Player mất 1 mạng. Còn lại: " + CurrentLives);
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
    }
}