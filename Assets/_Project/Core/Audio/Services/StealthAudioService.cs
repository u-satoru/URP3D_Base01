using asterivo.Unity60.Core;
using UnityEngine;
using System.Collections.Generic;
using asterivo.Unity60.Core.Audio.Data;
using asterivo.Unity60.Core.Events;
using asterivo.Unity60.Core.Debug;
using Sirenix.OdinInspector;
using asterivo.Unity60.Core.Audio.Interfaces;

namespace asterivo.Unity60.Core.Audio.Services
{
    /// <summary>
    /// ステルスオーディオサービス (ServiceLocator専用)
    /// 従来のStealthAudioCoordinatorから完全移行した新実装
    /// Phase 3 Step 3.5 - ServiceLocator完全移行版
    /// </summary>
    public class StealthAudioService : MonoBehaviour, IStealthAudioService, IInitializable
    {
        [TabGroup("Stealth Service", "Settings")]
        [Header("AI Integration Settings")]
        [SerializeField, Range(0f, 1f)] private float aiAlertThreshold = 0.5f;
        [SerializeField, Range(1f, 10f)] private float playerHidingRadius = 3f;
        [SerializeField] private LayerMask aiLayerMask = -1;

        [TabGroup("Stealth Service", "Audio")]
        [Header("Audio Reduction Settings")]
        [SerializeField, Range(0f, 1f)] private float bgmReductionAmount = 0.4f;
        [SerializeField, Range(0f, 1f)] private float ambientReductionAmount = 0.6f;
        [SerializeField, Range(0f, 1f)] private float effectReductionAmount = 0.3f;

        [TabGroup("Stealth Service", "Events")]
        [Header("Event Integration")]
        [SerializeField] private GameEvent stealthModeActivatedEvent;
        [SerializeField] private GameEvent stealthModeDeactivatedEvent;
        [SerializeField] private GameEvent maskingLevelChangedEvent;

        [TabGroup("Stealth Service", "Runtime")]
        [Header("Player Reference")]
        [SerializeField] private Transform playerTransform;

        [Header("Runtime Information")]
        [SerializeField, ReadOnly] private bool isStealthModeActive;
        [SerializeField, ReadOnly] private float currentMaskingLevel;
        [SerializeField, ReadOnly] private int nearbyAlertAICount;
        [SerializeField, ReadOnly] private bool isServiceRegistered;

        // IInitializable実装
        public int Priority => 25;
        public bool IsInitialized { get; private set; }

        // 内部状態管理
        private IAudioService audioService;
        private List<Transform> nearbyAI = new List<Transform>();
        private Dictionary<AudioCategory, float> categoryVolumeMultipliers = new Dictionary<AudioCategory, float>();
        
        // ステルス検出状態
        private bool previousStealthModeState = false;
        private float globalMaskingStrength = 0f;

        #region Unity Lifecycle

        private void Awake()
        {
            RegisterToServiceLocator();
        }

        private void Start()
        {
            Initialize();
        }

        private void Update()
        {
            if (!IsInitialized) return;

            UpdateNearbyAIDetection();
            UpdateStealthModeState();
            UpdateMaskingEffects();
        }

        private void OnDestroy()
        {
            UnregisterFromServiceLocator();
        }

        #endregion

        #region ServiceLocator Integration

        /// <summary>
        /// ServiceLocatorへの登録
        /// </summary>
        private void RegisterToServiceLocator()
        {
            if (FeatureFlags.UseServiceLocator && FeatureFlags.MigrateStealthAudioCoordinator)
            {
                try
                {
                    ServiceLocator.RegisterService<IStealthAudioService>(this);
                    isServiceRegistered = true;
                    
                    if (FeatureFlags.EnableDebugLogging)
                    {
                        EventLogger.LogStatic("[StealthAudioService] Successfully registered to ServiceLocator as IStealthAudioService");
                    }
                }
                catch (System.Exception ex)
                {
                    EventLogger.LogErrorStatic($"[StealthAudioService] Failed to register to ServiceLocator: {ex.Message}");
                    isServiceRegistered = false;
                }
            }
        }

