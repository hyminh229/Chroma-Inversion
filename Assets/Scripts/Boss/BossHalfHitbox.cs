using UnityEngine;

// Gắn trên từng box con (TopHalf/BottomHalf). Mỗi box giữ 1 màu CỐ ĐỊNH, không
// đổi theo runtime — việc "đổi phe" đến từ BossController xoay 180° cả cụm,
// khiến box này đổi VỊ TRÍ trên màn hình (trên <-> dưới). Vì màu không bao giờ
// đổi lúc chạy, chỉ cần 1 sprite duy nhất khớp assignedColor — không cần cặp
// blueSprite/redSprite như ChromaPolarityBase/ProjectileBase (những cái đó
// switch màu lúc runtime nên phải cache sẵn cả 2).
public class BossHalfHitbox : MonoBehaviour
{
    [SerializeField] private ElementColor assignedColor = ElementColor.BLUE;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Sprite halfSprite;

    private BossHealth bossHealth;

    private void Awake()
    {
        bossHealth = GetComponentInParent<BossHealth>();

        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        if (spriteRenderer != null && halfSprite != null)
        {
            spriteRenderer.sprite = halfSprite;
            spriteRenderer.color = Color.white;
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