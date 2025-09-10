using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using asterivo.Unity60.Core.Audio.Controllers;
using asterivo.Unity60.Core.Audio.Events;
using asterivo.Unity60.Core.Events;
using asterivo.Unity60.Core.Debug;
using asterivo.Unity60.Core.Shared;
using asterivo.Unity60.Core.Audio.Interfaces;
using asterivo.Unity60.Core.Audio.Services;
using asterivo.Unity60.Core.Helpers;
using _Project.Core;
using Sirenix.OdinInspector;

namespace asterivo.Unity60.Core.Audio
{
    /// <summary>
    /// オーディオシステム全体の統一更新コーディネーター（ServiceLocator対応）
    /// リアルタイム同期の最適化とパフォーマンス向上を担当
    /// Service Locatorパターンを使用して他のサービスと連携
    /// </summary>
    public class AudioUpdateCoordinator : MonoBehaviour, IAudioUpdateService, _Project.Core.IInitializable
    {
        [Header("Update Settings")]
        [SerializeField, Range(0.05f, 1f)] private float updateInterval = AudioConstants.AUDIO_UPDATE_INTERVAL;
        [SerializeField] private bool enableCoordinatedUpdates = true;
        // TODO: バッチ処理での一度に更新するAudioSourceの最大数制限（パフォーマンス最適化用）
#pragma warning disable CS0414 // Field assigned but never used - planned for performance batch processing
        [SerializeField, Range(1, 10)] private int maxAudioSourcesPerUpdate = 5;
#pragma warning restore CS0414

        [Header("Spatial Optimization")]
        [SerializeField] private LayerMask audioSourceLayerMask = -1;
        [SerializeField, Range(5f, 50f)] private float maxAudioDetectionRange = 25f;
        [SerializeField] private int spatialGridSize = 10;

        [Header("Performance Monitoring")]
        [SerializeField, ReadOnly] private float currentUpdateTime;
        [SerializeField, ReadOnly] private int totalManagedAudioSources;
        [SerializeField, ReadOnly] private int activeAudioSources;

        // システム参照
        private WeatherAmbientController weatherController;
        private TimeAmbientController timeController;
        private MaskingEffectController maskingController;
        private StealthAudioCoordinator stealthCoordinator;
        private Transform playerTransform;

        // 最適化用キャッシュ
        private Dictionary<Vector3Int, List<AudioSource>> spatialAudioCache;
        private HashSet<AudioSource> trackedAudioSources;
        private Queue<AudioSource> updateQueue;
        private Coroutine coordinatedUpdateCoroutine;
        
        // IAudioUpdatable管理
        private HashSet<IAudioUpdatable> registeredUpdatables;

        // 削除: 同期イベントはインターフェースで定義済み

        // IInitializable実装
        public int Priority => 15; // オーディオ更新コーディネーターは基本サービスの後に初期化
        public bool IsInitialized { get; private set; }

        
        
        // IAudioUpdateService interface properties
        public float UpdateInterval 
        { 
            get => updateInterval; 
            set => SetUpdateInterval(value); 
        }
        
        public bool IsCoordinatedUpdateEnabled => enableCoordinatedUpdates;
        
        // イベント
        public event System.Action<AudioSystemSyncData> OnAudioSystemSync;

        #region Unity Lifecycle

        private void Awake()
        {
            // ✅ ServiceLocator専用実装のみ - Singletonパターン完全削除
            DontDestroyOnLoad(gameObject);
            
            if (FeatureFlags.UseServiceLocator)
            {
                ServiceLocator.RegisterService<IAudioUpdateService>(this);
                
                if (FeatureFlags.EnableDebugLogging)
                {
                    EventLogger.Log("[AudioUpdateCoordinator] Registered to ServiceLocator as IAudioUpdateService");
                }
            }
            
            InitializeCoordinator();
        }

        private void Start()
        {
            Initialize();
        }

        private void OnDestroy()
        {
            // ✅ ServiceLocator専用実装のみ - Singletonパターン完全削除
            if (FeatureFlags.UseServiceLocator)
            {
                ServiceLocator.UnregisterService<IAudioUpdateService>();
                
                if (FeatureFlags.EnableDebugLogging)
                {
                    EventLogger.Log("[AudioUpdateCoordinator] Unregistered from ServiceLocator");
                }
            }
            
            StopCoordinatedUpdates();
        }

