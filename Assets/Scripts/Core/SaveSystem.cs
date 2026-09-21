using System;
using System.IO;
using UnityEngine;

/// <summary>
/// Static utility managing JSON-based save/load operations in Application.persistentDataPath.
/// Provides safe handling of missing files, corrupted JSON, and invalid values.
/// </summary>
public static class SaveSystem
{
    private const string SAVE_FILE_NAME = "chromainversion_save.json";
    private const int CURRENT_VERSION = 1;

    /// <summary>
    /// Runtime flag indicating whether the current game session was started via Continue.
    /// </summary>
    public static bool IsContinuing { get; set; } = false;

    public static string SaveFilePath => Path.Combine(Application.persistentDataPath, SAVE_FILE_NAME);

    /// <summary>
    /// Checks if a valid, non-empty save file exists.
    /// </summary>
    public static bool HasSave()
    {
        try
        {
            if (!File.Exists(SaveFilePath))
            {
                return false;
            }

            FileInfo fileInfo = new FileInfo(SaveFilePath);
            return fileInfo.Length > 0;
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[SaveSystem] Error checking save file: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Saves the given GameSaveData object to a JSON file.
    /// </summary>
    public static bool SaveGame(GameSaveData data)
    {
        if (data == null)
        {
            Debug.LogError("[SaveSystem] Cannot save null GameSaveData.");
            return false;
        }

        try
        {
            data.saveVersion = CURRENT_VERSION;
            string json = JsonUtility.ToJson(data, true);
            File.WriteAllText(SaveFilePath, json);
            Debug.Log($"[SaveSystem] Checkpoint saved successfully at: {SaveFilePath}\n{json}");
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"[SaveSystem] Failed to save game: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Loads and validates GameSaveData from disk. Returns null if missing, corrupted, or invalid.
    /// </summary>
    public static GameSaveData LoadGame()
    {
        if (!HasSave())
        {
            Debug.LogWarning("[SaveSystem] No save file found to load.");
            return null;
        }

        try
        {
            string json = File.ReadAllText(SaveFilePath);
            if (string.IsNullOrWhiteSpace(json))
            {
                Debug.LogWarning("[SaveSystem] Save file is empty.");
                return null;
            }

            GameSaveData data = JsonUtility.FromJson<GameSaveData>(json);
            if (data == null)
            {
                Debug.LogWarning("[SaveSystem] Failed to deserialize save data (corrupted JSON).");
                return null;
            }

            // Version verification
            if (data.saveVersion != CURRENT_VERSION)
            {
                Debug.LogWarning($"[SaveSystem] Save version mismatch: found {data.saveVersion}, expected {CURRENT_VERSION}. Rejecting save.");
                return null;
            }

            // Sanitize & clamp values to ensure robust gameplay state
            if (data.currentWave < 1)
            {
                Debug.LogWarning($"[SaveSystem] Invalid currentWave ({data.currentWave}) clamped to 1.");
                data.currentWave = 1;
            }

            if (data.score < 0)
            {
                data.score = 0;
            }

            if (data.playerHealth < 1)
            {
                Debug.LogWarning($"[SaveSystem] Player health ({data.playerHealth}) was non-positive in save. Clamped to 1.");
                data.playerHealth = 1;
            }

            if (data.blueEnergy < 0) data.blueEnergy = 0;
            if (data.redEnergy < 0) data.redEnergy = 0;
            if (data.shotLevel < 1) data.shotLevel = 1;
            if (data.shieldCharges < 0) data.shieldCharges = 0;

            Debug.Log($"[SaveSystem] Save data loaded successfully: Wave {data.currentWave}, HP {data.playerHealth}, ShotLevel {data.shotLevel}.");
            return data;
        }
        catch (Exception ex)
        {
            Debug.LogError($"[SaveSystem] Error loading or parsing save file: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Deletes the save file if present. Used by New Game and game completion.
    /// </summary>
    public static bool DeleteSave()
    {
        try
        {
            if (File.Exists(SaveFilePath))
            {
                File.Delete(SaveFilePath);
                Debug.Log("[SaveSystem] Save file deleted.");
            }
            IsContinuing = false;
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"[SaveSystem] Failed to delete save file: {ex.Message}");
            return false;
        }
    }
}
