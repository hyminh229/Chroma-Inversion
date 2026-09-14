using System.Collections;
using UnityEngine;

// Tấn công "Laser Grid": khoá hướng nhắm Player NGAY LÚC BẮT ĐẦU cảnh báo, hiện
// đường tím mờ đúng hình dạng hitbox sắp nổ, sau đó chuyển tia tím đặc gây damage.
// Nằm NGOÀI hệ màu BLUE/RED — Player không absorb được, bắt buộc di chuyển né.
[RequireComponent(typeof(BossHealth))]
public class BossBeamAttack : MonoBehaviour
{
    [Header("Timing")]
    [SerializeField] private float attackInterval = 6f;
    [SerializeField] private float telegraphDuration = 1f;
    [SerializeField] private float activeDuration = 0.6f;

    [Header("Beam Shape")]
    [SerializeField] private float beamLength = 20f;
    [SerializeField] private float beamWidth = 0.5f;

    [Header("Visual (tím — nằm ngoài hệ màu BLUE/RED)")]
    [SerializeField] private Color telegraphColor = new Color(0.8f, 0f, 1f, 0.25f);
    [SerializeField] private Color activeColor = new Color(0.8f, 0f, 1f, 1f);

    [Header("References (BeamVisual — GameObject ĐỘC LẬP, KHÔNG đặt làm con của Boss)")]
    [SerializeField] private Transform beamTransform;
    [SerializeField] private SpriteRenderer beamRenderer;
    [SerializeField] private Collider2D beamCollider;

    private BossHealth bossHealth;
    private Transform player;

    private void Awake()
    {
        bossHealth = GetComponent<BossHealth>();
    }

    private void Start()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null) player = playerObject.transform;

        SetBeamActive(false);
        StartCoroutine(AttackLoop());
    }

    private IEnumerator AttackLoop()
    {
        while (bossHealth.IsAlive)
        {
            yield return new WaitForSeconds(attackInterval);

            if (player == null || !bossHealth.IsAlive) continue;

            yield return StartCoroutine(FireBeam());
        }
    }

    private IEnumerator FireBeam()
    {
        Vector2 origin = transform.position;
        Vector2 direction = ((Vector2)player.position - origin).normalized;
        PositionBeam(origin, direction);

        // Telegraph — cảnh báo, collider TẮT nên chưa gây damage.
        beamRenderer.enabled = true;
        beamRenderer.color = telegraphColor;
        beamCollider.enabled = false;

        yield return new WaitForSeconds(telegraphDuration);

        // Active — tia thật, collider BẬT. BeamHazard trên cùng object tự lo tick damage.
        beamRenderer.color = activeColor;
        beamCollider.enabled = true;

        yield return new WaitForSeconds(activeDuration);

        SetBeamActive(false);
    }

    private void PositionBeam(Vector2 origin, Vector2 direction)
    {
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        Vector2 midpoint = origin + direction * (beamLength / 2f);

        beamTransform.position = midpoint;
        beamTransform.rotation = Quaternion.Euler(0f, 0f, angle);
        beamTransform.localScale = new Vector3(beamLength, beamWidth, 1f);
    }

    private void SetBeamActive(bool active)
    {
        beamRenderer.enabled = active;
        beamCollider.enabled = active;
    }
}