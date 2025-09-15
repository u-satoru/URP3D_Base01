using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections.Generic;

namespace asterivo.Unity60.Features.Templates.Platformer
{
    /// <summary>
    /// プラットフォーマー学習支援・教育価値設定クラス
    /// Learn & Grow教育価値システム（学習コスト70%削減目標）
    /// 5段階学習システム・段階的成長支援・15分完全ゲームプレイ体験
    /// </summary>
    [System.Serializable]
    public class PlatformerLearningSettings
    {
        [BoxGroup("Learn & Grow System")]
        [LabelText("学習システム有効")]
        [SerializeField] private bool enableLearningSystem = true;

        [LabelText("学習コスト削減目標"), Range(50, 90)]
        [ShowIf("enableLearningSystem")]
        [SerializeField] private int learningCostReduction = 70; // 70%削減目標

        [LabelText("目標学習時間（時間）"), Range(8, 20)]
        [ShowIf("enableLearningSystem")]
        [SerializeField] private int targetLearningHours = 12; // 40時間→12時間

        [BoxGroup("5-Phase Learning System")]
        [LabelText("段階的学習有効")]
        [SerializeField] private bool enableProgressiveLearning = true;

        [LabelText("現在フェーズ"), Range(1, 5)]
        [ShowIf("enableProgressiveLearning")]
        [SerializeField] private int currentPhase = 1;

        [LabelText("総フェーズ数"), Range(3, 7)]
        [ShowIf("enableProgressiveLearning")]
        [SerializeField] private int totalPhases = 5;

        [LabelText("フェーズ自動進行")]
        [ShowIf("enableProgressiveLearning")]
        [SerializeField] private bool autoPhaseProgression = true;

        [BoxGroup("Phase 1: 基礎")]
        [LabelText("基礎操作学習")]
        [SerializeField] private bool enableBasicControls = true;

        [LabelText("移動練習時間（分）"), Range(2, 10)]
        [ShowIf("enableBasicControls")]
        [SerializeField] private float movementPracticeTime = 3f;

        [LabelText("ジャンプ練習時間（分）"), Range(2, 10)]
        [ShowIf("enableBasicControls")]
        [SerializeField] private float jumpPracticeTime = 4f;

        [BoxGroup("Phase 2: 応用")]
        [LabelText("応用テクニック学習")]
        [SerializeField] private bool enableAdvancedTechniques = true;

        [LabelText("コンボ操作練習")]
        [ShowIf("enableAdvancedTechniques")]
        [SerializeField] private bool enableComboTraining = true;

        [LabelText("タイミング練習")]
        [ShowIf("enableAdvancedTechniques")]
        [SerializeField] private bool enableTimingTraining = true;

        [BoxGroup("Phase 3: 実践")]
        [LabelText("実践ゲームプレイ")]
        [SerializeField] private bool enablePracticalGameplay = true;

        [LabelText("チャレンジレベル数"), Range(3, 10)]
        [ShowIf("enablePracticalGameplay")]
        [SerializeField] private int challengeLevelCount = 5;

        [LabelText("目標クリア時間（分）"), Range(10, 20)]
        [ShowIf("enablePracticalGameplay")]
        [SerializeField] private float targetCompletionTime = 15f; // 15分ゲームプレイ

        [BoxGroup("Phase 4: カスタマイズ")]
        [LabelText("カスタマイズ学習")]
        [SerializeField] private bool enableCustomization = true;

        [LabelText("設定調整学習")]
        [ShowIf("enableCustomization")]
        [SerializeField] private bool enableSettingsLearning = true;

        [LabelText("レベルエディタ学習")]
        [ShowIf("enableCustomization")]
        [SerializeField] private bool enableLevelEditorLearning = false;

        [BoxGroup("Phase 5: 出版")]
        [LabelText("出版・共有学習")]
        [SerializeField] private bool enablePublishing = false; // 将来機能

        [LabelText("コミュニティ統合")]
        [ShowIf("enablePublishing")]
        [SerializeField] private bool enableCommunityIntegration = false;

        [BoxGroup("Interactive Tutorial")]
        [LabelText("インタラクティブチュートリアル")]
        [SerializeField] private bool enableInteractiveTutorial = true;

        [LabelText("リアルタイムヒント")]
        [ShowIf("enableInteractiveTutorial")]
        [SerializeField] private bool enableRealtimeHints = true;

        [LabelText("ヒント表示時間（秒）"), Range(3, 10)]
        [ShowIf("@enableInteractiveTutorial && enableRealtimeHints")]
        [SerializeField] private float hintDisplayTime = 5f;

