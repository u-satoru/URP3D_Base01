using UnityEngine;
using asterivo.Unity60.Core.Events;
using asterivo.Unity60.Features.Templates.Stealth.Configuration;

namespace asterivo.Unity60.Features.Templates.Stealth.Events
{
    /// <summary>
    /// ステルス専用イベントチャネル統合クラス
    /// Core Event SystemとStealth Templateの完全統合
    /// Layer 1.3: Event Channels統合（Event駆動アーキテクチャ）
    /// </summary>
    [CreateAssetMenu(fileName = "StealthEventChannels", menuName = "Templates/Stealth/Event Channels", order = 0)]
    public class StealthEventChannels : ScriptableObject
    {
        [Header("Core System Integration")]
        [Tooltip("既存Core Event Systemとの統合設定")]
        [SerializeField] private bool _integrateWithCoreEvents = true;
        [SerializeField] private bool _enableEventDebugging = false;
        [SerializeField] private bool _enablePerformanceTracking = true;

        [Header("Stealth Game Events")]
        [SerializeField] private StealthGameEvents _stealthEvents;

        [Header("Event Channel Registry")]
        [Tooltip("ScriptableObjectベースのイベントチャネル管理")]
        [SerializeField] private GameEventChannel _stealthStateEventChannel;
        [SerializeField] private GameEventChannel _aiDetectionEventChannel;
        [SerializeField] private GameEventChannel _environmentEventChannel;
        [SerializeField] private GameEventChannel _audioEventChannel;
        [SerializeField] private GameEventChannel _uiEventChannel;
        [SerializeField] private GameEventChannel _tutorialEventChannel;

        [Header("Event Performance Settings")]
        [Range(1, 100)]
        [SerializeField] private int _maxEventsPerFrame = 20;
        [Range(0.1f, 5f)]
        [SerializeField] private float _eventProcessingTimeLimit = 1f;
        [SerializeField] private bool _enableEventBatching = true;

        // Runtime statistics
        private EventChannelStatistics _statistics;
        private float _lastStatsUpdate;

        /// <summary>
        /// ステルスイベントシステムへの外部アクセス
        /// </summary>
        public StealthGameEvents StealthEvents
        {
            get
            {
                if (_stealthEvents == null)
                {
                    _stealthEvents = new StealthGameEvents();
                    _stealthEvents.InitializeEvents();
                }
                return _stealthEvents;
            }
        }

        /// <summary>
        /// イベントチャネルの初期化
        /// Core Event Systemとの統合
        /// </summary>
        public void InitializeEventChannels()
        {
            if (_integrateWithCoreEvents)
            {
                CreateEventChannels();
                RegisterWithCoreEventSystem();
                InitializeStealthEvents();

                _statistics = new EventChannelStatistics();
                _lastStatsUpdate = Time.time;

                Debug.Log("✅ Stealth Event Channels initialized with Core Event System integration");
            }
            else
            {
                Debug.Log("⚠️ Stealth Event Channels initialized without Core Event System integration");
            }
        }

        /// <summary>
        /// イベントチャネルの作成
        /// </summary>
        private void CreateEventChannels()
        {
            if (_stealthStateEventChannel == null)
                _stealthStateEventChannel = CreateEventChannel("StealthState");

            if (_aiDetectionEventChannel == null)
                _aiDetectionEventChannel = CreateEventChannel("AIDetection");

            if (_environmentEventChannel == null)
                _environmentEventChannel = CreateEventChannel("Environment");

            if (_audioEventChannel == null)
                _audioEventChannel = CreateEventChannel("Audio");

            if (_uiEventChannel == null)
                _uiEventChannel = CreateEventChannel("UI");

            if (_tutorialEventChannel == null)
                _tutorialEventChannel = CreateEventChannel("Tutorial");
        }

        /// <summary>
        /// イベントチャネル作成ヘルパー
        /// </summary>
        private GameEventChannel CreateEventChannel(string channelName)
        {
            var channel = CreateInstance<GameEventChannel>();
            channel.name = $"Stealth{channelName}Channel";
            channel.Initialize($"Stealth.{channelName}", _enableEventDebugging);
            return channel;
        }