        #endregion

        #region IInitializable Implementation

        public void Initialize()
        {
            if (IsInitialized) return;

            FindSystemReferences();
            InitializeSpatialCache();
            
            if (enableCoordinatedUpdates)
            {
                StartCoordinatedUpdates();
            }

            IsInitialized = true;
            
            if (FeatureFlags.EnableDebugLogging)
            {
                EventLogger.Log("[AudioUpdateCoordinator] Initialization complete");
            }
        }

        #endregion

        #region Initialization

        /// <summary>
        /// コーディネーターの初期化
        /// </summary>
        private void InitializeCoordinator()
        {
            spatialAudioCache = new Dictionary<Vector3Int, List<AudioSource>>();
            trackedAudioSources = new HashSet<AudioSource>();
            updateQueue = new Queue<AudioSource>();
            registeredUpdatables = new HashSet<IAudioUpdatable>();
            
            EventLogger.Log("<color=green>[AudioUpdateCoordinator]</color> Audio update coordinator initialized");
        }

        /// <summary>
        /// システム参照の検索
        /// </summary>
        private void FindSystemReferences()
        {
            weatherController = ServiceHelper.GetServiceWithFallback<WeatherAmbientController>();
            timeController = ServiceHelper.GetServiceWithFallback<TimeAmbientController>();
            maskingController = ServiceHelper.GetServiceWithFallback<MaskingEffectController>();
            stealthCoordinator = ServiceHelper.GetServiceWithFallback<StealthAudioCoordinator>();

            // プレイヤーTransformの検索
                        var audioListener = ServiceHelper.GetServiceWithFallback<AudioListener>();;
            if (audioListener != null)
            {
                playerTransform = audioListener.transform;
            }

            EventLogger.Log($"<color=green>[AudioUpdateCoordinator]</color> Found {GetFoundSystemsCount()} audio systems");
        }

        /// <summary>
        /// 空間キャッシュの初期化
        /// </summary>
        private void InitializeSpatialCache()
        {
            spatialAudioCache.Clear();
            RebuildSpatialCache();
        }

        #endregion

        #region IAudioUpdateService Implementation

        /// <summary>
        /// 更新可能なコンポーネントを登録
        /// </summary>
        public void RegisterUpdatable(IAudioUpdatable updatable)
        {
            if (updatable != null)
            {
                registeredUpdatables.Add(updatable);
                
                if (FeatureFlags.EnableDebugLogging)
                {
                    EventLogger.Log($"[AudioUpdateCoordinator] Registered updatable: {updatable.GetType().Name}");
                }
            }
        }

        /// <summary>
        /// 更新可能なコンポーネントの登録解除
        /// </summary>
        public void UnregisterUpdatable(IAudioUpdatable updatable)
        {
            if (updatable != null)
            {
                registeredUpdatables.Remove(updatable);
                
                if (FeatureFlags.EnableDebugLogging)
                {
                    EventLogger.Log($"[AudioUpdateCoordinator] Unregistered updatable: {updatable.GetType().Name}");
                }
            }
        }

        #endregion

        #region Coordinated Update System

        /// <summary>
        /// 協調更新の開始
        /// </summary>
        public void StartCoordinatedUpdates()
        {
            if (coordinatedUpdateCoroutine == null)
            {
                coordinatedUpdateCoroutine = StartCoroutine(CoordinatedUpdateLoop());
                EventLogger.Log("<color=green>[AudioUpdateCoordinator]</color> Coordinated updates started");
            }
        }

        /// <summary>
        /// 協調更新の停止
        /// </summary>
        public void StopCoordinatedUpdates()
        {
            if (coordinatedUpdateCoroutine != null)
            {
                StopCoroutine(coordinatedUpdateCoroutine);
                coordinatedUpdateCoroutine = null;
                EventLogger.Log("<color=green>[AudioUpdateCoordinator]</color> Coordinated updates stopped");
            }
        }

