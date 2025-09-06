using UnityEngine;

namespace asterivo.Unity60.Core.Audio.Data
{
    /// <summary>
    /// 音響イベントで渡されるデータ構造
    /// ステルスゲームに必要な音響情報を包含
    /// </summary>
    [System.Serializable]
    public struct AudioEventData
    {
        [Header("基本音響情報")]
        public string soundID;
        public float volume;
        public float pitch;
        
        [Header("空間情報")]
        public Vector3 worldPosition;
        public bool use3D;
        
        [Header("ゲームプレイ情報")]
        public AudioSourceType sourceType;
        public bool isPlayerGenerated;
        public float timestamp;
        
        [Header("ステルス特化設定")]
        public float hearingRadius;        // NPCが聞き取れる範囲
        public SurfaceMaterial surfaceType; // 音を発生させる表面素材
        public bool canBemasked;           // 他の音でマスクされ得るか
        public float priority;             // 音の優先度（NPCの注意を引く度合い）
        
        /// <summary>
        /// デフォルト値でデータを初期化
        /// </summary>
        public static AudioEventData CreateDefault(string soundID)
        {
            return new AudioEventData
            {
                soundID = soundID,
                volume = 1f,
                pitch = 1f,
                use3D = true,
                hearingRadius = 5f,
                surfaceType = SurfaceMaterial.Default,
                canBemasked = true,
                priority = 0.5f,
                timestamp = Time.time
            };
        }
    }
    
    /// <summary>
    /// 音源の種類を定義
    /// </summary>
    public enum AudioSourceType
    {
        Player,           // プレイヤーが発生させた音
        Environment,      // 環境音（風、雨など）
        Interactive,      // インタラクション音（ドア、ボタンなど）
        NPC,             // NPCが発生させた音
        Ambient,         // アンビエント音（BGM的な音）
        UI               // UIサウンド
    }
    
    /// <summary>
    /// 表面素材による音響特性の定義
    /// </summary>
    public enum SurfaceMaterial
    {
        Default,         // デフォルト
        Concrete,        // コンクリート（音が響く）
        Carpet,          // カーペット（音を吸収）
        Metal,           // 金属（音が響く、反響）
        Wood,            // 木材（適度な響き）
        Grass,           // 草地（音を吸収）
        Water,           // 水（特殊な音響）
        Gravel           // 砂利（特徴的な音）
    }
}