using UnityEngine;

public class PlayerShooting : MonoBehaviour
{
    [Header("Normal Shooting")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float baseFireRate = 0.2f;

    [Header("Bullet Upgrade (Level 1-10)")]
    [SerializeField] private int shotLevel = 1;
    [SerializeField] private float multiShotSpreadAngle = 15f;
    [SerializeField][Range(0.5f, 0.95f)] private float fireRateBoostMultiplier = 0.88f;

    [Header("Mega Beam")]
    [SerializeField] private GameObject megaBeamPrefab;
    [SerializeField] private float megaBeamSpawnOffset = 1.5f;

    private const int MaxShotLevel = 10;

    // Số tia theo Level (index 0 = Level 1) — đúng bảng: 1,2,2,3,3,3,4,4,4,5.
    private static readonly int[] BulletCountByLevel = { 1, 2, 2, 3, 3, 3, 4, 4, 4, 5 };
    // Level nào là mốc "tăng tốc độ bắn" thay vì thêm tia (Level 3,5,6,8,9).
    private static readonly bool[] IsFireRateBoostLevel = { false, false, true, false, true, true, false, true, true, false };

    private PlayerColorController colorController;
    private PlayerEnergy playerEnergy;

    private float timer;
    private bool isChanneling;

    public bool IsChanneling => isChanneling;
    public int ShotLevel => shotLevel;

    private void Awake()
    {
        colorController = GetComponent<PlayerColorController>();
        playerEnergy = GetComponent<PlayerEnergy>();
    }

    private void Update()
    {
        HandleShooting();
        HandleMegaBeam();
    }

    private void HandleShooting()
    {
        if (isChanneling) return;

        timer += Time.deltaTime;

        if (Input.GetMouseButton(0) && timer >= GetCurrentFireRate())
        {
            Shoot();
            timer = 0f;
        }
    }

    private void HandleMegaBeam()
    {
        if (isChanneling) return;

        // Đổi từ phím E sang Space.
        if (Input.GetKeyDown(KeyCode.Space))
        {
            ShootMegaBeam();
        }
    }

    private void Shoot()
    {
        if (bulletPrefab == null || firePoint == null)
        {
            Debug.LogWarning("PlayerShooting is missing bullet references.");
            return;
        }

        int bulletCount = BulletCountByLevel[shotLevel - 1];
        float startAngle = -(bulletCount - 1) / 2f * multiShotSpreadAngle;

        for (int i = 0; i < bulletCount; i++)
        {
            float angleOffset = startAngle + i * multiShotSpreadAngle;
            SpawnBullet(angleOffset);
        }
    }

    // Nhân dồn multiplier cho MỖI mốc tăng tốc đã đạt tới tính từ Level 1 —
    // đúng khớp việc Lv3/5/6/8/9 đều làm bắn nhanh hơn cộng dồn, không phải
    // chỉ có hiệu lực ở đúng level đó rồi thôi.
    private float GetCurrentFireRate()
    {
        float rate = baseFireRate;

        for (int i = 0; i < shotLevel; i++)
        {
            if (IsFireRateBoostLevel[i])
            {
                rate *= fireRateBoostMultiplier;
            }
        }

        return rate;
    }

    private void SpawnBullet(float angleOffset)
    {
        Quaternion rotation = firePoint.rotation * Quaternion.Euler(0f, 0f, angleOffset);

        GameObject bulletObject = ObjectPooler.Instance != null
            ? ObjectPooler.Instance.Spawn(bulletPrefab, firePoint.position, rotation)
            : Instantiate(bulletPrefab, firePoint.position, rotation);

        if (bulletObject == null) return;

        if (!bulletObject.TryGetComponent(out Bullet bullet))
        {
            Debug.LogError("Bullet prefab does not contain Bullet component.");
            return;
        }

        if (colorController != null)
        {
            bullet.SetColor(colorController.CurrentColor);
        }
    }

    public void UpgradeShot()
    {
        if (shotLevel >= MaxShotLevel)
        {
            Debug.Log("Bullet already at max level (10).");
            return;
        }

        shotLevel++;
        Debug.Log("Bullet upgraded! Level: " + shotLevel + " (" + BulletCountByLevel[shotLevel - 1] + " tia)");
    }

    // Dùng khi Continue — set thẳng level đã lưu, không tăng dần từng bước như UpgradeShot().
    public void SetShotLevel(int level)
    {
        shotLevel = Mathf.Clamp(level, 1, MaxShotLevel);
        Debug.Log("Player shot level restored: " + shotLevel + "/" + MaxShotLevel + " (" + BulletCountByLevel[shotLevel - 1] + " tia)");
    }

    private void ShootMegaBeam()
    {
        if (megaBeamPrefab == null || firePoint == null)
        {
            Debug.LogWarning("PlayerShooting is missing Mega Beam references.");
            return;
        }

        if (playerEnergy == null)
        {
            Debug.LogWarning("Player does not have PlayerEnergy component.");
            return;
        }

        if (!playerEnergy.UseMegaBeam())
        {
            Debug.Log("Energy is not full. Cannot use Mega Beam.");
            return;
        }

        Vector3 spawnPos = firePoint.position + firePoint.up * megaBeamSpawnOffset;
        GameObject beamObject = Instantiate(megaBeamPrefab, spawnPos, firePoint.rotation);

        if (beamObject.TryGetComponent(out MegaBeam beam))
        {
            beam.Init(this);
        }

        isChanneling = true;
        Debug.Log("MEGA BEAM FIRED!");
    }

    public void EndChanneling()
    {
        isChanneling = false;
    }
}