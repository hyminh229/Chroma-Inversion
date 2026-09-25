using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float smoothSpeed = 15f;
    [SerializeField] private float padding = 0.5f;
    [SerializeField] private PlayerLife playerLife;
    [SerializeField] private PlayerShooting playerShooting;

    private Camera mainCamera;
    private Vector2 minBounds;
    private Vector2 maxBounds;

    private void Awake()
    {
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = false;

        if (playerShooting == null)
        {
            playerShooting = GetComponent<PlayerShooting>();
        }
    }

    private void Start()
    {
        mainCamera = Camera.main;
        CalculateScreenBounds();
    }

    private void Update()
    {
        if (!playerLife.IsAlive) return;
        if (playerShooting != null && playerShooting.IsChanneling) return;

        MoveWithMouse();
    }

    private void MoveWithMouse()
    {
        Vector3 mouseScreenPos = Input.mousePosition;
        Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(mouseScreenPos);

        float clampedX = Mathf.Clamp(mouseWorldPos.x, minBounds.x + padding, maxBounds.x - padding);
        float clampedY = Mathf.Clamp(mouseWorldPos.y, minBounds.y + padding, maxBounds.y - padding);
        Vector2 targetPosition = new Vector2(clampedX, clampedY);

        transform.position = Vector2.Lerp(transform.position, targetPosition, smoothSpeed * Time.deltaTime);
    }

    private void CalculateScreenBounds()
    {
        Vector3 bottomLeft = mainCamera.ViewportToWorldPoint(new Vector3(0, 0, mainCamera.nearClipPlane));
        Vector3 topRight = mainCamera.ViewportToWorldPoint(new Vector3(1, 1, mainCamera.nearClipPlane));

        minBounds = new Vector2(bottomLeft.x, bottomLeft.y);
        maxBounds = new Vector2(topRight.x, topRight.y);
    }
}