        /// <summary>
        /// メインの協調更新ループ
        /// </summary>
        private IEnumerator CoordinatedUpdateLoop()
        {
            while (enableCoordinatedUpdates)
            {
                float updateStartTime = Time.realtimeSinceStartup;

                // 空間キャッシュの定期的な再構築
                if (Time.frameCount % (int)(60 * updateInterval * 5) == 0) // 約5秒間隔
                {
                    RebuildSpatialCache();
                }

                // 同期データの作成
                var syncData = CreateAudioSystemSyncData();

                // 全システムの協調更新
                UpdateAllAudioSystems(syncData);

                // 同期イベントの発火
                OnAudioSystemSync?.Invoke(syncData);
                
                // 登録されたIAudioUpdatableの更新
                UpdateRegisteredUpdatables(syncData);

                // パフォーマンス計測
                currentUpdateTime = (Time.realtimeSinceStartup - updateStartTime) * 1000f; // ms

                yield return new WaitForSeconds(updateInterval);
            }
        }

        /// <summary>
        /// 全オーディオシステムの更新
        /// </summary>
        private void UpdateAllAudioSystems(AudioSystemSyncData syncData)
        {
            // Weather Controller の最適化更新
            if (weatherController != null)
            {
                UpdateWeatherControllerOptimized(syncData);
            }

            // Time Controller の最適化更新
            if (timeController != null)
            {
                UpdateTimeControllerOptimized(syncData);
            }

            // Masking Controller の最適化更新
            if (maskingController != null)
            {
                UpdateMaskingControllerOptimized(syncData);
            }

            // Stealth Coordinator への同期通知
            if (stealthCoordinator != null)
            {
                NotifyStealthCoordinator(syncData);
            }
        }
        
        /// <summary>
        /// 登録されたIAudioUpdatableの更新
        /// </summary>
        private void UpdateRegisteredUpdatables(AudioSystemSyncData syncData)
        {
            foreach (var updatable in registeredUpdatables)
            {
                if (updatable != null)
                {
                    try
                    {
                        updatable.UpdateAudio(syncData.deltaTime);;
                    }
                    catch (System.Exception ex)
                    {
                        EventLogger.LogError($"[AudioUpdateCoordinator] Error updating {updatable.GetType().Name}: {ex.Message}");
                    }
                }
            }
        }

        #endregion

        #region Optimized System Updates

        /// <summary>
        /// WeatherControllerの最適化更新
        /// </summary>
        private void UpdateWeatherControllerOptimized(AudioSystemSyncData syncData)
        {
            // 天気変更が必要な場合のみ処理
            if (syncData.weatherChanged)
            {
                // 非同期での天気変更（既存メソッドを活用）
                // WeatherControllerは既に効率的な実装のため、そのまま使用
            }
        }

        /// <summary>
        /// TimeControllerの最適化更新
        /// </summary>
        private void UpdateTimeControllerOptimized(AudioSystemSyncData syncData)
        {
            // 時間変更が必要な場合のみ処理
            if (syncData.timeChanged)
            {
                timeController.ChangeTimeOfDay(syncData.currentTimeOfDay);
            }

            // 音量調整（ステルス状態に応じて）
            if (syncData.stealthStateChanged)
            {
                float volumeMultiplier = syncData.isStealthActive ? 0.6f : 1f;
                timeController.SetMasterVolume(syncData.ambientVolume * volumeMultiplier);
            }
        }

        /// <summary>
        /// MaskingControllerの最適化更新
        /// </summary>
        private void UpdateMaskingControllerOptimized(AudioSystemSyncData syncData)
        {
            if (syncData.nearbyAudioSources.Count > 0)
            {
                // バッチ処理でマスキング効果を適用
                foreach (var audioSource in syncData.nearbyAudioSources)
                {
                    if (audioSource != null && audioSource.isPlaying)
                    {
                        maskingController.ApplyMaskingToAudioSource(audioSource, syncData.currentMaskingStrength);
                    }
                }
            }
        }

        /// <summary>
        /// StealthCoordinatorへの同期通知
        /// </summary>
        private void NotifyStealthCoordinator(AudioSystemSyncData syncData)
        {
            // ステルス状態に変化がある場合のみ通知
            if (syncData.stealthStateChanged)
            {
                // StealthCoordinatorに状態変更を通知
                // 既存のイベントシステムを活用
            }
        }

        #endregion

        #region Spatial Cache System

