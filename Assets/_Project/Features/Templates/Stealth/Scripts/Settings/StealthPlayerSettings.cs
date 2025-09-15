using UnityEngine;
using Sirenix.OdinInspector;

namespace asterivo.Unity60.Features.Templates.Stealth.Settings
{
    /// <summary>
    /// ステルステンプレート: プレイヤー設定
    /// プレイヤーのステルス能力、移動パラメータ、ノイズ生成、検知耐性、学習進行設定
    /// Learn & Grow価値実現: 段階的スキル習得支援
    /// </summary>
    [CreateAssetMenu(menuName = "Templates/Stealth/Settings/Player Settings", fileName = "StealthPlayerSettings")]
    public class StealthPlayerSettings : ScriptableObject
    {
        #region Movement & Stealth Mechanics

        [TabGroup("Movement", "Basic Movement")]
        [Title("基本移動設定", "プレイヤーの基本移動能力とステルス移動", TitleAlignments.Centered)]
        [SerializeField, Range(0.5f, 3.0f)]
        [Tooltip("しゃがみ時の移動速度倍率（ステルス性向上、速度低下）")]
        private float crouchSpeedMultiplier = 0.3f;

        [SerializeField, Range(0.1f, 1.0f)]
        [Tooltip("匍匐時の移動速度倍率（最高ステルス性、大幅速度低下）")]
        private float proneSpeedMultiplier = 0.1f;

        [SerializeField, Range(1.0f, 2.5f)]
        [Tooltip("通常歩行速度倍率")]
        private float walkSpeedMultiplier = 1.0f;

        [SerializeField, Range(1.5f, 4.0f)]
        [Tooltip("走行速度倍率（検知リスク増大）")]
        private float runSpeedMultiplier = 2.0f;

        [TabGroup("Movement", "Stealth Mechanics")]
        [Title("ステルス機能設定", "隠密行動の詳細設定", TitleAlignments.Centered)]
        [SerializeField, Range(0.0f, 1.0f)]
        [Tooltip("しゃがみ時のノイズ減少効果（0=無音、1=通常音量）")]
        private float crouchNoiseReduction = 0.3f;

        [SerializeField, Range(0.0f, 1.0f)]
        [Tooltip("匍匐時のノイズ減少効果")]
        private float proneNoiseReduction = 0.1f;

        [SerializeField, Range(0.0f, 1.0f)]
        [Tooltip("影に隠れた時の視覚検知耐性（0=完全透明、1=通常視認性）")]
        private float shadowHidingEffectiveness = 0.2f;

        [SerializeField, Range(0.0f, 1.0f)]
        [Tooltip("カバーポイント使用時の検知耐性")]
        private float coverHidingEffectiveness = 0.15f;

        #endregion

        #region Detection & Awareness

        [TabGroup("Detection", "Player Detection")]
        [Title("検知・警戒システム", "プレイヤーの被検知特性", TitleAlignments.Centered)]
        [SerializeField, Range(0.5f, 2.0f)]
        [Tooltip("プレイヤーの基本視覚検知サイズ倍率")]
        private float visualDetectionSizeMultiplier = 1.0f;

        [SerializeField, Range(0.5f, 2.0f)]
        [Tooltip("プレイヤーの聴覚検知サイズ倍率")]
        private float auditoryDetectionSizeMultiplier = 1.0f;

        [SerializeField, Range(0.1f, 2.0f)]
        [Tooltip("明るい場所での視覚検知倍率")]
        private float lightDetectionMultiplier = 1.5f;

        [SerializeField, Range(0.1f, 1.0f)]
        [Tooltip("暗い場所での視覚検知倍率")]
        private float darkDetectionMultiplier = 0.4f;