        [LabelText("視覚的ガイド")]
        [ShowIf("enableInteractiveTutorial")]
        [SerializeField] private bool enableVisualGuides = true;

        [BoxGroup("Progress Tracking")]
        [LabelText("進捗追跡有効")]
        [SerializeField] private bool enableProgressTracking = true;

        [LabelText("スキル評価システム")]
        [ShowIf("enableProgressTracking")]
        [SerializeField] private bool enableSkillAssessment = true;

        [LabelText("学習統計収集")]
        [ShowIf("enableProgressTracking")]
        [SerializeField] private bool enableLearningAnalytics = true;

        [LabelText("成果可視化")]
        [ShowIf("enableProgressTracking")]
        [SerializeField] private bool enableAchievementVisualization = true;

        [BoxGroup("Adaptive Learning")]
        [LabelText("適応的学習")]
        [SerializeField] private bool enableAdaptiveLearning = true;

        [LabelText("難易度自動調整")]
        [ShowIf("enableAdaptiveLearning")]
        [SerializeField] private bool enableDynamicDifficulty = true;

        [LabelText("学習ペース調整")]
        [ShowIf("enableAdaptiveLearning")]
        [SerializeField] private bool enablePaceAdjustment = true;

        [LabelText("個人最適化")]
        [ShowIf("enableAdaptiveLearning")]
        [SerializeField] private bool enablePersonalization = true;

        [BoxGroup("Assessment")]
        [LabelText("スキル評価間隔（分）"), Range(5, 30)]
        [SerializeField] private float assessmentInterval = 10f;

        [LabelText("合格基準（%）"), Range(60, 95)]
        [SerializeField] private int passingGrade = 80;

        [LabelText("リトライ回数制限"), Range(1, 5)]
        [SerializeField] private int maxRetryAttempts = 3;

        #region Public Properties
        public bool EnableLearningSystem => enableLearningSystem;
        public int LearningCostReduction => learningCostReduction;
        public int TargetLearningHours => targetLearningHours;
        public bool EnableProgressiveLearning => enableProgressiveLearning;
        public int CurrentPhase => currentPhase;
        public int TotalPhases => totalPhases;
        public bool AutoPhaseProgression => autoPhaseProgression;
        public bool EnableBasicControls => enableBasicControls;
        public float MovementPracticeTime => movementPracticeTime;
        public float JumpPracticeTime => jumpPracticeTime;
        public bool EnableAdvancedTechniques => enableAdvancedTechniques;
        public bool EnableComboTraining => enableComboTraining;
        public bool EnableTimingTraining => enableTimingTraining;
        public bool EnablePracticalGameplay => enablePracticalGameplay;
        public int ChallengeLevelCount => challengeLevelCount;
        public float TargetCompletionTime => targetCompletionTime;
        public bool EnableCustomization => enableCustomization;
        public bool EnableSettingsLearning => enableSettingsLearning;
        public bool EnableLevelEditorLearning => enableLevelEditorLearning;
        public bool EnablePublishing => enablePublishing;
        public bool EnableCommunityIntegration => enableCommunityIntegration;
        public bool EnableInteractiveTutorial => enableInteractiveTutorial;
        public bool EnableRealtimeHints => enableRealtimeHints;
        public float HintDisplayTime => hintDisplayTime;
        public bool EnableVisualGuides => enableVisualGuides;
        public bool EnableProgressTracking => enableProgressTracking;
        public bool EnableSkillAssessment => enableSkillAssessment;
        public bool EnableLearningAnalytics => enableLearningAnalytics;
        public bool EnableAchievementVisualization => enableAchievementVisualization;
        public bool EnableAdaptiveLearning => enableAdaptiveLearning;
        public bool EnableDynamicDifficulty => enableDynamicDifficulty;
        public bool EnablePaceAdjustment => enablePaceAdjustment;
        public bool EnablePersonalization => enablePersonalization;
        public float AssessmentInterval => assessmentInterval;
        public int PassingGrade => passingGrade;
        public int MaxRetryAttempts => maxRetryAttempts;
        #endregion

        #region Learning Phase Data
        [System.Serializable]
        public class LearningPhase
        {
            public string phaseName;
            public string description;
            public float estimatedDuration; // minutes
            public List<string> learningObjectives;
            public List<string> assessmentCriteria;
            public bool isCompleted;
            public float completionScore;
        }

        private LearningPhase[] _phases;

        public LearningPhase[] GetLearningPhases()
        {
            if (_phases == null)
            {
                InitializeLearningPhases();
            }
            return _phases;
        }

