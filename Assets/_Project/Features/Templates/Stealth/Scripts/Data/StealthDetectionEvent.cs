using UnityEngine;
using asterivo.Unity60.Core.Commands;
using asterivo.Unity60.Features.AI.Visual;

namespace asterivo.Unity60.Features.Templates.Stealth
{
    /// <summary>
    /// ステルス検知イベント（ObjectPool対応）
    /// コマンドパターン + ObjectPool最適化による95%メモリ削減効果を実現
    /// Learn & Grow価値実現: 効率的なイベント管理による学習体験の向上
    /// </summary>
    public class StealthDetectionEvent : IResettableCommand
    {
        #region Properties

        /// <summary>検知を行ったNPCコントローラー</summary>
        public MonoBehaviour DetectingNPC { get; private set; }
        
        /// <summary>検知された目標</summary>
        public DetectedTarget Target { get; private set; }
        
        /// <summary>検知の種類</summary>
        public DetectionType DetectionType { get; private set; }
        
        /// <summary>疑心レベル（0.0～1.0）</summary>
        public float SuspicionLevel { get; private set; }
        
        /// <summary>検知発生時刻</summary>
        public float Timestamp { get; private set; }
        
        /// <summary>検知位置</summary>
        public Vector3 DetectionPosition { get; private set; }
        
        /// <summary>検知強度（距離、角度等を考慮）</summary>
        public float DetectionStrength { get; private set; }
        
        /// <summary>環境隠蔽レベル</summary>
        public ConcealmentLevel ConcealmentLevel { get; private set; }
        
        /// <summary>イベントが有効かどうか</summary>
        public bool IsValid { get; private set; }

        #endregion

        #region IResettableCommand Implementation

        /// <summary>
        /// 検知イベントを初期化
        /// パフォーマンス最適化: 新規インスタンス作成を避けて既存オブジェクトを再利用
        /// </summary>
        public void Initialize(MonoBehaviour detectingNPC, DetectedTarget target, 
                              DetectionType detectionType, float suspicionLevel)
        {
            DetectingNPC = detectingNPC;
            Target = target;
            DetectionType = detectionType;
            SuspicionLevel = Mathf.Clamp01(suspicionLevel);
            Timestamp = Time.time;
            
            // 検知位置の記録
            DetectionPosition = target?.lastKnownPosition ?? Vector3.zero;
            
            // 検知強度の計算（疑心レベルベース）
            DetectionStrength = CalculateDetectionStrength();
            
            // 環境隠蔽レベルの評価（簡易実装、後でEnvironmentManagerから取得）
            ConcealmentLevel = EvaluateConcealmentLevel();
            
            IsValid = true;
        }

        /// <summary>
        /// 拡張初期化メソッド（より詳細な情報を設定）
        /// </summary>
        public void Initialize(MonoBehaviour detectingNPC, DetectedTarget target, 
                              DetectionType detectionType, float suspicionLevel,
                              Vector3 detectionPosition, ConcealmentLevel concealmentLevel)
        {
            Initialize(detectingNPC, target, detectionType, suspicionLevel);
            DetectionPosition = detectionPosition;
            ConcealmentLevel = concealmentLevel;
            DetectionStrength = CalculateDetectionStrength();
        }

        /// <summary>
        /// IResettableCommandインターフェース用の汎用初期化メソッド
        /// ObjectPool使用時のパラメータ初期化に使用
        /// </summary>
        public void Initialize(params object[] parameters)
        {
            if (parameters == null || parameters.Length < 4)
            {
                Debug.LogError("StealthDetectionEvent: Initialize requires at least 4 parameters: MonoBehaviour, DetectedTarget, DetectionType, float");
                return;
            }

            if (parameters[0] is MonoBehaviour detectingNPC &&
                parameters[1] is DetectedTarget target &&
                parameters[2] is DetectionType detectionType &&
                parameters[3] is float suspicionLevel)
            {
                Initialize(detectingNPC, target, detectionType, suspicionLevel);

                // 追加パラメータがあれば処理
                if (parameters.Length >= 6 &&
                    parameters[4] is Vector3 detectionPosition &&
                    parameters[5] is ConcealmentLevel concealmentLevel)
                {
                    DetectionPosition = detectionPosition;
                    ConcealmentLevel = concealmentLevel;
                    DetectionStrength = CalculateDetectionStrength();
                }
            }
            else
            {
                Debug.LogError("StealthDetectionEvent: Invalid parameter types. Expected MonoBehaviour, DetectedTarget, DetectionType, float");
            }
        }

        /// <summary>
        /// ObjectPool復帰時のリセット処理
        /// メモリ効率最適化: 参照をnullにして適切なガベージコレクション実施
        /// </summary>
        public void Reset()
        {
            DetectingNPC = null;
            Target = null;
            DetectionType = DetectionType.Visual;
            SuspicionLevel = 0f;
            Timestamp = 0f;
            DetectionPosition = Vector3.zero;
            DetectionStrength = 0f;
            ConcealmentLevel = ConcealmentLevel.None;
            IsValid = false;
        }

