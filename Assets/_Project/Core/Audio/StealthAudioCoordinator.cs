using asterivo.Unity60.Core;
using UnityEngine;
using System.Collections.Generic;
using asterivo.Unity60.Core.Audio.Data;
using asterivo.Unity60.Core.Events;
using asterivo.Unity60.Core.Debug;
using asterivo.Unity60.Core.Audio.Interfaces;
using Sirenix.OdinInspector;
using asterivo.Unity60.Core.Helpers;

namespace asterivo.Unity60.Core.Audio
{
    /// <summary>
    /// ステルスゲームプレイと一般オーディオの協調制御
    /// NPCの聴覚センサーシステムとの連携を管理
    /// ServiceLocator対応版
    /// </summary>
    public class StealthAudioCoordinator : MonoBehaviour, IStealthAudioService
    {
        [TabGroup("Stealth Coordinator", "AI Integration")]
        [Header("AI Integration Settings")]
        [SerializeField, Range(0f, 1f)] private float aiAlertThreshold = 0.5f;
        [SerializeField, Range(1f, 10f)] private float playerHidingRadius = 3f;
        [SerializeField] private LayerMask aiLayerMask = -1;

        [TabGroup("Stealth Coordinator", "Audio Reduction")]
        [Header("Audio Reduction Settings")]
        [SerializeField, Range(0f, 1f)] private float bgmReductionAmount = 0.4f;
        [SerializeField, Range(0f, 1f)] private float ambientReductionAmount = 0.6f;
        [SerializeField, Range(0f, 1f)] private float effectReductionAmount = 0.3f;

        [TabGroup("Stealth Coordinator", "Masking System")]
        [Header("Masking Effect Settings")]
        [SerializeField, Range(0f, 1f)] private float baseMaskingStrength = 0.2f;
        [SerializeField] private AnimationCurve weatherMaskingCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 0.8f);
        [SerializeField] private AnimationCurve timeMaskingCurve = AnimationCurve.EaseInOut(0f, 0.2f, 1f, 0.05f);

        [TabGroup("Stealth Coordinator", "Critical Actions")]
        [Header("Critical Stealth Actions")]
        [SerializeField] private string[] criticalActionTags = { "Lockpicking", "Hacking", "Infiltration" };
        [SerializeField, Range(0f, 3f)] private float criticalActionRadius = 2f;

        [TabGroup("Stealth Coordinator", "Events")]
        [Header("Event Integration")]
        [SerializeField] private GameEvent stealthModeActivatedEvent;
        [SerializeField] private GameEvent stealthModeDeactivatedEvent;
        [SerializeField] private GameEvent maskingLevelChangedEvent;

        [TabGroup("Stealth Coordinator", "Runtime Info")]
        [Header("Runtime Information")]
        [SerializeField, ReadOnly] private bool isStealthModeActive;
        [SerializeField, ReadOnly] private bool isOverrideActive;
        [SerializeField, ReadOnly] private float currentMaskingLevel;
        [SerializeField, ReadOnly] private int nearbyAlertAICount;

        // 内部状態管理
        private Transform playerTransform;
        private List<Transform> nearbyAI = new List<Transform>();
        private Dictionary<AudioCategory, float> categoryVolumeMultipliers = new Dictionary<AudioCategory, float>();
        private bool previousStealthModeState = false;
        private float globalMaskingStrength = 0f;

        // システム連携（ServiceLocator経由）
        private IAudioService audioService;
        private AudioManager audioManager;
        private DynamicAudioEnvironment dynamicEnvironment;
        
        // ✅ Singleton パターンを完全削除 - ServiceLocator専用実装
        
        // 初期化状態管理
        public bool IsInitialized { get; private set; }

        #region Unity Lifecycle

