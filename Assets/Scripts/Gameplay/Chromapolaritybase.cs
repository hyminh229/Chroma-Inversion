using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public abstract class ChromaPolarityBase : MonoBehaviour
{
    [Header("Chroma Polarity")]
    [SerializeField] protected ElementColor startingColor = ElementColor.BLUE;
    [SerializeField] protected SpriteRenderer spriteRenderer;
    [SerializeField] protected Sprite blueSprite;
    [SerializeField] protected Sprite redSprite;

    public ElementColor CurrentColor { get; protected set; }
    public float LastSwitchTime { get; protected set; } = -Mathf.Infinity;

    protected virtual void Awake()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        CurrentColor = startingColor;
        ApplySprite(CurrentColor == ElementColor.BLUE ? blueSprite : redSprite, skipCompensation: true);
    }

    public virtual void SetColor(ElementColor newColor)
    {
        CurrentColor = newColor;
        LastSwitchTime = Time.time;
        ApplySprite(CurrentColor == ElementColor.BLUE ? blueSprite : redSprite, skipCompensation: false);
    }

    public void SwitchColor()
    {
        SetColor(CurrentColor == ElementColor.BLUE ? ElementColor.RED : ElementColor.BLUE);
    }

    // skipCompensation = true khi gán sprite lần đầu (Awake) — lúc đó chưa có
    // sprite cũ để so lệch pivot, bù trừ sẽ vô nghĩa (hoặc sai) nên bỏ qua.
    private void ApplySprite(Sprite target, bool skipCompensation)
    {
        if (spriteRenderer == null) return;

        if (target == null)
        {
            Debug.LogWarning(gameObject.name + ": chưa gán " + (CurrentColor == ElementColor.BLUE ? "Blue" : "Red") + " Sprite trên ChromaPolarityBase.");
            return;
        }

        if (!skipCompensation && spriteRenderer.sprite != null)
        {
            Vector3 oldCenter = spriteRenderer.sprite.bounds.center;
            Vector3 newCenter = target.bounds.center;
            spriteRenderer.transform.localPosition += (oldCenter - newCenter);
        }

        spriteRenderer.sprite = target;
    }
}