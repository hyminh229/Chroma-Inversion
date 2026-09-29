using System;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    private static ScoreManager instance;
    public static ScoreManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindAnyObjectByType<ScoreManager>();
                if (instance == null)
                {
                    GameObject go = new GameObject("ScoreManager");
                    instance = go.AddComponent<ScoreManager>();
                    Debug.Log("[ScoreManager] Auto-created ScoreManager in scene.");
                }
            }
            return instance;
        }
        private set => instance = value;
    }

    public int TotalScore { get; private set; }

    public event Action<int> OnScoreChanged;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
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