        private void Awake()
        {
            // ✅ ServiceLocator専用実装のみ - Singletonパターン完全削除
            DontDestroyOnLoad(gameObject);
            
            // ServiceLocatorに登録
            if (asterivo.Unity60.Core.FeatureFlags.UseServiceLocator)
            {
                try
                {
                    ServiceLocator.RegisterService<IStealthAudioService>(this);
                    
                    if (asterivo.Unity60.Core.FeatureFlags.EnableDebugLogging)
                    {
                        EventLogger.Log("[StealthAudioCoordinator] Successfully registered to ServiceLocator as IStealthAudioService");
                    }
                }
                catch (System.Exception ex)
                {
                    EventLogger.LogError($"[StealthAudioCoordinator] Failed to register to ServiceLocator: {ex.Message}");
                }
            }
            else
            {
                EventLogger.LogWarning("[StealthAudioCoordinator] ServiceLocator is disabled - service not registered");
            }
            
            InitializeCoordinator();
        }

        private void Start()
        {
            Initialize();
        }

        private void Update()
        {
            UpdateNearbyAIDetection();
            UpdateStealthModeState();
            UpdateMaskingEffects();
        }
        
        private void OnDestroy()
        {
            // ✅ ServiceLocator専用実装のみ - Singletonパターン完全削除
            // ServiceLocatorから登録解除
            if (asterivo.Unity60.Core.FeatureFlags.UseServiceLocator)
            {
                try
                {
                    ServiceLocator.UnregisterService<IStealthAudioService>();
                    
                    if (asterivo.Unity60.Core.FeatureFlags.EnableDebugLogging)
                    {
                        EventLogger.Log("[StealthAudioCoordinator] Unregistered from ServiceLocator");
                    }
                }
                catch (System.Exception ex)
                {
                    EventLogger.LogError($"[StealthAudioCoordinator] Failed to unregister from ServiceLocator: {ex.Message}");
                }
            }
        }

        #endregion

        #region Initialization

        /// <summary>
        /// コーディネーターの初期化
        /// </summary>
        private void InitializeCoordinator()
        {
            // プレイヤートランスフォームの検索
            var player = GameObject.FindWithTag("Player");
            if (player != null)
            {
                playerTransform = player.transform;
            }
            else
            {
                EventLogger.LogWarning("[StealthAudioCoordinator] Player object not found! Please assign a Player tag.");
            }
        }

