using UnityEngine;

public class Bullet : ProjectileBase
{
    [SerializeField] private bool isPlayerBullet = true;
    public bool IsPlayerBullet => isPlayerBullet;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!isPlayerBullet) return;
        if (!collision.TryGetComponent(out EnemyHealth enemyHealth)) return;

        if (collision.TryGetComponent(out EnemyPolarity enemyPolarity))
        {
            bool sameColor = ColorType == enemyPolarity.CurrentColor;

            if (sameColor)
            {
                enemyHealth.TakeDamage(damage);
                Debug.Log("Correct color! Damage = " + damage);
            }
            else
            {
                Debug.Log("Wrong color — no damage.");
            }
        }
        else
        {
            // Phòng hờ enemy thiếu EnemyPolarity — không để vô tình thành bất tử.
            enemyHealth.TakeDamage(damage);
        }

        DestroyObject();
    }
}