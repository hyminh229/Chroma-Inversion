using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class EnergyOrb : MonoBehaviour
{
    [SerializeField] private ElementColor color = ElementColor.BLUE;
    [SerializeField] private int energyAmount = 4;
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

        spriteRenderer.sprite = color == ElementColor.BLUE ? blueSprite : redSprite;
        spriteRenderer.color = Color.white;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.TryGetComponent(out PlayerEnergy playerEnergy)) return;

        playerEnergy.AddEnergy(color, energyAmount);

        Debug.Log("Energy Orb collected! +" + energyAmount + " " + color + " Energy.");

        Destroy(gameObject);
    }
}