        private void InitializeLearningPhases()
        {
            _phases = new LearningPhase[5]
            {
                new LearningPhase
                {
                    phaseName = "Phase 1: 基礎操作",
                    description = "移動・ジャンプ・基本操作の習得",
                    estimatedDuration = movementPracticeTime + jumpPracticeTime,
                    learningObjectives = new List<string>
                    {
                        "WASD移動操作の習得",
                        "スペースキージャンプの習得",
                        "マウス視点操作の習得",
                        "基本的な3D空間認識"
                    },
                    assessmentCriteria = new List<string>
                    {
                        "指定ルートを3分以内で移動完了",
                        "ジャンプ課題を5回連続成功",
                        "視点操作で目標を3秒以内に捕捉"
                    }
                },
                new LearningPhase
                {
                    phaseName = "Phase 2: 応用テクニック",
                    description = "コンボ操作・タイミング・応用技術の習得",
                    estimatedDuration = 120f, // 2 hours
                    learningObjectives = new List<string>
                    {
                        "ダッシュ・ローリング操作",
                        "壁ジャンプ・二段ジャンプ",
                        "タイミング重視のアクション",
                        "コンボ操作の連携"
                    },
                    assessmentCriteria = new List<string>
                    {
                        "壁ジャンプ課題を80%以上の精度で完了",
                        "コンボ操作を3回連続成功",
                        "タイミング課題を規定時間内完了"
                    }
                },
                new LearningPhase
                {
                    phaseName = "Phase 3: 実践ゲームプレイ",
                    description = "実際のゲームレベルでの総合的な技能実践",
                    estimatedDuration = targetCompletionTime,
                    learningObjectives = new List<string>
                    {
                        "総合的なプラットフォームアクション",
                        "戦略的なルート選択",
                        "時間管理とリソース効率",
                        "チャレンジレベルの攻略"
                    },
                    assessmentCriteria = new List<string>
                    {
                        "チャレンジレベル80%以上クリア",
                        "15分以内での完全ゲームプレイ体験",
                        "効率的なルート選択の実証"
                    }
                },
                new LearningPhase
                {
                    phaseName = "Phase 4: カスタマイズ",
                    description = "ゲーム設定・カスタマイズ・個人最適化",
                    estimatedDuration = 90f, // 1.5 hours
                    learningObjectives = new List<string>
                    {
                        "設定画面の理解と操作",
                        "個人プレイスタイルの最適化",
                        "パフォーマンス調整の理解",
                        "アクセシビリティ設定の活用"
                    },
                    assessmentCriteria = new List<string>
                    {
                        "設定変更による性能向上の実証",
                        "個人最適化設定の完了",
                        "カスタマイズ機能の理解度テスト"
                    }
                },
                new LearningPhase
                {
                    phaseName = "Phase 5: 出版・共有",
                    description = "コミュニティ参加・成果共有・継続学習",
                    estimatedDuration = 60f, // 1 hour
                    learningObjectives = new List<string>
                    {
                        "成果の記録と共有",
                        "コミュニティ参加",
                        "継続学習計画の策定",
                        "他ジャンル展開の準備"
                    },
                    assessmentCriteria = new List<string>
                    {
                        "学習成果レポートの作成",
                        "コミュニティ活動への参加",
                        "継続学習目標の設定"
                    }
                }
            };
        }
        #endregion

        #region Initialization & Validation
        public void Initialize()
        {
            // 学習システム設定の妥当性確認
            learningCostReduction = Mathf.Clamp(learningCostReduction, 50, 90);
            targetLearningHours = Mathf.Clamp(targetLearningHours, 8, 20);
            totalPhases = Mathf.Clamp(totalPhases, 3, 7);
            currentPhase = Mathf.Clamp(currentPhase, 1, totalPhases);

            InitializeLearningPhases();

            Debug.Log($"[PlatformerLearning] Initialized: Learning System Active, Target: {learningCostReduction}% reduction ({targetLearningHours}h)");
        }

        public bool Validate()
        {
            bool isValid = true;

            // 学習時間目標検証
            if (enableLearningSystem && targetLearningHours < 8)
            {
                Debug.LogError("[PlatformerLearning] Target learning hours too low for effective learning");
                isValid = false;
            }

            // フェーズ設定検証
            if (enableProgressiveLearning && totalPhases < 3)
            {
                Debug.LogError("[PlatformerLearning] Minimum 3 phases required for progressive learning");
                isValid = false;
            }

            // 15分ゲームプレイ要件検証
            if (enablePracticalGameplay && targetCompletionTime > 20f)
            {
                Debug.LogError("[PlatformerLearning] Target completion time exceeds 15-minute gameplay requirement");
                isValid = false;
            }

            // 合格基準検証
            if (passingGrade < 60 || passingGrade > 95)
            {
                Debug.LogError("[PlatformerLearning] Passing grade must be between 60% and 95%");
                isValid = false;
            }

            return isValid;
        }

