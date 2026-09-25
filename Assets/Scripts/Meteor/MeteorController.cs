using UnityEngine;

[RequireComponent(typeof(MeteorHealth))]
public class MeteorController : MonoBehaviour, IDestroyable
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 1.5f;
    [SerializeField] private Vector2 moveDirection = Vector2.down;

    [Header("Explosion khi chết (chỉ dùng khi meteorSize = LARGE)")]
    [SerializeField] private MeteorSize meteorSize = MeteorSize.SMALL;
    [SerializeField] private float explosionRadius = 2f;
    [SerializeField] private int explosionDamage = 1;

    private MeteorHealth meteorHealth;

    private void Awake()
    {
        meteorHealth = GetComponent<MeteorHealth>();
    }

    private void OnEnable()
    {
        meteorHealth.OnDeath += HandleDeath;
    }

    private void OnDisable()
    {
        meteorHealth.OnDeath -= HandleDeath;
    }

    public void ConfigureMovement(Vector2 newDirection, float newSpeed)
    {
        moveDirection = newDirection.sqrMagnitude > 0.0001f ? newDirection.normalized : Vector2.down;
        moveSpeed = newSpeed;
    }

    private void Update()
    {
        transform.Translate(moveDirection * moveSpeed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.TryGetComponent(out Bullet bullet)) return;
        if (!bullet.IsPlayerBullet) return;

        meteorHealth.TakeDamage(bullet.Damage);
        bullet.DestroyObject();
    }

    private void HandleDeath()
    {
        if (meteorSize != MeteorSize.LARGE) return;

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, explosionRadius);

        foreach (Collider2D hit in hits)
        {
            if (hit.TryGetComponent(out PlayerLife playerLife))
            {
                playerLife.TakeDamage(explosionDamage);
                Debug.Log("Meteor exploded! Player mất 1 mạng (nếu không bất tử).");
            }
        }
    }

    public void DestroyObject()
    {
        meteorHealth.Kill();
    }
}