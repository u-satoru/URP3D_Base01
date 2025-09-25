using asterivo.Unity60.Core;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using asterivo.Unity60.Core.Audio.Controllers;
using asterivo.Unity60.Core.Audio.Events;
using asterivo.Unity60.Core.Events;
// using asterivo.Unity60.Core.Debug;
// using asterivo.Unity60.Core.Shared;
using asterivo.Unity60.Core.Audio.Interfaces;

using Sirenix.OdinInspector;

namespace asterivo.Unity60.Core.Audio.Services
{
    /// <summary>
    /// オーディオシステム全体の統一更新サービス
    /// Service Locatorパターンを使用した疎結合実装
    /// </summary>
    public class AudioUpdateService : MonoBehaviour, IAudioUpdateService, IInitializable
    {
        #region Properties
        
        public int Priority => 15; // AudioServiceより後、SpatialAudioServiceより前
        
        public bool IsInitialized { get; private set; }
        
        public float UpdateInterval 
        { 
            get => updateInterval;
            set => SetUpdateInterval(value);
        }
        
        public bool IsCoordinatedUpdateEnabled => enableCoordinatedUpdates;
        
        #endregion
        
        #region Events
        
        public event System.Action<AudioSystemSyncData> OnAudioSystemSync;
        
        #endregion
        
        #region Configuration
        
        [Header("Update Settings")]
        [SerializeField, Range(0.05f, 1f)] private float updateInterval = AudioConstants.AUDIO_UPDATE_INTERVAL;
        [SerializeField] private bool enableCoordinatedUpdates = true;
        [SerializeField, Range(1, 10)] private int maxUpdatablesPerFrame = 5;
        
        [Header("Spatial Optimization")]
        [SerializeField] private LayerMask audioSourceLayerMask = -1;
        [SerializeField, Range(5f, 50f)] private float maxAudioDetectionRange = 25f;
        [SerializeField] private int spatialGridSize = 10;
        
        [Header("Performance Monitoring")]
        [SerializeField, ReadOnly] private float currentUpdateTime;
        [SerializeField, ReadOnly] private int totalManagedAudioSources;
        [SerializeField, ReadOnly] private int activeAudioSources;
        [SerializeField, ReadOnly] private int registeredUpdatables;
        
        #endregion
        
        #region Private Fields
        
        // システム参照
        private WeatherAmbientController weatherController;
        private TimeAmbientController timeController;
        private MaskingEffectController maskingController;
        private StealthAudioCoordinator stealthCoordinator;
        private Transform playerTransform;
        
        // 更新管理
        private readonly List<IAudioUpdatable> updatables = new();
        private readonly HashSet<IAudioUpdatable> updatableSet = new();
        
        // 最適化用キャッシュ
        private Dictionary<Vector3Int, List<AudioSource>> spatialAudioCache;
        private HashSet<AudioSource> trackedAudioSources;
        private Coroutine coordinatedUpdateCoroutine;
        
        // Feature Flag
        private bool useNewUpdateSystem;
        
        #endregion
        
        #region Unity Lifecycle
        
        private void Awake()
        {
            InitializeService();
        }
        
        private void Start()
        {
            // Feature Flag確認
            useNewUpdateSystem = FeatureFlags.UseNewAudioUpdateSystem;
            
            if (!useNewUpdateSystem)
            {
                EventLogger.LogStatic("<color=yellow>[AudioUpdateService]</color> New update system disabled by feature flag");
                enabled = false;
                return;
            }
            
            FindSystemReferences();
            InitializeSpatialCache();
            
            if (enableCoordinatedUpdates)
            {
                StartCoordinatedUpdates();
            }
        }
        
        private void OnDestroy()
        {
            StopCoordinatedUpdates();
            
            // Service Locatorから登録解除
            if (ServiceLocator.HasService<IAudioUpdateService>())
            {
                ServiceLocator.UnregisterService<IAudioUpdateService>();
            }
        }
        
        #endregion
        
        #region IInitializable Implementation
        
        public void Initialize()
        {
            // Service Locatorへの登録
            ServiceLocator.RegisterService<IAudioUpdateService>(this);
            EventLogger.LogStatic("<color=green>[AudioUpdateService]</color> Service registered to ServiceLocator");
            IsInitialized = true;
        }
        
