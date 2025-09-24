using UnityEngine;
using Sirenix.OdinInspector;

namespace asterivo.Unity60.Features.Templates.Stealth.Settings
{
    /// <summary>
    /// ステルステンプレート: ミッション設定
    /// ミッション構造、目標、15分ゲームプレイ体験設計
    /// Learn & Grow価値実現のための段階的ミッション進行
    /// </summary>
    [CreateAssetMenu(menuName = "Templates/Stealth/Settings/Mission Settings", fileName = "StealthMissionSettings")]
    public class StealthMissionSettings : ScriptableObject
    {
        #region 15-Minute Gameplay Structure

        [TabGroup("Gameplay", "15-Min Experience")]
        [Title("15分ゲームプレイ体験", "Clone & Create価値実現", TitleAlignments.Centered)]
        [SerializeField]
        [Tooltip("15分体験有効化")]
        private bool enable15MinuteExperience = true;

        [SerializeField]
        [Tooltip("15分体験フェーズ構成")]
        private GameplayPhase[] gameplayPhases = new GameplayPhase[]
        {
            new GameplayPhase 
            { 
                PhaseName = "Tutorial",
                Duration = 180f, // 3 minutes
                Description = "基本操作習得",
                MainObjective = "基本的なステルス移動をマスターする",
                LearningFocus = "Movement, Crouching, Basic Stealth"
            },
            new GameplayPhase 
            { 
                PhaseName = "Practice",
                Duration = 300f, // 5 minutes
                Description = "AI検知システム理解",
                MainObjective = "NPCを避けながら目標地点に到達する",
                LearningFocus = "AI Behavior, Detection System, Cover Usage"
            },
            new GameplayPhase 
            { 
                PhaseName = "Challenge",
                Duration = 420f, // 7 minutes
                Description = "総合ステルスミッション",
                MainObjective = "複数の目標を達成する総合ミッション",
                LearningFocus = "Advanced Techniques, Mission Planning"
            }
        };

        [SerializeField, Range(0.5f, 2f)]
        [Tooltip("フェーズ遷移時間倍率")]
        private float phaseTransitionMultiplier = 1f;

        [TabGroup("Gameplay", "Mission Flow")]
        [Title("ミッション進行管理", "スムーズな体験流れ", TitleAlignments.Centered)]
        [SerializeField]
        [Tooltip("自動フェーズ進行")]
        private bool enableAutoPhaseProgression = true;

        [SerializeField]
        [Tooltip("失敗時の自動リスタート")]
        private bool enableAutoRestart = true;

        [SerializeField, Range(1, 5)]
        [Tooltip("最大リスタート回数")]
        private int maxRestartAttempts = 3;

        [SerializeField]
        [Tooltip("適応的難易度調整")]
        private bool enableAdaptiveDifficulty = true;

        #endregion

        #region Mission Objectives System

        [TabGroup("Objectives", "Primary Goals")]
        [Title("主要目標システム", "ミッション核心目標", TitleAlignments.Centered)]
        [SerializeField]
        [Tooltip("主要目標設定")]
        private StealthObjective[] primaryObjectives = new StealthObjective[]
        {
            new StealthObjective 
            { 
                ObjectiveID = "STEALTH_001",
                Title = "侵入",
                Description = "警備員に発見されずに建物に侵入する",
                ObjectiveType = "Infiltration",
                RequiredAction = "ReachLocation",
                TargetLocation = "EntryPoint",
                TimeLimit = 300f,
                StealthRequired = true,
                Priority = 1
            },
            new StealthObjective 
            { 
                ObjectiveID = "STEALTH_002",
                Title = "情報収集",
                Description = "ターゲットのコンピューターにアクセスする",
                ObjectiveType = "DataRetrieval",
                RequiredAction = "InteractWithObject",
                TargetLocation = "Server Room",
                TimeLimit = 180f,
                StealthRequired = true,
                Priority = 2
            },
            new StealthObjective 
            { 
                ObjectiveID = "STEALTH_003",
                Title = "脱出",
                Description = "警報を鳴らさずに脱出する",
                ObjectiveType = "Exfiltration",
                RequiredAction = "ReachLocation",
                TargetLocation = "ExitPoint",
                TimeLimit = 240f,
                StealthRequired = true,
                Priority = 3
            }
        };

