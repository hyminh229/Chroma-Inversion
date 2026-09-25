using System;

[Serializable]
public class GameSaveData
{
    public int saveVersion = 1;
    public int currentWave = 1;
    public int score = 0;
    public int playerLives = 1;   // thay cho playerHealth cũ — không còn thanh máu
    public int blueEnergy = 0;
    public int redEnergy = 0;
    public int shotLevel = 1;
    public int shieldCharges = 0;

    public GameSaveData()
    {
        saveVersion = 1;
        currentWave = 1;
        score = 0;
        playerLives = 1;
        blueEnergy = 0;
        redEnergy = 0;
        shotLevel = 1;
        shieldCharges = 0;
    }
}