        /// <summary>
        /// ServiceLocatorからの登録解除
        /// </summary>
        private void UnregisterFromServiceLocator()
        {
            if (isServiceRegistered && FeatureFlags.UseServiceLocator)
            {
                try
                {
                    ServiceLocator.UnregisterService<IStealthAudioService>();
                    
                    if (FeatureFlags.EnableDebugLogging)
                    {
                        EventLogger.LogStatic("[StealthAudioService] Unregistered from ServiceLocator");
                    }
                }
                catch (System.Exception ex)
                {
                    EventLogger.LogErrorStatic($"[StealthAudioService] Failed to unregister from ServiceLocator: {ex.Message}");
                }
                finally
                {
                    isServiceRegistered = false;
                }
            }
        }

        #endregion

        #region IInitializable Implementation

        public void Initialize()
        {
            if (IsInitialized) return;
            
            // プレイヤー参照の取得
            FindPlayerReference();
            
            // AudioServiceの取得
            GetAudioServiceReference();
            
            // カテゴリ別音量倍率の初期化
            InitializeCategoryMultipliers();
            
            IsInitialized = true;
            
            if (FeatureFlags.EnableDebugLogging)
            {
                EventLogger.LogStatic("[StealthAudioService] Initialization complete");
            }
        }

        /// <summary>
        /// プレイヤー参照の検索 (SerializeField経由 - アーキテクチャ準拠)
        /// </summary>
        private void FindPlayerReference()
        {
            // Note: Core層からFeatures層への直接参照はアーキテクチャ違反のため
            // SerializeField による Inspector設定を推奨
            if (playerTransform == null)
            {
                EventLogger.LogWarningStatic("[StealthAudioService] Player Transform not assigned! Please set in Inspector.");
            }
            else
            {
                if (FeatureFlags.EnableDebugLogging)
                {
                    EventLogger.LogStatic("[StealthAudioService] Player reference found via Inspector");
                }
            }
        }