        /// <summary>
        /// 空間キャッシュの再構築
        /// </summary>
        private void RebuildSpatialCache()
        {
            spatialAudioCache.Clear();
            trackedAudioSources.Clear();

            if (playerTransform == null) return;

            // 効率的なAudioSource検索
            var allAudioSources = FindObjectsByType<AudioSource>(FindObjectsSortMode.None);
            totalManagedAudioSources = allAudioSources.Length;
            activeAudioSources = 0;

            foreach (var audioSource in allAudioSources)
            {
                if (audioSource == null) continue;

                // レイヤーマスクチェック
                if ((audioSourceLayerMask.value & (1 << audioSource.gameObject.layer)) == 0) continue;

                // 距離チェック
                float distance = Vector3.Distance(audioSource.transform.position, playerTransform.position);
                if (distance > maxAudioDetectionRange) continue;

                // 空間グリッドへの登録
                Vector3Int gridKey = WorldToGridKey(audioSource.transform.position);
                if (!spatialAudioCache.ContainsKey(gridKey))
                {
                    spatialAudioCache[gridKey] = new List<AudioSource>();
                }

                spatialAudioCache[gridKey].Add(audioSource);
                trackedAudioSources.Add(audioSource);

                if (audioSource.isPlaying)
                {
                    activeAudioSources++;
                }
            }

            EventLogger.Log($"<color=green>[AudioUpdateCoordinator]</color> Rebuilt spatial cache: {totalManagedAudioSources} total, {activeAudioSources} active");
        }

        /// <summary>
        /// ワールド座標をグリッドキーに変換
        /// </summary>
        private Vector3Int WorldToGridKey(Vector3 worldPosition)
        {
            return new Vector3Int(
                Mathf.FloorToInt(worldPosition.x / spatialGridSize),
                Mathf.FloorToInt(worldPosition.y / spatialGridSize),
                Mathf.FloorToInt(worldPosition.z / spatialGridSize)
            );
        }

        /// <summary>
        /// 近傍AudioSourceの効率的取得
        /// </summary>
        public List<AudioSource> GetNearbyAudioSources(Vector3 center, float radius)
        {
            var result = new List<AudioSource>();
            Vector3Int centerGrid = WorldToGridKey(center);

            // グリッド範囲の計算
            int gridRadius = Mathf.CeilToInt(radius / spatialGridSize);

            for (int x = -gridRadius; x <= gridRadius; x++)
            {
                for (int y = -gridRadius; y <= gridRadius; y++)
                {
                    for (int z = -gridRadius; z <= gridRadius; z++)
                    {
                        Vector3Int gridKey = centerGrid + new Vector3Int(x, y, z);
                        
                        if (spatialAudioCache.TryGetValue(gridKey, out var audioSources))
                        {
                            foreach (var audioSource in audioSources)
                            {
                                if (audioSource != null && 
                                    Vector3.Distance(audioSource.transform.position, center) <= radius)
                                {
                                    result.Add(audioSource);
                                }
                            }
                        }
                    }
                }
            }

            return result;
        }

        #endregion

        #region Sync Data Creation

