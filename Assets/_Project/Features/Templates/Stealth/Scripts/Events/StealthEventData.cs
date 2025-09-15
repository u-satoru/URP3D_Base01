using UnityEngine;
using asterivo.Unity60.Features.Templates.Stealth.Configuration;
using asterivo.Unity60.Features.Templates.Stealth.Data;

namespace asterivo.Unity60.Features.Templates.Stealth.Events
{
    /// <summary>
    /// ステルス専用イベントデータ構造体集
    /// Event駆動アーキテクチャ用データ定義
    /// Layer 1.3: Event Channels統合（Event駆動アーキテクチャ）
    /// </summary>

    /// <summary>
    /// AI検知情報データ
    /// NPCの検知レベル変化イベント用
    /// </summary>
    [System.Serializable]
    public struct AIDetectionData
    {
        public string NPCId;
        public DetectionType DetectionType;
        public float SuspicionLevel;
        public float Distance;
        public Vector3 DetectionPosition;
        public Vector3 PlayerPosition;
        public float DetectionConfidence;
        public bool IsLineOfSightClear;

        public static AIDetectionData Create(string npcId, DetectionType type, float suspicion, float distance, Vector3 detectionPos, Vector3 playerPos)
        {
            return new AIDetectionData
            {
                NPCId = npcId,
                DetectionType = type,
                SuspicionLevel = suspicion,
                Distance = distance,
                DetectionPosition = detectionPos,
                PlayerPosition = playerPos,
                DetectionConfidence = Mathf.Clamp01(suspicion),
                IsLineOfSightClear = distance <= 50f // 簡易的な視線判定
            };
        }
    }

    /// <summary>
    /// NPC警戒レベルデータ
    /// 警戒状態変化イベント用
    /// </summary>
    [System.Serializable]
    public struct NPCAlertData
    {
        public string NPCId;
        public AIAlertLevel PreviousAlertLevel;
        public AIAlertLevel CurrentAlertLevel;
        public float AlertTransitionTime;
        public Vector3 NPCPosition;
        public Vector3 LastKnownPlayerPosition;
        public float AlertDuration;
        public string AlertReason;

        public static NPCAlertData Create(string npcId, AIAlertLevel previous, AIAlertLevel current, Vector3 npcPos, Vector3 playerPos, string reason = "")
        {
            return new NPCAlertData
            {
                NPCId = npcId,
                PreviousAlertLevel = previous,
                CurrentAlertLevel = current,
                AlertTransitionTime = Time.time,
                NPCPosition = npcPos,
                LastKnownPlayerPosition = playerPos,
                AlertDuration = 0f,
                AlertReason = reason
            };
        }

        public bool IsAlertLevelIncreasing => CurrentAlertLevel > PreviousAlertLevel;
        public bool IsAlertLevelDecreasing => CurrentAlertLevel < PreviousAlertLevel;
    }

    /// <summary>
    /// 協調検知データ
    /// 複数NPC間の情報共有イベント用
    /// </summary>
    [System.Serializable]
    public struct CooperativeDetectionData
    {
        public string[] ParticipatingNPCIds;
        public Vector3 SharedPlayerPosition;
        public float CombinedConfidenceLevel;
        public DetectionType PrimaryDetectionType;
        public float CooperationRange;
        public string CooperationReason;

        public static CooperativeDetectionData Create(string[] npcIds, Vector3 playerPos, float confidence, DetectionType primaryType, float range)
        {
            return new CooperativeDetectionData
            {
                ParticipatingNPCIds = npcIds,
                SharedPlayerPosition = playerPos,
                CombinedConfidenceLevel = confidence,
                PrimaryDetectionType = primaryType,
                CooperationRange = range,
                CooperationReason = $"{npcIds.Length} NPCs cooperative detection"
            };
        }
    }

    /// <summary>
    /// 環境相互作用データ
    /// 環境オブジェクトとの相互作用イベント用
    /// </summary>
    [System.Serializable]
    public struct EnvironmentInteractionData
    {
        public string InteractableId;
        public Vector3 InteractionPosition;
        public float InteractionDuration;
        public bool IsInteractionSuccessful;
        public string InteractionType;
        public float NoiseGenerated;
        public bool IsStealthInteraction;

        public static EnvironmentInteractionData Create(string id, Vector3 position, float duration, string type, float noise = 0f, bool isStealth = false)
        {
            return new EnvironmentInteractionData
            {
                InteractableId = id,
                InteractionPosition = position,
                InteractionDuration = duration,
                IsInteractionSuccessful = true,
                InteractionType = type,
                NoiseGenerated = noise,
                IsStealthInteraction = isStealth
            };
        }
    }

    /// <summary>
    /// ノイズゾーンデータ
    /// 音響マスキングゾーン進入/退出イベント用
    /// </summary>
    [System.Serializable]
    public struct NoiseZoneData
    {
        public string ZoneId;
        public Vector3 ZoneCenter;
        public float ZoneRadius;
        public float MaskingStrength;
        public NoiseZoneType ZoneType;
        public AudioEffectType[] AffectedAudioTypes;
        public bool IsPlayerInZone;

