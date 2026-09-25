using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class EnergyOrb : MonoBehaviour
{
    [SerializeField] private ElementColor color = ElementColor.BLUE;
    [SerializeField] private int energyAmount = 4;
    [SerializeField] private int scoreValue = 20;
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

        playerEnergy.AddEnergy(color, energyAmount);
        ScoreManager.Instance?.AddScore(scoreValue);

        Debug.Log("EXP Orb collected! +" + energyAmount + " " + color + " Energy, +" + scoreValue + " Score.");

        Destroy(gameObject);
    }
}