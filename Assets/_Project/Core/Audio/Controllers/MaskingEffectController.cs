using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using asterivo.Unity60.Core.Audio.Data;
using asterivo.Unity60.Core.Audio.Events;
using asterivo.Unity60.Core.Events;
// Debug functionality is now in Core namespace
using asterivo.Unity60.Core;
// AudioConstants is now in Core namespace
using Sirenix.OdinInspector;

namespace asterivo.Unity60.Core.Audio.Controllers
{
    /// <summary>
    /// 繧ｹ繝・・ｽ・ｽ繧ｹ繧ｲ繝ｼ繝逕ｨ繝槭せ繧ｭ繝ｳ繧ｰ蜉ｹ譫懷宛蠕｡繧ｷ繧ｹ繝・・ｽ・ｽ
    /// AmbientManager縺九ｉ蛻・・ｽ・ｽ縺輔ｌ縺滂ｿｽE繧ｹ繧ｭ繝ｳ繧ｰ蜉ｹ譫懷ｰら畑繧ｳ繝ｳ繝医Ο繝ｼ繝ｩ繝ｼ
    /// </summary>
    public class MaskingEffectController : MonoBehaviour
    {
        [Header("Masking Configuration")]
        [SerializeField, Range(0f, 1f)] private float globalMaskingStrength = AudioConstants.DEFAULT_MASKING_STRENGTH;
        [SerializeField, Range(0f, 5f)] private float maskingRadius = AudioConstants.DEFAULT_MASKING_RADIUS;
        [SerializeField] private LayerMask stealthSoundLayerMask = -1;

        [Header("Detection Settings")]
        [SerializeField, Range(0.1f, 2f)] private float detectionUpdateInterval = 0.2f;
        [SerializeField] private bool enableDynamicMasking = true;
        [SerializeField] private AnimationCurve maskingFalloffCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);

        [Header("Audio Source Filtering")]
        [SerializeField] private string[] maskingAudioTags = { "AmbientSound", "EnvironmentalNoise" };
        [SerializeField, Range(1, 10)] private int maxTrackingAudioSources = 5;

        [Header("Events")]
        [SerializeField] private GameEvent maskingActivatedEvent;
        [SerializeField] private GameEvent maskingDeactivatedEvent;
        [SerializeField] private AudioEvent maskingSoundDetectedEvent;

        [Header("Runtime Info")]
        [SerializeField, ReadOnly] private bool isMaskingActive = false;
        [SerializeField, ReadOnly] private int activeMaskingSources = 0;
        [SerializeField, ReadOnly] private float currentMaskingStrength = 0f;
        [SerializeField, ReadOnly] private Vector3 lastMaskingPosition;

        // 繧ｷ繧ｹ繝・・ｽ・ｽ蜿ゑｿｽE
        private Transform playerTransform;
        private AudioListener audioListener;
        private StealthAudioCoordinator stealthCoordinator;

        // 繝槭せ繧ｭ繝ｳ繧ｰ邂｡逅・
        private List<MaskingAudioSource> trackingAudioSources = new List<MaskingAudioSource>();
        private Dictionary<AudioSource, float> maskingStrengthCache = new Dictionary<AudioSource, float>();
        private Coroutine maskingUpdateCoroutine;

        #region Unity Lifecycle

        private void Awake()
        {
            InitializeMaskingController();
        }

        private void Start()
        {
            FindSystemReferences();
            
            if (enableDynamicMasking)
            {
                StartMaskingDetection();
            }
        }

        private void OnDisable()
        {
            StopMaskingDetection();
        }

        #endregion

        #region Initialization

        /// <summary>
        /// 繝槭せ繧ｭ繝ｳ繧ｰ繧ｳ繝ｳ繝医Ο繝ｼ繝ｩ繝ｼ縺ｮ蛻晄悄蛹・        /// </summary>
        private void InitializeMaskingController()
        {
            trackingAudioSources = new List<MaskingAudioSource>();
            maskingStrengthCache = new Dictionary<AudioSource, float>();
            
            ServiceHelper.Log("<color=cyan>[MaskingEffectController]</color> Masking effect controller initialized");
        }