        public static NoiseZoneData Create(string id, Vector3 center, float radius, float strength, NoiseZoneType type)
        {
            return new NoiseZoneData
            {
                ZoneId = id,
                ZoneCenter = center,
                ZoneRadius = radius,
                MaskingStrength = strength,
                ZoneType = type,
                AffectedAudioTypes = new[] { AudioEffectType.Footstep, AudioEffectType.Movement },
                IsPlayerInZone = false
            };
        }
    }

    /// <summary>
    /// 照明変化データ
    /// 動的照明システムイベント用
    /// </summary>
    [System.Serializable]
    public struct LightingChangeData
    {
        public string LightId;
        public Vector3 LightPosition;
        public float PreviousIntensity;
        public float CurrentIntensity;
        public bool IsLightActive;
        public Color LightColor;
        public float AffectedRadius;
        public bool PlayerInLightRadius;

        public static LightingChangeData Create(string id, Vector3 position, float prevIntensity, float currentIntensity, bool active)
        {
            return new LightingChangeData
            {
                LightId = id,
                LightPosition = position,
                PreviousIntensity = prevIntensity,
                CurrentIntensity = currentIntensity,
                IsLightActive = active,
                LightColor = Color.white,
                AffectedRadius = 10f,
                PlayerInLightRadius = false
            };
        }
    }

    /// <summary>
    /// オーディオマスキングデータ
    /// 環境音によるマスキング効果イベント用
    /// </summary>
    [System.Serializable]
    public struct AudioMaskingData
    {
        public Vector3 SourcePosition;
        public float MaskingLevel;
        public float EffectRadius;
        public AudioEffectType[] MaskedTypes;
        public float MaskingDuration;
        public string MaskingSource;

        public static AudioMaskingData Create(Vector3 position, float level, float radius, string source)
        {
            return new AudioMaskingData
            {
                SourcePosition = position,
                MaskingLevel = level,
                EffectRadius = radius,
                MaskedTypes = new[] { AudioEffectType.Footstep, AudioEffectType.Movement, AudioEffectType.Voice },
                MaskingDuration = -1f, // 無制限
                MaskingSource = source
            };
        }
    }

    /// <summary>
    /// 3D空間オーディオデータ
    /// 空間オーディオイベント用
    /// </summary>
    [System.Serializable]
    public struct SpatialAudioData
    {
        public Vector3 SourcePosition;
        public Vector3 ListenerPosition;
        public AudioEffectType EffectType;
        public float Volume;
        public float Distance;
        public float Attenuation;
        public bool IsOccluded;

        public static SpatialAudioData Create(Vector3 source, Vector3 listener, AudioEffectType type, float volume)
        {
            float distance = Vector3.Distance(source, listener);
            return new SpatialAudioData
            {
                SourcePosition = source,
                ListenerPosition = listener,
                EffectType = type,
                Volume = volume,
                Distance = distance,
                Attenuation = 1f / (1f + distance),
                IsOccluded = false
            };
        }
    }

    /// <summary>
    /// 足音データ
    /// プレイヤー足音生成イベント用
    /// </summary>
    [System.Serializable]
    public struct FootstepData
    {
        public Vector3 Position;
        public PlayerMovementState MovementState;
        public float Volume;
        public string SurfaceType;
        public bool IsInStealthMode;
        public float NoiseLevel;

        public static FootstepData Create(Vector3 position, PlayerMovementState state, float volume, string surface, bool stealth)
        {
            return new FootstepData
            {
                Position = position,
                MovementState = state,
                Volume = volume,
                SurfaceType = surface,
                IsInStealthMode = stealth,
                NoiseLevel = stealth ? volume * 0.5f : volume
            };
        }
    }

    /// <summary>
    /// ステルスUI更新データ
    /// UI全般更新イベント用
    /// </summary>
    [System.Serializable]
    public struct StealthUIUpdateData
    {
        public StealthUIElement ElementType;
        public StealthState CurrentStealthState;
        public float StateTransitionProgress;
        public bool RequiresImmediateUpdate;
        public string UpdateReason;

        public static StealthUIUpdateData Create(StealthUIElement element, StealthState state, float progress, bool immediate, string reason)
        {
            return new StealthUIUpdateData
            {
                ElementType = element,
                CurrentStealthState = state,
                StateTransitionProgress = progress,
                RequiresImmediateUpdate = immediate,
                UpdateReason = reason
            };
        }
    }

    /// <summary>
    /// 検知メーターデータ
    /// 検知レベル表示更新イベント用
    /// </summary>
    [System.Serializable]
    public struct DetectionMeterData
    {
        public float CurrentDetectionLevel;
        public float PreviousDetectionLevel;
        public float DetectionRate;
        public bool IsIncreasing;
        public Color MeterColor;
        public bool ShouldPulse;

