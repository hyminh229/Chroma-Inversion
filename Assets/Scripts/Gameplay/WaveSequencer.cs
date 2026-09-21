using System;
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

    [Header("Player Components (Tùy chọn - tự tìm nếu để trống)")]
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private PlayerEnergy playerEnergy;
    [SerializeField] private PlayerShooting playerShooting;
    [SerializeField] private PlayerShield playerShield;

    private void Start()
    {
        FindPlayerReferences();

        int startWaveIndex = 0;

        // Nếu bấm Continue từ Main Menu và có file save hợp lệ -> Load checkpoint
        if (SaveSystem.IsContinuing && SaveSystem.HasSave())
        {
            GameSaveData saveData = SaveSystem.LoadGame();
            if (saveData != null)
            {
                RestorePlayerState(saveData);

                // currentWave được lưu theo định dạng 1-based (Wave 1, Wave 2...).
                // startWaveIndex là 0-based.
                startWaveIndex = saveData.currentWave - 1;

                if (startWaveIndex < 0 || startWaveIndex >= waveSpawners.Count)
                {
                    Debug.LogWarning($"[WaveSequencer] Saved wave index ({startWaveIndex}) out of bounds (0..{waveSpawners.Count - 1}). Starting from wave 1.");
                    startWaveIndex = 0;
                }
                else
                {
                    Debug.Log($"[WaveSequencer] Resuming gameplay at Wave {saveData.currentWave} (index {startWaveIndex}).");
                }
            }

            // Đặt lại flag sau khi đã xử lý xong
            SaveSystem.IsContinuing = false;
        }

        StartCoroutine(RunSequence(startWaveIndex));
    }

    private void FindPlayerReferences()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            if (playerHealth == null) playerHealth = playerObj.GetComponent<PlayerHealth>();
            if (playerEnergy == null) playerEnergy = playerObj.GetComponent<PlayerEnergy>();
            if (playerShooting == null) playerShooting = playerObj.GetComponent<PlayerShooting>();
            if (playerShield == null) playerShield = playerObj.GetComponent<PlayerShield>();
        }
    }

    private void RestorePlayerState(GameSaveData data)
    {
        if (playerHealth != null)
        {
            playerHealth.RestoreHealth(data.playerHealth);
        }

        if (playerEnergy != null)
        {
            playerEnergy.RestoreEnergy(data.blueEnergy, data.redEnergy);
        }

        if (playerShooting != null)
        {
            playerShooting.SetShotLevel(data.shotLevel);
        }

        if (playerShield != null)
        {
            playerShield.RestoreShield(data.shieldCharges);
        }
    }

    private void SaveCheckpoint(int nextWaveIndex)
    {
        GameSaveData data = new GameSaveData();
        data.currentWave = nextWaveIndex + 1; // 1-based wave number
        data.score = 0;

        if (playerHealth != null)
        {
            data.playerHealth = playerHealth.CurrentHealth;
        }

        if (playerEnergy != null)
        {
            data.blueEnergy = playerEnergy.CurrentBlueEnergy;
            data.redEnergy = playerEnergy.CurrentRedEnergy;
        }

        if (playerShooting != null)
        {
            data.shotLevel = playerShooting.ShotLevel;
        }

        if (playerShield != null)
        {
            data.shieldCharges = playerShield.CurrentCharges;
        }

        SaveSystem.SaveGame(data);
    }

    private IEnumerator RunSequence(int startIndex = 0)
    {
        for (int i = startIndex; i < waveSpawners.Count; i++)
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
            Action onCleared = () => cleared = true;
            spawner.OnWaveCleared += onCleared;

            spawner.StartWave();

            yield return new WaitUntil(() => cleared);
            spawner.OnWaveCleared -= onCleared;

            // Wave hoàn thành -> Lưu checkpoint cho wave kế tiếp
            int nextWaveIndex = i + 1;
            if (nextWaveIndex < waveSpawners.Count)
            {
                SaveCheckpoint(nextWaveIndex);
            }
            else
            {
                // Hoàn thành tất cả các wave trong game -> xóa save để lần chơi sau bắt đầu mới
                SaveSystem.DeleteSave();
            }

            yield return new WaitForSeconds(delayBetweenWaves);
        }

        Debug.Log("Tất cả wave đã hoàn thành! (Boss chưa được cài đặt — Phase 7)");
    }
}