using System;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    public int TotalScore { get; private set; }

    public event Action<int> OnScoreChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void RestoreScore(int score)
    {
        TotalScore = Mathf.Max(0, score);
        OnScoreChanged?.Invoke(TotalScore);
        Debug.Log("Score restored: " + TotalScore);
    }

    public void AddScore(int amount)
    {
        if (amount <= 0) return;

        TotalScore += amount;
        OnScoreChanged?.Invoke(TotalScore);

        Debug.Log("Score +" + amount + " → Total: " + TotalScore);
    }
}