        /// <summary>
        /// オーディオシステム同期データの作成
        /// </summary>
        private AudioSystemSyncData CreateAudioSystemSyncData()
        {
            var syncData = new AudioSystemSyncData();

            // 基本情報
            syncData.deltaTime = Time.deltaTime;
            syncData.currentTime = Time.time;
            syncData.playerPosition = playerTransform?.position ?? Vector3.zero;

            // ステルス状態
            bool previousStealthState = syncData.isStealthActive;
            syncData.isStealthActive = stealthCoordinator != null && stealthCoordinator.ShouldReduceNonStealthAudio();
            syncData.stealthStateChanged = previousStealthState != syncData.isStealthActive;

            // 時間情報
            var currentSystemTime = System.DateTime.Now;
            var newTimeOfDay = DetermineTimeOfDayFromSystemTime(currentSystemTime);
            syncData.timeChanged = syncData.currentTimeOfDay != newTimeOfDay;
            syncData.currentTimeOfDay = newTimeOfDay;

            // 天気情報（DynamicAudioEnvironmentから取得）
            var dynamicEnvironment = ServiceHelper.GetServiceWithFallback<DynamicAudioEnvironment>();
            if (dynamicEnvironment != null)
            {
                var (env, weather, time) = dynamicEnvironment.GetCurrentState();
                syncData.weatherChanged = syncData.currentWeatherType != weather;
                syncData.currentWeatherType = weather;
                syncData.currentEnvironmentType = env;
            }

            // 音量設定: ServiceLocator優先、Singletonフォールバック
            var audioService = GetAudioService();
            if (audioService != null)
            {
                try
                {
                    syncData.masterVolume = audioService.GetMasterVolume();
                    syncData.bgmVolume = audioService.GetBGMVolume();
                    syncData.ambientVolume = audioService.GetAmbientVolume();
                    syncData.effectVolume = audioService.GetEffectVolume();
                }
                catch (System.Exception ex)
                {
                    EventLogger.LogError($"[AudioUpdateCoordinator] Failed to get audio state from service: {ex.Message}");
                }
            }
            else
            {
                // フォールバック: FindFirstObjectByType (ServiceLocator専用実装)
                // ✅ ServiceLocator専用実装 - 直接AudioManagerを検索
                var audioManager = ServiceHelper.GetServiceWithFallback<AudioManager>();
                if (audioManager != null)
                {
                    try
                    {
                        var audioState = audioManager.GetCurrentAudioState();
                        syncData.masterVolume = audioState.masterVolume;
                        syncData.bgmVolume = audioState.bgmVolume;
                        syncData.ambientVolume = audioState.ambientVolume;
                        syncData.effectVolume = audioState.effectVolume;
                        
                        if (FeatureFlags.EnableDebugLogging)
                        {
                            EventLogger.Log("[AudioUpdateCoordinator] Found AudioManager via FindFirstObjectByType");
                        }
                    }
                    catch (System.Exception ex)
                    {
                        EventLogger.LogError($"[AudioUpdateCoordinator] Failed to get audio state from AudioManager: {ex.Message}");
                    }
                }
            }

            // 近傍AudioSource（空間キャッシュを活用）
            if (playerTransform != null)
            {
                syncData.nearbyAudioSources = GetNearbyAudioSources(playerTransform.position, maxAudioDetectionRange);
                
                // マスキング強度の計算
                syncData.currentMaskingStrength = CalculateCurrentMaskingStrength(syncData.nearbyAudioSources);
            }

            return syncData;
        }

        /// <summary>
        /// システム時刻から時間帯を判定
        /// </summary>
        private TimeOfDay DetermineTimeOfDayFromSystemTime(System.DateTime time)
        {
            int hour = time.Hour;
            
            if (hour >= 6 && hour < 12)
                return TimeOfDay.Day;
            else if (hour >= 12 && hour < 18)
                return TimeOfDay.Day;
            else if (hour >= 18 && hour < 22)
                return TimeOfDay.Evening;
            else
                return TimeOfDay.Night;
        }

        /// <summary>
        /// 現在のマスキング強度を計算
        /// </summary>
        private float CalculateCurrentMaskingStrength(List<AudioSource> audioSources)
        {
            float maxMasking = 0f;

            foreach (var audioSource in audioSources)
            {
                if (audioSource != null && audioSource.isPlaying)
                {
                    // 音量と距離に基づくマスキング強度の計算
                    float distance = Vector3.Distance(audioSource.transform.position, playerTransform.position);
                    float volumeContribution = audioSource.volume;
                    float distanceAttenuation = 1f - (distance / maxAudioDetectionRange);
                    
                    float maskingContribution = volumeContribution * distanceAttenuation * AudioConstants.DEFAULT_MASKING_STRENGTH;
                    maxMasking = Mathf.Max(maxMasking, maskingContribution);
                }
            }

            return Mathf.Clamp01(maxMasking);
        }

        #endregion

        #region Public Interface

        /// <summary>
        /// 更新間隔の動的変更
        /// </summary>
        public void SetUpdateInterval(float interval)
        {
            updateInterval = Mathf.Clamp(interval, 0.05f, 1f);
            
            // 現在の更新を再開
            if (coordinatedUpdateCoroutine != null)
            {
                StopCoordinatedUpdates();
                StartCoordinatedUpdates();
            }
        }

        /// <summary>
        /// 協調更新の有効/無効
        /// </summary>
        public void SetCoordinatedUpdatesEnabled(bool enabled)
        {
            enableCoordinatedUpdates = enabled;
            
            if (enabled)
            {
                StartCoordinatedUpdates();
            }
            else
            {
                StopCoordinatedUpdates();
            }
        }