        [TabGroup("Objectives", "Secondary Goals")]
        [Title("副次目標システム", "追加チャレンジ", TitleAlignments.Centered)]
        [SerializeField]
        [Tooltip("副次目標設定")]
        private StealthObjective[] secondaryObjectives = new StealthObjective[]
        {
            new StealthObjective 
            { 
                ObjectiveID = "STEALTH_SEC_001",
                Title = "完全ステルス",
                Description = "一度も発見されることなくミッションを完了する",
                ObjectiveType = "Bonus",
                RequiredAction = "AvoidDetection",
                TargetLocation = "Entire Mission",
                TimeLimit = 900f,
                StealthRequired = true,
                Priority = 4
            },
            new StealthObjective 
            { 
                ObjectiveID = "STEALTH_SEC_002",
                Title = "スピードラン",
                Description = "10分以内にミッションを完了する",
                ObjectiveType = "Time Challenge",
                RequiredAction = "CompleteWithinTime",
                TargetLocation = "Entire Mission",
                TimeLimit = 600f,
                StealthRequired = false,
                Priority = 5
            }
        };

        [TabGroup("Objectives", "Learning Goals")]
        [Title("学習目標システム", "Learn & Grow価値実現", TitleAlignments.Centered)]
        [SerializeField]
        [Tooltip("学習目標設定")]
        private LearningObjective[] learningObjectives = new LearningObjective[]
        {
            new LearningObjective 
            { 
                SkillName = "Basic Stealth Movement",
                Description = "しゃがみ移動で10メートル移動する",
                RequiredCount = 1,
                LearningPhase = 1,
                Difficulty = "Beginner",
                RewardType = "Skill Unlock"
            },
            new LearningObjective 
            { 
                SkillName = "AI Behavior Understanding",
                Description = "NPCの視界と聴覚範囲を理解する",
                RequiredCount = 3,
                LearningPhase = 2,
                Difficulty = "Intermediate",
                RewardType = "Detection Indicators"
            },
            new LearningObjective 
            { 
                SkillName = "Advanced Stealth Tactics",
                Description = "カバーと注意散漫を組み合わせる",
                RequiredCount = 5,
                LearningPhase = 3,
                Difficulty = "Advanced",
                RewardType = "Master Techniques"
            }
        };

        #endregion

        #region Mission Environment

        [TabGroup("Environment", "Level Design")]
        [Title("レベルデザイン設定", "ステルスに最適化された環境", TitleAlignments.Centered)]
        [SerializeField]
        [Tooltip("ミッション環境設定")]
        private MissionEnvironment[] missionEnvironments = new MissionEnvironment[]
        {
            new MissionEnvironment 
            { 
                AreaName = "Entrance",
                AreaType = "Outdoor",
                LightLevel = "Medium",
                CoverDensity = "High",
                PatrolCount = 2,
                DifficultyRating = 2,
                Description = "屋外入口エリア - カバーポイント豊富"
            },
            new MissionEnvironment 
            { 
                AreaName = "Main Hall",
                AreaType = "Indoor",
                LightLevel = "Bright",
                CoverDensity = "Low",
                PatrolCount = 3,
                DifficultyRating = 4,
                Description = "メインホール - 高難易度エリア"
            },
            new MissionEnvironment 
            { 
                AreaName = "Server Room",
                AreaType = "Indoor",
                LightLevel = "Dark",
                CoverDensity = "Medium",
                PatrolCount = 1,
                DifficultyRating = 3,
                Description = "サーバールーム - 目標エリア"
            }
        };

        [TabGroup("Environment", "Dynamic Elements")]
        [Title("動的環境要素", "リアルタイム環境変化", TitleAlignments.Centered)]
        [SerializeField]
        [Tooltip("動的環境変化有効化")]
        private bool enableDynamicEnvironment = true;

        [SerializeField]
        [Tooltip("環境変化イベント")]
        private EnvironmentEvent[] environmentEvents = new EnvironmentEvent[]
        {
            new EnvironmentEvent 
            { 
                EventName = "Light Flicker",
                TriggerTime = 120f,
                Duration = 10f,
                Effect = "Lighting Change",
                Description = "照明の点滅で一時的な暗闇"
            },
            new EnvironmentEvent 
            { 
                EventName = "Guard Change",
                TriggerTime = 300f,
                Duration = 30f,
                Effect = "Patrol Route Change",
                Description = "警備員の交代と巡回ルート変更"
            },
            new EnvironmentEvent 
            { 
                EventName = "Noise Distraction",
                TriggerTime = 450f,
                Duration = 15f,
                Effect = "Audio Masking",
                Description = "機械音による自然な注意散漫"
            }
        };

