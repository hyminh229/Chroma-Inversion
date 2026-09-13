using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveSequencer : MonoBehaviour
{
    [Header("Danh sách wave, ĐÚNG THỨ TỰ chạy")]
    [SerializeField] private List<MonoBehaviour> waveSpawners = new List<MonoBehaviour>();

    [Header("Banner thông báo")]
    [SerializeField] private WaveBannerUI bannerUI;
    [SerializeField] private float bannerDuration = 1.5f;
    [SerializeField] private float delayBetweenWaves = 1f;

    private void Start()
    {
        StartCoroutine(RunSequence());
    }

    private IEnumerator RunSequence()
    {
        for (int i = 0; i < waveSpawners.Count; i++)
        {
            if (!(waveSpawners[i] is IWaveSpawner spawner))
            {
                Debug.LogError(waveSpawners[i].name + " không implement IWaveSpawner — bỏ qua.");
                continue;
            }

            if (bannerUI != null)
            {
                yield return StartCoroutine(bannerUI.ShowBanner("WAVE " + (i + 1), bannerDuration));
            }

            bool cleared = false;
            spawner.OnWaveCleared += () => cleared = true;

            spawner.StartWave();

            yield return new WaitUntil(() => cleared);
            yield return new WaitForSeconds(delayBetweenWaves);
        }

        Debug.Log("Tất cả wave đã hoàn thành! (Boss chưa được cài đặt — Phase 7)");
    }
}