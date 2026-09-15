using UnityEngine;

public class PlayerContactDamage : MonoBehaviour
{
    [SerializeField] private int enemyContactDamage = 2;
    [SerializeField] private int meteorContactDamage = 3;
    [SerializeField] private int bossContactDamage = 3;

    private PlayerHealth playerHealth;

    private void Awake()
    {
        playerHealth = GetComponent<PlayerHealth>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out EnemyController enemy))
        {
            playerHealth.TakeDamage(enemyContactDamage);
            enemy.DestroyObject();

            Debug.Log("Player va chạm Enemy! -" + enemyContactDamage + " HP.");
        }
        else if (collision.TryGetComponent(out MeteorController meteor))
        {
            playerHealth.TakeDamage(meteorContactDamage);

            Debug.Log("Player va chạm Meteor! -" + meteorContactDamage + " HP.");
        }
        else
        {
            // Boss không còn Collider2D trên chính root (đã chuyển xuống 2 box
            // con TopHalf/BottomHalf) nên phải tìm ngược lên bằng GetComponentInParent.
            BossController boss = collision.GetComponentInParent<BossController>();
            if (boss != null)
            {
                playerHealth.TakeDamage(bossContactDamage);
                Debug.Log("Player va chạm Boss! -" + bossContactDamage + " HP.");
            }
        }
    }
}