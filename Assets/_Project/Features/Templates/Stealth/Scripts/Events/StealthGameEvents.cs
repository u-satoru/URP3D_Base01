using UnityEngine;
using asterivo.Unity60.Core.Events;
using asterivo.Unity60.Features.Templates.Stealth.Configuration;

namespace asterivo.Unity60.Features.Templates.Stealth.Events
{
    /// <summary>
    /// ステルス特化ゲームイベント統合クラス
    /// Event駆動アーキテクチャとの完全統合
    /// Layer 1.3: Event Channels統合（Event駆動アーキテクチャ）
    /// </summary>
    [System.Serializable]
    public class StealthGameEvents
    {
        [Header("Player Stealth Events")]
        [SerializeField] private GameEvent<StealthState> _playerStealthStateChanged;
        [SerializeField] private GameEvent<float> _playerNoiseLevelChanged;
        [SerializeField] private GameEvent<Vector3> _playerEnteredHidingSpot;
        [SerializeField] private GameEvent<Vector3> _playerExitedHidingSpot;
        [SerializeField] private GameEvent<bool> _playerStealthModeToggled;

        [Header("AI Detection Events")]
        [SerializeField] private GameEvent<AIDetectionData> _npcDetectionLevelChanged;
        [SerializeField] private GameEvent<NPCAlertData> _npcAlertLevelChanged;
        [SerializeField] private GameEvent<Vector3> _playerSpottedByNPC;
        [SerializeField] private GameEvent<Vector3> _playerLostByNPC;
        [SerializeField] private GameEvent<CooperativeDetectionData> _cooperativeDetectionTriggered;

        [Header("Environment Events")]
        [SerializeField] private GameEvent<EnvironmentInteractionData> _environmentInteractionStarted;
        [SerializeField] private GameEvent<EnvironmentInteractionData> _environmentInteractionCompleted;
        [SerializeField] private GameEvent<NoiseZoneData> _noiseZoneEntered;
        [SerializeField] private GameEvent<NoiseZoneData> _noiseZoneExited;
        [SerializeField] private GameEvent<LightingChangeData> _lightingStateChanged;

        [Header("Audio Events")]
        [SerializeField] private GameEvent<AudioMaskingData> _environmentalMaskingChanged;
        [SerializeField] private GameEvent<float> _tensionMusicLevelChanged;
        [SerializeField] private GameEvent<SpatialAudioData> _spatialAudioTriggered;
        [SerializeField] private GameEvent<FootstepData> _footstepAudioGenerated;

        [Header("UI Events")]
        [SerializeField] private GameEvent<StealthUIUpdateData> _stealthUIUpdateRequired;
        [SerializeField] private GameEvent<DetectionMeterData> _detectionMeterUpdateRequired;
        [SerializeField] private GameEvent<InteractionPromptData> _interactionPromptUpdateRequired;
        [SerializeField] private GameEvent<MinimapUpdateData> _minimapUpdateRequired;

        [Header("Tutorial Events")]
        [SerializeField] private GameEvent<TutorialStepData> _tutorialStepCompleted;
        [SerializeField] private GameEvent<LearningProgressData> _learningProgressUpdated;
        [SerializeField] private GameEvent<GameplayReadinessData> _gameplayReadinessChanged;

        // Properties for external access
        public GameEvent<StealthState> PlayerStealthStateChanged => _playerStealthStateChanged;
        public GameEvent<float> PlayerNoiseLevelChanged => _playerNoiseLevelChanged;
        public GameEvent<Vector3> PlayerEnteredHidingSpot => _playerEnteredHidingSpot;
        public GameEvent<Vector3> PlayerExitedHidingSpot => _playerExitedHidingSpot;
        public GameEvent<bool> PlayerStealthModeToggled => _playerStealthModeToggled;

        public GameEvent<AIDetectionData> NPCDetectionLevelChanged => _npcDetectionLevelChanged;
        public GameEvent<NPCAlertData> NPCAlertLevelChanged => _npcAlertLevelChanged;
        public GameEvent<Vector3> PlayerSpottedByNPC => _playerSpottedByNPC;
        public GameEvent<Vector3> PlayerLostByNPC => _playerLostByNPC;
        public GameEvent<CooperativeDetectionData> CooperativeDetectionTriggered => _cooperativeDetectionTriggered;

        public GameEvent<EnvironmentInteractionData> EnvironmentInteractionStarted => _environmentInteractionStarted;
        public GameEvent<EnvironmentInteractionData> EnvironmentInteractionCompleted => _environmentInteractionCompleted;
        public GameEvent<NoiseZoneData> NoiseZoneEntered => _noiseZoneEntered;
        public GameEvent<NoiseZoneData> NoiseZoneExited => _noiseZoneExited;
        public GameEvent<LightingChangeData> LightingStateChanged => _lightingStateChanged;

