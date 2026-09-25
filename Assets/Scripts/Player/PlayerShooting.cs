using UnityEngine;

public class PlayerShooting : MonoBehaviour
{
    [Header("Normal Shooting")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float fireRate = 0.2f;

    [Header("Bullet Upgrade")]
    [SerializeField] private int shotLevel = 1;
    [SerializeField] private int maxShotLevel = 3;
    [SerializeField] private float multiShotSpreadAngle = 15f;

    [Header("Mega Beam")]
    [SerializeField] private GameObject megaBeamPrefab;
    [SerializeField] private float megaBeamSpawnOffset = 1.5f;

    private PlayerColorController colorController;
    private PlayerEnergy playerEnergy;

    private float timer;
    private bool isChanneling;

    public bool IsChanneling => isChanneling;

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

        if (Input.GetMouseButton(0) && timer >= fireRate)
        {
            Shoot();
            timer = 0f;
        }
    }

    private void HandleMegaBeam()
    {
        if (isChanneling) return;

        if (Input.GetKeyDown(KeyCode.E))
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

        int bulletCount = GetBulletCountForLevel();
        float startAngle = -(bulletCount - 1) / 2f * multiShotSpreadAngle;

        for (int i = 0; i < bulletCount; i++)
        {
            float angleOffset = startAngle + i * multiShotSpreadAngle;
            SpawnBullet(angleOffset);
        }
    }

    private int GetBulletCountForLevel()
    {
        return (shotLevel - 1) * 2 + 1;
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
        if (shotLevel >= maxShotLevel)
        {
            Debug.Log("Bullet already at max level.");
            return;
        }

        shotLevel++;
        Debug.Log("Bullet upgraded! Level: " + shotLevel);
    }

    public int ShotLevel => shotLevel;
    public int MaxShotLevel => maxShotLevel;

    public void SetShotLevel(int level)
    {
        shotLevel = Mathf.Clamp(level, 1, maxShotLevel);
        Debug.Log("Player shot level restored: " + shotLevel + "/" + maxShotLevel);
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