        #endregion

        #region Scoring & Progression

        [TabGroup("Scoring", "Performance Metrics")]
        [Title("パフォーマンス評価", "プレイヤー成果測定", TitleAlignments.Centered)]
        [SerializeField]
        [Tooltip("スコアリングシステム有効化")]
        private bool enableScoringSystem = true;

        [SerializeField]
        [Tooltip("スコア要素設定")]
        private ScoreElement[] scoreElements = new ScoreElement[]
        {
            new ScoreElement 
            { 
                ElementName = "Stealth Bonus",
                BaseScore = 1000,
                Multiplier = 1.5f,
                Condition = "No Detection",
                Description = "発見されなかった場合のボーナス"
            },
            new ScoreElement 
            { 
                ElementName = "Speed Bonus",
                BaseScore = 500,
                Multiplier = 2.0f,
                Condition = "Under Time Limit",
                Description = "時間内完了ボーナス"
            },
            new ScoreElement 
            { 
                ElementName = "Objective Complete",
                BaseScore = 300,
                Multiplier = 1.0f,
                Condition = "Per Objective",
                Description = "目標完了ごとの基本スコア"
            }
        };

        [TabGroup("Scoring", "Progression System")]
        [Title("進行システム", "段階的スキル開放", TitleAlignments.Centered)]
        [SerializeField]
        [Tooltip("進行システム有効化")]
        private bool enableProgressionSystem = true;

        [SerializeField]
        [Tooltip("進行マイルストーン")]
        private ProgressionMilestone[] progressionMilestones = new ProgressionMilestone[]
        {
            new ProgressionMilestone 
            { 
                MilestoneName = "First Steps",
                RequiredScore = 500,
                UnlockedFeatures = new string[] { "Advanced Movement", "Detection Indicators" },
                Description = "ステルスの基礎をマスター"
            },
            new ProgressionMilestone 
            { 
                MilestoneName = "Shadow Walker",
                RequiredScore = 1500,
                UnlockedFeatures = new string[] { "Environmental Tools", "Advanced AI Info" },
                Description = "中級ステルステクニックを習得"
            },
            new ProgressionMilestone 
            { 
                MilestoneName = "Master Infiltrator",
                RequiredScore = 3000,
                UnlockedFeatures = new string[] { "Master Mode", "Custom Missions" },
                Description = "ステルスマスターレベルに到達"
            }
        };

        #endregion

        #region Difficulty & Accessibility

        [TabGroup("Difficulty", "Adaptive System")]
        [Title("適応的難易度システム", "プレイヤーレベル連動", TitleAlignments.Centered)]
        [SerializeField]
        [Tooltip("適応的難易度有効化")]
        private bool enableAdaptiveDifficultySystem = true;

        [SerializeField]
        [Tooltip("難易度レベル設定")]
        private DifficultyLevel[] difficultyLevels = new DifficultyLevel[]
        {
            new DifficultyLevel 
            { 
                LevelName = "Novice",
                AIDetectionMultiplier = 0.7f,
                TimeMultiplier = 1.3f,
                HintFrequency = 0.8f,
                Description = "初心者向け - 寛容な設定"
            },
            new DifficultyLevel 
            { 
                LevelName = "Agent",
                AIDetectionMultiplier = 1.0f,
                TimeMultiplier = 1.0f,
                HintFrequency = 0.5f,
                Description = "標準設定"
            },
            new DifficultyLevel 
            { 
                LevelName = "Professional",
                AIDetectionMultiplier = 1.3f,
                TimeMultiplier = 0.8f,
                HintFrequency = 0.2f,
                Description = "上級者向け - 厳しい設定"
            },
            new DifficultyLevel 
            { 
                LevelName = "Master",
                AIDetectionMultiplier = 1.5f,
                TimeMultiplier = 0.6f,
                HintFrequency = 0.0f,
                Description = "マスターレベル - 最高難易度"
            }
        };

        [TabGroup("Difficulty", "Accessibility")]
        [Title("アクセシビリティ機能", "全プレイヤー対応", TitleAlignments.Centered)]
        [SerializeField]
        [Tooltip("アクセシビリティ機能有効化")]
        private bool enableAccessibilityFeatures = true;

