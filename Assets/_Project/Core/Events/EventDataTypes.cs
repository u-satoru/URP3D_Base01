using UnityEngine;
using System;
using System.Collections.Generic;

namespace asterivo.Unity60.Core.Events
{
    /// <summary>
    /// Events専用データ型定義
    /// 循環依存回避のため、Core.EventsアセンブリでのEvent定義に必要な型を独立して提供
    ///
    /// 設計原則:
    /// - Core.Eventsアセンブリの独立性確保
    /// - イベント通信に必要最小限のデータ構造
    /// - 循環依存を発生させない軽量なデータ型定義
    /// </summary>

    #region Game Data Types

    /// <summary>
    /// ゲーム全体の状態データ (Events用)
    /// </summary>
    [System.Serializable]
    public class GameData
    {
        public int level;
        public float gameTime;
        public int score;
        public int lives;
        public int highScore;
        public string playerName;
        public Dictionary<string, object> customData;

        public GameData()
        {
            customData = new Dictionary<string, object>();
        }
    }

    /// <summary>
    /// プレイヤーデータペイロード (Events用)
    /// </summary>
    [System.Serializable]
    public class PlayerDataPayload
    {
        public string playerName;
        public Vector3 position;
        public Quaternion rotation;
        public float currentHealth;
        public float maxHealth;
        public float currentStamina;
        public float maxStamina;
        public int score;
    }

    #endregion

}