using UnityEngine;
using System.Collections.Generic;
using asterivo.Unity60.Core.Audio.Data;
using asterivo.Unity60.Core.Events;
using asterivo.Unity60.Core.Debug;
using Sirenix.OdinInspector;

namespace asterivo.Unity60.Core.Audio
{
    /// <summary>
    /// ステルスゲームプレイと一般オーディオの協調制御
    /// NPCの聴覚センサーシステムとの連携を管理
    /// </summary>
    public class StealthAudioCoordinator : MonoBehaviour
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

        // システム連携
        private AudioManager audioManager;
        private DynamicAudioEnvironment dynamicEnvironment;
        
        // Singleton パターン
        private static StealthAudioCoordinator instance;
        public static StealthAudioCoordinator Instance => instance;

        #region Unity Lifecycle

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            instance = this;
            DontDestroyOnLoad(gameObject);
            
            InitializeCoordinator();
        }

        private void Start()
        {
            FindSystemReferences();
            InitializeCategoryMultipliers();
        }

        private void Update()
        {
            UpdateNearbyAIDetection();
            UpdateStealthModeState();
            UpdateMaskingEffects();
        }
        
        private void OnDestroy()
        {
            if (instance == this)
            {
                instance = null;
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
        /// </summary>
        private void FindSystemReferences()
        {
            if (audioManager == null)
                audioManager = AudioManager.Instance;

            if (dynamicEnvironment == null)
                dynamicEnvironment = FindFirstObjectByType<DynamicAudioEnvironment>();
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
                    var aiController = collider.GetComponent<IAIStateProvider>();
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
            var playerController = playerTransform.GetComponent<IPlayerStateProvider>();
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
                        var interactable = obj.GetComponent<IInteractable>();
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

    #region Supporting Interfaces

    /// <summary>
    /// AIの状態情報を提供するインターフェース
    /// </summary>
    public interface IAIStateProvider
    {
        float GetAlertLevel();
        bool IsAwareOfPlayer();
    }

    /// <summary>
    /// プレイヤーの状態情報を提供するインターフェース
    /// </summary>
    public interface IPlayerStateProvider
    {
        bool IsInHidingMode();
        bool IsCrouching();
        bool IsPerformingStealthAction();
    }

    /// <summary>
    /// インタラクション可能オブジェクトのインターフェース
    /// </summary>
    public interface IInteractable
    {
        bool IsBeingUsed();
        float GetInteractionProgress();
    }

    #endregion
}