        [SerializeField]
        [Tooltip("アクセシビリティオプション")]
        private AccessibilityOption[] accessibilityOptions = new AccessibilityOption[]
        {
            new AccessibilityOption 
            { 
                OptionName = "Visual Detection Aids",
                Description = "検知範囲の視覚的表示強化",
                Category = "Visual",
                DefaultEnabled = false
            },
            new AccessibilityOption 
            { 
                OptionName = "Audio Cues Enhancement",
                Description = "音響フィードバックの強化",
                Category = "Audio",
                DefaultEnabled = false
            },
            new AccessibilityOption 
            { 
                OptionName = "Simplified Controls",
                Description = "簡略化された操作スキーム",
                Category = "Input",
                DefaultEnabled = false
            },
            new AccessibilityOption 
            { 
                OptionName = "Extended Time Limits",
                Description = "制限時間の延長",
                Category = "Gameplay",
                DefaultEnabled = false
            }
        };

        #endregion

        #region Properties (Public API)

        // 15-Minute Gameplay Properties
        public bool Enable15MinuteExperience => enable15MinuteExperience;
        public GameplayPhase[] GameplayPhases => gameplayPhases;
        public float PhaseTransitionMultiplier => phaseTransitionMultiplier;
        public bool EnableAutoPhaseProgression => enableAutoPhaseProgression;
        public bool EnableAutoRestart => enableAutoRestart;
        public int MaxRestartAttempts => maxRestartAttempts;
        public bool EnableAdaptiveDifficulty => enableAdaptiveDifficulty;

        // Mission Objectives Properties
        public StealthObjective[] PrimaryObjectives => primaryObjectives;
        public StealthObjective[] SecondaryObjectives => secondaryObjectives;
        public LearningObjective[] LearningObjectives => learningObjectives;

        // Mission Environment Properties
        public MissionEnvironment[] MissionEnvironments => missionEnvironments;
        public bool EnableDynamicEnvironment => enableDynamicEnvironment;
        public EnvironmentEvent[] EnvironmentEvents => environmentEvents;

        // Scoring & Progression Properties
        public bool EnableScoringSystem => enableScoringSystem;
        public ScoreElement[] ScoreElements => scoreElements;
        public bool EnableProgressionSystem => enableProgressionSystem;
        public ProgressionMilestone[] ProgressionMilestones => progressionMilestones;

        // Difficulty & Accessibility Properties
        public bool EnableAdaptiveDifficultySystem => enableAdaptiveDifficultySystem;
        public DifficultyLevel[] DifficultyLevels => difficultyLevels;
        public bool EnableAccessibilityFeatures => enableAccessibilityFeatures;
        public AccessibilityOption[] AccessibilityOptions => accessibilityOptions;

        #endregion

        #region Mission Management Methods

        /// <summary>
        /// ゲームプレイフェーズを取得
        /// </summary>
        public GameplayPhase GetGameplayPhase(string phaseName)
        {
            foreach (var phase in gameplayPhases)
            {
                if (phase.PhaseName == phaseName)
                    return phase;
            }
            return gameplayPhases[0]; // Default to first phase
        }

        /// <summary>
        /// 主要目標を取得
        /// </summary>
        public StealthObjective GetPrimaryObjective(string objectiveID)
        {
            foreach (var objective in primaryObjectives)
            {
                if (objective.ObjectiveID == objectiveID)
                    return objective;
            }
            return null;
        }

        /// <summary>
        /// 学習目標を取得
        /// </summary>
        public LearningObjective GetLearningObjective(string skillName)
        {
            foreach (var objective in learningObjectives)
            {
                if (objective.SkillName == skillName)
                    return objective;
            }
            return null;
        }

        /// <summary>
        /// ミッション環境を取得
        /// </summary>
        public MissionEnvironment GetMissionEnvironment(string areaName)
        {
            foreach (var environment in missionEnvironments)
            {
                if (environment.AreaName == areaName)
                    return environment;
            }
            return missionEnvironments[0]; // Default to first environment
        }

        /// <summary>
        /// 難易度レベルを取得
        /// </summary>
        public DifficultyLevel GetDifficultyLevel(string levelName)
        {
            foreach (var level in difficultyLevels)
            {
                if (level.LevelName == levelName)
                    return level;
            }
            return difficultyLevels[1]; // Default to Agent level
        }

