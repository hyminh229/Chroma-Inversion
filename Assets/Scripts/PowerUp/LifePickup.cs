using UnityEngine;

public class LifePickup : PowerUp
{
    [SerializeField] private int livesToAdd = 1;

    protected override void ApplyEffect(GameObject player)
    {
        if (!player.TryGetComponent(out PlayerLife playerLife))
        {
            Debug.LogWarning("LifePickup: Player is missing PlayerLife component.");
            return;
        }

        playerLife.AddLife(livesToAdd);
    }
}