        #endregion
        
        #region Initialization
        
        private void InitializeService()
        {
            spatialAudioCache = new Dictionary<Vector3Int, List<AudioSource>>();
            trackedAudioSources = new HashSet<AudioSource>();
            
            EventLogger.LogStatic("<color=green>[AudioUpdateService]</color> Audio update service initialized");
        }
        
        private void FindSystemReferences()
        {
            weatherController = FindFirstObjectByType<WeatherAmbientController>();
            timeController = FindFirstObjectByType<TimeAmbientController>();
            maskingController = FindFirstObjectByType<MaskingEffectController>();
            stealthCoordinator = FindFirstObjectByType<StealthAudioCoordinator>();
            
            // プレイヤーTransformの検索
            var audioListener = FindFirstObjectByType<AudioListener>();
            if (audioListener != null)
            {
                playerTransform = audioListener.transform;
            }
            
            EventLogger.LogStatic($"<color=green>[AudioUpdateService]</color> Found {GetFoundSystemsCount()} audio systems");
        }
        
        private void InitializeSpatialCache()
        {
            spatialAudioCache.Clear();
            RebuildSpatialCache();
        }
        
        #endregion
        
        #region IAudioUpdateService Implementation
        
        public void RegisterUpdatable(IAudioUpdatable updatable)
        {
            if (updatable == null || updatableSet.Contains(updatable))
                return;
            
            updatables.Add(updatable);
            updatableSet.Add(updatable);
            
            // 優先度でソート
            updatables.Sort((a, b) => a.UpdatePriority.CompareTo(b.UpdatePriority));
            registeredUpdatables = updatables.Count;
            
            EventLogger.LogStatic($"<color=green>[AudioUpdateService]</color> Registered updatable: {updatable.GetType().Name}");
        }
        
        public void UnregisterUpdatable(IAudioUpdatable updatable)
        {
            if (updatable == null || !updatableSet.Contains(updatable))
                return;
            
            updatables.Remove(updatable);
            updatableSet.Remove(updatable);
            registeredUpdatables = updatables.Count;
            
            EventLogger.LogStatic($"<color=green>[AudioUpdateService]</color> Unregistered updatable: {updatable.GetType().Name}");
        }
        
        public void StartCoordinatedUpdates()
        {
            if (coordinatedUpdateCoroutine == null)
            {
                coordinatedUpdateCoroutine = StartCoroutine(CoordinatedUpdateLoop());
                EventLogger.LogStatic("<color=green>[AudioUpdateService]</color> Coordinated updates started");
            }
        }
        
        public void StopCoordinatedUpdates()
        {
            if (coordinatedUpdateCoroutine != null)
            {
                StopCoroutine(coordinatedUpdateCoroutine);
                coordinatedUpdateCoroutine = null;
                EventLogger.LogStatic("<color=green>[AudioUpdateService]</color> Coordinated updates stopped");
            }
        }
        
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
        
        public void ForceRebuildSpatialCache()
        {
            RebuildSpatialCache();
        }
        
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
        
        #region Update System
        
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
                
                // 登録された更新可能コンポーネントの更新
                UpdateRegisteredUpdatables(updateInterval);
                
                // 全システムの協調更新
                UpdateAllAudioSystems(syncData);
                
                // 同期イベントの発火
                OnAudioSystemSync?.Invoke(syncData);
                
                // パフォーマンス計測
                currentUpdateTime = (Time.realtimeSinceStartup - updateStartTime) * 1000f; // ms
                
