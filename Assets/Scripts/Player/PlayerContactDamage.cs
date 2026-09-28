using UnityEngine;

public class PlayerContactDamage : MonoBehaviour
{
    private PlayerLife playerLife;

    private void Awake()
    {
        playerLife = GetComponent<PlayerLife>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
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