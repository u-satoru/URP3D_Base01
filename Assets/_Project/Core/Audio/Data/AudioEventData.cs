using UnityEngine;
using asterivo.Unity60.Core.Audio;

namespace asterivo.Unity60.Core.Audio.Data
{
    /// <summary>
    /// 拡張された音響イベントデータ
    /// 既存のステルス機能を維持しつつ新機能を追加
    /// </summary>
    [System.Serializable]
    public struct AudioEventData
    {
        // === 既存フィールド（維持） ===
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
        
        // === 新規追加フィールド ===
        [Header("カテゴリ統合機能")]
        public AudioCategory category;              // 音響カテゴリ
        public bool affectsStealthGameplay;        // ステルスゲームプレイに影響するか
        public float maskingStrength;              // この音が提供するマスキング効果の強度
        public bool canBeDuckedByTension;          // 緊張状態で音量を下げるか
        public int layerPriority;                  // レイヤー内での優先度（高い値ほど優先）
        
        [Header("オーディオクリップ")]
        public AudioClip audioClip;                // 再生するオーディオクリップ
        
        /// <summary>
        /// デフォルト値でデータを初期化（既存互換性維持）
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
                timestamp = Time.time,
                // 新規フィールドのデフォルト値
                category = AudioCategory.Stealth,
                affectsStealthGameplay = true,
                maskingStrength = 0f,
                canBeDuckedByTension = false,
                layerPriority = 0
            };
        }
        
        /// <summary>
        /// ステルス音響用のデフォルト作成
        /// </summary>
        public static AudioEventData CreateStealthDefault(string soundID)
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
                timestamp = Time.time,
                category = AudioCategory.Stealth,
                affectsStealthGameplay = true,
                maskingStrength = 0f,
                canBeDuckedByTension = false,
                layerPriority = 100 // ステルス音響は高優先度
            };
        }
        
        /// <summary>
        /// BGM用のデフォルト作成
        /// </summary>
        public static AudioEventData CreateBGMDefault(string soundID)
        {
            return new AudioEventData
            {
                soundID = soundID,
                volume = 0.8f,
                pitch = 1f,
                use3D = false,
                hearingRadius = 0f,
                canBemasked = false,
                priority = 0.1f,
                timestamp = Time.time,
                category = AudioCategory.BGM,
                affectsStealthGameplay = false,
                maskingStrength = 0.3f,
                canBeDuckedByTension = true,
                layerPriority = 10
            };
        }
        
        /// <summary>
        /// 環境音用のデフォルト作成
        /// </summary>
        public static AudioEventData CreateAmbientDefault(string soundID)
        {
            return new AudioEventData
            {
                soundID = soundID,
                volume = 0.7f,
                pitch = 1f,
                use3D = true,
                hearingRadius = 15f,
                surfaceType = SurfaceMaterial.Default,
                canBemasked = false,
                priority = 0.2f,
                timestamp = Time.time,
                category = AudioCategory.Ambient,
                affectsStealthGameplay = false,
                maskingStrength = 0.5f,
                canBeDuckedByTension = true,
                layerPriority = 20
            };
        }
        
        /// <summary>
        /// 効果音用のデフォルト作成
        /// </summary>
        public static AudioEventData CreateEffectDefault(string soundID)
        {
            return new AudioEventData
            {
                soundID = soundID,
                volume = 1f,
                pitch = 1f,
                use3D = true,
                hearingRadius = 8f,
                surfaceType = SurfaceMaterial.Default,
                canBemasked = true,
                priority = 0.7f,
                timestamp = Time.time,
                category = AudioCategory.Effect,
                affectsStealthGameplay = true,
                maskingStrength = 0.1f,
                canBeDuckedByTension = false,
                layerPriority = 50
            };
        }
        
        /// <summary>
        /// UI音用のデフォルト作成
        /// </summary>
        public static AudioEventData CreateUIDefault(string soundID)
        {
            return new AudioEventData
            {
                soundID = soundID,
                volume = 0.8f,
                pitch = 1f,
                use3D = false,
                hearingRadius = 0f,
                canBemasked = false,
                priority = 0.3f,
                timestamp = Time.time,
                category = AudioCategory.UI,
                affectsStealthGameplay = false,
                maskingStrength = 0f,
                canBeDuckedByTension = false,
                layerPriority = 5
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