using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public abstract class ProjectileBase : MonoBehaviour, IDestroyable
{
    [SerializeField] protected float speed = 10f;
    [SerializeField] protected int damage = 1;
    [SerializeField] protected SpriteRenderer spriteRenderer;
    [SerializeField] protected Sprite blueSprite;
    [SerializeField] protected Sprite redSprite;

    [Header("Animation (Optional)")]
    [SerializeField] protected Animator animator;
    [SerializeField] protected string blueAnimationState = "player_blue_bullet";
    [SerializeField] protected string redAnimationState = "player_bullet";

    public ElementColor ColorType { get; private set; }
    public int Damage => damage;

    protected virtual void Awake()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }
        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }
    }

    protected virtual void OnEnable()
    {
        ApplyAnimation(ColorType);
    }

    protected virtual void Update()
    {
        Move();
    }

    protected virtual void Move()
    {
        transform.Translate(Vector2.up * speed * Time.deltaTime);
    }

    public void SetColor(ElementColor newColor)
    {
        ColorType = newColor;
        ApplySprite(ColorType == ElementColor.BLUE ? blueSprite : redSprite);
        ApplyAnimation(ColorType);
    }

    public void SetDamage(int newDamage)
    {
        damage = newDamage;
    }

    // Cho các attack pattern (Boss Triple Shot, Fan Shot...) chỉnh tốc độ đạn
    // riêng qua Inspector thay vì phải sửa tốc độ mặc định trên prefab gốc.
    public void SetSpeed(float newSpeed)
    {
        speed = newSpeed;
    }

    private void ApplySprite(Sprite target)
    {
        if (spriteRenderer == null) return;

        if (target == null)
        {
            // Nếu có Animator thì sprite do animation quản lý, không cần cảnh báo thiếu sprite tĩnh
            if (animator == null)
            {
                Debug.LogWarning(gameObject.name + ": chưa gán " + (ColorType == ElementColor.BLUE ? "Blue" : "Red") + " Sprite trên ProjectileBase.");
            }
            return;
        }

        if (spriteRenderer.sprite != null)
        {
            Vector3 oldCenter = spriteRenderer.sprite.bounds.center;
            Vector3 newCenter = target.bounds.center;
            spriteRenderer.transform.localPosition += (oldCenter - newCenter);
        }

        spriteRenderer.sprite = target;
    }

    private void ApplyAnimation(ElementColor color)
    {
        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }

        if (animator == null || !animator.isActiveAndEnabled) return;

        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }
        if (spriteRenderer != null)
        {
            // Tránh việc SpriteRenderer bị lưu tint đỏ khiến animation đạn xanh bị tối/đen
            spriteRenderer.color = Color.white;
        }

        string primary = (color == ElementColor.BLUE) ? blueAnimationState : redAnimationState;
        string[] candidates = (color == ElementColor.BLUE)
            ? new[] { primary, "player_blue_bullet", "player_blue", "Blue" }
            : new[] { primary, "player_bullet", "player_red_bullet", "player_red", "Red" };

        foreach (string candidate in candidates)
        {
            if (!string.IsNullOrEmpty(candidate) && animator.HasState(0, Animator.StringToHash(candidate)))
            {
                animator.Play(candidate, 0, 0f);
                return;
            }
        }
    }

    public virtual void DestroyObject()
    {
        if (ObjectPooler.Instance != null)
        {
            ObjectPooler.Instance.Despawn(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}