        [TabGroup("Detection", "Alert Response")]
        [Title("警戒レベル対応", "各警戒段階での行動制限", TitleAlignments.Centered)]
        [SerializeField]
        [Tooltip("警戒レベルごとの移動速度制限")]
        private AlertLevelMovementRestriction[] alertLevelRestrictions = new AlertLevelMovementRestriction[]
        {
            new AlertLevelMovementRestriction { AlertLevel = "Relaxed", MaxSpeedMultiplier = 1.0f, CanRun = true },
            new AlertLevelMovementRestriction { AlertLevel = "Suspicious", MaxSpeedMultiplier = 0.8f, CanRun = true },
            new AlertLevelMovementRestriction { AlertLevel = "Investigating", MaxSpeedMultiplier = 0.6f, CanRun = false },
            new AlertLevelMovementRestriction { AlertLevel = "Alert", MaxSpeedMultiplier = 0.4f, CanRun = false }
        };

        #endregion

        #region Audio & Footsteps

        [TabGroup("Audio", "Footstep System")]
        [Title("足音システム", "表面材質別の音響特性", TitleAlignments.Centered)]
        [SerializeField]
        [Tooltip("表面材質別の足音設定")]
        private SurfaceAudioProfile[] surfaceProfiles = new SurfaceAudioProfile[]
        {
            new SurfaceAudioProfile { SurfaceType = "Concrete", BaseVolume = 0.8f, DetectionRadius = 8.0f },
            new SurfaceAudioProfile { SurfaceType = "Wood", BaseVolume = 0.9f, DetectionRadius = 10.0f },
            new SurfaceAudioProfile { SurfaceType = "Metal", BaseVolume = 1.0f, DetectionRadius = 12.0f },
            new SurfaceAudioProfile { SurfaceType = "Grass", BaseVolume = 0.3f, DetectionRadius = 4.0f },
            new SurfaceAudioProfile { SurfaceType = "Sand", BaseVolume = 0.2f, DetectionRadius = 3.0f },
            new SurfaceAudioProfile { SurfaceType = "Water", BaseVolume = 0.6f, DetectionRadius = 6.0f }
        };

        [TabGroup("Audio", "Breathing & Ambient")]
        [Title("呼吸・環境音", "プレイヤーの生体音響", TitleAlignments.Centered)]
        [SerializeField, Range(0.0f, 1.0f)]
        [Tooltip("通常時の呼吸音量")]
        private float normalBreathingVolume = 0.1f;

        [SerializeField, Range(0.0f, 1.0f)]
        [Tooltip("緊張時の呼吸音量")]
        private float stressedBreathingVolume = 0.3f;

        [SerializeField, Range(0.0f, 1.0f)]
        [Tooltip("走行後の息切れ音量")]
        private float windedBreathingVolume = 0.6f;

        #endregion

        #region Learn & Grow System

        [TabGroup("Learning", "Skill Progression")]
        [Title("スキル習得システム", "Learn & Grow価値実現: 70%学習コスト削減", TitleAlignments.Centered)]
        [SerializeField]
        [Tooltip("段階的スキル習得目標")]
        private StealthSkillObjective[] skillObjectives = new StealthSkillObjective[]
        {
            new StealthSkillObjective 
            { 
                SkillName = "Basic Crouching", 
                Description = "しゃがみ移動の基本",
                RequiredActions = 10,
                UnlocksAbility = "Quieter Footsteps",
                LearningPhase = 1
            },
            new StealthSkillObjective 
            { 
                SkillName = "Shadow Movement", 
                Description = "影を利用した移動",
                RequiredActions = 25,
                UnlocksAbility = "Shadow Detection Resistance",
                LearningPhase = 2
            },
            new StealthSkillObjective 
            { 
                SkillName = "Cover Usage", 
                Description = "カバーポイントの効果的利用",
                RequiredActions = 15,
                UnlocksAbility = "Enhanced Cover Effectiveness",
                LearningPhase = 2
            },
            new StealthSkillObjective 
            { 
                SkillName = "Sound Management", 
                Description = "足音と環境音の理解",
                RequiredActions = 30,
                UnlocksAbility = "Surface Type Recognition",
                LearningPhase = 3
            },
            new StealthSkillObjective 
            { 
                SkillName = "Advanced Stealth", 
                Description = "高度なステルステクニック",
                RequiredActions = 50,
                UnlocksAbility = "Master Stealth Mode",
                LearningPhase = 4
            }
        };

