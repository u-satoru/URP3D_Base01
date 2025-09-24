using asterivo.Unity60.Core;
using UnityEngine;
using System.Collections.Generic;
using asterivo.Unity60.Core.Audio.Data;
using asterivo.Unity60.Core.Events;
// using asterivo.Unity60.Core.Debug;
using asterivo.Unity60.Core.Audio.Interfaces;
// using asterivo.Unity60.Core.Data;
// using asterivo.Unity60.Core.Services; // Removed to avoid circular dependency
using Sirenix.OdinInspector;
// using asterivo.Unity60.Core.Helpers;

namespace asterivo.Unity60.Core.Audio
{
    /// <summary>
    /// スチE��スゲームプレイと一般オーチE��オの協調制御
    /// NPCの聴覚センサーシスチE��との連携を管琁E    /// ServiceLocator対応版
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

        // 冁E��状態管琁E        private Transform playerTransform;
        private List<Transform> nearbyAI = new List<Transform>();
        private Dictionary<AudioCategory, float> categoryVolumeMultipliers = new Dictionary<AudioCategory, float>();
        private bool previousStealthModeState = false;
        private float globalMaskingStrength = 0f;

        // シスチE��連携�E�EerviceLocator経由�E�E        private IAudioService audioService;
        private AudioManager audioManager;
        private DynamicAudioEnvironment dynamicEnvironment;

        // ✁ESingleton パターンを完�E削除 - ServiceLocator専用実裁E
        // 初期化状態管琁E        public bool IsInitialized { get; private set; }

        #region Unity Lifecycle

