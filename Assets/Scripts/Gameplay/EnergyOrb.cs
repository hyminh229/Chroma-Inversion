using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class EnergyOrb : MonoBehaviour
{
    [SerializeField] private ElementColor color = ElementColor.BLUE;
    [SerializeField] private int energyAmount = 4;
    [SerializeField] private int scoreValue = 20;
    [SerializeField] private float fallSpeed = 1.2f;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Sprite blueSprite;
    [SerializeField] private Sprite redSprite;

    private void Awake()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        UpdateVisual();
    }

    private void Update()
    {
        // Rơi đều với tốc độ cố định — KHÔNG dùng trọng lực vật lý (Rigidbody2D
        // Gravity Scale phải để 0 trong prefab), tránh rơi tăng tốc mất kiểm soát.
        transform.Translate(Vector2.down * fallSpeed * Time.deltaTime);
    }

    public void SetColor(ElementColor newColor)
    {
        color = newColor;
        UpdateVisual();
    }

    private void UpdateVisual()
    {
        if (spriteRenderer == null) return;

        Sprite target = color == ElementColor.BLUE ? blueSprite : redSprite;

        if (target == null)
        {
            Debug.LogWarning(gameObject.name + ": chưa gán " + (color == ElementColor.BLUE ? "Blue" : "Red") + " Sprite trên EnergyOrb.");
            return;
        }

        spriteRenderer.sprite = target;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.TryGetComponent(out PlayerEnergy playerEnergy)) return;

        // PlayerColorController nằm trên GameObject con "Sprite", không cùng
        // GameObject với Collider2D của Player (root) — phải tìm xuống con.
        PlayerColorController playerColor = collision.GetComponentInChildren<PlayerColorController>();
        if (playerColor == null) return;

        // Sai màu thì KHÔNG hấp thụ được — orb vẫn còn nguyên đó, player phải
        // đổi màu đúng rồi mới nhặt được, giống logic EnemyBullet.
        if (playerColor.CurrentColor != color) return;

        playerEnergy.AddEnergy(color, energyAmount);
        ScoreManager.Instance?.AddScore(scoreValue);
        AudioManager.Instance?.PlayEnergyCollect();

        Debug.Log("EXP Orb collected! +" + energyAmount + " " + color + " Energy, +" + scoreValue + " Score.");

        Destroy(gameObject);
    }
}