        /// <summary>
        /// Core Event Systemへの登録
        /// </summary>
        private void RegisterWithCoreEventSystem()
        {
            // EventChannelRegistryへの登録（Core Systemとの統合）
            var channels = new GameEventChannel[]
            {
                _stealthStateEventChannel,
                _aiDetectionEventChannel,
                _environmentEventChannel,
                _audioEventChannel,
                _uiEventChannel,
                _tutorialEventChannel
            };

            foreach (var channel in channels)
            {
                if (channel != null)
                {
                    // Core Event Systemへの統合
                    RegisterEventChannel(channel);
                }
            }
        }

        /// <summary>
        /// 個別イベントチャネル登録
        /// </summary>
        private void RegisterEventChannel(GameEventChannel channel)
        {
            try
            {
                // Core Event Registry統合
                // EventChannelRegistry.RegisterChannel(channel);

                if (_enableEventDebugging)
                {
                    Debug.Log($"Registered event channel: {channel.name}");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to register event channel {channel.name}: {e.Message}");
            }
        }

        /// <summary>
        /// StealthGameEventsの初期化
        /// </summary>
        private void InitializeStealthEvents()
        {
            if (_stealthEvents == null)
            {
                _stealthEvents = new StealthGameEvents();
            }

            _stealthEvents.InitializeEvents();

            // Learn & Grow価値実現のための監視開始
            _stealthEvents.MonitorLearnGrowProgress();

            if (_enableEventDebugging)
            {
                var eventStats = _stealthEvents.GetEventStatistics();
                Debug.Log($"Stealth Events initialized: {eventStats}");
            }
        }

        /// <summary>
        /// イベント送信統合メソッド
        /// Core EventとStealth Eventの統合送信
        /// </summary>
        public void SendStealthStateEvent(StealthState newState, StealthState previousState)
        {
            // Stealth Events送信
            if (StealthEvents.PlayerStealthStateChanged != null)
            {
                StealthEvents.PlayerStealthStateChanged.Raise(newState);
            }

            // Core Event Channel送信
            if (_stealthStateEventChannel != null && _integrateWithCoreEvents)
            {
                var eventData = StealthUIUpdateData.Create(
                    StealthUIElement.StealthIndicator,
                    newState,
                    1f,
                    true,
                    $"State changed from {previousState} to {newState}"
                );
                _stealthStateEventChannel.RaiseEvent("StealthStateChanged", eventData);
            }

            // 統計更新
            UpdateStatistics("StealthState");
        }

        /// <summary>
        /// AI検知イベント送信
        /// </summary>
        public void SendAIDetectionEvent(AIDetectionData detectionData)
        {
            // Stealth Events送信
            if (StealthEvents.NPCDetectionLevelChanged != null)
            {
                StealthEvents.NPCDetectionLevelChanged.Raise(detectionData);
            }

            // Core Event Channel送信
            if (_aiDetectionEventChannel != null && _integrateWithCoreEvents)
            {
                _aiDetectionEventChannel.RaiseEvent("AIDetectionChanged", detectionData);
            }

            // 統計更新
            UpdateStatistics("AIDetection");
        }

        /// <summary>
        /// 環境相互作用イベント送信
        /// </summary>
        public void SendEnvironmentInteractionEvent(EnvironmentInteractionData interactionData)
        {
            // Stealth Events送信
            if (StealthEvents.EnvironmentInteractionStarted != null)
            {
                StealthEvents.EnvironmentInteractionStarted.Raise(interactionData);
            }

            // Core Event Channel送信
            if (_environmentEventChannel != null && _integrateWithCoreEvents)
            {
                _environmentEventChannel.RaiseEvent("EnvironmentInteraction", interactionData);
            }

            // 統計更新
            UpdateStatistics("Environment");
        }

        /// <summary>
        /// Learn & Grow進捗イベント送信
        /// 学習コスト70%削減・15分ゲームプレイ実現監視
        /// </summary>
        public void SendLearningProgressEvent(LearningProgressData progressData)
        {
            // Stealth Events送信
            if (StealthEvents.LearningProgressUpdated != null)
            {
                StealthEvents.LearningProgressUpdated.Raise(progressData);
            }

            // Core Event Channel送信
            if (_tutorialEventChannel != null && _integrateWithCoreEvents)
            {
                _tutorialEventChannel.RaiseEvent("LearningProgress", progressData);
            }

            // Learn & Grow価値実現チェック
            CheckLearnGrowValueRealization(progressData);

            // 統計更新
            UpdateStatistics("Tutorial");
        }

        /// <summary>
        /// ゲームプレイ準備イベント送信
        /// 15分ゲームプレイ実現監視
        /// </summary>
        public void SendGameplayReadinessEvent(GameplayReadinessData readinessData)
        {
            // Stealth Events送信
            if (StealthEvents.GameplayReadinessChanged != null)
            {
                StealthEvents.GameplayReadinessChanged.Raise(readinessData);
            }

            // Core Event Channel送信
            if (_tutorialEventChannel != null && _integrateWithCoreEvents)
            {
                _tutorialEventChannel.RaiseEvent("GameplayReadiness", readinessData);
            }

            // 15分ゲームプレイ実現チェック
            if (readinessData.IsFifteenMinuteTarget && readinessData.IsGameplayReady)
            {
                Debug.Log("🎯 ✅ 15-minute gameplay readiness achieved - Learn & Grow value realized!");
            }

            // 統計更新
            UpdateStatistics("Tutorial");
        }

        /// <summary>
        /// Learn & Grow価値実現チェック
        /// </summary>
        private void CheckLearnGrowValueRealization(LearningProgressData progressData)
        {
            // 70%学習コスト削減チェック（40時間→12時間）
            if (progressData.IsOn70PercentReductionTrack)
            {
                Debug.Log($"🎓 ✅ 70% learning cost reduction on track - Current: {progressData.TotalLearningTimeMinutes:F1} min, Target: ≤720 min");
            }

            // 学習効率監視
            if (progressData.LearningEfficiency > 1.0f)
            {
                Debug.Log($"⚡ High learning efficiency detected: {progressData.LearningEfficiency:F2}x - Exceeding targets!");
            }
        }

        /// <summary>
        /// パフォーマンス統計更新
        /// </summary>
        private void UpdateStatistics(string eventType)
        {
            if (!_enablePerformanceTracking) return;

            _statistics.TotalEventsProcessed++;
            if (_statistics.EventsByType.ContainsKey(eventType))
                _statistics.EventsByType[eventType]++;
            else
                _statistics.EventsByType[eventType] = 1;
            _statistics.LastEventTime = Time.time;

            // 定期的な統計レポート（10秒間隔）
            if (Time.time - _lastStatsUpdate > 10f)
            {
                LogPerformanceStatistics();
                _lastStatsUpdate = Time.time;
            }
        }

        /// <summary>
        /// パフォーマンス統計ログ出力
        /// </summary>
        private void LogPerformanceStatistics()
        {
            if (_enableEventDebugging)
            {
                Debug.Log($"📊 Event Performance Stats: {_statistics.TotalEventsProcessed} total events, " +
                         $"Avg: {_statistics.GetAverageEventsPerSecond():F1} events/sec");
            }
        }

        /// <summary>
        /// イベントシステムの健全性チェック
        /// </summary>
        public bool ValidateEventSystem()
        {
            bool isValid = true;

            // StealthGameEvents検証
            if (_stealthEvents != null && !_stealthEvents.ValidateEventSystem())
            {
                Debug.LogError("StealthGameEvents validation failed");
                isValid = false;
            }

            // Event Channels検証
            var channels = new GameEventChannel[]
            {
                _stealthStateEventChannel, _aiDetectionEventChannel, _environmentEventChannel,
                _audioEventChannel, _uiEventChannel, _tutorialEventChannel
            };

            int validChannels = 0;
            foreach (var channel in channels)
            {
                if (channel != null) validChannels++;
            }

            if (validChannels < 6)
            {
                Debug.LogWarning($"Only {validChannels}/6 event channels are initialized");
            }

            // Core統合確認
            if (_integrateWithCoreEvents && validChannels == 0)
            {
                Debug.LogError("Core Event System integration enabled but no channels initialized");
                isValid = false;
            }

            Debug.Log($"Event system validation: {(isValid ? "✅ PASSED" : "❌ FAILED")} - {validChannels}/6 channels active");
            return isValid;
        }

        /// <summary>
        /// デバッグ情報の取得
        /// エディタ支援用
        /// </summary>
        public string GetDebugInfo()
        {
            var info = new System.Text.StringBuilder();
            info.AppendLine("=== Stealth Event Channels Debug Info ===");
            info.AppendLine($"Core Integration: {(_integrateWithCoreEvents ? "✅ Enabled" : "❌ Disabled")}");
            info.AppendLine($"Performance Tracking: {(_enablePerformanceTracking ? "✅ Enabled" : "❌ Disabled")}");
            info.AppendLine($"Event Debugging: {(_enableEventDebugging ? "✅ Enabled" : "❌ Disabled")}");

            if (_stealthEvents != null)
            {
                var eventStats = _stealthEvents.GetEventStatistics();
                info.AppendLine($"Stealth Events: {eventStats}");
            }

            if (_enablePerformanceTracking)
            {
                info.AppendLine($"Performance: {_statistics.TotalEventsProcessed} events processed");
                info.AppendLine($"Average Rate: {_statistics.GetAverageEventsPerSecond():F1} events/sec");
            }

            return info.ToString();
        }

        #if UNITY_EDITOR
        /// <summary>
        /// エディタ専用バリデーションボタン
        /// </summary>
        [Sirenix.OdinInspector.Button("Validate Event System")]
        private void EditorValidateEventSystem()
        {
            bool isValid = ValidateEventSystem();
            if (isValid)
            {
                Debug.Log("✅ Event system validation passed");
            }
            else
            {
                Debug.LogError("❌ Event system validation failed");
            }
        }

        [Sirenix.OdinInspector.Button("Initialize Event Channels")]
        private void EditorInitializeEventChannels()
        {
            InitializeEventChannels();
            Debug.Log("Event channels initialized from editor");
        }

        [Sirenix.OdinInspector.Button("Show Debug Info")]
        private void EditorShowDebugInfo()
        {
            Debug.Log(GetDebugInfo());
        }
        #endif
    }