        private void Awake()
        {
            // ✁EServiceLocator専用実裁E�Eみ - Singletonパターン完�E削除
            DontDestroyOnLoad(gameObject);

            // ServiceLocatorに登録
            if (asterivo.Unity60.Core.FeatureFlags.UseServiceLocator)
            {
                try
                {
                    ServiceLocator.RegisterService<IStealthAudioService>(this);

                    if (asterivo.Unity60.Core.FeatureFlags.EnableDebugLogging)
                    {
                        var eventLogger = ServiceLocator.GetService<IEventLogger>();
                        if (eventLogger != null) {
                            eventLogger.Log("[StealthAudioCoordinator] Successfully registered to ServiceLocator as IStealthAudioService");
                        }
                    }
                }
                catch (System.Exception ex)
                {
                    var eventLogger = ServiceLocator.GetService<IEventLogger>();
                    if (eventLogger != null) {
                        eventLogger.LogError($"[StealthAudioCoordinator] Failed to register to ServiceLocator: {ex.Message}");
                    }
                }
            }
            else
            {
                var eventLogger = ServiceLocator.GetService<IEventLogger>();
                if (eventLogger != null) {
                    eventLogger.LogWarning("[StealthAudioCoordinator] ServiceLocator is disabled - service not registered");
                }
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
            // ✁EServiceLocator専用実裁E�Eみ - Singletonパターン完�E削除
            // ServiceLocatorから登録解除
            if (asterivo.Unity60.Core.FeatureFlags.UseServiceLocator)
            {
                try
                {
                    ServiceLocator.UnregisterService<IStealthAudioService>();

                    if (asterivo.Unity60.Core.FeatureFlags.EnableDebugLogging)
                    {
                        var eventLogger = ServiceLocator.GetService<IEventLogger>();
                        if (eventLogger != null) {
                            eventLogger.Log("[StealthAudioCoordinator] Unregistered from ServiceLocator");
                        }
                    }
                }
                catch (System.Exception ex)
                {
                    var eventLogger = ServiceLocator.GetService<IEventLogger>();
                    if (eventLogger != null) {
                        eventLogger.LogError($"[StealthAudioCoordinator] Failed to unregister from ServiceLocator: {ex.Message}");
                    }
                }
            }
        }

        #endregion

        #region Initialization

        /// <summary>
        /// コーチE��ネ�Eターの初期匁E        /// </summary>
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
                var eventLogger = ServiceLocator.GetService<IEventLogger>();
                if (eventLogger != null) {
                    eventLogger.LogWarning("[StealthAudioCoordinator] Player object not found! Please assign a Player tag.");
                }
            }
        }

        /// <summary>
        /// シスチE��参�Eの検索
        /// Phase 3 移行パターン: ServiceLocator優先、Singleton フォールバック
        /// </summary>
        private void FindSystemReferences()
        {
            // ServiceLocator経由でAudioServiceを取征E            if (asterivo.Unity60.Core.FeatureFlags.UseServiceLocator && audioService == null)
            {
                try
                {
                    audioService = ServiceLocator.GetService<IAudioService>();

                    if (audioService != null)
                    {
                        if (asterivo.Unity60.Core.FeatureFlags.EnableDebugLogging)
                        {
                            var eventLogger = ServiceLocator.GetService<IEventLogger>();
                            if (eventLogger != null) {
                                eventLogger.Log("[StealthAudioCoordinator] Successfully retrieved AudioService from ServiceLocator");
                            }
                        }
                    }
                    else
                    {
                        if (asterivo.Unity60.Core.FeatureFlags.EnableMigrationMonitoring)
                        {
                            var eventLogger = ServiceLocator.GetService<IEventLogger>();
                            if (eventLogger != null) {
                                eventLogger.LogWarning("[StealthAudioCoordinator] ServiceLocator returned null for IAudioService, falling back to legacy approach");
                            }
                        }
                    }
                }
                catch (System.Exception ex)
                {
                    var eventLogger = ServiceLocator.GetService<IEventLogger>();
                    if (eventLogger != null) {
                        eventLogger.LogError($"[StealthAudioCoordinator] Failed to retrieve AudioService from ServiceLocator: {ex.Message}");
                    }
                }
            }

            // ✁EServiceLocator専用実裁E- 直接AudioManagerを検索
            if (audioManager == null)
            {
                try
                {
                    audioManager = ServiceHelper.GetServiceWithFallback<AudioManager>();
                    if (audioManager != null && asterivo.Unity60.Core.FeatureFlags.EnableDebugLogging)
                    {
                        var eventLogger = ServiceLocator.GetService<IEventLogger>();
                        if (eventLogger != null) {
                            eventLogger.Log("[StealthAudioCoordinator] Found AudioManager via FindFirstObjectByType");
                        }
                    }
                    else if (audioManager == null)
                    {
                        var eventLogger = ServiceLocator.GetService<IEventLogger>();
                        if (eventLogger != null) {
                            eventLogger.LogError("[StealthAudioCoordinator] No AudioManager found");
                        }
                    }
                }
                catch (System.Exception ex)
                {
                    var eventLogger = ServiceLocator.GetService<IEventLogger>();
                    if (eventLogger != null) {
                        eventLogger.LogError($"[StealthAudioCoordinator] Failed to get AudioManager: {ex.Message}");
                    }
                }
            }

            if (dynamicEnvironment == null)
                dynamicEnvironment = ServiceHelper.GetServiceWithFallback<DynamicAudioEnvironment>();
        }

        /// <summary>
        /// カチE��リ別音量倍率の初期匁E        /// </summary>
        private void InitializeCategoryMultipliers()
        {
            categoryVolumeMultipliers[AudioCategory.BGM] = 1f;
            categoryVolumeMultipliers[AudioCategory.Ambient] = 1f;
            categoryVolumeMultipliers[AudioCategory.Effect] = 1f;
            categoryVolumeMultipliers[AudioCategory.Stealth] = 1f;
            categoryVolumeMultipliers[AudioCategory.UI] = 1f;
        }

        /// <summary>
        /// IInitializable実裁E- スチE��スオーチE��オコーチE��ネ�Eターの初期匁E        /// </summary>
        public void Initialize()
        {
            if (IsInitialized) return;

            FindSystemReferences();
            InitializeCategoryMultipliers();

            IsInitialized = true;

            // チE��チE��ログ (一時的に簡素匁E
            {
                var eventLogger = ServiceLocator.GetService<IEventLogger>();
                if (eventLogger != null) eventLogger.Log("[StealthAudioCoordinator] Initialization complete");
            }
        }

        #endregion

        #region IStealthAudioService Implementation

        /// <summary>
        /// 足音を生戁E        /// </summary>
        public void CreateFootstep(Vector3 position, float intensity, string surfaceType)
        {
            if (!IsInitialized)
            {
                {
                    var eventLogger = ServiceLocator.GetService<IEventLogger>();
                    if (eventLogger != null) eventLogger.LogWarning("[StealthAudioCoordinator] System not initialized");
                }
                return;
            }

            // TODO: 表面タイプに応じた足音の生�E
            // チE��チE��ログ出劁E            {
                var eventLogger = ServiceLocator.GetService<IEventLogger>();
                if (eventLogger != null) eventLogger.Log($"[StealthAudioCoordinator] Creating footstep at {position}, intensity: {intensity}, surface: {surfaceType}");
            }
        }

        /// <summary>
        /// 環墁E��イズレベルを設定（�Eスキング効果用�E�E        /// </summary>
        public void SetEnvironmentNoiseLevel(float level)
        {
            if (!IsInitialized) return;

            globalMaskingStrength = Mathf.Clamp01(level);

            // チE��チE��ログ出劁E            {
                var eventLogger = ServiceLocator.GetService<IEventLogger>();
                if (eventLogger != null) eventLogger.Log($"[StealthAudioCoordinator] Environment noise level set to: {level}");
            }
        }

        /// <summary>
        /// NPCに聞こえる音を生戁E        /// </summary>
        public void EmitDetectableSound(Vector3 position, float radius, float intensity, string soundType)
        {
            if (!IsInitialized) return;

            // TODO: NPCの聴覚センサーへの通知
            NotifyAuditorySensors(position, radius, intensity);

            // チE��チE��ログ出劁E            {
                var eventLogger = ServiceLocator.GetService<IEventLogger>();
                if (eventLogger != null) eventLogger.Log($"[StealthAudioCoordinator] Detectable sound emitted: {soundType} at {position}");
            }
        }

        /// <summary>
        /// 注意を引く音を�E甁E        /// </summary>
        public void PlayDistraction(Vector3 position, float radius)
        {
            if (!IsInitialized) return;

            EmitDetectableSound(position, radius, 0.8f, "Distraction");
        }

        /// <summary>
        /// 警戒レベルに応じたBGMを設宁E        /// </summary>
        public void SetAlertLevelMusic(AlertLevel level)
        {
            if (!IsInitialized || audioService == null) return;

            // TODO: 警戒レベルに応じたBGM刁E��替ぁE            string bgmName = level switch
            {
                AlertLevel.Relaxed => "Normal",
                AlertLevel.Suspicious => "Suspicious",
                AlertLevel.Investigating => "Alert",
                AlertLevel.Alert => "Combat",
                _ => "Normal"
            };

            // audioService.PlayBGM(bgmName); // TODO: IBGMServiceが忁E��E
            // チE��チE��ログ出劁E            {
                var eventLogger = ServiceLocator.GetService<IEventLogger>();
                if (eventLogger != null) eventLogger.Log($"[StealthAudioCoordinator] Alert level music set: {level} -> {bgmName}");
            }
        }

        /// <summary>
        /// オーチE��オマスキング効果を適用
        /// </summary>
        public void ApplyAudioMasking(float maskingLevel)
        {
            if (!IsInitialized) return;

            currentMaskingLevel = Mathf.Clamp01(maskingLevel);

            // 既存�EマスキングシスチE��を使用
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

            // TODO: AIシスチE��との連携実裁E            // チE��チE��ログ出劁E            {
                var eventLogger = ServiceLocator.GetService<IEventLogger>();
                if (eventLogger != null) eventLogger.Log($"[StealthAudioCoordinator] Notifying auditory sensors: origin={origin}, radius={radius}, intensity={intensity}");
            }
        }

        /// <summary>
        /// プレイヤーの隠寁E��に応じた音響調整
        /// </summary>
        public void AdjustStealthAudio(float stealthLevel)
        {
            if (!IsInitialized) return;

            // スチE��スレベルに応じて音量を調整
            float volumeReduction = 1f - (stealthLevel * 0.3f); // 最大30%減音

            if (audioService != null)
            {
                audioService.SetCategoryVolume("bgm", bgmReductionAmount * volumeReduction);
                audioService.SetCategoryVolume("ambient", ambientReductionAmount * volumeReduction);
                audioService.SetCategoryVolume("effect", effectReductionAmount * volumeReduction);
            }

            // チE��チE��ログ出劁E            {
                var eventLogger = ServiceLocator.GetService<IEventLogger>();
                if (eventLogger != null) eventLogger.Log($"[StealthAudioCoordinator] Stealth audio adjusted: level={stealthLevel}, reduction={volumeReduction}");
            }
        }

        #endregion

        #region Stealth Mode Detection

        /// <summary>
        /// 近くのAI検�Eの更新
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

                    // AI の警戒レベルを確誁E                    var aiController = collider.GetComponent<IGameStateProvider>();
                    if (aiController != null && aiController.GetAlertLevel() > aiAlertThreshold)
                    {
                        nearbyAlertAICount++;
                    }
                }
            }
        }

        /// <summary>
        /// スチE��スモード状態�E更新
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

                // イベント発衁E                if (newStealthMode)
                {
                    stealthModeActivatedEvent?.Raise();
                }
                else
                {
                    stealthModeDeactivatedEvent?.Raise();
                }

                {
                    var eventLogger = ServiceLocator.GetService<IEventLogger>();
                    if (eventLogger != null) eventLogger.Log($"<color=orange>[StealthAudioCoordinator]</color> Stealth mode {(newStealthMode ? "activated" : "deactivated")}");
                }
            }
        }

        #endregion

        #region Public Interface

        /// <summary>
        /// 非スチE��ス音響を抑制すべきかを判宁E        /// </summary>
        public bool ShouldReduceNonStealthAudio()
        {
            if (isOverrideActive)
                return isStealthModeActive;

            // プレイヤーが隠れモード�E晁E            if (IsPlayerInHidingMode())
                return true;

            // 近くのAIが警戒状態�E晁E            if (nearbyAlertAICount > 0)
                return true;

            // 重要なスチE��スアクション中
            if (IsPerformingCriticalStealthAction())
                return true;

            return false;
        }

        /// <summary>
        /// マスキング効果�E計箁E        /// </summary>
        public float CalculateMaskingEffect(Vector3 soundPosition, AudioEventData audioData)
        {
            float totalMasking = baseMaskingStrength;

            // BGMによるマスキング
            if (IsBGMPlaying())
            {
                totalMasking += GetBGMMaskingStrength() * 0.3f;
            }

            // 環墁E��によるマスキング
            totalMasking += GetEnvironmentalMaskingAt(soundPosition) * 0.5f;

            // 天候�E時間帯によるマスキング
            if (dynamicEnvironment != null)
            {
                var (env, weather, time) = dynamicEnvironment.GetCurrentState();
                totalMasking += GetWeatherMaskingEffect(weather) * 0.4f;
                totalMasking += GetTimeMaskingEffect(time) * 0.2f;
            }

            // 音響カチE��リによる調整
            totalMasking *= GetCategoryMaskingMultiplier(audioData.category);

            return Mathf.Clamp01(totalMasking);
        }

        /// <summary>
        /// NPCの聴覚シスチE��への影響度を計箁E        /// </summary>
        public float GetNPCAudibilityMultiplier(AudioEventData audioData)
        {
            if (!audioData.affectsStealthGameplay)
                return 0f; // スチE��スに影響しなぁE��は NPCが感知しなぁE
            float maskingEffect = CalculateMaskingEffect(audioData.worldPosition, audioData);
            float audibilityMultiplier = 1f - maskingEffect;

            // スチE��スモード時の追加減衰
            if (isStealthModeActive && audioData.canBeDuckedByTension)
            {
                audibilityMultiplier *= 0.7f;
            }

            return Mathf.Clamp01(audibilityMultiplier);
        }

        /// <summary>
        /// 音響カチE��リの音量倍率を取征E        /// </summary>
        public float GetCategoryVolumeMultiplier(AudioCategory category)
        {
            return categoryVolumeMultipliers.TryGetValue(category, out float multiplier) ? multiplier : 1f;
        }

        /// <summary>
        /// スチE��スモード�E強制設宁E        /// </summary>
        public void SetOverrideStealthMode(bool forceStealthMode)
        {
            isOverrideActive = true;
            isStealthModeActive = forceStealthMode;
            UpdateCategoryVolumeMultipliers();
        }

        /// <summary>
        /// オーバ�Eライド�E解除
        /// </summary>
        public void ClearStealthModeOverride()
        {
            isOverrideActive = false;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// プレイヤーが隠れモードかを判宁E        /// </summary>
        private bool IsPlayerInHidingMode()
        {
            if (playerTransform == null) return false;

            // プレイヤーコントローラーからの状態取得を試みめE            var playerController = playerTransform.GetComponent<IGameStateProvider>();
            if (playerController != null)
            {
                return playerController.IsInHidingMode();
            }

            // フォールバック�E�近くの隠れ場所オブジェクトをチェチE��
            Collider[] hideSpots = Physics.OverlapSphere(
                playerTransform.position,
                playerHidingRadius,
                LayerMask.GetMask("HideSpot")
            );

            return hideSpots.Length > 0;
        }

        /// <summary>
        /// 重要なスチE��スアクション中かを判宁E        /// </summary>
        private bool IsPerformingCriticalStealthAction()
        {
            if (playerTransform == null) return false;

            // 周囲の重要アクションオブジェクトをチェチE��
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
                        // オブジェクトがアクチE��ブ状態かを確誁E                        var interactable = obj.GetComponent<IGameStateProvider>();
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
        /// カチE��リ別音量倍率の更新
        /// </summary>
        private void UpdateCategoryVolumeMultipliers()
        {
            if (isStealthModeActive)
            {
                categoryVolumeMultipliers[AudioCategory.BGM] = 1f - bgmReductionAmount;
                categoryVolumeMultipliers[AudioCategory.Ambient] = 1f - ambientReductionAmount;
                categoryVolumeMultipliers[AudioCategory.Effect] = 1f - effectReductionAmount;
                categoryVolumeMultipliers[AudioCategory.Stealth] = 1f; // スチE��ス音は維持E                categoryVolumeMultipliers[AudioCategory.UI] = 1f; // UI音は維持E            }
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
        /// マスキング効果�E通知を受ける
        /// </summary>
        public void NotifyMaskingEffect(Vector3 position, float strength, float radius)
        {
            if (playerTransform == null) return;

            float distance = Vector3.Distance(playerTransform.position, position);
            if (distance > radius) return;

            // プレイヤー周辺のマスキング効果として記録
            // こ�E惁E��は音響シスチE��のマスキング計算に使用されめE            float normalizedDistance = distance / radius;
            float effectiveStrength = strength * (1f - normalizedDistance);

            // グローバルマスキング強度を一時的に増加
            globalMaskingStrength = Mathf.Max(globalMaskingStrength, effectiveStrength * 0.5f);

            {
                var eventLogger = ServiceLocator.GetService<IEventLogger>();
                if (eventLogger != null) eventLogger.Log($"<color=purple>[StealthAudioCoordinator]</color> Masking effect applied: strength={effectiveStrength:F2}, distance={distance:F1}m");
            }
        }

        /// <summary>
        /// マスキング効果�E更新
        /// </summary>
        private void UpdateMaskingEffects()
        {
            if (playerTransform == null) return;

            // プレイヤー位置でのマスキング効果を計箁E            var dummyAudioData = AudioEventData.CreateDefault("MaskingCalculation");
            dummyAudioData.worldPosition = playerTransform.position;

            float newMaskingLevel = CalculateMaskingEffect(playerTransform.position, dummyAudioData);

            if (Mathf.Abs(newMaskingLevel - currentMaskingLevel) > 0.05f)
            {
                currentMaskingLevel = newMaskingLevel;
                maskingLevelChangedEvent?.Raise();
            }
        }

        /// <summary>
        /// BGMが�E生中かを確誁E        /// </summary>
        private bool IsBGMPlaying()
        {
            // AudioManagerから BGM の再生状態を取征E            if (audioManager != null)
            {
                // 実裁E�E AudioManager の API に依孁E                return true; // プレースホルダー
            }
            return false;
        }

        /// <summary>
        /// BGMのマスキング強度を取征E        /// </summary>
        private float GetBGMMaskingStrength()
        {
            return 0.3f; // 基本皁E��BGMマスキング強度
        }

        /// <summary>
        /// 環墁E��よるマスキング効果を取征E        /// </summary>
        private float GetEnvironmentalMaskingAt(Vector3 position)
        {
            // DynamicAudioEnvironment から環墁E��報を取征E            if (dynamicEnvironment != null)
            {
                return dynamicEnvironment.GetCurrentMaskingLevel();
            }
            return 0f;
        }

        /// <summary>
        /// 天候によるマスキング効极E        /// </summary>
        private float GetWeatherMaskingEffect(WeatherType weather)
        {
            float weatherValue = (float)weather / (System.Enum.GetValues(typeof(WeatherType)).Length - 1);
            return weatherMaskingCurve.Evaluate(weatherValue);
        }

        /// <summary>
        /// 時間帯によるマスキング効极E        /// </summary>
        private float GetTimeMaskingEffect(TimeOfDay time)
        {
            float timeValue = (float)time / (System.Enum.GetValues(typeof(TimeOfDay)).Length - 1);
            return timeMaskingCurve.Evaluate(timeValue);
        }

        /// <summary>
        /// カチE��リによるマスキング倍率
        /// </summary>
        private float GetCategoryMaskingMultiplier(AudioCategory category)
        {
            return category switch
            {
                AudioCategory.Stealth => 1f,   // スチE��ス音は完�Eなマスキング対象
                AudioCategory.Effect => 0.8f,  // 効果音は部刁E��にマスク
                AudioCategory.Ambient => 0.3f, // 環墁E��は軽く�Eスク
                AudioCategory.BGM => 0.1f,     // BGMは殁E��マスクされなぁE                AudioCategory.UI => 0f,        // UI音はマスクされなぁE                _ => 1f
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
                {
                    var eventLogger = ServiceLocator.GetService<IEventLogger>();
                    if (eventLogger != null) eventLogger.Log($"Current masking level at player position: {masking:F2}");
                }
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (playerTransform == null) return;

            // プレイヤー隠れ篁E��の表示
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(playerTransform.position, playerHidingRadius);

            // 重要アクション篁E��の表示
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

        #region IStealthAudioService Implementation

        /// <summary>
        /// 目標達成時のサウンドを再生
        /// </summary>
        /// <param name="withBonus">ボ�Eナス付きかどぁE��</param>
        public void PlayObjectiveCompleteSound(bool withBonus)
        {
            if (!IsInitialized || audioService == null)
            {
                var eventLogger = ServiceLocator.GetService<IEventLogger>();
                if (eventLogger != null && FeatureFlags.EnableDebugLogging)
                {
                    eventLogger.LogWarning("[StealthAudioCoordinator] Cannot play objective complete sound - system not initialized");
                }
                return;
            }

            try
            {
                // Determine which sound effect to play based on bonus status
                string soundEffect = withBonus ? "objective_complete_bonus" : "objective_complete";
                float volume = withBonus ? 1.0f : 0.8f;

                // Play the objective complete sound effect
                // Using the effect category for objective completion sounds
                audioService.PlaySound(soundEffect,
                    playerTransform != null ? playerTransform.position : Vector3.zero,
                    volume);;

                // If in stealth mode, apply appropriate volume adjustments
                if (isStealthModeActive)
                {
                    // Apply stealth mode volume reduction to maintain stealth atmosphere
                    float stealthVolume = volume * (1f - effectReductionAmount);
                    audioService.SetCategoryVolume("effect", stealthVolume);
                }

                // Log the action if debug logging is enabled
                if (FeatureFlags.EnableDebugLogging)
                {
                    var eventLogger = ServiceLocator.GetService<IEventLogger>();
                    if (eventLogger != null)
                    {
                        eventLogger.Log($"[StealthAudioCoordinator] Objective complete sound played: withBonus={withBonus}, volume={volume:F2}");
                    }
                }

                // Raise event for objective completion if events are configured
                if (withBonus && stealthModeActivatedEvent != null)
                {
                    // Could trigger a special event for bonus objectives
                    // This is optional and can be customized based on game requirements
                }
            }
            catch (System.Exception ex)
            {
                var eventLogger = ServiceLocator.GetService<IEventLogger>();
                if (eventLogger != null)
                {
                    eventLogger.LogError($"[StealthAudioCoordinator] Failed to play objective complete sound: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// プレイヤーが隠蔽状態に入った時の音響調整
        /// </summary>
        /// <param name="concealmentLevel">隠蔽レベル (0.0 - 1.0)</param>
        public void OnPlayerConcealed(float concealmentLevel)
        {
            if (!IsInitialized)
            {
                var eventLogger = ServiceLocator.GetService<IEventLogger>();
                if (eventLogger != null)
                {
                    eventLogger.LogWarning("[StealthAudioCoordinator] System not initialized for OnPlayerConcealed");
                }
                return;
            }

            // 隠蔽レベルに応じたスチE��ス音響調整
            AdjustStealthAudio(concealmentLevel);

            // マスキング効果�E適用
            ApplyAudioMasking(concealmentLevel * 0.8f);

            // スチE��スモード�E強制有効匁E            SetOverrideStealthMode(true);

            if (FeatureFlags.EnableDebugLogging)
            {
                var eventLogger = ServiceLocator.GetService<IEventLogger>();
                if (eventLogger != null)
                {
                    eventLogger.Log($"[StealthAudioCoordinator] Player concealed with level: {concealmentLevel:F2}");
                }
            }
        }

        /// <summary>
        /// プレイヤーが露出状態になった時の音響調整
        /// </summary>
        public void OnPlayerExposed()
        {
            if (!IsInitialized)
            {
                var eventLogger = ServiceLocator.GetService<IEventLogger>();
                if (eventLogger != null)
                {
                    eventLogger.LogWarning("[StealthAudioCoordinator] System not initialized for OnPlayerExposed");
                }
                return;
            }

            // スチE��ス音響調整を解除
            AdjustStealthAudio(0f);

            // マスキング効果を解除
            ApplyAudioMasking(0f);

            // スチE��スモードオーバ�Eライドを解除
            ClearStealthModeOverride();

            if (FeatureFlags.EnableDebugLogging)
            {
                var eventLogger = ServiceLocator.GetService<IEventLogger>();
                if (eventLogger != null)
                {
                    eventLogger.Log("[StealthAudioCoordinator] Player exposed, audio adjustments cleared");
                }
            }
        }

        #endregion

        #endregion
    }
}

