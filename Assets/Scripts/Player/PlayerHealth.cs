using UnityEngine;

public class PlayerHealth : MonoBehaviour, IDamageable
{
    [SerializeField] private int maxHealth = 10;

    private int currentHealth;
    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;

    public bool IsAlive { get; private set; }

    private void Awake()
    {
        currentHealth = maxHealth;
        IsAlive = true;
    }

    public void TakeDamage(int damage)
    {
        if (!IsAlive)
        {
            return;
        }

        if (damage <= 0)
        {
            return;
        }

        currentHealth -= damage;

        Debug.Log("Player took " + damage + " damage. HP: " + currentHealth + "/" + maxHealth);
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        IsAlive = false;

        currentHealth = 0;

        Debug.Log("Player Died!");

        // Game Over sẽ xử lý ở Phase 8.
    }

    public void Heal(int amount)
    {
        if (!IsAlive)
        {
            return;
        }

        if (amount <= 0)
        {
            return;
        }

        currentHealth += amount;

        if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }

        Debug.Log("Player healed. HP: " + currentHealth + "/" + maxHealth);
    }

    public void RestoreHealth(int health)
    {
        currentHealth = Mathf.Clamp(health, 1, maxHealth);
        IsAlive = currentHealth > 0;
        Debug.Log("Player health restored. HP: " + currentHealth + "/" + maxHealth);
    }
}