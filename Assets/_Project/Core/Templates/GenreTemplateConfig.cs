using System;
using UnityEngine;

namespace asterivo.Unity60.Core.Templates
{
    /// <summary>
    /// ジャンルテンプレート設定の基盤クラス
    /// ScriptableObjectベースのデータ駆動設計
    /// DESIGN.md Layer 1: Template Configuration Layer準拠
    /// </summary>
    [CreateAssetMenu(fileName = "New Genre Template Config", menuName = "Templates/Genre Template Config")]
    public abstract class GenreTemplateConfig : ScriptableObject
    {
        [Header("基本設定")]
        [SerializeField] private GenreType genreType;
        [SerializeField] private string displayName;
        [SerializeField, TextArea(3, 5)] private string description;
        [SerializeField] private Sprite iconSprite;
        
        [Header("テンプレート情報")]
        [SerializeField] private string version = "1.0.0";
        [SerializeField] private bool isEnabled = true;
        [SerializeField] private int priority;
        
        [Header("学習支援設定")]
        [SerializeField] private bool enableTutorial = true;
        [SerializeField] private string[] learningSteps = new string[5]; // 5段階学習システム
        [SerializeField] private float estimatedLearningTime = 12f; // 時間（70%削減目標：40h→12h）
        
        [Header("パフォーマンス設定")]
        [SerializeField] private int maxConcurrentEntities = 50; // 50体NPC同時稼働対応
        [SerializeField] private float targetFrameRate = 60f;
        [SerializeField] private bool enableOptimizations = true;
        
        /// <summary>ジャンルタイプ</summary>
        public GenreType Genre => genreType;
        
        /// <summary>表示名</summary>
        public string DisplayName => !string.IsNullOrEmpty(displayName) ? displayName : genreType.GetDisplayName();
        
        /// <summary>説明</summary>
        public string Description => description;
        
        /// <summary>アイコン</summary>
        public Sprite Icon => iconSprite;
        
        /// <summary>バージョン</summary>
        public string Version => version;
        
        /// <summary>有効状態</summary>
        public bool IsEnabled => isEnabled;
        
        /// <summary>優先度</summary>
        public int Priority => priority > 0 ? priority : genreType.GetPriority();
        
        /// <summary>チュートリアル有効</summary>
        public bool EnableTutorial => enableTutorial;
        
        /// <summary>学習ステップ</summary>
        public string[] LearningSteps => learningSteps;
        
        /// <summary>推定学習時間（時間）</summary>
        public float EstimatedLearningTime => estimatedLearningTime;
        
        /// <summary>最大同時エンティティ数</summary>
        public int MaxConcurrentEntities => maxConcurrentEntities;
        
        /// <summary>目標フレームレート</summary>
        public float TargetFrameRate => targetFrameRate;
        
        /// <summary>最適化有効</summary>
        public bool EnableOptimizations => enableOptimizations;
        
        /// <summary>
        /// テンプレート設定の初期化
        /// 各ジャンル固有の初期化処理をオーバーライド
        /// </summary>
        public abstract void Initialize();
        
        /// <summary>
        /// テンプレート設定の検証
        /// </summary>
        public virtual bool Validate()
        {
            if (string.IsNullOrEmpty(displayName))
            {
                Debug.LogWarning($"Genre Template Config {name}: Display name is empty");
                return false;
            }
            
            if (estimatedLearningTime <= 0)
            {
                Debug.LogWarning($"Genre Template Config {name}: Invalid learning time");
                return false;
            }
            
            if (maxConcurrentEntities <= 0)
            {
                Debug.LogWarning($"Genre Template Config {name}: Invalid max concurrent entities");
                return false;
            }
            
            return true;
        }
        
        /// <summary>
        /// 15分ゲームプレイ実現チェック
        /// TASK-004受入条件：各ジャンル15分以内で基本ゲームプレイ実現
        /// </summary>
        public abstract bool CanAchieveBasicGameplayIn15Minutes();
        
        /// <summary>
        /// サンプルシーン30秒起動チェック
        /// TASK-004受入条件：サンプルシーン起動30秒以内
        /// </summary>
        public abstract bool CanLoadSampleSceneIn30Seconds();
        
        /// <summary>
        /// テンプレート切り替え3分以内チェック
        /// TASK-004受入条件：テンプレート切り替え時のデータ整合性保証（3分以内）
        /// </summary>
        public abstract bool CanSwitchTemplateIn3Minutes();
        
        #if UNITY_EDITOR
        private void OnValidate()
        {
            if (Application.isPlaying) return;
            
            // ジャンルタイプに基づく自動設定
            if (string.IsNullOrEmpty(displayName))
            {
                displayName = genreType.GetDisplayName();
            }
            
            if (priority <= 0)
            {
                priority = genreType.GetPriority();
            }
            
            // 学習時間の妥当性チェック（70%削減目標：40h→12h以下）
            if (estimatedLearningTime > 12f)
            {
                Debug.LogWarning($"Learning time {estimatedLearningTime}h exceeds target 12h (70% reduction goal)");
            }
        }
        #endif
    }
}