        [TabGroup("Learning", "Adaptive Difficulty")]
        [Title("適応的難易度", "プレイヤーレベルに応じた動的調整", TitleAlignments.Centered)]
        [SerializeField]
        [Tooltip("学習フェーズごとの難易度調整")]
        private LearningPhaseSettings[] learningPhases = new LearningPhaseSettings[]
        {
            new LearningPhaseSettings 
            { 
                Phase = 1, 
                Name = "Beginner", 
                DetectionTolerance = 1.5f, 
                HintFrequency = 0.8f,
                Description = "基本操作習得"
            },
            new LearningPhaseSettings 
            { 
                Phase = 2, 
                Name = "Intermediate", 
                DetectionTolerance = 1.2f, 
                HintFrequency = 0.5f,
                Description = "ステルス理解"
            },
            new LearningPhaseSettings 
            { 
                Phase = 3, 
                Name = "Advanced", 
                DetectionTolerance = 1.0f, 
                HintFrequency = 0.3f,
                Description = "テクニック習得"
            },
            new LearningPhaseSettings 
            { 
                Phase = 4, 
                Name = "Expert", 
                DetectionTolerance = 0.8f, 
                HintFrequency = 0.1f,
                Description = "マスタリー"
            }
        };

        [SerializeField, Range(1, 4)]
        [Tooltip("現在の学習フェーズ")]
        private int currentLearningPhase = 1;

        #endregion

        #region Properties (Public API)

        // Movement Properties
        public float CrouchSpeedMultiplier => crouchSpeedMultiplier;
        public float ProneSpeedMultiplier => proneSpeedMultiplier;
        public float WalkSpeedMultiplier => walkSpeedMultiplier;
        public float RunSpeedMultiplier => runSpeedMultiplier;

        // Stealth Properties
        public float CrouchNoiseReduction => crouchNoiseReduction;
        public float ProneNoiseReduction => proneNoiseReduction;
        public float ShadowHidingEffectiveness => shadowHidingEffectiveness;
        public float CoverHidingEffectiveness => coverHidingEffectiveness;

        // Detection Properties
        public float VisualDetectionSizeMultiplier => visualDetectionSizeMultiplier;
        public float AuditoryDetectionSizeMultiplier => auditoryDetectionSizeMultiplier;
        public float LightDetectionMultiplier => lightDetectionMultiplier;
        public float DarkDetectionMultiplier => darkDetectionMultiplier;
        public AlertLevelMovementRestriction[] AlertLevelRestrictions => alertLevelRestrictions;

        // Audio Properties
        public SurfaceAudioProfile[] SurfaceProfiles => surfaceProfiles;
        public float NormalBreathingVolume => normalBreathingVolume;
        public float StressedBreathingVolume => stressedBreathingVolume;
        public float WindedBreathingVolume => windedBreathingVolume;

        // Learning Properties
        public StealthSkillObjective[] SkillObjectives => skillObjectives;
        public LearningPhaseSettings[] LearningPhases => learningPhases;
        public int CurrentLearningPhase => currentLearningPhase;

        #endregion

        #region Learning System Methods

        /// <summary>
        /// 学習フェーズを進める
        /// </summary>
        public void AdvanceLearningPhase()
        {
            if (currentLearningPhase < learningPhases.Length)
            {
                currentLearningPhase++;
                Debug.Log($"[StealthPlayer] Advanced to learning phase {currentLearningPhase}: {GetCurrentPhase().Name}");
            }
        }

        /// <summary>
        /// 現在の学習フェーズ設定を取得
        /// </summary>
        public LearningPhaseSettings GetCurrentPhase()
        {
            return learningPhases[Mathf.Clamp(currentLearningPhase - 1, 0, learningPhases.Length - 1)];
        }

        /// <summary>
        /// 指定されたスキルの進捗を取得
        /// </summary>
        public StealthSkillObjective GetSkillObjective(string skillName)
        {
            foreach (var skill in skillObjectives)
            {
                if (skill.SkillName == skillName)
                    return skill;
            }
            return null;
        }

        /// <summary>
        /// 表面材質に基づく音響プロファイルを取得
        /// </summary>
        public SurfaceAudioProfile GetSurfaceProfile(string surfaceType)
        {
            foreach (var profile in surfaceProfiles)
            {
                if (profile.SurfaceType == surfaceType)
                    return profile;
            }
            return surfaceProfiles[0]; // Default to first profile
        }

