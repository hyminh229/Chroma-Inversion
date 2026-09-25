using UnityEngine;

public class EnemyBullet : ProjectileBase
{
    [SerializeField] private int absorbEnergy = 10;
    [SerializeField] private int perfectAbsorbEnergy = 15;
    [SerializeField] private float perfectParryWindow = 0.3f;
    [SerializeField] private int absorbScore = 10;
    [SerializeField] private int perfectAbsorbScore = 25;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.TryGetComponent(out PlayerLife playerLife)) return;
        if (!collision.TryGetComponent(out PlayerColorController playerColor)) return;

        if (ColorType == playerColor.CurrentColor)
        {
            Absorb(playerLife, playerColor);
        }
        else if (collision.TryGetComponent(out PlayerShield shield) && shield.TryBlockHit())
        {
            // Khiên đã chặn cú va chạm sai màu này, không mất mạng.
        }
        else
        {
            DamagePlayer(playerLife);
        }

        DestroyObject();
    }

    private void Absorb(PlayerLife playerLife, PlayerColorController playerColor)
    {
        if (!playerLife.TryGetComponent(out PlayerEnergy playerEnergy))
        {
            Debug.LogWarning("Player does not have PlayerEnergy component.");
            return;
        }

        bool isPerfect = (Time.time - playerColor.LastSwitchTime) <= perfectParryWindow;
        int energyGained = isPerfect ? perfectAbsorbEnergy : absorbEnergy;
        int scoreGained = isPerfect ? perfectAbsorbScore : absorbScore;

        playerEnergy.AddEnergy(ColorType, energyGained);
        ScoreManager.Instance?.AddScore(scoreGained);

        Debug.Log(isPerfect
            ? "Perfect Absorb! +" + energyGained + " " + ColorType + " Energy, +" + scoreGained + " Score."
            : "Enemy bullet absorbed! +" + energyGained + " " + ColorType + " Energy, +" + scoreGained + " Score.");
    }

    private void DamagePlayer(PlayerLife playerLife)
    {
        playerLife.TakeDamage(damage * 2);
        Debug.Log("Enemy bullet hit player! Mất 1 mạng (nếu không bất tử).");
    }
}