        /// <summary>
        /// 繧ｷ繧ｹ繝・・ｽ・ｽ蜿ゑｿｽE縺ｮ讀懃ｴ｢
        /// </summary>
        private void FindSystemReferences()
        {
            // AudioListener繧呈､懃ｴ｢
            audioListener = FindFirstObjectByType<AudioListener>();
            if (audioListener != null)
            {
                playerTransform = audioListener.transform;
            }

            // StealthAudioCoordinator繧呈､懃ｴ｢
            stealthCoordinator = FindFirstObjectByType<StealthAudioCoordinator>();
            
            if (playerTransform == null)
            {
                ServiceLocator.GetService<IEventLogger>().LogWarning("[MaskingEffectController] Player transform not found! Masking effects may not work properly.");
            }
        }

        #endregion

        #region Public Interface

        /// <summary>
        /// 繧ｰ繝ｭ繝ｼ繝舌Ν繝槭せ繧ｭ繝ｳ繧ｰ蠑ｷ蠎ｦ縺ｮ險ｭ螳・
        /// </summary>
        public void SetGlobalMaskingStrength(float strength)
        {
            globalMaskingStrength = Mathf.Clamp01(strength);
            ServiceHelper.Log($"<color=cyan>[MaskingEffectController]</color> Global masking strength set to {globalMaskingStrength:F2}");
        }

        /// <summary>
        /// 繝槭せ繧ｭ繝ｳ繧ｰ蜊雁ｾ・・險ｭ螳・
        /// </summary>
        public void SetMaskingRadius(float radius)
        {
            maskingRadius = Mathf.Max(0f, radius);
        }

        /// <summary>
        /// 蜍慕噪繝槭せ繧ｭ繝ｳ繧ｰ縺ｮ譛牙柑/辟｡蜉ｹ
        /// </summary>
        public void SetDynamicMasking(bool enabled)
        {
            enableDynamicMasking = enabled;
            
            if (enabled)
            {
                StartMaskingDetection();
            }
            else
            {
                StopMaskingDetection();
            }
        }

        /// <summary>
        /// 迚ｹ螳壻ｽ咲ｽｮ縺ｧ縺ｮ繝槭せ繧ｭ繝ｳ繧ｰ蜉ｹ譫懊ｒ險育ｮ・        /// </summary>
        public float CalculateMaskingAtPosition(Vector3 position, float baseMaskingStrength = 1f)
        {
            if (playerTransform == null || !isMaskingActive)
                return 0f;

            float distance = Vector3.Distance(position, playerTransform.position);
            
            if (distance > maskingRadius)
                return 0f;

            float normalizedDistance = distance / maskingRadius;
            float falloff = maskingFalloffCurve.Evaluate(normalizedDistance);
            
            return globalMaskingStrength * baseMaskingStrength * falloff;
        }

        /// <summary>
        /// AudioSource縺ｫ繝槭せ繧ｭ繝ｳ繧ｰ蜉ｹ譫懊ｒ驕ｩ逕ｨ
        /// </summary>
        public bool ApplyMaskingToAudioSource(AudioSource audioSource, float customMaskingStrength = -1f)
        {
            if (audioSource == null || playerTransform == null)
                return false;

            float maskingStrength = customMaskingStrength >= 0f ? customMaskingStrength : 
                CalculateMaskingAtPosition(audioSource.transform.position);

            if (maskingStrength > AudioConstants.MIN_AUDIBLE_VOLUME)
            {
                // 繝槭せ繧ｭ繝ｳ繧ｰ蜉ｹ譫懊ｒ髻ｳ驥上↓蜿肴丐
                float originalVolume = GetOriginalVolume(audioSource);
                float maskedVolume = originalVolume * (1f - maskingStrength);
                audioSource.volume = maskedVolume;

                // 繧ｭ繝｣繝・・ｽ・ｽ繝･繧呈峩譁ｰ
                maskingStrengthCache[audioSource] = maskingStrength;
                return true;
            }

            return false;
        }

        /// <summary>
        /// 謇句虚縺ｧ繝槭せ繧ｭ繝ｳ繧ｰ蜉ｹ譫懊ｒ繝医Μ繧ｬ繝ｼ
        /// </summary>
        public void TriggerMaskingAtPosition(Vector3 position, float duration = 2f, float strength = 1f)
        {
            StartCoroutine(ManualMaskingCoroutine(position, duration, strength));
        }

