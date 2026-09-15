using System;
using System.Collections;
using UnityEngine;

public class BossWaveSpawner : MonoBehaviour, IWaveSpawner
{
    [SerializeField] private GameObject bossPrefab;
    [SerializeField] private float spawnTopMargin = 2f;

    public event Action OnWaveCleared;

    public void StartWave()
    {
        StartCoroutine(SpawnBoss());
    }

    private IEnumerator SpawnBoss()
    {
        yield return null;

        ScreenBoundsUtil.GetWorldBounds(out float minX, out float maxX, out _, out float topY);
        Vector3 spawnPos = new Vector3((minX + maxX) / 2f, topY - spawnTopMargin, 0f);

        GameObject instance = Instantiate(bossPrefab, spawnPos, Quaternion.identity);

        if (instance.TryGetComponent(out BossHealth bossHealth))
        {
            bossHealth.OnDeath += HandleBossDeath;
        }
        else
        {
            Debug.LogWarning("BossWaveSpawner: bossPrefab thiếu BossHealth.");
        }
    }

    private void HandleBossDeath()
    {
        Debug.Log("BOSS DEFEATED — Wave cleared!");
        OnWaveCleared?.Invoke();
    }
}