                yield return new WaitForSeconds(updateInterval);
            }
        }
        
        private void UpdateRegisteredUpdatables(float deltaTime)
        {
            int updatedCount = 0;
            
            foreach (var updatable in updatables)
            {
                if (updatable == null || !updatable.IsUpdateEnabled)
                    continue;
                
                try
                {
                    updatable.UpdateAudio(deltaTime);
                    updatedCount++;
                    
                    // フレームごとの更新数制限
                    if (updatedCount >= maxUpdatablesPerFrame)
                        break;
                }
                catch (System.Exception e)
                {
                    EventLogger.LogErrorStatic($"[AudioUpdateService] Error updating {updatable.GetType().Name}: {e.Message}");
                }
            }
        }
        
        private void UpdateAllAudioSystems(AudioSystemSyncData syncData)
        {
            // Weather Controller の最適化更新
            if (weatherController != null && syncData.weatherChanged)
            {
                UpdateWeatherControllerOptimized(syncData);
            }
            
            // Time Controller の最適化更新
            if (timeController != null && (syncData.timeChanged || syncData.stealthStateChanged))
            {
                UpdateTimeControllerOptimized(syncData);
            }
            
            // Masking Controller の最適化更新
            if (maskingController != null && syncData.nearbyAudioSources.Count > 0)
            {
                UpdateMaskingControllerOptimized(syncData);
            }
            
            // Stealth Coordinator への同期通知
            if (stealthCoordinator != null && syncData.stealthStateChanged)
            {
                NotifyStealthCoordinator(syncData);
            }
        }
        
        private void UpdateWeatherControllerOptimized(AudioSystemSyncData syncData)
        {
            // 天気変更処理（既存のWeatherControllerメソッドを活用）
        }
        
        private void UpdateTimeControllerOptimized(AudioSystemSyncData syncData)
        {
            if (syncData.timeChanged)
            {
                timeController.ChangeTimeOfDay(syncData.currentTimeOfDay);
            }
            
            if (syncData.stealthStateChanged)
            {
                float volumeMultiplier = syncData.isStealthActive ? 0.6f : 1f;
                timeController.SetMasterVolume(syncData.ambientVolume * volumeMultiplier);
            }
        }
        
        private void UpdateMaskingControllerOptimized(AudioSystemSyncData syncData)
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
        
        private void NotifyStealthCoordinator(AudioSystemSyncData syncData)
        {
            // StealthCoordinatorに状態変更を通知
            // 既存のイベントシステムを活用
        }
        
        #endregion
        
        #region Spatial Cache System
        
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
            
            EventLogger.LogStatic($"<color=green>[AudioUpdateService]</color> Rebuilt spatial cache: {totalManagedAudioSources} total, {activeAudioSources} active");
        }
        
        private Vector3Int WorldToGridKey(Vector3 worldPosition)
        {
            return new Vector3Int(
                Mathf.FloorToInt(worldPosition.x / spatialGridSize),
                Mathf.FloorToInt(worldPosition.y / spatialGridSize),
                Mathf.FloorToInt(worldPosition.z / spatialGridSize)
            );
        }
        
        #endregion
        
        #region Sync Data Creation
        
        private AudioSystemSyncData CreateAudioSystemSyncData()
        {
            var syncData = new AudioSystemSyncData();
            
            // 基本情報
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
            var dynamicEnvironment = FindFirstObjectByType<DynamicAudioEnvironment>();
            if (dynamicEnvironment != null)
            {
                var (env, weather, time) = dynamicEnvironment.GetCurrentState();
                syncData.weatherChanged = syncData.currentWeatherType != weather;
                syncData.currentWeatherType = weather;
                syncData.currentEnvironmentType = env;
            }
            
            // 音量設定（ServiceLocatorから取得）
            var audioService = ServiceLocator.GetService<IAudioService>();
            if (audioService != null)
            {
                syncData.masterVolume = audioService.GetMasterVolume();
                syncData.bgmVolume = audioService.GetBGMVolume();
                syncData.ambientVolume = audioService.GetAmbientVolume();
                syncData.effectVolume = audioService.GetEffectVolume();
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
        
        #region Helper Methods
        
        private void SetUpdateInterval(float interval)
        {
            updateInterval = Mathf.Clamp(interval, 0.05f, 1f);
            
            // 現在の更新を再開
            if (coordinatedUpdateCoroutine != null)
            {
                StopCoordinatedUpdates();
                StartCoordinatedUpdates();
            }
        }
        
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
                if (enableCoordinatedUpdates)
                {
                    StopCoordinatedUpdates();
                    enableCoordinatedUpdates = false;
                }
                else
                {
                    enableCoordinatedUpdates = true;
                    StartCoordinatedUpdates();
                }
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
}