        public void ApplyRecommendedSettings()
        {
            // Learn & Grow価値実現のための推奨設定
            enableLearningSystem = true;
            learningCostReduction = 70;        // 70%削減目標
            targetLearningHours = 12;          // 40時間→12時間

            enableProgressiveLearning = true;
            currentPhase = 1;                  // 基礎から開始
            totalPhases = 5;                   // 5段階学習システム
            autoPhaseProgression = true;       // 自動進行

            // Phase 1: 基礎 - 短時間で効果的
            enableBasicControls = true;
            movementPracticeTime = 3f;         // 3分間の移動練習
            jumpPracticeTime = 4f;             // 4分間のジャンプ練習

            // Phase 2: 応用 - 実用的スキル
            enableAdvancedTechniques = true;
            enableComboTraining = true;        // コンボ操作習得
            enableTimingTraining = true;       // タイミング重視

            // Phase 3: 実践 - 15分ゲームプレイ実現
            enablePracticalGameplay = true;
            challengeLevelCount = 5;           // 適度なチャレンジ数
            targetCompletionTime = 15f;        // 15分完全体験

            // Phase 4: カスタマイズ - 個人最適化
            enableCustomization = true;
            enableSettingsLearning = true;     // 設定理解促進
            enableLevelEditorLearning = false; // シンプル化

            // Phase 5: 出版 - 将来拡張
            enablePublishing = false;          // 段階的導入
            enableCommunityIntegration = false;

            // インタラクティブ機能 - 学習効率向上
            enableInteractiveTutorial = true;
            enableRealtimeHints = true;        // リアルタイム支援
            hintDisplayTime = 5f;              // 適度な表示時間
            enableVisualGuides = true;         // 視覚的理解促進

            // 進捗追跡 - 成果可視化
            enableProgressTracking = true;
            enableSkillAssessment = true;      // スキル評価
            enableLearningAnalytics = true;    // 学習分析
            enableAchievementVisualization = true; // 成果表示

            // 適応的学習 - 個人対応
            enableAdaptiveLearning = true;
            enableDynamicDifficulty = true;    // 難易度自動調整
            enablePaceAdjustment = true;       // ペース調整
            enablePersonalization = true;      // 個人最適化

            // 評価設定 - 適切な基準
            assessmentInterval = 10f;          // 10分間隔評価
            passingGrade = 80;                 // 80%合格基準
            maxRetryAttempts = 3;              // 3回リトライ

            Debug.Log("[PlatformerLearning] Applied recommended settings for Learn & Grow value realization (70% learning cost reduction)");
        }
        #endregion

        #region Learning Progress Calculations
        /// <summary>
        /// 現在の学習進捗計算
        /// </summary>
        /// <returns>進捗率（0.0-1.0）</returns>
        public float CalculateLearningProgress()
        {
            if (!enableLearningSystem) return 0f;

            var phases = GetLearningPhases();
            float totalProgress = 0f;

            for (int i = 0; i < phases.Length; i++)
            {
                if (phases[i].isCompleted)
                {
                    totalProgress += 1f;
                }
                else if (i == currentPhase - 1)
                {
                    // 現在フェーズの部分進捗
                    totalProgress += phases[i].completionScore / 100f;
                }
            }

            return totalProgress / phases.Length;
        }

        /// <summary>
        /// 学習コスト削減効果計算
        /// </summary>
        /// <param name="originalHours">元の学習時間</param>
        /// <returns>削減後の学習時間</returns>
        public float CalculateReducedLearningTime(float originalHours)
        {
            if (!enableLearningSystem) return originalHours;

            float reductionFactor = learningCostReduction / 100f;
            return originalHours * (1f - reductionFactor);
        }

        /// <summary>
        /// スキル評価スコア計算
        /// </summary>
        /// <param name="completionTime">完了時間</param>
        /// <param name="targetTime">目標時間</param>
        /// <param name="accuracy">精度</param>
        /// <returns>評価スコア（0-100）</returns>
        public int CalculateSkillScore(float completionTime, float targetTime, float accuracy)
        {
            if (!enableSkillAssessment) return 0;

            // 時間効率スコア（50%）
            float timeEfficiency = Mathf.Clamp01(targetTime / completionTime);
            int timeScore = Mathf.RoundToInt(timeEfficiency * 50f);

            // 精度スコア（50%）
            int accuracyScore = Mathf.RoundToInt(accuracy * 50f);

            return Mathf.Clamp(timeScore + accuracyScore, 0, 100);
        }

