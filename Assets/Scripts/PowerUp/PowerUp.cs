using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public abstract class PowerUp : MonoBehaviour
{
    [SerializeField] private float fallSpeed = 2f;

    private void Update()
    {
        transform.Translate(Vector2.down * fallSpeed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.TryGetComponent(out PlayerLife playerLife)) return;

        ApplyEffect(collision.gameObject);
        Destroy(gameObject);
    }

    protected abstract void ApplyEffect(GameObject player);
}