        /// <summary>
        /// 進行マイルストーンを取得
        /// </summary>
        public ProgressionMilestone GetProgressionMilestone(int score)
        {
            ProgressionMilestone currentMilestone = null;
            foreach (var milestone in progressionMilestones)
            {
                if (score >= milestone.RequiredScore)
                {
                    currentMilestone = milestone;
                }
                else
                {
                    break;
                }
            }
            return currentMilestone;
        }

        /// <summary>
        /// 15分体験の総時間を計算
        /// </summary>
        public float CalculateTotalExperienceTime()
        {
            float totalTime = 0f;
            foreach (var phase in gameplayPhases)
            {
                totalTime += phase.Duration * phaseTransitionMultiplier;
            }
            return totalTime;
        }

        /// <summary>
        /// スコア計算
        /// </summary>
        public int CalculateScore(bool stealthMaintained, float completionTime, int objectivesCompleted)
        {
            int totalScore = 0;

            foreach (var element in scoreElements)
            {
                bool conditionMet = false;
                switch (element.Condition)
                {
                    case "No Detection":
                        conditionMet = stealthMaintained;
                        break;
                    case "Under Time Limit":
                        conditionMet = completionTime <= CalculateTotalExperienceTime();
                        break;
                    case "Per Objective":
                        totalScore += (int)(element.BaseScore * element.Multiplier * objectivesCompleted);
                        continue;
                }

                if (conditionMet)
                {
                    totalScore += (int)(element.BaseScore * element.Multiplier);
                }
            }

            return totalScore;
        }

        #endregion

        #region Nested Data Classes

        [System.Serializable]
        public class GameplayPhase
        {
            [Tooltip("フェーズ名")]
            public string PhaseName;
            
            [Tooltip("継続時間（秒）")]
            public float Duration;
            
            [Tooltip("説明")]
            public string Description;
            
            [Tooltip("主要目標")]
            public string MainObjective;
            
            [Tooltip("学習焦点")]
            public string LearningFocus;
        }

        [System.Serializable]
        public class StealthObjective
        {
            [Tooltip("目標ID")]
            public string ObjectiveID;
            
            [Tooltip("タイトル")]
            public string Title;
            
            [Tooltip("説明")]
            public string Description;
            
            [Tooltip("目標タイプ")]
            public string ObjectiveType;
            
            [Tooltip("必要アクション")]
            public string RequiredAction;
            
            [Tooltip("ターゲット位置")]
            public string TargetLocation;
            
            [Tooltip("制限時間")]
            public float TimeLimit;
            
            [Tooltip("ステルス必須")]
            public bool StealthRequired;
            
            [Range(1, 10)]
            [Tooltip("優先度")]
            public int Priority;
        }

        [System.Serializable]
        public class LearningObjective
        {
            [Tooltip("スキル名")]
            public string SkillName;
            
            [Tooltip("説明")]
            public string Description;
            
            [Tooltip("必要回数")]
            public int RequiredCount;
            
            [Range(1, 4)]
            [Tooltip("学習フェーズ")]
            public int LearningPhase;
            
            [Tooltip("難易度")]
            public string Difficulty;
            
            [Tooltip("報酬タイプ")]
            public string RewardType;
        }

        [System.Serializable]
        public class MissionEnvironment
        {
            [Tooltip("エリア名")]
            public string AreaName;
            
            [Tooltip("エリアタイプ")]
            public string AreaType;
            
            [Tooltip("明度レベル")]
            public string LightLevel;
            
            [Tooltip("カバー密度")]
            public string CoverDensity;
            
            [Tooltip("パトロール数")]
            public int PatrolCount;
            
            [Range(1, 5)]
            [Tooltip("難易度評価")]
            public int DifficultyRating;
            
            [Tooltip("説明")]
            public string Description;
        }

        [System.Serializable]
        public class EnvironmentEvent
        {
            [Tooltip("イベント名")]
            public string EventName;
            
            [Tooltip("発生時間")]
            public float TriggerTime;
            
            [Tooltip("継続時間")]
            public float Duration;
            
            [Tooltip("効果")]
            public string Effect;
            
            [Tooltip("説明")]
            public string Description;
        }

        [System.Serializable]
        public class ScoreElement
        {
            [Tooltip("要素名")]
            public string ElementName;
            
            [Tooltip("基本スコア")]
            public int BaseScore;
            
            [Range(0.1f, 5f)]
            [Tooltip("倍率")]
            public float Multiplier;
            
            [Tooltip("条件")]
            public string Condition;
            