        /// <summary>
        /// 蜈ｨ繝槭せ繧ｭ繝ｳ繧ｰ蜉ｹ譫懶ｿｽE蛛懈ｭ｢
        /// </summary>
        public void StopAllMaskingEffects()
        {
            StopMaskingDetection();
            
            // 蜈ｨAudioSource縺ｮ繝槭せ繧ｭ繝ｳ繧ｰ蜉ｹ譫懊ｒ繝ｪ繧ｻ繝・・ｽ・ｽ
            foreach (var kvp in maskingStrengthCache.ToList())
            {
                if (kvp.Key != null)
                {
                    RestoreOriginalVolume(kvp.Key);
                }
            }
            
            maskingStrengthCache.Clear();
            trackingAudioSources.Clear();
            isMaskingActive = false;
            currentMaskingStrength = 0f;

            if (maskingDeactivatedEvent != null)
            {
                maskingDeactivatedEvent.Raise();
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// 繝槭せ繧ｭ繝ｳ繧ｰ讀懶ｿｽE縺ｮ髢句ｧ・        /// </summary>
        private void StartMaskingDetection()
        {
            // 窶ｻServiceLocator蟆ら畑螳溯｣・- IAudioUpdateService繧貞叙蠕・
            if (asterivo.Unity60.Core.FeatureFlags.UseServiceLocator)
            {
                try
                {
                    var audioUpdateService = asterivo.Unity60.Core.ServiceLocator.GetService<asterivo.Unity60.Core.Audio.Interfaces.IAudioUpdateService>();
                    if (audioUpdateService is AudioUpdateCoordinator coordinator && coordinator.enabled)
                    {
                        // 蜊碑ｪｿ譖ｴ譁ｰ繧ｷ繧ｹ繝・・ｽ・ｽ縺ｫ逋ｻ骭ｲ・ｽE・ｽ繧､繝吶Φ繝磯ｧ・・ｽ・ｽ・ｽE・ｽE                        coordinator.OnAudioSystemSync += OnAudioSystemSync;
                        ServiceHelper.Log("<color=cyan>[MaskingEffectController]</color> Registered with AudioUpdateCoordinator via ServiceLocator");
                        return;
                    }
                }
                catch (System.Exception ex)
                {
                    ServiceLocator.GetService<IEventLogger>().LogError($"[MaskingEffectController] Failed to get IAudioUpdateService from ServiceLocator: {ex.Message}");
                }
            }
            
            // 繝輔か繝ｼ繝ｫ繝舌ャ繧ｯ: 逶ｴ謗･讀懃ｴ｢
            var fallbackCoordinator = FindFirstObjectByType<AudioUpdateCoordinator>();
            if (fallbackCoordinator != null && fallbackCoordinator.enabled)
            {
                fallbackCoordinator.OnAudioSystemSync += OnAudioSystemSync;
                ServiceHelper.Log("<color=cyan>[MaskingEffectController]</color> Registered with AudioUpdateCoordinator via fallback");
                return;
            }
            
            // 繝輔か繝ｼ繝ｫ繝舌ャ繧ｯ・ｽE・ｽ蠕捺擂縺ｮ讀懶ｿｽE繧ｷ繧ｹ繝・・ｽ・ｽ
            if (maskingUpdateCoroutine == null)
            {
                maskingUpdateCoroutine = StartCoroutine(MaskingDetectionCoroutine());
            }
        }

        /// <summary>
        /// 繝槭せ繧ｭ繝ｳ繧ｰ讀懶ｿｽE縺ｮ蛛懈ｭ｢
        /// </summary>
        private void StopMaskingDetection()
        {
            // 窶ｻServiceLocator蟆ら畑螳溯｣・- IAudioUpdateService縺九ｉ縺ｮ逋ｻ骭ｲ隗｣髯､
            if (asterivo.Unity60.Core.FeatureFlags.UseServiceLocator)
            {
                try
                {
                    var audioUpdateService = asterivo.Unity60.Core.ServiceLocator.GetService<asterivo.Unity60.Core.Audio.Interfaces.IAudioUpdateService>();
                    if (audioUpdateService is AudioUpdateCoordinator coordinator)
                    {
                        coordinator.OnAudioSystemSync -= OnAudioSystemSync;
                    }
                }
                catch (System.Exception ex)
                {
                    ServiceLocator.GetService<IEventLogger>().LogError($"[MaskingEffectController] Failed to unregister from IAudioUpdateService: {ex.Message}");
                }
            }
            
            // 繝輔か繝ｼ繝ｫ繝舌ャ繧ｯ: 逶ｴ謗･讀懃ｴ｢縺ｧ縺ｮ逋ｻ骭ｲ隗｣髯､
            var fallbackCoordinator = FindFirstObjectByType<AudioUpdateCoordinator>();
            if (fallbackCoordinator != null)
            {
                fallbackCoordinator.OnAudioSystemSync -= OnAudioSystemSync;
            }
            
            // 蠕捺擂縺ｮ繧ｳ繝ｫ繝ｼ繝√Φ縺ｮ蛛懈ｭ｢
            if (maskingUpdateCoroutine != null)
            {
                StopCoroutine(maskingUpdateCoroutine);
                maskingUpdateCoroutine = null;
            }
        }

        /// <summary>
        /// 繝槭せ繧ｭ繝ｳ繧ｰ讀懶ｿｽE縺ｮ繝｡繧､繝ｳ繧ｳ繝ｫ繝ｼ繝√Φ
        /// </summary>
        private IEnumerator MaskingDetectionCoroutine()
        {
            while (enableDynamicMasking)
            {
                UpdateMaskingAudioSources();
                ProcessMaskingEffects();
                
                yield return new WaitForSeconds(detectionUpdateInterval);
            }
        }

        /// <summary>
        /// 繝槭せ繧ｭ繝ｳ繧ｰ蟇ｾ雎｡AudioSource縺ｮ譖ｴ譁ｰ
        /// </summary>
        private void UpdateMaskingAudioSources()
        {
            if (playerTransform == null) return;

            // 莉倩ｿ代・AudioSource繧呈､懃ｴ｢
            var nearbyAudioSources = FindObjectsByType<AudioSource>(FindObjectsSortMode.None)
                .Where(audioSource => audioSource.isPlaying && IsValidMaskingSource(audioSource))
                .Select(audioSource => new { 
                    source = audioSource, 
                    distance = Vector3.Distance(audioSource.transform.position, playerTransform.position) 
                })
                .Where(item => item.distance <= maskingRadius)
                .OrderBy(item => item.distance)
                .Take(maxTrackingAudioSources)
                .Select(item => new MaskingAudioSource 
                { 
                    audioSource = item.source, 
                    distance = item.distance,
                    maskingStrength = CalculateMaskingAtPosition(item.source.transform.position)
                })
                .ToList();

            trackingAudioSources = nearbyAudioSources;
            activeMaskingSources = trackingAudioSources.Count;
        }

        /// <summary>
        /// AudioSource縺鯉ｿｽE繧ｹ繧ｭ繝ｳ繧ｰ蟇ｾ雎｡縺ｨ縺励※譛牙柑縺九メ繧ｧ繝・・ｽ・ｽ
        /// </summary>
        private bool IsValidMaskingSource(AudioSource audioSource)
        {
            // 繧ｿ繧ｰ繝√ぉ繝・・ｽ・ｽ
            bool hasValidTag = maskingAudioTags.Length == 0 || maskingAudioTags.Contains(audioSource.tag);
            
            // 繝ｬ繧､繝､繝ｼ繝√ぉ繝・・ｽ・ｽ
            bool hasValidLayer = ((1 << audioSource.gameObject.layer) & stealthSoundLayerMask) != 0;
            
            // 髻ｳ驥上メ繧ｧ繝・・ｽ・ｽ・ｽE・ｽ髱槫ｸｸ縺ｫ蟆上＆縺・・ｽ・ｽ縺ｯ髯､螟厄ｼ・            bool hasAudibleVolume = audioSource.volume > AudioConstants.MIN_AUDIBLE_VOLUME;
            
            return hasValidTag && hasValidLayer && hasAudibleVolume;
        }

        /// <summary>
        /// 繝槭せ繧ｭ繝ｳ繧ｰ蜉ｹ譫懊・蜃ｦ逅・
        /// </summary>
        private void ProcessMaskingEffects()
        {
            bool wasActive = isMaskingActive;
            currentMaskingStrength = 0f;

            if (trackingAudioSources.Count > 0)
            {
                isMaskingActive = true;
                
                // 譛繧ょｼｷ縺・・ｽE繧ｹ繧ｭ繝ｳ繧ｰ蜉ｹ譫懊ｒ蜿門ｾ・                currentMaskingStrength = trackingAudioSources.Max(source => source.maskingStrength);
                lastMaskingPosition = trackingAudioSources
                    .OrderByDescending(source => source.maskingStrength)
                    .First().audioSource.transform.position;

                // 繧ｹ繝・Ν繧ｹ邉ｻ繧ｷ繧ｹ繝・Β縺ｫ騾夂衍
                if (stealthCoordinator != null)
                {
                    stealthCoordinator.NotifyMaskingEffect(lastMaskingPosition, currentMaskingStrength, maskingRadius);
                }
            }
            else
            {
                isMaskingActive = false;
            }

            // 迥ｶ諷句､画峩譎ゑｿｽE繧､繝吶Φ繝育匱轣ｫ
            if (isMaskingActive != wasActive)
            {
                if (isMaskingActive && maskingActivatedEvent != null)
                {
                    maskingActivatedEvent.Raise();
                }
                else if (!isMaskingActive && maskingDeactivatedEvent != null)
                {
                    maskingDeactivatedEvent.Raise();
                }
            }

            // 繝槭せ繧ｭ繝ｳ繧ｰ髻ｳ髻ｿ讀懶ｿｽE繧､繝吶Φ繝・            if (isMaskingActive && maskingSoundDetectedEvent != null)
            {
                var strongestSource = trackingAudioSources.OrderByDescending(s => s.maskingStrength).First();
                var eventData = new AudioEventData
                {
                    audioClip = strongestSource.audioSource.clip,
                    volume = currentMaskingStrength,
                    category = AudioCategory.Ambient,
                    worldPosition = strongestSource.audioSource.transform.position
                };
                maskingSoundDetectedEvent.Raise(eventData);
            }
        }

        /// <summary>
        /// 謇句虚繝槭せ繧ｭ繝ｳ繧ｰ蜉ｹ譫懶ｿｽE繧ｳ繝ｫ繝ｼ繝√Φ
        /// </summary>
        private IEnumerator ManualMaskingCoroutine(Vector3 position, float duration, float strength)
        {
            ServiceHelper.Log($"<color=cyan>[MaskingEffectController]</color> Manual masking triggered at {position} for {duration}s");

            float elapsed = 0f;
            bool wasActive = isMaskingActive;
            
            isMaskingActive = true;
            lastMaskingPosition = position;

            if (!wasActive && maskingActivatedEvent != null)
            {
                maskingActivatedEvent.Raise();
            }

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float fadeMultiplier = 1f - (elapsed / duration);
                currentMaskingStrength = strength * fadeMultiplier;
                
                yield return null;
            }

            currentMaskingStrength = 0f;
            isMaskingActive = false;

            if (maskingDeactivatedEvent != null)
            {
                maskingDeactivatedEvent.Raise();
            }
        }

        /// <summary>
        /// AudioSource縺ｮ蜈・・ｽE髻ｳ驥上ｒ蜿門ｾ・        /// </summary>
        private float GetOriginalVolume(AudioSource audioSource)
        {
            // 蜈・・ｽE髻ｳ驥擾ｿｽE菫晏ｭ倥＠縺ｦ縺・・ｽ・ｽ縺・・ｽ・ｽ繧√∫樟蝨ｨ縺ｮ髻ｳ驥上ｒ蝓ｺ貅悶→縺吶ｋ
            // 螳滄圀縺ｮ繝励Ο繧ｸ繧ｧ繧ｯ繝医〒縺ｯ縲、udioSource縺ｮ蜈・・ｽ・ｽ繝ｼ繧ｿ繧剃ｿ晄戟縺吶ｋ莉慕ｵ・・ｽ・ｽ縺悟ｿ・・ｽ・ｽE            return Mathf.Max(audioSource.volume, 0.1f);
        }

        /// <summary>
        /// AudioSource縺ｮ髻ｳ驥上ｒ蜈・・ｽ・ｽ謌ｻ縺・        /// </summary>
        private void RestoreOriginalVolume(AudioSource audioSource)
        {
            // 螳滄圀縺ｮ蜈・・髻ｳ驥上ョ繝ｼ繧ｿ菫晄戟譁ｹ豕輔↓萓晏ｭ・
            // 縺薙％縺ｧ縺ｯ邁｡蜊倥↑蠕ｩ蜈・・逅・
            if (maskingStrengthCache.ContainsKey(audioSource))
            {
                maskingStrengthCache.Remove(audioSource);
            }
        }

        #endregion

        #region Helper Classes

        /// <summary>
        /// 繝槭せ繧ｭ繝ｳ繧ｰ蟇ｾ雎｡AudioSource諠・ｱ
        /// </summary>
        [System.Serializable]
        private class MaskingAudioSource
        {
            public AudioSource audioSource;
            public float distance;
            public float maskingStrength;
        }

        /// <summary>
        /// AudioUpdateCoordinator縺九ｉ縺ｮ蜷梧悄繧ｳ繝ｼ繝ｫ繝舌ャ繧ｯ
        /// </summary>
        private void OnAudioSystemSync(AudioSystemSyncData syncData)
        {
            // 蜉ｹ邇・噪縺ｪ繝槭せ繧ｭ繝ｳ繧ｰ蜃ｦ逅・↓譌｢縺ｫ譛驕ｩ蛹悶＆繧後◆繝・・繧ｿ繧剃ｽｿ逕ｨ
            if (syncData.nearbyAudioSources.Count > 0)
            {
                currentMaskingStrength = syncData.currentMaskingStrength;
                
                // 繝舌ャ繝∝・逅・〒繝槭せ繧ｭ繝ｳ繧ｰ蜉ｹ譫懊ｒ驕ｩ逕ｨ
                foreach (var audioSource in syncData.nearbyAudioSources)
                {
                    if (audioSource != null && audioSource.isPlaying)
                    {
                        ApplyMaskingToAudioSource(audioSource, currentMaskingStrength);
                    }
                }

                // 迥ｶ諷区峩譁ｰ
                isMaskingActive = currentMaskingStrength > AudioConstants.MIN_AUDIBLE_VOLUME;
                activeMaskingSources = syncData.nearbyAudioSources.Count;
                lastMaskingPosition = syncData.playerPosition;

                // 繧ｹ繝・Ν繧ｹ邉ｻ繧ｷ繧ｹ繝・Β縺ｫ騾夂衍
                if (stealthCoordinator != null)
                {
                    stealthCoordinator.NotifyMaskingEffect(lastMaskingPosition, currentMaskingStrength, maskingRadius);
                }
            }
            else
            {
                isMaskingActive = false;
                activeMaskingSources = 0;
                currentMaskingStrength = 0f;
            }
        }

        #endregion

        #region Editor Helpers

        #if UNITY_EDITOR
        [Button("Test Manual Masking")]
        public void TestManualMasking()
        {
            if (Application.isPlaying && playerTransform != null)
            {
                TriggerMaskingAtPosition(playerTransform.position + Vector3.forward * 5f, 3f, 0.7f);
            }
        }

        [Button("Stop All Masking")]
        public void EditorStopAllMasking()
        {
            if (Application.isPlaying)
            {
                StopAllMaskingEffects();
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (playerTransform == null) return;

            // 繝槭せ繧ｭ繝ｳ繧ｰ遽・峇縺ｮ蜿ｯ隕門喧
            Gizmos.color = isMaskingActive ? Color.yellow : Color.gray;
            Gizmos.DrawWireSphere(playerTransform.position, maskingRadius);

            // 繧｢繧ｯ繝・・ｽ・ｽ繝悶↑繝槭せ繧ｭ繝ｳ繧ｰ繧ｽ繝ｼ繧ｹ縺ｮ蜿ｯ隕門喧
            if (isMaskingActive)
            {
                Gizmos.color = Color.red;
                foreach (var source in trackingAudioSources)
                {
                    if (source.audioSource != null)
                    {
                        Gizmos.DrawWireSphere(source.audioSource.transform.position, 2f);
                        Gizmos.DrawLine(playerTransform.position, source.audioSource.transform.position);
                    }
                }
            }
        }
        #endif

        #endregion
    }
}