        /// <summary>
        /// 次フェーズへの進行可能性判定
        /// </summary>
        /// <param name="currentScore">現在スコア</param>
        /// <returns>進行可能かどうか</returns>
        public bool CanProgressToNextPhase(int currentScore)
        {
            if (!enableProgressiveLearning) return false;
            if (currentPhase >= totalPhases) return false;

            return currentScore >= passingGrade;
        }

        /// <summary>
        /// 個人最適化レコメンデーション
        /// </summary>
        /// <param name="playerStats">プレイヤー統計</param>
        /// <returns>推奨設定</returns>
        public string GetPersonalizationRecommendation(Dictionary<string, float> playerStats)
        {
            if (!enablePersonalization || playerStats == null) return "";

            var recommendations = new List<string>();

            // 移動速度に基づく推奨
            if (playerStats.ContainsKey("MovementAccuracy") && playerStats["MovementAccuracy"] < 0.7f)
            {
                recommendations.Add("移動速度を下げて精度を向上させることを推奨");
            }

            // ジャンプタイミングに基づく推奨
            if (playerStats.ContainsKey("JumpTiming") && playerStats["JumpTiming"] < 0.6f)
            {
                recommendations.Add("ジャンプタイミング練習の追加を推奨");
            }

            // 完了時間に基づく推奨
            if (playerStats.ContainsKey("CompletionTime") && playerStats["CompletionTime"] > targetCompletionTime * 1.5f)
            {
                recommendations.Add("難易度を下げて学習ペースを調整することを推奨");
            }

            return string.Join(", ", recommendations);
        }
        #endregion

        #region Progress Report Generation
        /// <summary>
        /// 学習進捗レポート生成
        /// </summary>
        /// <returns>詳細レポート</returns>
        public string GenerateProgressReport()
        {
            if (!enableProgressTracking) return "Progress tracking is disabled.";

            var report = new System.Text.StringBuilder();
            report.AppendLine("=== Platformer Learning Progress Report ===");
            report.AppendLine($"Learning Cost Reduction Target: {learningCostReduction}%");
            report.AppendLine($"Target Learning Hours: {targetLearningHours}h (from 40h)");
            report.AppendLine($"Overall Progress: {CalculateLearningProgress():P1}");
            report.AppendLine();

            var phases = GetLearningPhases();
            for (int i = 0; i < phases.Length; i++)
            {
                var phase = phases[i];
                string status = phase.isCompleted ? "✅ Completed" :
                               (i == currentPhase - 1) ? "🚧 In Progress" : "⏳ Pending";

                report.AppendLine($"Phase {i + 1}: {phase.phaseName} - {status}");
                report.AppendLine($"  Duration: {phase.estimatedDuration:F1} minutes");
                report.AppendLine($"  Score: {phase.completionScore:F1}%");
                report.AppendLine();
            }

            if (enableLearningAnalytics)
            {
                report.AppendLine("=== Learning Analytics ===");
                report.AppendLine($"Estimated Completion: {targetLearningHours}h");
                report.AppendLine($"15-minute Gameplay Target: {(targetCompletionTime <= 15f ? "✅ Met" : "❌ Needs improvement")}");
            }

            return report.ToString();
        }
        #endregion

        #region Editor Support
#if UNITY_EDITOR
        [Button("Generate Learning Report")]
        [PropertySpace(10)]
        public void GenerateLearningReport()
        {
            string report = GenerateProgressReport();
            Debug.Log(report);
        }

        [Button("Test Learning Progress")]
        public void TestLearningProgress()
        {
            float progress = CalculateLearningProgress();
            Debug.Log($"Current Learning Progress: {progress:P1}");

            float reducedTime = CalculateReducedLearningTime(40f);
            Debug.Log($"Reduced Learning Time: {reducedTime:F1}h (from 40h, {learningCostReduction}% reduction)");
        }

        [Button("Validate Learning Settings")]
        public void EditorValidate()
        {
            bool isValid = Validate();
            string message = isValid ?
                "✅ Learning settings are valid for Learn & Grow value!" :
                "❌ Learning settings validation failed!";
            Debug.Log($"[PlatformerLearning] {message}");
        }

        [Button("Simulate Phase Progression")]
        public void SimulatePhaseProgression()
        {
            for (int i = 1; i <= totalPhases; i++)
            {
                bool canProgress = CanProgressToNextPhase(85); // 85% score simulation
                Debug.Log($"Phase {i}: Can progress = {canProgress}");
            }
        }
#endif
        #endregion
    }
}