            [Tooltip("説明")]
            public string Description;
        }

        [System.Serializable]
        public class ProgressionMilestone
        {
            [Tooltip("マイルストーン名")]
            public string MilestoneName;
            
            [Tooltip("必要スコア")]
            public int RequiredScore;
            
            [Tooltip("開放機能")]
            public string[] UnlockedFeatures;
            
            [Tooltip("説明")]
            public string Description;
        }

        [System.Serializable]
        public class DifficultyLevel
        {
            [Tooltip("レベル名")]
            public string LevelName;
            
            [Range(0.1f, 3f)]
            [Tooltip("AI検知倍率")]
            public float AIDetectionMultiplier;
            
            [Range(0.5f, 2f)]
            [Tooltip("時間倍率")]
            public float TimeMultiplier;
            
            [Range(0f, 1f)]
            [Tooltip("ヒント頻度")]
            public float HintFrequency;
            
            [Tooltip("説明")]
            public string Description;
        }

        [System.Serializable]
        public class AccessibilityOption
        {
            [Tooltip("オプション名")]
            public string OptionName;
            
            [Tooltip("説明")]
            public string Description;
            
            [Tooltip("カテゴリ")]
            public string Category;
            
            [Tooltip("デフォルト有効")]
            public bool DefaultEnabled;
        }

        #endregion

        #region Editor Support

#if UNITY_EDITOR
        [TabGroup("Debug", "Debug Actions")]
        [Button("Test 15-Minute Experience Flow")]
        public void Test15MinuteFlow()
        {
            Debug.Log("=== 15-Minute Experience Flow Test ===");
            float totalTime = CalculateTotalExperienceTime();
            Debug.Log($"Total Experience Time: {totalTime / 60f:F1} minutes");
            
            foreach (var phase in gameplayPhases)
            {
                float adjustedDuration = phase.Duration * phaseTransitionMultiplier;
                Debug.Log($"Phase: {phase.PhaseName} - {adjustedDuration / 60f:F1} min");
                Debug.Log($"  Focus: {phase.LearningFocus}");
                Debug.Log($"  Objective: {phase.MainObjective}");
            }
        }

        [Button("Test Scoring System")]
        public void TestScoringSystem()
        {
            Debug.Log("=== Scoring System Test ===");
            
            // Test different scenarios
            int[] scenarios = { 
                CalculateScore(true, 800f, 3),   // Perfect stealth, fast completion
                CalculateScore(false, 900f, 3),  // Detected, normal completion
                CalculateScore(true, 1000f, 2)   // Stealth, slow, partial completion
            };
            
            string[] descriptions = { "Perfect", "Detected", "Slow but Stealthy" };
            
            for (int i = 0; i < scenarios.Length; i++)
            {
                Debug.Log($"{descriptions[i]}: {scenarios[i]} points");
                var milestone = GetProgressionMilestone(scenarios[i]);
                if (milestone != null)
                {
                    Debug.Log($"  Milestone: {milestone.MilestoneName}");
                }
            }
        }

        [Button("Test Difficulty Levels")]
        public void TestDifficultyLevels()
        {
            Debug.Log("=== Difficulty Levels Test ===");
            foreach (var level in difficultyLevels)
            {
                Debug.Log($"Level: {level.LevelName}");
                Debug.Log($"  AI Detection: {level.AIDetectionMultiplier:F1}x");
                Debug.Log($"  Time Limit: {level.TimeMultiplier:F1}x");
                Debug.Log($"  Hint Frequency: {level.HintFrequency:P}");
            }
        }

        [Button("Print Mission Configuration")]
        public void PrintConfiguration()
        {
            Debug.Log("=== Stealth Mission Configuration ===");
            Debug.Log($"15-Minute Experience: {enable15MinuteExperience}");
            Debug.Log($"Total Experience Time: {CalculateTotalExperienceTime() / 60f:F1} minutes");
            Debug.Log($"Primary Objectives: {primaryObjectives.Length}");
            Debug.Log($"Secondary Objectives: {secondaryObjectives.Length}");
            Debug.Log($"Learning Objectives: {learningObjectives.Length}");
            Debug.Log($"Mission Environments: {missionEnvironments.Length}");
            Debug.Log($"Difficulty Levels: {difficultyLevels.Length}");
            Debug.Log($"Accessibility Features: {enableAccessibilityFeatures}");
        }
#endif

        #endregion
    }
}
