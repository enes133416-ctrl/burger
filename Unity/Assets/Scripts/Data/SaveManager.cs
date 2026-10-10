using System;
using System.IO;
using BurgerRush.Core;
using UnityEngine;

namespace BurgerRush.Data
{
    /// <summary>
    /// JSON kayıt sistemi (GDD 6.3). Bulut senkronizasyon için CloudSave arayüzü ayrılmıştır.
    /// </summary>
    public class SaveManager : Utils.Singleton<SaveManager>
    {
        private const string FileName = "burger_rush_save.json";
        private static string Path => System.IO.Path.Combine(Application.persistentDataPath, FileName);

        public PlayerData Data { get; private set; } = new();

        public void Load()
        {
            try
            {
                if (File.Exists(Path))
                {
                    var json = File.ReadAllText(Path);
                    Data = JsonUtility.FromJson<PlayerData>(json) ?? new PlayerData();
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[SaveManager] Kayıt okunamadı: {e.Message}");
                Data = new PlayerData();
            }
        }

        public void Save()
        {
            try
            {
                var json = JsonUtility.ToJson(Data, true);
                File.WriteAllText(Path, json);
            }
            catch (Exception e) { Debug.LogWarning($"[SaveManager] Kayıt yazılamadı: {e.Message}"); }
        }

        public void Reset()
        {
            Data = new PlayerData();
            if (File.Exists(Path)) File.Delete(Path);
            Save();
        }

        /// <summary>Offline gelir hesabı için çıkış damgası.</summary>
        public void StampExit()
        {
            Data.lastOfflineTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            Save();
        }

        public void SetStar(int level, int stars)
        {
            int prev = Data.levelStars.TryGetValue(level, out var s) ? s : 0;
            if (stars > prev)
            {
                Data.totalStars += stars - prev;
                Data.levelStars[level] = stars;
            }
            Data.maxLevelReached = Mathf.Max(Data.maxLevelReached, Mathf.Min(50, level + 1));
            Save();
        }
    }
}
