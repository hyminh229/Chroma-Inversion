using UnityEngine;

public class PlayerEnergy : MonoBehaviour
{
    public static PlayerEnergy Instance { get; private set; }

    [Header("Energy")]
    [SerializeField] private int maxEnergy = 100;
    [SerializeField] private int currentBlueEnergy = 0;
    [SerializeField] private int currentRedEnergy = 0;

    public int CurrentBlueEnergy => currentBlueEnergy;
    public int CurrentRedEnergy => currentRedEnergy;
    public int MaxEnergy => maxEnergy;

    public bool IsBlueFull => currentBlueEnergy >= maxEnergy;
    public bool IsRedFull => currentRedEnergy >= maxEnergy;
    public bool IsFull => IsBlueFull && IsRedFull;

    private void Awake()
    {
        Instance = this;
    }

    public void AddEnergy(ElementColor color, int amount)
    {
        if (amount <= 0) return;

        if (color == ElementColor.BLUE)
        {
            currentBlueEnergy += amount;
            if (currentBlueEnergy > maxEnergy) currentBlueEnergy = maxEnergy;
        }
        else
        {
            currentRedEnergy += amount;
            if (currentRedEnergy > maxEnergy) currentRedEnergy = maxEnergy;
        }

        Debug.Log("Blue Energy: " + currentBlueEnergy + "/" + maxEnergy + " | Red Energy: " + currentRedEnergy + "/" + maxEnergy);
    }

    public bool UseMegaBeam()
    {
        if (!IsFull) return false;

        currentBlueEnergy = 0;
        currentRedEnergy = 0;

        Debug.Log("Mega Beam used! Both energies reset to 0.");

        return true;
    }
    public void RestoreEnergy(int blue, int red)
    {
        currentBlueEnergy = Mathf.Clamp(blue, 0, maxEnergy);
        currentRedEnergy = Mathf.Clamp(red, 0, maxEnergy);
        Debug.Log("Player energy restored. Blue: " + currentBlueEnergy + "/" + maxEnergy + " | Red: " + currentRedEnergy + "/" + maxEnergy);
    }
}