        /// <summary>
        /// 警戒レベルに基づく移動制限を取得
        /// </summary>
        public AlertLevelMovementRestriction GetMovementRestriction(string alertLevel)
        {
            foreach (var restriction in alertLevelRestrictions)
            {
                if (restriction.AlertLevel == alertLevel)
                    return restriction;
            }
            return alertLevelRestrictions[0]; // Default to first restriction
        }

        #endregion

        #region Nested Data Classes

        [System.Serializable]
        public class AlertLevelMovementRestriction
        {
            [Tooltip("警戒レベル名")]
            public string AlertLevel;
            
            [Range(0.1f, 1.0f)]
            [Tooltip("最大速度倍率")]
            public float MaxSpeedMultiplier;
            
            [Tooltip("走行可能かどうか")]
            public bool CanRun;
        }

        [System.Serializable]
        public class SurfaceAudioProfile
        {
            [Tooltip("表面材質タイプ")]
            public string SurfaceType;
            
            [Range(0.0f, 1.0f)]
            [Tooltip("基本音量")]
            public float BaseVolume;
            
            [Range(1.0f, 15.0f)]
            [Tooltip("検知半径（メートル）")]
            public float DetectionRadius;
        }

        [System.Serializable]
        public class StealthSkillObjective
        {
            [Tooltip("スキル名")]
            public string SkillName;
            
            [Tooltip("スキル説明")]
            public string Description;
            
            [Tooltip("習得に必要な実行回数")]
            public int RequiredActions;
            
            [Tooltip("解放される能力")]
            public string UnlocksAbility;
            
            [Range(1, 4)]
            [Tooltip("学習フェーズ")]
            public int LearningPhase;
            
            [Tooltip("現在の進捗")]
            [System.NonSerialized]
            public int CurrentProgress = 0;
        }

        [System.Serializable]
        public class LearningPhaseSettings
        {
            [Tooltip("フェーズ番号")]
            public int Phase;
            
            [Tooltip("フェーズ名")]
            public string Name;
            
            [Range(0.5f, 2.0f)]
            [Tooltip("検知耐性（高いほど寛容）")]
            public float DetectionTolerance;
            
            [Range(0.0f, 1.0f)]
            [Tooltip("ヒント表示頻度")]
            public float HintFrequency;
            
            [Tooltip("フェーズ説明")]
            public string Description;
        }

        #endregion

        #region Editor Support

#if UNITY_EDITOR
        [TabGroup("Debug", "Debug Actions")]
        [Button("Reset All Skills")]
        public void ResetAllSkills()
        {
            foreach (var skill in skillObjectives)
            {
                skill.CurrentProgress = 0;
            }
            currentLearningPhase = 1;
            Debug.Log("[StealthPlayer] All skills reset to beginner level");
        }

        [Button("Complete Current Phase")]
        public void CompleteCurrentPhase()
        {
            var currentPhase = GetCurrentPhase();
            foreach (var skill in skillObjectives)
            {
                if (skill.LearningPhase == currentLearningPhase)
                {
                    skill.CurrentProgress = skill.RequiredActions;
                }
            }
            Debug.Log($"[StealthPlayer] Completed all skills for phase {currentLearningPhase}: {currentPhase.Name}");
        }

        [Button("Print Learning Status")]
        public void PrintLearningStatus()
        {
            Debug.Log("=== Stealth Player Learning Status ===");
            Debug.Log($"Current Phase: {currentLearningPhase} - {GetCurrentPhase().Name}");
            Debug.Log($"Detection Tolerance: {GetCurrentPhase().DetectionTolerance:F2}");
            Debug.Log($"Hint Frequency: {GetCurrentPhase().HintFrequency:F2}");
            
            foreach (var skill in skillObjectives)
            {
                var progress = (float)skill.CurrentProgress / skill.RequiredActions;
                Debug.Log($"Skill: {skill.SkillName} - {progress:P} ({skill.CurrentProgress}/{skill.RequiredActions})");
            }
        }
#endif

        #endregion
    }
}