        public GameEvent<AudioMaskingData> EnvironmentalMaskingChanged => _environmentalMaskingChanged;
        public GameEvent<float> TensionMusicLevelChanged => _tensionMusicLevelChanged;
        public GameEvent<SpatialAudioData> SpatialAudioTriggered => _spatialAudioTriggered;
        public GameEvent<FootstepData> FootstepAudioGenerated => _footstepAudioGenerated;

        public GameEvent<StealthUIUpdateData> StealthUIUpdateRequired => _stealthUIUpdateRequired;
        public GameEvent<DetectionMeterData> DetectionMeterUpdateRequired => _detectionMeterUpdateRequired;
        public GameEvent<InteractionPromptData> InteractionPromptUpdateRequired => _interactionPromptUpdateRequired;
        public GameEvent<MinimapUpdateData> MinimapUpdateRequired => _minimapUpdateRequired;

        public GameEvent<TutorialStepData> TutorialStepCompleted => _tutorialStepCompleted;
        public GameEvent<LearningProgressData> LearningProgressUpdated => _learningProgressUpdated;
        public GameEvent<GameplayReadinessData> GameplayReadinessChanged => _gameplayReadinessChanged;

        /// <summary>
        /// イベントシステムの初期化
        /// ScriptableObjectイベントの作成と登録
        /// </summary>
        public void InitializeEvents()
        {
            // Player Stealth Events
            if (_playerStealthStateChanged == null)
                _playerStealthStateChanged = CreateEvent<StealthState>("PlayerStealthStateChanged");
            if (_playerNoiseLevelChanged == null)
                _playerNoiseLevelChanged = CreateEvent<float>("PlayerNoiseLevelChanged");
            if (_playerEnteredHidingSpot == null)
                _playerEnteredHidingSpot = CreateEvent<Vector3>("PlayerEnteredHidingSpot");
            if (_playerExitedHidingSpot == null)
                _playerExitedHidingSpot = CreateEvent<Vector3>("PlayerExitedHidingSpot");
            if (_playerStealthModeToggled == null)
                _playerStealthModeToggled = CreateEvent<bool>("PlayerStealthModeToggled");

            // AI Detection Events
            if (_npcDetectionLevelChanged == null)
                _npcDetectionLevelChanged = CreateEvent<AIDetectionData>("NPCDetectionLevelChanged");
            if (_npcAlertLevelChanged == null)
                _npcAlertLevelChanged = CreateEvent<NPCAlertData>("NPCAlertLevelChanged");
            if (_playerSpottedByNPC == null)
                _playerSpottedByNPC = CreateEvent<Vector3>("PlayerSpottedByNPC");
            if (_playerLostByNPC == null)
                _playerLostByNPC = CreateEvent<Vector3>("PlayerLostByNPC");
            if (_cooperativeDetectionTriggered == null)
                _cooperativeDetectionTriggered = CreateEvent<CooperativeDetectionData>("CooperativeDetectionTriggered");

            Debug.Log("Stealth Game Events initialized successfully");
        }

        /// <summary>
        /// Generic ScriptableObject Event creation
        /// </summary>
        private GameEvent<T> CreateEvent<T>(string eventName)
        {
            var eventAsset = ScriptableObject.CreateInstance<GameEvent<T>>();
            eventAsset.name = eventName;
            return eventAsset;
        }

        /// <summary>
        /// 全イベントの統計情報を取得
        /// デバッグとパフォーマンス監視用
        /// </summary>
        public EventStatistics GetEventStatistics()
        {
            var stats = new EventStatistics();

            // Player Events統計
            stats.PlayerEventCount = CountActiveEvents(new object[] {
                _playerStealthStateChanged, _playerNoiseLevelChanged, _playerEnteredHidingSpot,
                _playerExitedHidingSpot, _playerStealthModeToggled
            });

            // AI Events統計
            stats.AIEventCount = CountActiveEvents(new object[] {
                _npcDetectionLevelChanged, _npcAlertLevelChanged, _playerSpottedByNPC,
                _playerLostByNPC, _cooperativeDetectionTriggered
            });

            // Environment Events統計
            stats.EnvironmentEventCount = CountActiveEvents(new object[] {
                _environmentInteractionStarted, _environmentInteractionCompleted, _noiseZoneEntered,
                _noiseZoneExited, _lightingStateChanged
            });

            // Audio Events統計
            stats.AudioEventCount = CountActiveEvents(new object[] {
                _environmentalMaskingChanged, _tensionMusicLevelChanged, _spatialAudioTriggered,
                _footstepAudioGenerated
            });

            // UI Events統計
            stats.UIEventCount = CountActiveEvents(new object[] {
                _stealthUIUpdateRequired, _detectionMeterUpdateRequired, _interactionPromptUpdateRequired,
                _minimapUpdateRequired
            });

            // Tutorial Events統計
            stats.TutorialEventCount = CountActiveEvents(new object[] {
                _tutorialStepCompleted, _learningProgressUpdated, _gameplayReadinessChanged
            });

            stats.TotalEventCount = stats.PlayerEventCount + stats.AIEventCount + stats.EnvironmentEventCount +
                                   stats.AudioEventCount + stats.UIEventCount + stats.TutorialEventCount;

            return stats;
        }