        public static DetectionMeterData Create(float current, float previous, float rate)
        {
            return new DetectionMeterData
            {
                CurrentDetectionLevel = current,
                PreviousDetectionLevel = previous,
                DetectionRate = rate,
                IsIncreasing = current > previous,
                MeterColor = Color.Lerp(Color.green, Color.red, current),
                ShouldPulse = current > 0.8f
            };
        }
    }

    /// <summary>
    /// インタラクションプロンプトデータ
    /// 相互作用UI表示イベント用
    /// </summary>
    [System.Serializable]
    public struct InteractionPromptData
    {
        public Vector3 InteractablePosition;
        public string PromptText;
        public bool ShouldShow;
        public float Distance;
        public string InteractionType;

        public static InteractionPromptData Create(Vector3 position, string text, bool show, float distance, string type)
        {
            return new InteractionPromptData
            {
                InteractablePosition = position,
                PromptText = text,
                ShouldShow = show,
                Distance = distance,
                InteractionType = type
            };
        }
    }

    /// <summary>
    /// ミニマップ更新データ
    /// ミニマップ表示更新イベント用
    /// </summary>
    [System.Serializable]
    public struct MinimapUpdateData
    {
        public Vector3 PlayerPosition;
        public Vector3[] NPCPositions;
        public Vector3[] HidingSpotPositions;
        public MinimapObjectType[] VisibleObjectTypes;
        public float MapRange;

        public static MinimapUpdateData Create(Vector3 playerPos, Vector3[] npcPos, Vector3[] hidingPos, float range)
        {
            return new MinimapUpdateData
            {
                PlayerPosition = playerPos,
                NPCPositions = npcPos ?? new Vector3[0],
                HidingSpotPositions = hidingPos ?? new Vector3[0],
                VisibleObjectTypes = new[] { MinimapObjectType.Player, MinimapObjectType.NPC, MinimapObjectType.HidingSpot },
                MapRange = range
            };
        }
    }

    /// <summary>
    /// チュートリアルステップデータ
    /// Learn & Grow価値実現用
    /// </summary>
    [System.Serializable]
    public struct TutorialStepData
    {
        public string StepId;
        public string StepName;
        public TutorialStepType StepType;
        public float CompletionPercentage;
        public float TimeSpentMinutes;
        public bool IsCompleted;
        public string[] CompletedActions;
        public string NextStepHint;

        public static TutorialStepData Create(string id, string name, TutorialStepType type, float completion, float timeSpent)
        {
            return new TutorialStepData
            {
                StepId = id,
                StepName = name,
                StepType = type,
                CompletionPercentage = completion,
                TimeSpentMinutes = timeSpent,
                IsCompleted = completion >= 1f,
                CompletedActions = new string[0],
                NextStepHint = ""
            };
        }
    }

    /// <summary>
    /// 学習進捗データ
    /// Learn & Grow価値実現用
    /// </summary>
    [System.Serializable]
    public struct LearningProgressData
    {
        public float OverallProgress;
        public float TotalLearningTimeMinutes;
        public int CompletedStepsCount;
        public int TotalStepsCount;
        public float LearningEfficiency;
        public bool IsOn70PercentReductionTrack;
        public float ProjectedCompletionTimeMinutes;

        public static LearningProgressData Create(float progress, float timeMinutes, int completed, int total)
        {
            float efficiency = completed > 0 ? progress / (timeMinutes / 60f) : 0f;
            float projected = efficiency > 0 ? (1f - progress) / efficiency * 60f : float.MaxValue;

            return new LearningProgressData
            {
                OverallProgress = progress,
                TotalLearningTimeMinutes = timeMinutes,
                CompletedStepsCount = completed,
                TotalStepsCount = total,
                LearningEfficiency = efficiency,
                IsOn70PercentReductionTrack = projected <= 12 * 60, // 12時間以内
                ProjectedCompletionTimeMinutes = projected
            };
        }
    }

    /// <summary>
    /// ゲームプレイ準備データ
    /// 15分ゲームプレイ実現用
    /// </summary>
    [System.Serializable]
    public struct GameplayReadinessData
    {
        public bool IsGameplayReady;
        public float TimeToGameplayMinutes;
        public float GameplayQualityScore;
        public string[] ReadyComponents;
        public string[] PendingComponents;
        public bool IsFifteenMinuteTarget;

        public static GameplayReadinessData Create(bool ready, float timeMinutes, float quality, string[] readyComponents, string[] pending)
        {
            return new GameplayReadinessData
            {
                IsGameplayReady = ready,
                TimeToGameplayMinutes = timeMinutes,
                GameplayQualityScore = quality,
                ReadyComponents = readyComponents ?? new string[0],
                PendingComponents = pending ?? new string[0],
                IsFifteenMinuteTarget = timeMinutes <= 15f
            };
        }
    }
}