        /// <summary>
        /// システム参照の検索
        /// Phase 3 移行パターン: ServiceLocator優先、Singleton フォールバック
        /// </summary>
        private void FindSystemReferences()
        {
            // ServiceLocator経由でAudioServiceを取得
            if (asterivo.Unity60.Core.FeatureFlags.UseServiceLocator && audioService == null)
            {
                try
                {
                    audioService = ServiceLocator.GetService<IAudioService>();
                    
                    if (audioService != null)
                    {
                        if (asterivo.Unity60.Core.FeatureFlags.EnableDebugLogging)
                        {
                            EventLogger.Log("[StealthAudioCoordinator] Successfully retrieved AudioService from ServiceLocator");
                        }
                    }
                    else
                    {
                        if (asterivo.Unity60.Core.FeatureFlags.EnableMigrationMonitoring)
                        {
                            EventLogger.LogWarning("[StealthAudioCoordinator] ServiceLocator returned null for IAudioService, falling back to legacy approach");
                        }
                    }
                }
                catch (System.Exception ex)
                {
                    EventLogger.LogError($"[StealthAudioCoordinator] Failed to retrieve AudioService from ServiceLocator: {ex.Message}");
                }
            }
            
            // ✅ ServiceLocator専用実装 - 直接AudioManagerを検索
            if (audioManager == null)
            {
                try
                {
                    audioManager = ServiceHelper.GetServiceWithFallback<AudioManager>();
                    if (audioManager != null && asterivo.Unity60.Core.FeatureFlags.EnableDebugLogging)
                    {
                        EventLogger.Log("[StealthAudioCoordinator] Found AudioManager via FindFirstObjectByType");
                    }
                    else if (audioManager == null)
                    {
                        EventLogger.LogError("[StealthAudioCoordinator] No AudioManager found");
                    }
                }
                catch (System.Exception ex)
                {
                    EventLogger.LogError($"[StealthAudioCoordinator] Failed to get AudioManager: {ex.Message}");
                }
            }

            if (dynamicEnvironment == null)
                dynamicEnvironment = ServiceHelper.GetServiceWithFallback<DynamicAudioEnvironment>();
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
        
        /// <summary>
        /// IInitializable実装 - ステルスオーディオコーディネーターの初期化
        /// </summary>
        public void Initialize()
        {
            if (IsInitialized) return;
            
            FindSystemReferences();
            InitializeCategoryMultipliers();
            
            IsInitialized = true;
            
            // デバッグログ (一時的に簡素化)
            EventLogger.Log("[StealthAudioCoordinator] Initialization complete");
        }

        #endregion
        
        #region IStealthAudioService Implementation
        
        /// <summary>
        /// 足音を生成
        /// </summary>
        public void CreateFootstep(Vector3 position, float intensity, string surfaceType)
        {
            if (!IsInitialized)
            {
                EventLogger.LogWarning("[StealthAudioCoordinator] System not initialized");
                return;
            }
            
            // TODO: 表面タイプに応じた足音の生成
            // デバッグログ出力
            EventLogger.Log($"[StealthAudioCoordinator] Creating footstep at {position}, intensity: {intensity}, surface: {surfaceType}");
        }
        
        /// <summary>
        /// 環境ノイズレベルを設定（マスキング効果用）
        /// </summary>
        public void SetEnvironmentNoiseLevel(float level)
        {
            if (!IsInitialized) return;
            
            globalMaskingStrength = Mathf.Clamp01(level);
            
            // デバッグログ出力
            EventLogger.Log($"[StealthAudioCoordinator] Environment noise level set to: {level}");
        }
        
        /// <summary>
        /// NPCに聞こえる音を生成
        /// </summary>
        public void EmitDetectableSound(Vector3 position, float radius, float intensity, string soundType)
        {
            if (!IsInitialized) return;
            
            // TODO: NPCの聴覚センサーへの通知
            NotifyAuditorySensors(position, radius, intensity);
            
            // デバッグログ出力
            EventLogger.Log($"[StealthAudioCoordinator] Detectable sound emitted: {soundType} at {position}");
        }
        
        /// <summary>
        /// 注意を引く音を再生
        /// </summary>
        public void PlayDistraction(Vector3 position, float radius)
        {
            if (!IsInitialized) return;
            
            EmitDetectableSound(position, radius, 0.8f, "Distraction");
        }
        
        /// <summary>
        /// 警戒レベルに応じたBGMを設定
        /// </summary>
        public void SetAlertLevelMusic(AlertLevel level)
        {
            if (!IsInitialized || audioService == null) return;
            
            // TODO: 警戒レベルに応じたBGM切り替え
            string bgmName = level switch
            {
                AlertLevel.None => "Normal",
                AlertLevel.Low => "Suspicious",
                AlertLevel.Medium => "Alert",
                AlertLevel.High => "Combat",
                AlertLevel.Combat => "Combat",
                _ => "Normal"
            };
            
            // audioService.PlayBGM(bgmName); // TODO: IBGMServiceが必要
            
            // デバッグログ出力
            EventLogger.Log($"[StealthAudioCoordinator] Alert level music set: {level} -> {bgmName}");
        }
        
        /// <summary>
        /// オーディオマスキング効果を適用
        /// </summary>
        public void ApplyAudioMasking(float maskingLevel)
        {
            if (!IsInitialized) return;
            
            currentMaskingLevel = Mathf.Clamp01(maskingLevel);
            
            // 既存のマスキングシステムを使用
            UpdateMaskingEffects();
            
            if (maskingLevelChangedEvent != null)
            {
                maskingLevelChangedEvent.Raise();
            }
        }
        
        /// <summary>
        /// NPCの聴覚センサーにサウンドイベントを通知
        /// </summary>
        public void NotifyAuditorySensors(Vector3 origin, float radius, float intensity)
        {
            if (!IsInitialized) return;
            
            // TODO: AIシステムとの連携実装
            // デバッグログ出力
            EventLogger.Log($"[StealthAudioCoordinator] Notifying auditory sensors: origin={origin}, radius={radius}, intensity={intensity}");
        }
        
        /// <summary>
        /// プレイヤーの隠密度に応じた音響調整
        /// </summary>
        public void AdjustStealthAudio(float stealthLevel)
        {
            if (!IsInitialized) return;
            
            // ステルスレベルに応じて音量を調整
            float volumeReduction = 1f - (stealthLevel * 0.3f); // 最大30%減音
            
            if (audioService != null)
            {
                audioService.SetCategoryVolume("bgm", bgmReductionAmount * volumeReduction);
                audioService.SetCategoryVolume("ambient", ambientReductionAmount * volumeReduction);
                audioService.SetCategoryVolume("effect", effectReductionAmount * volumeReduction);
            }
            
            // デバッグログ出力
            EventLogger.Log($"[StealthAudioCoordinator] Stealth audio adjusted: level={stealthLevel}, reduction={volumeReduction}");
        }

        #endregion

        #region Stealth Mode Detection

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

                    // AI の警戒レベルを確認
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

                EventLogger.Log($"<color=orange>[StealthAudioCoordinator]</color> Stealth mode {(newStealthMode ? "activated" : "deactivated")}");
            }
        }

        #endregion

        #region Public Interface

        /// <summary>
        /// 非ステルス音響を抑制すべきかを判定
        /// </summary>
        public bool ShouldReduceNonStealthAudio()
        {
            if (isOverrideActive)
                return isStealthModeActive;

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
            float totalMasking = baseMaskingStrength;

            // BGMによるマスキング
            if (IsBGMPlaying())
            {
                totalMasking += GetBGMMaskingStrength() * 0.3f;
            }

            // 環境音によるマスキング
            totalMasking += GetEnvironmentalMaskingAt(soundPosition) * 0.5f;

            // 天候・時間帯によるマスキング
            if (dynamicEnvironment != null)
            {
                var (env, weather, time) = dynamicEnvironment.GetCurrentState();
                totalMasking += GetWeatherMaskingEffect(weather) * 0.4f;
                totalMasking += GetTimeMaskingEffect(time) * 0.2f;
            }

            // 音響カテゴリによる調整
            totalMasking *= GetCategoryMaskingMultiplier(audioData.category);

            return Mathf.Clamp01(totalMasking);
        }

        /// <summary>
        /// NPCの聴覚システムへの影響度を計算
        /// </summary>
        public float GetNPCAudibilityMultiplier(AudioEventData audioData)
        {
            if (!audioData.affectsStealthGameplay)
                return 0f; // ステルスに影響しない音は NPCが感知しない

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
        /// 音響カテゴリの音量倍率を取得
        /// </summary>
        public float GetCategoryVolumeMultiplier(AudioCategory category)
        {
            return categoryVolumeMultipliers.TryGetValue(category, out float multiplier) ? multiplier : 1f;
        }

        /// <summary>
        /// ステルスモードの強制設定
        /// </summary>
        public void SetOverrideStealthMode(bool forceStealthMode)
        {
            isOverrideActive = true;
            isStealthModeActive = forceStealthMode;
            UpdateCategoryVolumeMultipliers();
        }

        /// <summary>
        /// オーバーライドの解除
        /// </summary>
        public void ClearStealthModeOverride()
        {
            isOverrideActive = false;
        }

        #endregion

        #region Private Methods

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

            // フォールバック：近くの隠れ場所オブジェクトをチェック
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

            // 周囲の重要アクションオブジェクトをチェック
            Collider[] actionObjects = Physics.OverlapSphere(
                playerTransform.position, 
                criticalActionRadius
            );

            foreach (var obj in actionObjects)
            {
                foreach (var tag in criticalActionTags)
                {
                    if (obj.CompareTag(tag))
                    {
                        // オブジェクトがアクティブ状態かを確認
                        var interactable = obj.GetComponent<IGameStateProvider>();
                        if (interactable != null && interactable.IsBeingUsed())
                        {
                            return true;
                        }
                    }
                }
            }

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

        /// <summary>
        /// マスキング効果の通知を受ける
        /// </summary>
        public void NotifyMaskingEffect(Vector3 position, float strength, float radius)
        {
            if (playerTransform == null) return;
            
            float distance = Vector3.Distance(playerTransform.position, position);
            if (distance > radius) return;
            
            // プレイヤー周辺のマスキング効果として記録
            // この情報は音響システムのマスキング計算に使用される
            float normalizedDistance = distance / radius;
            float effectiveStrength = strength * (1f - normalizedDistance);
            
            // グローバルマスキング強度を一時的に増加
            globalMaskingStrength = Mathf.Max(globalMaskingStrength, effectiveStrength * 0.5f);
            
            EventLogger.Log($"<color=purple>[StealthAudioCoordinator]</color> Masking effect applied: strength={effectiveStrength:F2}, distance={distance:F1}m");
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
        /// BGMが再生中かを確認
        /// </summary>
        private bool IsBGMPlaying()
        {
            // AudioManagerから BGM の再生状態を取得
            if (audioManager != null)
            {
                // 実装は AudioManager の API に依存
                return true; // プレースホルダー
            }
            return false;
        }

        /// <summary>
        /// BGMのマスキング強度を取得
        /// </summary>
        private float GetBGMMaskingStrength()
        {
            return 0.3f; // 基本的なBGMマスキング強度
        }

        /// <summary>
        /// 環境によるマスキング効果を取得
        /// </summary>
        private float GetEnvironmentalMaskingAt(Vector3 position)
        {
            // DynamicAudioEnvironment から環境情報を取得
            if (dynamicEnvironment != null)
            {
                return dynamicEnvironment.GetCurrentMaskingLevel();
            }
            return 0f;
        }

        /// <summary>
        /// 天候によるマスキング効果
        /// </summary>
        private float GetWeatherMaskingEffect(WeatherType weather)
        {
            float weatherValue = (float)weather / (System.Enum.GetValues(typeof(WeatherType)).Length - 1);
            return weatherMaskingCurve.Evaluate(weatherValue);
        }

        /// <summary>
        /// 時間帯によるマスキング効果
        /// </summary>
        private float GetTimeMaskingEffect(TimeOfDay time)
        {
            float timeValue = (float)time / (System.Enum.GetValues(typeof(TimeOfDay)).Length - 1);
            return timeMaskingCurve.Evaluate(timeValue);
        }

        /// <summary>
        /// カテゴリによるマスキング倍率
        /// </summary>
        private float GetCategoryMaskingMultiplier(AudioCategory category)
        {
            return category switch
            {
                AudioCategory.Stealth => 1f,   // ステルス音は完全なマスキング対象
                AudioCategory.Effect => 0.8f,  // 効果音は部分的にマスク
                AudioCategory.Ambient => 0.3f, // 環境音は軽くマスク
                AudioCategory.BGM => 0.1f,     // BGMは殆どマスクされない
                AudioCategory.UI => 0f,        // UI音はマスクされない
                _ => 1f
            };
        }

        #endregion

        #region Editor Helpers

#if UNITY_EDITOR
        [TabGroup("Stealth Coordinator", "Debug Tools")]
        [Button("Test Stealth Mode Activation")]
        public void TestStealthModeActivation()
        {
            SetOverrideStealthMode(!isStealthModeActive);
        }

        [TabGroup("Stealth Coordinator", "Debug Tools")]
        [Button("Calculate Current Masking")]
        public void DebugCalculateCurrentMasking()
        {
            if (playerTransform != null)
            {
                var testData = AudioEventData.CreateStealthDefault("DebugTest");
                testData.worldPosition = playerTransform.position;
                
                float masking = CalculateMaskingEffect(playerTransform.position, testData);
                EventLogger.Log($"Current masking level at player position: {masking:F2}");
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (playerTransform == null) return;

            // プレイヤー隠れ範囲の表示
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(playerTransform.position, playerHidingRadius);

            // 重要アクション範囲の表示
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(playerTransform.position, criticalActionRadius);

            // 近くのAIの表示
            Gizmos.color = Color.yellow;
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
