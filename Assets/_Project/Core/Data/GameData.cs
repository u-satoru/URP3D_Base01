using UnityEngine;
using System.Collections.Generic;

namespace asterivo.Unity60.Core.Data
{
    /// <summary>
    /// ゲームデータのペイロード
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
    /// プレイヤーデータのペイロード
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
}