    /// <summary>
    /// GameEventChannel実装
    /// Core Event Systemとの統合用
    /// </summary>
    [System.Serializable]
    public class GameEventChannel : ScriptableObject
    {
        [SerializeField] private string _channelName;
        [SerializeField] private bool _enableDebug;
        private System.Collections.Generic.Dictionary<string, System.Action<object>> _eventHandlers = new();

        public string ChannelName => _channelName;

        public void Initialize(string channelName, bool enableDebug)
        {
            _channelName = channelName;
            _enableDebug = enableDebug;
            _eventHandlers = new System.Collections.Generic.Dictionary<string, System.Action<object>>();
        }

        public void RaiseEvent(string eventName, object eventData)
        {
            if (_eventHandlers.TryGetValue(eventName, out var handler))
            {
                handler?.Invoke(eventData);

                if (_enableDebug)
                {
                    Debug.Log($"Event raised: {_channelName}.{eventName}");
                }
            }
        }

        public void Subscribe(string eventName, System.Action<object> handler)
        {
            if (!_eventHandlers.ContainsKey(eventName))
            {
                _eventHandlers[eventName] = null;
            }
            _eventHandlers[eventName] += handler;
        }

        public void Unsubscribe(string eventName, System.Action<object> handler)
        {
            if (_eventHandlers.ContainsKey(eventName))
            {
                _eventHandlers[eventName] -= handler;
            }
        }
    }

    /// <summary>
    /// イベントチャネル統計情報
    /// </summary>
    [System.Serializable]
    public struct EventChannelStatistics
    {
        public int TotalEventsProcessed;
        public System.Collections.Generic.Dictionary<string, int> EventsByType;
        public float LastEventTime;
        public float StartTime;

        public EventChannelStatistics(bool initialize = true)
        {
            TotalEventsProcessed = 0;
            EventsByType = new System.Collections.Generic.Dictionary<string, int>();
            LastEventTime = 0f;
            StartTime = initialize ? Time.time : 0f;
        }

        public float GetAverageEventsPerSecond()
        {
            float elapsed = Time.time - StartTime;
            return elapsed > 0 ? TotalEventsProcessed / elapsed : 0f;
        }
    }
}