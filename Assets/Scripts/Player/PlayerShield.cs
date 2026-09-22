using UnityEngine;

public class PlayerShield : MonoBehaviour
{
    [SerializeField] private int maxCharges = 3;

    private int currentCharges;

    public int CurrentCharges => currentCharges;
    public int MaxCharges => maxCharges;
    public bool IsActive => currentCharges > 0;

    public void Activate(int charges)
    {
        currentCharges = Mathf.Clamp(charges, 0, maxCharges);
        Debug.Log("Shield activated! Charges: " + currentCharges);
    }

    public void RestoreShield(int charges)
    {
        currentCharges = Mathf.Clamp(charges, 0, maxCharges);
        Debug.Log("Shield restored! Charges: " + currentCharges + "/" + maxCharges);
    }

    // Trả về true nếu khiên đã chặn được cú va chạm này (EnemyBullet gọi khi đạn sai màu).
    public bool TryBlockHit()
    {
        if (currentCharges <= 0) return false;

        currentCharges--;

        Debug.Log("Shield blocked a hit! Charges left: " + currentCharges);

        if (currentCharges <= 0)
        {
            Break();
        }

        return true;
    }

    private void Break()
    {
        Debug.Log("Shield broke!");
        // TODO Phase 10: tắt hiệu ứng visual khiên ở đây.
    }
}