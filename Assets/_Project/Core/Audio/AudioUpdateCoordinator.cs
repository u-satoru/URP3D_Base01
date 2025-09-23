using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using asterivo.Unity60.Core.Audio.Controllers;
using asterivo.Unity60.Core.Audio.Events;
using asterivo.Unity60.Core.Events;
// using asterivo.Unity60.Core.Debug;
using asterivo.Unity60.Core;
// using asterivo.Unity60.Core.Shared;
using asterivo.Unity60.Core.Audio.Interfaces;
using asterivo.Unity60.Core.Audio.Services;
// using asterivo.Unity60.Core.Helpers;
using Sirenix.OdinInspector;

namespace asterivo.Unity60.Core.Audio
{
    /// <summary>
    /// 繧ｪ繝ｼ繝・ぅ繧ｪ繧ｷ繧ｹ繝・Β蜈ｨ菴薙・邨ｱ荳譖ｴ譁ｰ繧ｳ繝ｼ繝・ぅ繝阪・繧ｿ繝ｼ・・erviceLocator蟇ｾ蠢懶ｼ・    /// 繝ｪ繧｢繝ｫ繧ｿ繧､繝蜷梧悄縺ｮ譛驕ｩ蛹悶→繝代ヵ繧ｩ繝ｼ繝槭Φ繧ｹ蜷台ｸ翫ｒ諡・ｽ・    /// Service Locator繝代ち繝ｼ繝ｳ繧剃ｽｿ逕ｨ縺励※莉悶・繧ｵ繝ｼ繝薙せ縺ｨ騾｣謳ｺ
    /// </summary>
    public class AudioUpdateCoordinator : MonoBehaviour, IAudioUpdateService, IInitializable
    {
        [Header("Update Settings")]
        [SerializeField, Range(0.05f, 1f)] private float updateInterval = AudioConstants.AUDIO_UPDATE_INTERVAL;
        [SerializeField] private bool enableCoordinatedUpdates = true;
        // TODO: 繝舌ャ繝∝・逅・〒縺ｮ荳蠎ｦ縺ｫ譖ｴ譁ｰ縺吶ｋAudioSource縺ｮ譛螟ｧ謨ｰ蛻ｶ髯撰ｼ医ヱ繝輔か繝ｼ繝槭Φ繧ｹ譛驕ｩ蛹也畑・・#pragma warning disable CS0414 // Field assigned but never used - planned for performance batch processing
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

        // 繧ｷ繧ｹ繝・Β蜿ら・
        private WeatherAmbientController weatherController;
        private TimeAmbientController timeController;
        private MaskingEffectController maskingController;
        private StealthAudioCoordinator stealthCoordinator;
        private Transform playerTransform;

        // 譛驕ｩ蛹也畑繧ｭ繝｣繝・す繝･
        private Dictionary<Vector3Int, List<AudioSource>> spatialAudioCache;
        private HashSet<AudioSource> trackedAudioSources;
        private Queue<AudioSource> updateQueue;
        private Coroutine coordinatedUpdateCoroutine;
        
        // IAudioUpdatable邂｡逅・        private HashSet<IAudioUpdatable> registeredUpdatables;

        // 蜑企勁: 蜷梧悄繧､繝吶Φ繝医・繧､繝ｳ繧ｿ繝ｼ繝輔ぉ繝ｼ繧ｹ縺ｧ螳夂ｾｩ貂医∩

        // IInitializable螳溯｣・        public int Priority => 15; // 繧ｪ繝ｼ繝・ぅ繧ｪ譖ｴ譁ｰ繧ｳ繝ｼ繝・ぅ繝阪・繧ｿ繝ｼ縺ｯ蝓ｺ譛ｬ繧ｵ繝ｼ繝薙せ縺ｮ蠕後↓蛻晄悄蛹・        public bool IsInitialized { get; private set; }

        
        
        // IAudioUpdateService interface properties
        public float UpdateInterval 
        { 
            get => updateInterval; 
            set => SetUpdateInterval(value); 
        }
        
        public bool IsCoordinatedUpdateEnabled => enableCoordinatedUpdates;
        
        // 繧､繝吶Φ繝・        public event System.Action<AudioSystemSyncData> OnAudioSystemSync;

        #region Unity Lifecycle

