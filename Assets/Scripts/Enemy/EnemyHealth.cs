using System;
using UnityEngine;

public class EnemyHealth : MonoBehaviour, IDamageable
{
    [SerializeField] private int maxHealth = 5;
    private int currentHealth;
    public bool IsAlive { get; private set; }

    public event Action OnDeath;

    private void Awake()
    {
        currentHealth = maxHealth;
        IsAlive = true;
    }

    public void TakeDamage(int damage)
    {
        if (!IsAlive) return;
        if (damage <= 0) return;

        currentHealth -= damage;
        AudioManager.Instance?.PlayEnemyHit();
        Debug.Log(gameObject.name + " took " + damage + " damage. HP: " + currentHealth + "/" + maxHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    // Dùng khi enemy cần "chết" ngay lập tức mà không qua damage (VD: bay ra khỏi
    // DestroyZone, bị Player ramming) — vẫn phải fire OnDeath để wave đếm đúng.
    public void Kill()
    {
        if (!IsAlive) return;
        currentHealth = 0;
        Die();
    }

    private void Die()
    {
        IsAlive = false;
        currentHealth = 0;

        AudioManager.Instance?.PlayEnemyDead();
        Debug.Log(gameObject.name + " destroyed.");

        OnDeath?.Invoke();

        Destroy(gameObject);
    }
}