        /// <summary>
        /// AudioService参照の取得
        /// </summary>
        private void GetAudioServiceReference()
        {
            if (FeatureFlags.UseServiceLocator)
            {
                try
                {
                    audioService = ServiceLocator.GetService<IAudioService>();
                    
                    if (audioService != null && FeatureFlags.EnableDebugLogging)
                    {
                        EventLogger.LogStatic("[StealthAudioService] Successfully retrieved AudioService from ServiceLocator");
                    }
                }
                catch (System.Exception ex)
                {
                    EventLogger.LogErrorStatic($"[StealthAudioService] Failed to retrieve AudioService from ServiceLocator: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// カテゴリ別音量倍率の初期化
        /// </summary>
        private void InitializeCategoryMultipliers()
        {
            categoryVolumeMultipliers[AudioCategory.BGM] = 1f;
            categoryVolumeMultipliers[AudioCategory.Ambient] = 1f;
            categoryVolumeMultipliers[AudioCategory.Effect] = 1f;
            categoryVolumeMultipliers[AudioCategory.Stealth] = 1f;
            categoryVolumeMultipliers[AudioCategory.UI] = 1f;
        }

        #endregion

        #region IStealthAudioService Implementation

        public void CreateFootstep(Vector3 position, float intensity, string surfaceType)
        {
            if (!IsInitialized) return;
            
            // 足音生成ロジック (従来のStealthAudioCoordinatorから移植)
            if (FeatureFlags.EnableDebugLogging)
            {
                EventLogger.LogStatic($"[StealthAudioService] Creating footstep at {position}, intensity: {intensity}, surface: {surfaceType}");
            }

            // TODO: 実際の足音生成実装
            // - 表面タイプに応じた音声選択
            // - インテンシティに基づく音量調整
            // - NPCの聴覚センサーへの通知
        }

        public void SetEnvironmentNoiseLevel(float level)
        {
            if (!IsInitialized) return;
            
            globalMaskingStrength = Mathf.Clamp01(level);
            
            if (FeatureFlags.EnableDebugLogging)
            {
                EventLogger.LogStatic($"[StealthAudioService] Environment noise level set to: {level}");
            }
        }

        public void EmitDetectableSound(Vector3 position, float radius, float intensity, string soundType)
        {
            if (!IsInitialized) return;
            
            NotifyAuditorySensors(position, radius, intensity);
            
            if (FeatureFlags.EnableDebugLogging)
            {
                EventLogger.LogStatic($"[StealthAudioService] Detectable sound emitted: {soundType} at {position}");
            }
        }

        public void PlayDistraction(Vector3 position, float radius)
        {
            if (!IsInitialized) return;
            
            EmitDetectableSound(position, radius, 0.8f, "Distraction");
        }

        public void SetAlertLevelMusic(AlertLevel level)
        {
            if (!IsInitialized) return;
            
            string bgmName = level switch
            {
                AlertLevel.None => "Normal",
                AlertLevel.Low => "Suspicious", 
                AlertLevel.Medium => "Alert",
                AlertLevel.High => "Combat",
                AlertLevel.Combat => "Combat",
                _ => "Normal"
            };
            
            // TODO: IBGMServiceが必要
            // audioService.PlayBGM(bgmName); 
            
            if (FeatureFlags.EnableDebugLogging)
            {
                EventLogger.LogStatic($"[StealthAudioService] Alert level music set: {level} -> {bgmName}");
            }
        }

        public void ApplyAudioMasking(float maskingLevel)
        {
            if (!IsInitialized) return;
            
            currentMaskingLevel = Mathf.Clamp01(maskingLevel);
            
            if (maskingLevelChangedEvent != null)
            {
                maskingLevelChangedEvent.Raise();
            }

            if (FeatureFlags.EnableDebugLogging)
            {
                EventLogger.LogStatic($"[StealthAudioService] Audio masking applied: {maskingLevel}");
            }
        }

        public void NotifyAuditorySensors(Vector3 origin, float radius, float intensity)
        {
            if (!IsInitialized) return;
            
            // AI聴覚センサーへの通知ロジック
            if (FeatureFlags.EnableDebugLogging)
            {
                EventLogger.LogStatic($"[StealthAudioService] Notifying auditory sensors: origin={origin}, radius={radius}, intensity={intensity}");
            }

            // TODO: AI システムとの連携実装
        }

        public void AdjustStealthAudio(float stealthLevel)
        {
            if (!IsInitialized || audioService == null) return;
            
            float volumeReduction = 1f - (stealthLevel * 0.3f);
            
            try
            {
                audioService.SetCategoryVolume("bgm", bgmReductionAmount * volumeReduction);
                audioService.SetCategoryVolume("ambient", ambientReductionAmount * volumeReduction);
                audioService.SetCategoryVolume("effect", effectReductionAmount * volumeReduction);
                
                if (FeatureFlags.EnableDebugLogging)
                {
                    EventLogger.LogStatic($"[StealthAudioService] Stealth audio adjusted: level={stealthLevel}, reduction={volumeReduction}");
                }
            }
            catch (System.Exception ex)
            {
                EventLogger.LogErrorStatic($"[StealthAudioService] Failed to adjust stealth audio: {ex.Message}");
            }
        }

        #endregion

        #region Public Interface

        /// <summary>
        /// 非ステルス音響を抑制すべきかを判定
        /// </summary>
        public bool ShouldReduceNonStealthAudio()
        {
            if (!IsInitialized) return false;

            // プレイヤーが隠れモードの時
            if (IsPlayerInHidingMode())
                return true;

            // 近くのAIが警戒状態の時
            if (nearbyAlertAICount > 0)
                return true;

            // 重要なステルスアクション中
            if (IsPerformingCriticalStealthAction())
                return true;

            return false;
        }

        /// <summary>
        /// マスキング効果の計算
        /// </summary>
        public float CalculateMaskingEffect(Vector3 soundPosition, AudioEventData audioData)
        {
            if (!IsInitialized) return 0f;

            float totalMasking = globalMaskingStrength;

            // 環境要因によるマスキング効果を追加
            // TODO: より詳細なマスキング計算ロジックを実装

            return Mathf.Clamp01(totalMasking);
        }

        /// <summary>
        /// NPCの聴覚システムへの影響度を計算
        /// </summary>
        public float GetNPCAudibilityMultiplier(AudioEventData audioData)
        {
            if (!IsInitialized || !audioData.affectsStealthGameplay)
                return 0f;

            float maskingEffect = CalculateMaskingEffect(audioData.worldPosition, audioData);
            float audibilityMultiplier = 1f - maskingEffect;

            // ステルスモード時の追加減衰
            if (isStealthModeActive && audioData.canBeDuckedByTension)
            {
                audibilityMultiplier *= 0.7f;
            }

            return Mathf.Clamp01(audibilityMultiplier);
        }

        /// <summary>
        /// カテゴリ音量倍率を取得
        /// </summary>
        public float GetCategoryVolumeMultiplier(AudioCategory category)
        {
            return categoryVolumeMultipliers.TryGetValue(category, out float multiplier) ? multiplier : 1f;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// 近くのAI検出の更新
        /// </summary>
        private void UpdateNearbyAIDetection()
        {
            if (playerTransform == null) return;

            nearbyAI.Clear();
            nearbyAlertAICount = 0;

            // 周囲のAIエージェントを検索
            Collider[] nearbyColliders = Physics.OverlapSphere(
                playerTransform.position, 
                playerHidingRadius * 2f, 
                aiLayerMask
            );

            foreach (var collider in nearbyColliders)
            {
                if (collider.CompareTag("AI") || collider.CompareTag("Enemy"))
                {
                    nearbyAI.Add(collider.transform);

                    // AIの警戒レベルを確認
                    var aiController = collider.GetComponent<IGameStateProvider>();
                    if (aiController != null && aiController.GetAlertLevel() > aiAlertThreshold)
                    {
                        nearbyAlertAICount++;
                    }
                }
            }
        }

        /// <summary>
        /// ステルスモード状態の更新
        /// </summary>
        private void UpdateStealthModeState()
        {
            bool newStealthMode = ShouldReduceNonStealthAudio();

            if (newStealthMode != previousStealthModeState)
            {
                isStealthModeActive = newStealthMode;
                previousStealthModeState = newStealthMode;

                // 音量倍率の更新
                UpdateCategoryVolumeMultipliers();

                // イベント発行
                if (newStealthMode)
                {
                    stealthModeActivatedEvent?.Raise();
                }
                else
                {
                    stealthModeDeactivatedEvent?.Raise();
                }

                if (FeatureFlags.EnableDebugLogging)
                {
                    EventLogger.LogStatic($"<color=orange>[StealthAudioService]</color> Stealth mode {(newStealthMode ? "activated" : "deactivated")}");
                }
            }
        }

        /// <summary>
        /// マスキング効果の更新
        /// </summary>
        private void UpdateMaskingEffects()
        {
            if (playerTransform == null) return;

            // プレイヤー位置でのマスキング効果を計算
            var dummyAudioData = AudioEventData.CreateDefault("MaskingCalculation");
            dummyAudioData.worldPosition = playerTransform.position;

            float newMaskingLevel = CalculateMaskingEffect(playerTransform.position, dummyAudioData);

            if (Mathf.Abs(newMaskingLevel - currentMaskingLevel) > 0.05f)
            {
                currentMaskingLevel = newMaskingLevel;
                maskingLevelChangedEvent?.Raise();
            }
        }

        /// <summary>
        /// プレイヤーが隠れモードかを判定
        /// </summary>
        private bool IsPlayerInHidingMode()
        {
            if (playerTransform == null) return false;

            // プレイヤーコントローラーからの状態取得を試みる
            var playerController = playerTransform.GetComponent<IGameStateProvider>();
            if (playerController != null)
            {
                return playerController.IsInHidingMode();
            }

            // フォールバック: 近くの隠れ場所オブジェクトをチェック
            Collider[] hideSpots = Physics.OverlapSphere(
                playerTransform.position, 
                playerHidingRadius, 
                LayerMask.GetMask("HideSpot")
            );

            return hideSpots.Length > 0;
        }

        /// <summary>
        /// 重要なステルスアクション中かを判定
        /// </summary>
        private bool IsPerformingCriticalStealthAction()
        {
            if (playerTransform == null) return false;

            // TODO: 重要なステルスアクションの判定ロジックを実装
            return false;
        }

        /// <summary>
        /// カテゴリ別音量倍率の更新
        /// </summary>
        private void UpdateCategoryVolumeMultipliers()
        {
            if (isStealthModeActive)
            {
                categoryVolumeMultipliers[AudioCategory.BGM] = 1f - bgmReductionAmount;
                categoryVolumeMultipliers[AudioCategory.Ambient] = 1f - ambientReductionAmount;
                categoryVolumeMultipliers[AudioCategory.Effect] = 1f - effectReductionAmount;
                categoryVolumeMultipliers[AudioCategory.Stealth] = 1f; // ステルス音は維持
                categoryVolumeMultipliers[AudioCategory.UI] = 1f; // UI音は維持
            }
            else
            {
                // 通常状態に復帰
                categoryVolumeMultipliers[AudioCategory.BGM] = 1f;
                categoryVolumeMultipliers[AudioCategory.Ambient] = 1f;
                categoryVolumeMultipliers[AudioCategory.Effect] = 1f;
                categoryVolumeMultipliers[AudioCategory.Stealth] = 1f;
                categoryVolumeMultipliers[AudioCategory.UI] = 1f;
            }
        }

        #endregion

        #region Editor Support

#if UNITY_EDITOR
        [TabGroup("Stealth Service", "Debug")]
        [Button("Test Stealth Mode")]
        private void TestStealthMode()
        {
            isStealthModeActive = !isStealthModeActive;
            UpdateCategoryVolumeMultipliers();
            
            if (FeatureFlags.EnableDebugLogging)
            {
                EventLogger.LogStatic($"[StealthAudioService] Stealth mode {(isStealthModeActive ? "activated" : "deactivated")}");
            }
        }

        [Button("Test Service Registration")]
        private void TestServiceRegistration()
        {
            var service = ServiceLocator.GetService<IStealthAudioService>();
            if (service != null)
            {
                EventLogger.LogStatic($"[StealthAudioService] ✅ Service successfully retrieved from ServiceLocator");
            }
            else
            {
                EventLogger.LogErrorStatic($"[StealthAudioService] ❌ Service not found in ServiceLocator");
            }
        }

        [Button("Force Initialize")]
        private void ForceInitialize()
        {
            IsInitialized = false;
            Initialize();
        }

        private void OnDrawGizmosSelected()
        {
            if (playerTransform == null) return;

            // プレイヤー隠れ範囲の表示
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(playerTransform.position, playerHidingRadius);

            // AI検出範囲の表示
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(playerTransform.position, playerHidingRadius * 2f);

            // 近くのAIの表示
            Gizmos.color = Color.red;
            foreach (var ai in nearbyAI)
            {
                if (ai != null)
                {
                    Gizmos.DrawLine(playerTransform.position, ai.position);
                    Gizmos.DrawWireCube(ai.position, Vector3.one);
                }
            }
        }
#endif

        #endregion
    }
}