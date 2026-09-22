using System;

/// <summary>
/// Serializable data container representing persistent gameplay progression.
/// Excludes runtime transforms, temporary VFX, object pools, and scene-specific GameObjects.
/// </summary>
[Serializable]
public class GameSaveData
{
    public int saveVersion = 1;
    public int currentWave = 1;      // 1-based wave number of the next wave to play (Wave 1, Wave 2, etc.)
    public int score = 0;            // Progression score (reserved for score system)
    public int playerHealth = 10;
    public int blueEnergy = 0;
    public int redEnergy = 0;
    public int shotLevel = 1;
    public int shieldCharges = 0;

    public GameSaveData()
    {
        saveVersion = 1;
        currentWave = 1;
        score = 0;
        playerHealth = 10;
        blueEnergy = 0;
        redEnergy = 0;
        shotLevel = 1;
        shieldCharges = 0;
    }
}
