using System;
using System.Collections;
using UnityEngine;

public class PlayerLife : MonoBehaviour
{
    [SerializeField] private int startingLives = 1;
    [SerializeField] private float invulnerabilityDuration = 2f;

    public int CurrentLives { get; private set; }
    public bool IsAlive { get; private set; }
    public bool IsInvulnerable { get; private set; }

    public event Action<int> OnLifeChanged;
    public event Action OnLifeLost;
    public event Action OnRespawn;
    public event Action OnGameOver;

    private Coroutine invulnerabilityCoroutine;

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
        Debug.Log("Player lives restored: " + CurrentLives);
        OnLifeChanged?.Invoke(CurrentLives);
    }

    private IEnumerator RespawnInvulnerability()
    {
        IsInvulnerable = true;
        yield return new WaitForSeconds(invulnerabilityDuration);
        IsInvulnerable = false;
        invulnerabilityCoroutine = null;
        OnRespawn?.Invoke();
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

        Debug.Log("Player Died! Game Over.");
        OnGameOver?.Invoke();
    }
}