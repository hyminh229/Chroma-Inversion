using System;
using UnityEngine;

public class MeteorHealth : MonoBehaviour, IDamageable
{
    [SerializeField] private int maxHealth = 3;

    [Header("Death VFX")]
    [SerializeField] private GameObject explosionPrefab;
    [Tooltip("Tỉ lệ kích thước vụ nổ cơ bản")]
    [SerializeField] private float explosionScale = 1f;

    private int currentHealth;
    public bool IsAlive { get; private set; }

    public event Action OnDeath;

#if UNITY_EDITOR
    private void Reset()
    {
        if (explosionPrefab == null)
        {
            explosionPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/ExplosionVFX.prefab");
        }
    }
#endif

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

        SpawnExplosionVFX();

        AudioManager.Instance?.PlayMeteorExplosion();
        Debug.Log(gameObject.name + " destroyed.");

        OnDeath?.Invoke();

        Destroy(gameObject);
    }

    private void SpawnExplosionVFX()
    {
        GameObject prefabToSpawn = explosionPrefab;
#if UNITY_EDITOR
        if (prefabToSpawn == null)
        {
            prefabToSpawn = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/ExplosionVFX.prefab");
        }
#endif
        if (prefabToSpawn != null)
        {
            GameObject vfx = Instantiate(prefabToSpawn, transform.position, Quaternion.identity);
            float finalScale = explosionScale;
            if (TryGetComponent(out MeteorController controller) && controller.Size == MeteorSize.LARGE)
            {
                finalScale *= 2.5f;
            }
            vfx.transform.localScale = Vector3.one * finalScale;
        }
    }
}