using UnityEngine;

// Gắn trên từng box con (TopHalf/BottomHalf). Mỗi box giữ 1 màu CỐ ĐỊNH, không
// đổi theo runtime — việc "đổi phe" đến từ BossController xoay 180° cả cụm,
// khiến box này đổi VỊ TRÍ trên màn hình (trên <-> dưới). Tự xử lý va chạm của
// chính mình, đúng pattern Bullet/EnemyBullet/MeteorController đã dùng.
public class BossHalfHitbox : MonoBehaviour
{
    [SerializeField] private ElementColor assignedColor = ElementColor.BLUE;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Color blueColor = Color.blue;
    [SerializeField] private Color redColor = Color.red;

    private BossHealth bossHealth;

    private void Awake()
    {
        bossHealth = GetComponentInParent<BossHealth>();

        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        if (spriteRenderer != null)
        {
            spriteRenderer.color = assignedColor == ElementColor.BLUE ? blueColor : redColor;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.TryGetComponent(out Bullet bullet)) return;
        if (!bullet.IsPlayerBullet) return;

        if (bullet.ColorType == assignedColor)
        {
            bossHealth.TakeDamage(bullet.Damage);
        }
        else
        {
            Debug.Log("Boss: sai màu, không gây damage.");
        }

        bullet.DestroyObject();
    }
}