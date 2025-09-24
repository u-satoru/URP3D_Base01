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
    /// 繧ｪ繝ｼ繝・ぅ繧ｪ繧ｷ繧ｹ繝・Β蜈ｨ菴薙・邨ｱ荳譖ｴ譁ｰ繧ｵ繝ｼ繝薙せ
    /// Service Locator繝代ち繝ｼ繝ｳ繧剃ｽｿ逕ｨ縺励◆逍守ｵ仙粋螳溯｣・    /// </summary>
    public class AudioUpdateService : MonoBehaviour, IAudioUpdateService, IInitializable
    {
        #region Properties
        
        public int Priority => 15; // AudioService繧医ｊ蠕後ヾpatialAudioService繧医ｊ蜑・        
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
        
        // 繧ｷ繧ｹ繝・Β蜿ら・
        private WeatherAmbientController weatherController;
        private TimeAmbientController timeController;
        private MaskingEffectController maskingController;
        private StealthAudioCoordinator stealthCoordinator;
        private Transform playerTransform;
        
        // 譖ｴ譁ｰ邂｡逅・        private readonly List<IAudioUpdatable> updatables = new();
        private readonly HashSet<IAudioUpdatable> updatableSet = new();
        
        // 譛驕ｩ蛹也畑繧ｭ繝｣繝・す繝･
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
            // Feature Flag遒ｺ隱・            useNewUpdateSystem = FeatureFlags.UseNewAudioUpdateSystem;
            
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
            
            // Service Locator縺九ｉ逋ｻ骭ｲ隗｣髯､
            if (ServiceLocator.HasService<IAudioUpdateService>())
            {
                ServiceLocator.UnregisterService<IAudioUpdateService>();
            }
        }
        
        #endregion
        
        #region IInitializable Implementation
        
        public void Initialize()
        {
            // Service Locator縺ｸ縺ｮ逋ｻ骭ｲ
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
            
            // 繝励Ξ繧､繝､繝ｼTransform縺ｮ讀懃ｴ｢
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
            
            // 蜆ｪ蜈亥ｺｦ縺ｧ繧ｽ繝ｼ繝・            updatables.Sort((a, b) => a.UpdatePriority.CompareTo(b.UpdatePriority));
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
            
            // 繧ｰ繝ｪ繝・ラ遽・峇縺ｮ險育ｮ・            int gridRadius = Mathf.CeilToInt(radius / spatialGridSize);
            
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
                
                // 遨ｺ髢薙く繝｣繝・す繝･縺ｮ螳壽悄逧・↑蜀肴ｧ狗ｯ・                if (Time.frameCount % (int)(60 * updateInterval * 5) == 0) // 邏・遘帝俣髫・                {
                    RebuildSpatialCache();
                }
                
                // 蜷梧悄繝・・繧ｿ縺ｮ菴懈・
                var syncData = CreateAudioSystemSyncData();
                
                // 逋ｻ骭ｲ縺輔ｌ縺滓峩譁ｰ蜿ｯ閭ｽ繧ｳ繝ｳ繝昴・繝阪Φ繝医・譖ｴ譁ｰ
                UpdateRegisteredUpdatables(updateInterval);
                
                // 蜈ｨ繧ｷ繧ｹ繝・Β縺ｮ蜊碑ｪｿ譖ｴ譁ｰ
                UpdateAllAudioSystems(syncData);
                
                // 蜷梧悄繧､繝吶Φ繝医・逋ｺ轣ｫ
                OnAudioSystemSync?.Invoke(syncData);
                
                // 繝代ヵ繧ｩ繝ｼ繝槭Φ繧ｹ險域ｸｬ
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
                    
                    // 繝輔Ξ繝ｼ繝縺斐→縺ｮ譖ｴ譁ｰ謨ｰ蛻ｶ髯・                    if (updatedCount >= maxUpdatablesPerFrame)
                        break;
                }
                catch (System.Exception e)
                {
                    ServiceLocator.GetService<IEventLogger>().LogError($"[AudioUpdateService] Error updating {updatable.GetType().Name}: {e.Message}");
                }
            }
        }
        
        private void UpdateAllAudioSystems(AudioSystemSyncData syncData)
        {
            // Weather Controller 縺ｮ譛驕ｩ蛹匁峩譁ｰ
            if (weatherController != null && syncData.weatherChanged)
            {
                UpdateWeatherControllerOptimized(syncData);
            }
            
            // Time Controller 縺ｮ譛驕ｩ蛹匁峩譁ｰ
            if (timeController != null && (syncData.timeChanged || syncData.stealthStateChanged))
            {
                UpdateTimeControllerOptimized(syncData);
            }
            
            // Masking Controller 縺ｮ譛驕ｩ蛹匁峩譁ｰ
            if (maskingController != null && syncData.nearbyAudioSources.Count > 0)
            {
                UpdateMaskingControllerOptimized(syncData);
            }
            
            // Stealth Coordinator 縺ｸ縺ｮ蜷梧悄騾夂衍
            if (stealthCoordinator != null && syncData.stealthStateChanged)
            {
                NotifyStealthCoordinator(syncData);
            }
        }
        
        private void UpdateWeatherControllerOptimized(AudioSystemSyncData syncData)
        {
            // 螟ｩ豌怜､画峩蜃ｦ逅・ｼ域里蟄倥・WeatherController繝｡繧ｽ繝・ラ繧呈ｴｻ逕ｨ・・        }
        
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
            // 繝舌ャ繝∝・逅・〒繝槭せ繧ｭ繝ｳ繧ｰ蜉ｹ譫懊ｒ驕ｩ逕ｨ
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
            // StealthCoordinator縺ｫ迥ｶ諷句､画峩繧帝夂衍
            // 譌｢蟄倥・繧､繝吶Φ繝医す繧ｹ繝・Β繧呈ｴｻ逕ｨ
        }
        
        #endregion
        
        #region Spatial Cache System
        
        private void RebuildSpatialCache()
        {
            spatialAudioCache.Clear();
            trackedAudioSources.Clear();
            
            if (playerTransform == null) return;
            
            // 蜉ｹ邇・噪縺ｪAudioSource讀懃ｴ｢
            var allAudioSources = FindObjectsByType<AudioSource>(FindObjectsSortMode.None);
            totalManagedAudioSources = allAudioSources.Length;
            activeAudioSources = 0;
            
            foreach (var audioSource in allAudioSources)
            {
                if (audioSource == null) continue;
                
                // 繝ｬ繧､繝､繝ｼ繝槭せ繧ｯ繝√ぉ繝・け
                if ((audioSourceLayerMask.value & (1 << audioSource.gameObject.layer)) == 0) continue;
                
                // 霍晞屬繝√ぉ繝・け
                float distance = Vector3.Distance(audioSource.transform.position, playerTransform.position);
                if (distance > maxAudioDetectionRange) continue;
                
                // 遨ｺ髢薙げ繝ｪ繝・ラ縺ｸ縺ｮ逋ｻ骭ｲ
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
            
            // 蝓ｺ譛ｬ諠・ｱ
            syncData.currentTime = Time.time;
            syncData.playerPosition = playerTransform?.position ?? Vector3.zero;
            
            // 繧ｹ繝・Ν繧ｹ迥ｶ諷・            bool previousStealthState = syncData.isStealthActive;
            syncData.isStealthActive = stealthCoordinator != null && stealthCoordinator.ShouldReduceNonStealthAudio();
            syncData.stealthStateChanged = previousStealthState != syncData.isStealthActive;
            
            // 譎る俣諠・ｱ
            var currentSystemTime = System.DateTime.Now;
            var newTimeOfDay = DetermineTimeOfDayFromSystemTime(currentSystemTime);
            syncData.timeChanged = syncData.currentTimeOfDay != newTimeOfDay;
            syncData.currentTimeOfDay = newTimeOfDay;
            
            // 螟ｩ豌玲ュ蝣ｱ・・ynamicAudioEnvironment縺九ｉ蜿門ｾ暦ｼ・            var dynamicEnvironment = FindFirstObjectByType<DynamicAudioEnvironment>();
            if (dynamicEnvironment != null)
            {
                var (env, weather, time) = dynamicEnvironment.GetCurrentState();
                syncData.weatherChanged = syncData.currentWeatherType != weather;
                syncData.currentWeatherType = weather;
                syncData.currentEnvironmentType = env;
            }
            
            // 髻ｳ驥剰ｨｭ螳夲ｼ・erviceLocator縺九ｉ蜿門ｾ暦ｼ・            var audioService = ServiceLocator.GetService<IAudioService>();
            if (audioService != null)
            {
                syncData.masterVolume = audioService.GetMasterVolume();
                syncData.bgmVolume = audioService.GetBGMVolume();
                syncData.ambientVolume = audioService.GetAmbientVolume();
                syncData.effectVolume = audioService.GetEffectVolume();
            }
            
            // 霑大ｍAudioSource・育ｩｺ髢薙く繝｣繝・す繝･繧呈ｴｻ逕ｨ・・            if (playerTransform != null)
            {
                syncData.nearbyAudioSources = GetNearbyAudioSources(playerTransform.position, maxAudioDetectionRange);
                
                // 繝槭せ繧ｭ繝ｳ繧ｰ蠑ｷ蠎ｦ縺ｮ險育ｮ・                syncData.currentMaskingStrength = CalculateCurrentMaskingStrength(syncData.nearbyAudioSources);
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
                    // 髻ｳ驥上→霍晞屬縺ｫ蝓ｺ縺･縺上・繧ｹ繧ｭ繝ｳ繧ｰ蠑ｷ蠎ｦ縺ｮ險育ｮ・                    float distance = Vector3.Distance(audioSource.transform.position, playerTransform.position);
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
            
            // 迴ｾ蝨ｨ縺ｮ譖ｴ譁ｰ繧貞・髢・            if (coordinatedUpdateCoroutine != null)
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
            
            // 讀懷・遽・峇縺ｮ蜿ｯ隕門喧
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(playerTransform.position, maxAudioDetectionRange);
            
            // 遨ｺ髢薙げ繝ｪ繝・ラ縺ｮ蜿ｯ隕門喧
            Gizmos.color = Color.cyan;
            Vector3Int playerGrid = WorldToGridKey(playerTransform.position);
            Vector3 gridCenter = new Vector3(playerGrid.x * spatialGridSize, playerGrid.y * spatialGridSize, playerGrid.z * spatialGridSize);
            Gizmos.DrawWireCube(gridCenter, Vector3.one * spatialGridSize);
            
            // 霑ｽ霍｡荳ｭ縺ｮAudioSource縺ｮ蜿ｯ隕門喧
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
