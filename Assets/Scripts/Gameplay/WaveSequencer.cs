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
    [Tooltip("Thời gian hiển thị banner wave (2 đến 4 giây)")]
    [Range(2f, 4f)]
    [SerializeField] private float bannerDuration = 3f;
    [Tooltip("Thời gian nghỉ sau khi kết thúc wave trước khi hiện banner wave tiếp theo (2 giây)")]
    [SerializeField] private float delayBetweenWaves = 2f;
    [Tooltip("Thời gian nghỉ sau khi banner tắt trước khi quái xuất hiện")]
    [SerializeField] private float delayBeforeEnemySpawn = 0.5f;

    [Header("Player Components (Tùy chọn - tự tìm nếu để trống)")]
    [SerializeField] private PlayerLife playerLife;
    [SerializeField] private PlayerEnergy playerEnergy;
    [SerializeField] private PlayerShooting playerShooting;
    [SerializeField] private PlayerShield playerShield;

    public event Action OnAllWavesCompleted;
    public int CurrentWave { get; private set; } = 1;
    private bool allWavesCompleted;

    private void Start()
    {
        FindPlayerReferences();

        if (bannerUI == null)
        {
#if UNITY_2023_1_OR_NEWER
            bannerUI = FindAnyObjectByType<WaveBannerUI>(FindObjectsInactive.Include);
#else
            bannerUI = FindObjectOfType<WaveBannerUI>(true);
#endif
        }

        int startWaveIndex = 0;

        if (SaveSystem.IsContinuing && SaveSystem.HasSave())
        {
            GameSaveData saveData = SaveSystem.LoadGame();
            if (saveData != null)
            {
                RestorePlayerState(saveData);

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

            SaveSystem.IsContinuing = false;
        }

        CurrentWave = startWaveIndex + 1;
        StartCoroutine(RunSequence(startWaveIndex));
    }

    private void FindPlayerReferences()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            if (playerLife == null) playerLife = playerObj.GetComponent<PlayerLife>();
            if (playerEnergy == null) playerEnergy = playerObj.GetComponent<PlayerEnergy>();
            if (playerShooting == null) playerShooting = playerObj.GetComponent<PlayerShooting>();
            if (playerShield == null) playerShield = playerObj.GetComponent<PlayerShield>();
        }
    }

    private void RestorePlayerState(GameSaveData data)
    {
        if (playerLife != null)
        {
            playerLife.RestoreLives(data.playerLives);
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

        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.RestoreScore(data.score);
        }
    }

    private void SaveCheckpoint(int nextWaveIndex)
    {
        GameSaveData data = new GameSaveData();
        data.currentWave = nextWaveIndex + 1; // 1-based wave number
        data.score = ScoreManager.Instance != null ? ScoreManager.Instance.TotalScore : 0;

        if (playerLife != null)
        {
            data.playerLives = playerLife.CurrentLives;
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
            CurrentWave = i + 1;

            if (!(waveSpawners[i] is IWaveSpawner spawner))
            {
                Debug.LogError(waveSpawners[i].name + " không implement IWaveSpawner — bỏ qua.");
                continue;
            }

            if (bannerUI != null)
            {
                bannerUI.gameObject.SetActive(true);
                yield return bannerUI.ShowBanner("WAVE " + (i + 1), bannerDuration);
            }

            // Chờ sau khi banner tắt rồi quái mới xuất hiện
            if (delayBeforeEnemySpawn > 0f)
            {
                yield return new WaitForSeconds(delayBeforeEnemySpawn);
            }

            bool cleared = false;
            Action onCleared = () => cleared = true;
            spawner.OnWaveCleared += onCleared;

            spawner.StartWave();

            yield return new WaitUntil(() => cleared);
            spawner.OnWaveCleared -= onCleared;

            int nextWaveIndex = i + 1;
            if (nextWaveIndex < waveSpawners.Count)
            {
                SaveCheckpoint(nextWaveIndex);
            }
            else
            {
                SaveSystem.DeleteSave();
            }

            // Kết thúc wave: chờ khoảng 2 giây trước khi hiện banner wave tiếp theo
            yield return new WaitForSeconds(delayBetweenWaves);
        }

        Debug.Log("Tất cả wave đã hoàn thành!");
        if (!allWavesCompleted)
        {
            allWavesCompleted = true;
            OnAllWavesCompleted?.Invoke();
        }
    }
}