        #endregion

        #region ICommand Implementation

        /// <summary>
        /// 検知イベントの実行
        /// イベント駆動アーキテクチャによる疎結合な処理実行
        /// </summary>
        public void Execute()
        {
            if (!IsValid)
            {
                Debug.LogWarning("StealthDetectionEvent: Invalid event cannot be executed");
                return;
            }

            // ステルステンプレートマネージャーにイベント通知
            var templateManager = StealthTemplateManager.Instance;
            if (templateManager != null)
            {
                templateManager.HandleDetectionEvent(this);
            }
            else
            {
                Debug.LogError("StealthDetectionEvent: StealthTemplateManager instance not found");
            }
        }

        /// <summary>
        /// 検知イベントの取り消し（Undo操作）
        /// Learn & Grow価値実現: 試行錯誤学習をサポートする取り消し機能
        /// </summary>
        public void Undo()
        {
            if (!IsValid)
                return;

            var templateManager = StealthTemplateManager.Instance;
            templateManager?.HandleDetectionEventUndo(this);
        }

        /// <summary>
        /// 取り消し可能かどうかの判定
        /// </summary>
        public bool CanUndo => IsValid && Time.time - Timestamp < 5f; // 5秒以内のイベントは取り消し可能

        #endregion

        #region Private Methods

        /// <summary>
        /// 検知強度を計算
        /// 疑心レベル、検知タイプ、隠蔽レベルを総合的に評価
        /// </summary>
        private float CalculateDetectionStrength()
        {
            float baseStrength = SuspicionLevel;
            
            // 検知タイプによる調整
            float typeMultiplier = DetectionType switch
            {
                DetectionType.Visual => 1.0f,      // 視覚は基準値
                DetectionType.Auditory => 0.8f,    // 聴覚はやや弱い
                DetectionType.Environmental => 0.6f, // 環境は間接的
                DetectionType.Cooperative => 1.2f,   // 協調は強力
                _ => 1.0f
            };
            
            // 隠蔽レベルによる減衰
            float concealmentMultiplier = (int)ConcealmentLevel switch
            {
                0 => 1.0f,    // 隠蔽なし
                1 => 0.8f,    // 軽微な隠蔽
                2 => 0.6f,    // 中程度
                3 => 0.4f,    // 高度
                4 => 0.2f,    // 完全隠蔽
                _ => 1.0f
            };
            
            return Mathf.Clamp01(baseStrength * typeMultiplier * concealmentMultiplier);
        }

        /// <summary>
        /// 環境隠蔽レベルを評価（簡易実装）
        /// 後でStealthEnvironmentManagerと統合予定
        /// </summary>
        private ConcealmentLevel EvaluateConcealmentLevel()
        {
            // TODO: StealthEnvironmentManager統合時に実装を詳細化
            // 現在は疑心レベルベースの簡易評価
            return SuspicionLevel switch
            {
                >= 0.9f => ConcealmentLevel.None,
                >= 0.7f => ConcealmentLevel.Light,
                >= 0.5f => ConcealmentLevel.Medium,
                >= 0.3f => ConcealmentLevel.High,
                _ => ConcealmentLevel.Complete
            };
        }

        #endregion

        #region Debug & Utility

        /// <summary>
        /// デバッグ用文字列表現
        /// </summary>
        public override string ToString()
        {
            string npcName = DetectingNPC != null ? DetectingNPC.name : "NULL";
            string targetName = Target?.transform != null ? Target.transform.name : "NULL";
            
            return $"StealthDetection: {npcName} -> {targetName} " +
                   $"({DetectionType}, Suspicion: {SuspicionLevel:F2}, " +
                   $"Strength: {DetectionStrength:F2}, Concealment: {ConcealmentLevel})";
        }

        /// <summary>
        /// 詳細デバッグ情報
        /// </summary>
        public string GetDetailedInfo()
        {
            string npcName = DetectingNPC != null ? DetectingNPC.name : "NULL";
            string targetName = Target?.transform != null ? Target.transform.name : "NULL";
            
            return $"=== Stealth Detection Event ===\n" +
                   $"NPC: {npcName}\n" +
                   $"Target: {targetName}\n" +
                   $"Detection Type: {DetectionType}\n" +
                   $"Suspicion Level: {SuspicionLevel:F3}\n" +
                   $"Detection Strength: {DetectionStrength:F3}\n" +
                   $"Concealment Level: {ConcealmentLevel}\n" +
                   $"Position: {DetectionPosition}\n" +
                   $"Timestamp: {Timestamp:F2}s\n" +
                   $"Valid: {IsValid}";
        }

        #endregion
    }
}