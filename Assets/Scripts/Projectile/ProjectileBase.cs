using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public abstract class ProjectileBase : MonoBehaviour, IDestroyable
{
    [SerializeField] protected float speed = 10f;
    [SerializeField] protected int damage = 1;
    [SerializeField] protected SpriteRenderer spriteRenderer;
    [SerializeField] protected Sprite blueSprite;
    [SerializeField] protected Sprite redSprite;

    public ElementColor ColorType { get; private set; }
    public int Damage => damage;

    protected virtual void Awake()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }
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
    }

    public void SetDamage(int newDamage)
    {
        damage = newDamage;
    }

    private void ApplySprite(Sprite target)
    {
        if (spriteRenderer == null) return;

        if (target == null)
        {
            Debug.LogWarning(gameObject.name + ": chưa gán " + (ColorType == ElementColor.BLUE ? "Blue" : "Red") + " Sprite trên ProjectileBase.");
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