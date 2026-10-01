using System;
using UnityEngine;

public class MeteorHealth : MonoBehaviour, IDamageable
{
    [SerializeField] private int maxHealth = 3;
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
        Debug.Log(gameObject.name + " took " + damage + " damage. HP: " + currentHealth + "/" + maxHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    // Dùng khi Meteor cần chết ngay lập tức (VD: tự nổ AOE), bỏ qua HP còn lại.
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

        AudioManager.Instance?.PlayMeteorExplosion();
        Debug.Log(gameObject.name + " destroyed.");

        OnDeath?.Invoke();

        Destroy(gameObject);
    }
}