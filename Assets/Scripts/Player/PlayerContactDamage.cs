using UnityEngine;

public class PlayerContactDamage : MonoBehaviour
{
    [SerializeField] private int enemyContactDamage = 1;
    [SerializeField] private int meteorContactDamage = 1;
    [SerializeField] private int bossContactDamage = 1;

    private PlayerLife playerLife;

    private void Awake()
    {
        playerLife = GetComponent<PlayerLife>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out EnemyController enemy))
        {
            playerLife.TakeDamage(enemyContactDamage);
            enemy.DestroyObject();

            Debug.Log("Player va chạm Enemy!");
        }
        else if (collision.TryGetComponent(out MeteorController meteor))
        {
            playerLife.TakeDamage(meteorContactDamage);

            Debug.Log("Player va chạm Meteor!");
        }
        else
        {
            BossController boss = collision.GetComponentInParent<BossController>();
            if (boss != null)
            {
                playerLife.TakeDamage(bossContactDamage);
                Debug.Log("Player va chạm Boss!");
            }
        }
    }
}