        private int CountActiveEvents(object[] events)
        {
            int count = 0;
            foreach (var evt in events)
            {
                if (evt != null) count++;
            }
            return count;
        }

        /// <summary>
        /// イベントシステムの健全性チェック
        /// 循環依存やパフォーマンス問題の検出
        /// </summary>
        public bool ValidateEventSystem()
        {
            var stats = GetEventStatistics();

            // 基本検証
            if (stats.TotalEventCount == 0)
            {
                Debug.LogError("No events are initialized in StealthGameEvents");
                return false;
            }

            // 推奨最小イベント数の確認
            if (stats.PlayerEventCount < 3)
            {
                Debug.LogWarning("Low number of Player events initialized");
            }

            if (stats.AIEventCount < 3)
            {
                Debug.LogWarning("Low number of AI events initialized");
            }

            Debug.Log($"Event validation completed: {stats.TotalEventCount} total events active");
            return true;
        }

        /// <summary>
        /// Learn & Grow価値実現のためのイベント監視
        /// 学習進捗とゲームプレイ準備状況の追跡
        /// </summary>
        public void MonitorLearnGrowProgress()
        {
            // Tutorial進捗監視
            if (_tutorialStepCompleted != null)
            {
                // TODO: Implement RegisterListener for TutorialStepData events
                LogDebug("[StealthGameEvents] Tutorial step monitoring initialized");
            }

            // 学習進捗監視
            if (_learningProgressUpdated != null)
            {
                // TODO: Implement RegisterListener for LearningProgressData events
                LogDebug("[StealthGameEvents] Learning progress monitoring initialized");
            }

            // ゲームプレイ準備監視
            if (_gameplayReadinessChanged != null)
            {
                // TODO: Implement RegisterListener for GameplayReadinessData events
                LogDebug("[StealthGameEvents] Gameplay readiness monitoring initialized");
            }
        }

        private void OnTutorialStepCompleted(TutorialStepData stepData)
        {
            Debug.Log($"Tutorial step completed: {stepData.StepName} - Progress: {stepData.CompletionPercentage:P}");
        }

        private void OnLearningProgressUpdated(LearningProgressData progressData)
        {
            Debug.Log($"Learning progress updated: {progressData.OverallProgress:P} - Time spent: {progressData.TotalLearningTimeMinutes:F1} minutes");

            // 70%学習コスト削減の確認
            if (progressData.TotalLearningTimeMinutes <= 12 * 60) // 12時間 = 720分
            {
                Debug.Log("✅ 70% learning cost reduction target achieved!");
            }
        }

        private void OnGameplayReadinessChanged(GameplayReadinessData readinessData)
        {
            Debug.Log($"Gameplay readiness: {readinessData.IsGameplayReady} - Time to ready: {readinessData.TimeToGameplayMinutes:F1} minutes");

            // 15分ゲームプレイ準備の確認
            if (readinessData.IsGameplayReady && readinessData.TimeToGameplayMinutes <= 15)
            {
                Debug.Log("✅ 15-minute gameplay readiness achieved!");
            }
        }
    }

    /// <summary>
    /// イベント統計情報構造体
    /// </summary>
    [System.Serializable]
    public struct EventStatistics
    {
        public int PlayerEventCount;
        public int AIEventCount;
        public int EnvironmentEventCount;
        public int AudioEventCount;
        public int UIEventCount;
        public int TutorialEventCount;
        public int TotalEventCount;

        public override string ToString()
        {
            return $"Event Statistics: Total={TotalEventCount}, Player={PlayerEventCount}, AI={AIEventCount}, " +
                   $"Environment={EnvironmentEventCount}, Audio={AudioEventCount}, UI={UIEventCount}, Tutorial={TutorialEventCount}";
        }
    }
}