        private void Awake()
        {
            // 笨・ServiceLocator蟆ら畑螳溯｣・・縺ｿ - Singleton繝代ち繝ｼ繝ｳ螳悟・蜑企勁
            DontDestroyOnLoad(gameObject);
            
            if (FeatureFlags.UseServiceLocator)
            {
                ServiceLocator.RegisterService<IAudioUpdateService>(this);
                
                if (FeatureFlags.EnableDebugLogging)
                {
                    EventLogger.LogStatic("[AudioUpdateCoordinator] Registered to ServiceLocator as IAudioUpdateService");
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
            // 笨・ServiceLocator蟆ら畑螳溯｣・・縺ｿ - Singleton繝代ち繝ｼ繝ｳ螳悟・蜑企勁
            if (FeatureFlags.UseServiceLocator)
            {
                ServiceLocator.UnregisterService<IAudioUpdateService>();
                
                if (FeatureFlags.EnableDebugLogging)
                {
                    EventLogger.LogStatic("[AudioUpdateCoordinator] Unregistered from ServiceLocator");
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
                EventLogger.LogStatic("[AudioUpdateCoordinator] Initialization complete");
            }
        }

        #endregion

        #region Initialization

        /// <summary>
        /// 繧ｳ繝ｼ繝・ぅ繝阪・繧ｿ繝ｼ縺ｮ蛻晄悄蛹・        /// </summary>
        private void InitializeCoordinator()
        {
            spatialAudioCache = new Dictionary<Vector3Int, List<AudioSource>>();
            trackedAudioSources = new HashSet<AudioSource>();
            updateQueue = new Queue<AudioSource>();
            registeredUpdatables = new HashSet<IAudioUpdatable>();
            
            EventLogger.LogStatic("<color=green>[AudioUpdateCoordinator]</color> Audio update coordinator initialized");
        }

        /// <summary>
        /// 繧ｷ繧ｹ繝・Β蜿ら・縺ｮ讀懃ｴ｢
        /// </summary>
        private void FindSystemReferences()
        {
            weatherController = ServiceHelper.GetServiceWithFallback<WeatherAmbientController>();
            timeController = ServiceHelper.GetServiceWithFallback<TimeAmbientController>();
            maskingController = ServiceHelper.GetServiceWithFallback<MaskingEffectController>();
            stealthCoordinator = ServiceHelper.GetServiceWithFallback<StealthAudioCoordinator>();

            // 繝励Ξ繧､繝､繝ｼTransform縺ｮ讀懃ｴ｢
                        var audioListener = ServiceHelper.GetServiceWithFallback<AudioListener>();;
            if (audioListener != null)
            {
                playerTransform = audioListener.transform;
            }

            EventLogger.LogStatic($"<color=green>[AudioUpdateCoordinator]</color> Found {GetFoundSystemsCount()} audio systems");
        }

        /// <summary>
        /// 遨ｺ髢薙く繝｣繝・す繝･縺ｮ蛻晄悄蛹・        /// </summary>
        private void InitializeSpatialCache()
        {
            spatialAudioCache.Clear();
            RebuildSpatialCache();
        }

        #endregion

        #region IAudioUpdateService Implementation

        /// <summary>
        /// 譖ｴ譁ｰ蜿ｯ閭ｽ縺ｪ繧ｳ繝ｳ繝昴・繝阪Φ繝医ｒ逋ｻ骭ｲ
        /// </summary>
        public void RegisterUpdatable(IAudioUpdatable updatable)
        {
            if (updatable != null)
            {
                registeredUpdatables.Add(updatable);
                
                if (FeatureFlags.EnableDebugLogging)
                {
                    EventLogger.LogStatic($"[AudioUpdateCoordinator] Registered updatable: {updatable.GetType().Name}");
                }
            }
        }

        /// <summary>
        /// 譖ｴ譁ｰ蜿ｯ閭ｽ縺ｪ繧ｳ繝ｳ繝昴・繝阪Φ繝医・逋ｻ骭ｲ隗｣髯､
        /// </summary>
        public void UnregisterUpdatable(IAudioUpdatable updatable)
        {
            if (updatable != null)
            {
                registeredUpdatables.Remove(updatable);
                
                if (FeatureFlags.EnableDebugLogging)
                {
                    EventLogger.LogStatic($"[AudioUpdateCoordinator] Unregistered updatable: {updatable.GetType().Name}");
                }
            }
        }

        #endregion

        #region Coordinated Update System

        /// <summary>
        /// 蜊碑ｪｿ譖ｴ譁ｰ縺ｮ髢句ｧ・        /// </summary>
        public void StartCoordinatedUpdates()
        {
            if (coordinatedUpdateCoroutine == null)
            {
                coordinatedUpdateCoroutine = StartCoroutine(CoordinatedUpdateLoop());
                EventLogger.LogStatic("<color=green>[AudioUpdateCoordinator]</color> Coordinated updates started");
            }
        }

        /// <summary>
        /// 蜊碑ｪｿ譖ｴ譁ｰ縺ｮ蛛懈ｭ｢
        /// </summary>
        public void StopCoordinatedUpdates()
        {
            if (coordinatedUpdateCoroutine != null)
            {
                StopCoroutine(coordinatedUpdateCoroutine);
                coordinatedUpdateCoroutine = null;
                EventLogger.LogStatic("<color=green>[AudioUpdateCoordinator]</color> Coordinated updates stopped");
            }
        }

        /// <summary>
        /// 繝｡繧､繝ｳ縺ｮ蜊碑ｪｿ譖ｴ譁ｰ繝ｫ繝ｼ繝・        /// </summary>
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

                // 蜈ｨ繧ｷ繧ｹ繝・Β縺ｮ蜊碑ｪｿ譖ｴ譁ｰ
                UpdateAllAudioSystems(syncData);

                // 蜷梧悄繧､繝吶Φ繝医・逋ｺ轣ｫ
                OnAudioSystemSync?.Invoke(syncData);
                
                // 逋ｻ骭ｲ縺輔ｌ縺櫑AudioUpdatable縺ｮ譖ｴ譁ｰ
                UpdateRegisteredUpdatables(syncData);

                // 繝代ヵ繧ｩ繝ｼ繝槭Φ繧ｹ險域ｸｬ
                currentUpdateTime = (Time.realtimeSinceStartup - updateStartTime) * 1000f; // ms

                yield return new WaitForSeconds(updateInterval);
            }
        }

        /// <summary>
        /// 蜈ｨ繧ｪ繝ｼ繝・ぅ繧ｪ繧ｷ繧ｹ繝・Β縺ｮ譖ｴ譁ｰ
        /// </summary>
        private void UpdateAllAudioSystems(AudioSystemSyncData syncData)
        {
            // Weather Controller 縺ｮ譛驕ｩ蛹匁峩譁ｰ
            if (weatherController != null)
            {
                UpdateWeatherControllerOptimized(syncData);
            }

            // Time Controller 縺ｮ譛驕ｩ蛹匁峩譁ｰ
            if (timeController != null)
            {
                UpdateTimeControllerOptimized(syncData);
            }

            // Masking Controller 縺ｮ譛驕ｩ蛹匁峩譁ｰ
            if (maskingController != null)
            {
                UpdateMaskingControllerOptimized(syncData);
            }

            // Stealth Coordinator 縺ｸ縺ｮ蜷梧悄騾夂衍
            if (stealthCoordinator != null)
            {
                NotifyStealthCoordinator(syncData);
            }
        }
        
        /// <summary>
        /// 逋ｻ骭ｲ縺輔ｌ縺櫑AudioUpdatable縺ｮ譖ｴ譁ｰ
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
                        ServiceLocator.GetService<IEventLogger>().LogError($"[AudioUpdateCoordinator] Error updating {updatable.GetType().Name}: {ex.Message}");
                    }
                }
            }
        }

        #endregion

        #region Optimized System Updates

        /// <summary>
        /// WeatherController縺ｮ譛驕ｩ蛹匁峩譁ｰ
        /// </summary>
        private void UpdateWeatherControllerOptimized(AudioSystemSyncData syncData)
        {
            // 螟ｩ豌怜､画峩縺悟ｿ・ｦ√↑蝣ｴ蜷医・縺ｿ蜃ｦ逅・            if (syncData.weatherChanged)
            {
                // 髱槫酔譛溘〒縺ｮ螟ｩ豌怜､画峩・域里蟄倥Γ繧ｽ繝・ラ繧呈ｴｻ逕ｨ・・                // WeatherController縺ｯ譌｢縺ｫ蜉ｹ邇・噪縺ｪ螳溯｣・・縺溘ａ縲√◎縺ｮ縺ｾ縺ｾ菴ｿ逕ｨ
            }
        }

        /// <summary>
        /// TimeController縺ｮ譛驕ｩ蛹匁峩譁ｰ
        /// </summary>
        private void UpdateTimeControllerOptimized(AudioSystemSyncData syncData)
        {
            // 譎る俣螟画峩縺悟ｿ・ｦ√↑蝣ｴ蜷医・縺ｿ蜃ｦ逅・            if (syncData.timeChanged)
            {
                timeController.ChangeTimeOfDay(syncData.currentTimeOfDay);
            }

            // 髻ｳ驥剰ｪｿ謨ｴ・医せ繝・Ν繧ｹ迥ｶ諷九↓蠢懊§縺ｦ・・            if (syncData.stealthStateChanged)
            {
                float volumeMultiplier = syncData.isStealthActive ? 0.6f : 1f;
                timeController.SetMasterVolume(syncData.ambientVolume * volumeMultiplier);
            }
        }

        /// <summary>
        /// MaskingController縺ｮ譛驕ｩ蛹匁峩譁ｰ
        /// </summary>
        private void UpdateMaskingControllerOptimized(AudioSystemSyncData syncData)
        {
            if (syncData.nearbyAudioSources.Count > 0)
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
        }

        /// <summary>
        /// StealthCoordinator縺ｸ縺ｮ蜷梧悄騾夂衍
        /// </summary>
        private void NotifyStealthCoordinator(AudioSystemSyncData syncData)
        {
            // 繧ｹ繝・Ν繧ｹ迥ｶ諷九↓螟牙喧縺後≠繧句ｴ蜷医・縺ｿ騾夂衍
            if (syncData.stealthStateChanged)
            {
                // StealthCoordinator縺ｫ迥ｶ諷句､画峩繧帝夂衍
                // 譌｢蟄倥・繧､繝吶Φ繝医す繧ｹ繝・Β繧呈ｴｻ逕ｨ
            }
        }

        #endregion

        #region Spatial Cache System

        /// <summary>
        /// 遨ｺ髢薙く繝｣繝・す繝･縺ｮ蜀肴ｧ狗ｯ・        /// </summary>
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

            EventLogger.LogStatic($"<color=green>[AudioUpdateCoordinator]</color> Rebuilt spatial cache: {totalManagedAudioSources} total, {activeAudioSources} active");
        }

        /// <summary>
        /// 繝ｯ繝ｼ繝ｫ繝牙ｺｧ讓吶ｒ繧ｰ繝ｪ繝・ラ繧ｭ繝ｼ縺ｫ螟画鋤
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
        /// 霑大ｍAudioSource縺ｮ蜉ｹ邇・噪蜿門ｾ・        /// </summary>
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

        #endregion

        #region Sync Data Creation

        /// <summary>
        /// 繧ｪ繝ｼ繝・ぅ繧ｪ繧ｷ繧ｹ繝・Β蜷梧悄繝・・繧ｿ縺ｮ菴懈・
        /// </summary>
        private AudioSystemSyncData CreateAudioSystemSyncData()
        {
            var syncData = new AudioSystemSyncData();

            // 蝓ｺ譛ｬ諠・ｱ
            syncData.deltaTime = Time.deltaTime;
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

            // 螟ｩ豌玲ュ蝣ｱ・・ynamicAudioEnvironment縺九ｉ蜿門ｾ暦ｼ・            var dynamicEnvironment = ServiceHelper.GetServiceWithFallback<DynamicAudioEnvironment>();
            if (dynamicEnvironment != null)
            {
                var (env, weather, time) = dynamicEnvironment.GetCurrentState();
                syncData.weatherChanged = syncData.currentWeatherType != weather;
                syncData.currentWeatherType = weather;
                syncData.currentEnvironmentType = env;
            }

            // 髻ｳ驥剰ｨｭ螳・ ServiceLocator蜆ｪ蜈医ヾingleton繝輔か繝ｼ繝ｫ繝舌ャ繧ｯ
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
                    ServiceLocator.GetService<IEventLogger>().LogError($"[AudioUpdateCoordinator] Failed to get audio state from service: {ex.Message}");
                }
            }
            else
            {
                // 繝輔か繝ｼ繝ｫ繝舌ャ繧ｯ: FindFirstObjectByType (ServiceLocator蟆ら畑螳溯｣・
                // 笨・ServiceLocator蟆ら畑螳溯｣・- 逶ｴ謗･AudioManager繧呈､懃ｴ｢
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
                            EventLogger.LogStatic("[AudioUpdateCoordinator] Found AudioManager via FindFirstObjectByType");
                        }
                    }
                    catch (System.Exception ex)
                    {
                        ServiceLocator.GetService<IEventLogger>().LogError($"[AudioUpdateCoordinator] Failed to get audio state from AudioManager: {ex.Message}");
                    }
                }
            }

            // 霑大ｍAudioSource・育ｩｺ髢薙く繝｣繝・す繝･繧呈ｴｻ逕ｨ・・            if (playerTransform != null)
            {
                syncData.nearbyAudioSources = GetNearbyAudioSources(playerTransform.position, maxAudioDetectionRange);
                
                // 繝槭せ繧ｭ繝ｳ繧ｰ蠑ｷ蠎ｦ縺ｮ險育ｮ・                syncData.currentMaskingStrength = CalculateCurrentMaskingStrength(syncData.nearbyAudioSources);
            }

            return syncData;
        }

        /// <summary>
        /// 繧ｷ繧ｹ繝・Β譎ょ綾縺九ｉ譎る俣蟶ｯ繧貞愛螳・        /// </summary>
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
        /// 迴ｾ蝨ｨ縺ｮ繝槭せ繧ｭ繝ｳ繧ｰ蠑ｷ蠎ｦ繧定ｨ育ｮ・        /// </summary>
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

        #region Public Interface

        /// <summary>
        /// 譖ｴ譁ｰ髢馴囈縺ｮ蜍慕噪螟画峩
        /// </summary>
        public void SetUpdateInterval(float interval)
        {
            updateInterval = Mathf.Clamp(interval, 0.05f, 1f);
            
            // 迴ｾ蝨ｨ縺ｮ譖ｴ譁ｰ繧貞・髢・            if (coordinatedUpdateCoroutine != null)
            {
                StopCoordinatedUpdates();
                StartCoordinatedUpdates();
            }
        }

        /// <summary>
        /// 蜊碑ｪｿ譖ｴ譁ｰ縺ｮ譛牙柑/辟｡蜉ｹ
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
        /// 遨ｺ髢薙く繝｣繝・す繝･縺ｮ謇句虚蜀肴ｧ狗ｯ・        /// </summary>
        public void ForceRebuildSpatialCache()
        {
            RebuildSpatialCache();
        }

        /// <summary>
        /// 繝代ヵ繧ｩ繝ｼ繝槭Φ繧ｹ邨ｱ險医・蜿門ｾ・        /// </summary>
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
        /// ServiceLocator蜆ｪ蜈医〒IAudioService繧貞叙蠕・        /// Phase 3遘ｻ陦後ヱ繧ｿ繝ｼ繝ｳ縺ｮ螳溯｣・        /// </summary>
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
                    ServiceLocator.GetService<IEventLogger>().LogError($"[AudioUpdateCoordinator] Failed to get IAudioService from ServiceLocator: {ex.Message}");
                }
            }
            return null;
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// 讀懷・縺輔ｌ縺溘す繧ｹ繝・Β謨ｰ縺ｮ蜿門ｾ・        /// </summary>
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

    #region Supporting Types

    /// <summary>
    /// 繧ｪ繝ｼ繝・ぅ繧ｪ繧ｷ繧ｹ繝・Β蜷梧悄繝・・繧ｿ
    /// </summary>
    [System.Serializable]
    public class AudioSystemSyncData
    {
        [Header("蝓ｺ譛ｬ諠・ｱ")]
        public float currentTime;
        public float deltaTime;
        public Vector3 playerPosition;

        [Header("迥ｶ諷句､画峩繝輔Λ繧ｰ")]
        public bool stealthStateChanged;
        public bool timeChanged;
        public bool weatherChanged;

        [Header("迴ｾ蝨ｨ縺ｮ迥ｶ諷・)]
        public bool isStealthActive;
        public TimeOfDay currentTimeOfDay;
        public WeatherType currentWeatherType;
        public EnvironmentType currentEnvironmentType;

        [Header("髻ｳ驥剰ｨｭ螳・)]
        public float masterVolume;
        public float bgmVolume;
        public float ambientVolume;
        public float effectVolume;

        [Header("繧ｪ繝ｼ繝・ぅ繧ｪ繧ｽ繝ｼ繧ｹ諠・ｱ")]
        public List<AudioSource> nearbyAudioSources = new List<AudioSource>();
        public float currentMaskingStrength;
    }

    /// <summary>
    /// 繧ｪ繝ｼ繝・ぅ繧ｪ繧ｳ繝ｼ繝・ぅ繝阪・繧ｿ繝ｼ縺ｮ繝代ヵ繧ｩ繝ｼ繝槭Φ繧ｹ邨ｱ險・    /// </summary>
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