        /// <summary>
        /// 空間キャッシュの手動再構築
        /// </summary>
        public void ForceRebuildSpatialCache()
        {
            RebuildSpatialCache();
        }

        /// <summary>
        /// パフォーマンス統計の取得
        /// </summary>
        public AudioCoordinatorStats GetPerformanceStats()
        {
            return new AudioCoordinatorStats
            {
                updateInterval = updateInterval,
                currentUpdateTime = currentUpdateTime,
                totalManagedAudioSources = totalManagedAudioSources,
                activeAudioSources = activeAudioSources,
                spatialCacheSize = spatialAudioCache.Count,
                trackedAudioSources = trackedAudioSources.Count
            };
        }

        #endregion
        
        #region Service Access Methods

        /// <summary>
        /// ServiceLocator優先でIAudioServiceを取得
        /// Phase 3移行パターンの実装
        /// </summary>
        private IAudioService GetAudioService()
        {
            if (FeatureFlags.UseServiceLocator)
            {
                try
                {
                    return ServiceLocator.GetService<IAudioService>();
                }
                catch (System.Exception ex)
                {
                    EventLogger.LogError($"[AudioUpdateCoordinator] Failed to get IAudioService from ServiceLocator: {ex.Message}");
                }
            }
            return null;
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// 検出されたシステム数の取得
        /// </summary>
        private int GetFoundSystemsCount()
        {
            int count = 0;
            if (weatherController != null) count++;
            if (timeController != null) count++;
            if (maskingController != null) count++;
            if (stealthCoordinator != null) count++;
            return count;
        }

        #endregion

        #region Editor Helpers

#if UNITY_EDITOR
        [Button("Force Rebuild Cache")]
        public void EditorForceRebuildCache()
        {
            if (Application.isPlaying)
            {
                ForceRebuildSpatialCache();
            }
        }

        [Button("Toggle Coordinated Updates")]
        public void EditorToggleCoordinatedUpdates()
        {
            if (Application.isPlaying)
            {
                SetCoordinatedUpdatesEnabled(!enableCoordinatedUpdates);
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (playerTransform == null) return;

            // 検出範囲の可視化
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(playerTransform.position, maxAudioDetectionRange);

            // 空間グリッドの可視化
            Gizmos.color = Color.cyan;
            Vector3Int playerGrid = WorldToGridKey(playerTransform.position);
            Vector3 gridCenter = new Vector3(playerGrid.x * spatialGridSize, playerGrid.y * spatialGridSize, playerGrid.z * spatialGridSize);
            Gizmos.DrawWireCube(gridCenter, Vector3.one * spatialGridSize);

            // 追跡中のAudioSourceの可視化
            if (trackedAudioSources != null)
            {
                Gizmos.color = Color.green;
                foreach (var audioSource in trackedAudioSources)
                {
                    if (audioSource != null)
                    {
                        Gizmos.DrawWireSphere(audioSource.transform.position, 1f);
                    }
                }
            }
        }
#endif

        #endregion
    }

    #region Supporting Types

    /// <summary>
    /// オーディオシステム同期データ
    /// </summary>
    [System.Serializable]
    public class AudioSystemSyncData
    {
        [Header("基本情報")]
        public float currentTime;
        public float deltaTime;
        public Vector3 playerPosition;

        [Header("状態変更フラグ")]
        public bool stealthStateChanged;
        public bool timeChanged;
        public bool weatherChanged;

        [Header("現在の状態")]
        public bool isStealthActive;
        public TimeOfDay currentTimeOfDay;
        public WeatherType currentWeatherType;
        public EnvironmentType currentEnvironmentType;

        [Header("音量設定")]
        public float masterVolume;
        public float bgmVolume;
        public float ambientVolume;
        public float effectVolume;

        [Header("オーディオソース情報")]
        public List<AudioSource> nearbyAudioSources = new List<AudioSource>();
        public float currentMaskingStrength;
    }

    /// <summary>
    /// オーディオコーディネーターのパフォーマンス統計
    /// </summary>
    [System.Serializable]
    public struct AudioCoordinatorStats
    {
        public float updateInterval;
        public float currentUpdateTime;
        public int totalManagedAudioSources;
        public int activeAudioSources;
        public int spatialCacheSize;
        public int trackedAudioSources;
    }

    #endregion
}