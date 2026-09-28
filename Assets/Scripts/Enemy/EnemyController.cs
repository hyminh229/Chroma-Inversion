using UnityEngine;

public enum MovementPattern
{
    LinearDown,
    Hover,
    OrbitPoint,
    RandomFlutter,
    SideToSideDescent,
    DiagonalDrift,
    Stationary,
    IdleSway
}

public class EnemyController : MonoBehaviour, IDestroyable
{
    [Header("Movement Pattern")]
    [SerializeField] private MovementPattern pattern = MovementPattern.LinearDown;
    [SerializeField] private float moveSpeed = 4f;

    [Header("Linear Down")]
    [SerializeField] private bool shouldStop;
    [SerializeField] private float stopY = -3f;

    [Header("Hover")]
    [SerializeField] private float hoverY = 2f;
    [SerializeField] private float horizontalRange = 3f;
    [SerializeField] private float horizontalSpeed = 2f;

    [Header("Idle Sway (đứng yên nhưng lắc ngang nhẹ quanh vị trí spawn)")]
    [SerializeField] private float idleSwayRange = 0.1f;
    [SerializeField] private float idleSwaySpeed = 3f;

    [Header("Orbit Point")]
    [SerializeField] private Vector2 orbitCenter = Vector2.zero;
    [SerializeField] private float orbitRadius = 3f;
    [SerializeField] private float orbitAngularSpeed = 60f;

    [Header("Random Flutter")]
    [SerializeField] private Vector2 flutterMin = new Vector2(-8f, 1f);
    [SerializeField] private Vector2 flutterMax = new Vector2(8f, 4f);
    [SerializeField] private float flutterChangeInterval = 1.5f;

    public bool HasStopped { get; private set; }

    private float orbitAngle;
    private Vector2 flutterTarget;
    private float flutterTimer;
    private Vector2 hoverStartPos;
    private bool hoverReady;
    private Vector2 descentBasePos;
    private Vector2 idleSwayBasePos;
    private float idleSwayPhaseOffset;

    private float minX = float.NegativeInfinity;
    private float maxX = float.PositiveInfinity;
    private int driftDirection = 1;

    private void Start()
    {
        HasStopped = pattern == MovementPattern.Stationary;
        orbitAngle = Random.Range(0f, 360f);
        flutterTarget = transform.position;
        hoverStartPos = transform.position;
        descentBasePos = transform.position;
        idleSwayBasePos = transform.position;
        // Lệch pha ngẫu nhiên để cả hàng không lắc đồng bộ y hệt nhau — nhìn tự nhiên hơn.
        idleSwayPhaseOffset = Random.Range(0f, Mathf.PI * 2f);
    }

    private void Update()
    {
        switch (pattern)
        {
            case MovementPattern.LinearDown: MoveLinearDown(); break;
            case MovementPattern.Hover: MoveHover(); break;
            case MovementPattern.OrbitPoint: MoveOrbit(); break;
            case MovementPattern.RandomFlutter: MoveRandomFlutter(); break;
            case MovementPattern.SideToSideDescent: MoveSideToSideDescent(); break;
            case MovementPattern.DiagonalDrift: MoveDiagonalDrift(); break;
            case MovementPattern.Stationary: break;
            case MovementPattern.IdleSway: MoveIdleSway(); break;
        }

        ClampHorizontalBounds();
    }

    public void ConfigureMovement(MovementPattern newPattern, float newSpeed, float boundsMinX, float boundsMaxX, int newDriftDirection = 1)
    {
        pattern = newPattern;
        moveSpeed = newSpeed;
        minX = boundsMinX;
        maxX = boundsMaxX;
        driftDirection = newDriftDirection;

        HasStopped = pattern == MovementPattern.Stationary;
    }

    private void ClampHorizontalBounds()
    {
        Vector3 pos = transform.position;
        pos.x = Mathf.Clamp(pos.x, minX, maxX);
        transform.position = pos;
    }

    private void MoveLinearDown()
    {
        transform.Translate(Vector2.down * moveSpeed * Time.deltaTime);

        if (shouldStop && transform.position.y <= stopY)
        {
            moveSpeed = 0f;
            HasStopped = true;
        }
    }

    private void MoveHover()
    {
        if (!hoverReady)
        {
            transform.Translate(Vector2.down * moveSpeed * Time.deltaTime);

            if (transform.position.y <= hoverY)
            {
                hoverReady = true;
                hoverStartPos = transform.position;
                HasStopped = true;
            }
            return;
        }

        float offsetX = Mathf.Sin(Time.time * horizontalSpeed) * horizontalRange;
        transform.position = new Vector2(hoverStartPos.x + offsetX, transform.position.y);
    }

    // Đứng nguyên tại chỗ spawn, chỉ lắc ngang nhẹ — dùng cho đội hình Grid (Wave 4)
    // để đỡ trông "chết cứng" mà không phá vỡ formation.
    private void MoveIdleSway()
    {
        float offsetX = Mathf.Sin(Time.time * idleSwaySpeed + idleSwayPhaseOffset) * idleSwayRange;
        transform.position = new Vector2(idleSwayBasePos.x + offsetX, idleSwayBasePos.y);
        HasStopped = true;
    }

    private void MoveOrbit()
    {
        orbitAngle += orbitAngularSpeed * Time.deltaTime;
        float rad = orbitAngle * Mathf.Deg2Rad;

        Vector2 offset = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)) * orbitRadius;
        transform.position = orbitCenter + offset;

        HasStopped = true;
    }

    private void MoveRandomFlutter()
    {
        flutterTimer += Time.deltaTime;

        if (flutterTimer >= flutterChangeInterval ||
            Vector2.Distance(transform.position, flutterTarget) < 0.1f)
        {
            flutterTimer = 0f;
            flutterTarget = new Vector2(
                Random.Range(flutterMin.x, flutterMax.x),
                Random.Range(flutterMin.y, flutterMax.y)
            );
        }

        transform.position = Vector2.MoveTowards(transform.position, flutterTarget, moveSpeed * Time.deltaTime);
        HasStopped = true;
    }

    private void MoveSideToSideDescent()
    {
        descentBasePos += Vector2.down * moveSpeed * Time.deltaTime;
        float offsetX = Mathf.Sin(Time.time * horizontalSpeed) * horizontalRange;
        transform.position = new Vector2(descentBasePos.x + offsetX, descentBasePos.y);
    }

    private void MoveDiagonalDrift()
    {
        Vector2 velocity = new Vector2(driftDirection, -1f).normalized * moveSpeed;
        transform.Translate(velocity * Time.deltaTime);
    }

    // FIX: đi qua EnemyHealth.Kill() thay vì Destroy() thẳng, để OnDeath luôn
    // fire dù enemy biến mất kiểu gì (DestroyZone, ramming, hay chết do damage).
    public void DestroyObject()
    {
        if (TryGetComponent(out EnemyHealth enemyHealth))
        {
            enemyHealth.Kill();
        }
        else
        {
            Destroy(gameObject);
        }
    }
}