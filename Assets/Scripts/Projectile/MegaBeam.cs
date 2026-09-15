using UnityEngine;

public class MegaBeam : MonoBehaviour, IDestroyable
{
    [SerializeField] private float beamDuration = 1f;
    [SerializeField] private int damage = 10;

    private PlayerShooting owner;

    public void Init(PlayerShooting shootingOwner)
    {
        owner = shootingOwner;
    }

    private void Start()
    {
        Invoke(nameof(DestroyObject), beamDuration);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out EnemyHealth enemyHealth))
        {
            enemyHealth.TakeDamage(damage);
            return;
        }

        // Cùng lý do như PlayerContactDamage — Boss hết Collider2D ở root rồi.
        BossHealth bossHealth = collision.GetComponentInParent<BossHealth>();
        if (bossHealth != null)
        {
            bossHealth.TakeDamage(damage);
        }
    }

    public void DestroyObject()
    {
        owner?.EndChanneling();
        Destroy(gameObject);
    }
}