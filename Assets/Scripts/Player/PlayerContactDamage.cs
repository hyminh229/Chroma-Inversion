using UnityEngine;

public class PlayerContactDamage : MonoBehaviour
{
    private PlayerLife playerLife;
    private PlayerShooting playerShooting;

    private void Awake()
    {
        playerLife = GetComponent<PlayerLife>();
        playerShooting = GetComponent<PlayerShooting>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Đang kênh Mega Beam -> bỏ qua va chạm thường, tránh vừa bắn tia vừa
        // bị tính mất mạng khi Player chạy xuyên qua enemy trong lúc kênh.
        if (playerShooting != null && playerShooting.IsChanneling) return;

        if (collision.TryGetComponent(out EnemyController enemy))
        {
            playerLife.LoseLife();
            enemy.DestroyObject();
            Debug.Log("Player va chạm Enemy!");
        }
        else if (collision.TryGetComponent(out MeteorController meteor))
        {
            playerLife.LoseLife();
            Debug.Log("Player va chạm Meteor!");
        }
        else
        {
            BossController boss = collision.GetComponentInParent<BossController>();
            if (boss != null)
            {
                playerLife.LoseLife();
                Debug.Log("Player va chạm Boss!");
            }
        }
    }
}