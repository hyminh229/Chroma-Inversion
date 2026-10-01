using UnityEngine;

/// <summary>
/// Handles infinite scrolling for background elements.
/// Optimized for Tiled SpriteRenderers (seamless looping based on tile dimensions)
/// as well as standard 2D background transforms.
/// </summary>
public class BackgroundScroller : MonoBehaviour
{
    [Header("Scroll Settings")]
    [Tooltip("Tốc độ cuộn của background (đơn vị: units/giây)")]
    [SerializeField] private float scrollSpeed = 2f;

    [Tooltip("Hướng cuộn của background (mặc định cuộn xuống dưới)")]
    [SerializeField] private Vector2 scrollDirection = Vector2.down;

    [Header("Tile Settings (Tùy chọn)")]
    [Tooltip("Tự động tính chiều cao/rộng của tile từ SpriteRenderer nếu có")]
    [SerializeField] private bool autoCalculateTileSize = true;

    [Tooltip("Kích thước chu kỳ lặp nếu nhập thủ công (hoặc khi không có SpriteRenderer)")]
    [SerializeField] private Vector2 customTileSize = new Vector2(15.36f, 10.24f);

    private Vector3 startPosition;
    private float tileHeight;
    private float tileWidth;
    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        startPosition = transform.position;
        spriteRenderer = GetComponent<SpriteRenderer>();
        CalculateTileSize();
    }

    private void CalculateTileSize()
    {
        if (autoCalculateTileSize && spriteRenderer != null && spriteRenderer.sprite != null)
        {
            Sprite s = spriteRenderer.sprite;
            float ppu = s.pixelsPerUnit > 0 ? s.pixelsPerUnit : 100f;
            tileWidth = (s.rect.width / ppu) * Mathf.Abs(transform.lossyScale.x);
            tileHeight = (s.rect.height / ppu) * Mathf.Abs(transform.lossyScale.y);
        }
        else
        {
            tileWidth = customTileSize.x;
            tileHeight = customTileSize.y;
        }

        // Đảm bảo không bị chia/modulo cho 0 nếu kích thước không hợp lệ
        if (tileWidth <= 0.01f) tileWidth = 10f;
        if (tileHeight <= 0.01f) tileHeight = 10f;
    }

    private void Update()
    {
        // Di chuyển background theo hướng và tốc độ mượt mà
        Vector3 movement = (Vector3)(scrollDirection.normalized * (scrollSpeed * Time.deltaTime));
        transform.position += movement;

        // Vòng lặp vị trí theo trục Y (Seamless Loop)
        if (scrollDirection.y < 0 && transform.position.y <= startPosition.y - tileHeight)
        {
            transform.position += Vector3.up * tileHeight;
        }
        else if (scrollDirection.y > 0 && transform.position.y >= startPosition.y + tileHeight)
        {
            transform.position += Vector3.down * tileHeight;
        }

        // Vòng lặp vị trí theo trục X (nếu cuộn ngang hoặc chéo)
        if (scrollDirection.x < 0 && transform.position.x <= startPosition.x - tileWidth)
        {
            transform.position += Vector3.right * tileWidth;
        }
        else if (scrollDirection.x > 0 && transform.position.x >= startPosition.x + tileWidth)
        {
            transform.position += Vector3.left * tileWidth;
        }
    }

    /// <summary>
    /// Cho phép điều chỉnh tốc độ cuộn từ bên ngoài (ví dụ tăng tốc khi hyperdrive / boss đến).
    /// </summary>
    public void SetScrollSpeed(float speed)
    {
        scrollSpeed = speed;
    }

    public float GetScrollSpeed() => scrollSpeed;
}
