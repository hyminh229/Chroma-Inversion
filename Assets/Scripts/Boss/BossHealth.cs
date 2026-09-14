using System;
using UnityEngine;

public class BossHealth : MonoBehaviour, IDamageable
{
    [SerializeField] private int maxHealth = 200;
    private int currentHealth;

    public bool IsAlive { get; private set; }
    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;

    public event Action OnDeath;
    public event Action OnDamaged;

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
        Debug.Log("Boss took " + damage + " damage. HP: " + currentHealth + "/" + maxHealth);

        OnDamaged?.Invoke();

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        IsAlive = false;
        currentHealth = 0;

        Debug.Log("Boss destroyed!");
        OnDeath?.Invoke();
        Destroy(gameObject);
    }
}