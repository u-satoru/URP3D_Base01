using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using asterivo.Unity60.Core.Audio.Data;
using asterivo.Unity60.Core.Audio.Events;
using asterivo.Unity60.Core.Events;
// using asterivo.Unity60.Core.Debug;
using asterivo.Unity60.Core;
// using asterivo.Unity60.Core.Shared;
using Sirenix.OdinInspector;

namespace asterivo.Unity60.Core.Audio.Controllers
{
    /// <summary>
    /// スチE��スゲーム用マスキング効果制御シスチE��
    /// AmbientManagerから刁E��された�Eスキング効果専用コントローラー
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

        // シスチE��参�E
        private Transform playerTransform;
        private AudioListener audioListener;
        private StealthAudioCoordinator stealthCoordinator;

        // マスキング管理
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
        /// マスキングコントローラーの初期匁E        /// </summary>
        private void InitializeMaskingController()
        {
            trackingAudioSources = new List<MaskingAudioSource>();
            maskingStrengthCache = new Dictionary<AudioSource, float>();
            
            EventLogger.LogStatic("<color=cyan>[MaskingEffectController]</color> Masking effect controller initialized");
        }

        /// <summary>
        /// シスチE��参�Eの検索
        /// </summary>
        private void FindSystemReferences()
        {
            // AudioListenerを検索
            audioListener = FindFirstObjectByType<AudioListener>();
            if (audioListener != null)
            {
                playerTransform = audioListener.transform;
            }

            // StealthAudioCoordinatorを検索
            stealthCoordinator = FindFirstObjectByType<StealthAudioCoordinator>();
            
            if (playerTransform == null)
            {
                ServiceLocator.GetService<IEventLogger>().LogWarning("[MaskingEffectController] Player transform not found! Masking effects may not work properly.");
            }
        }

        #endregion

        #region Public Interface

        /// <summary>
        /// グローバルマスキング強度の設定
        /// </summary>
        public void SetGlobalMaskingStrength(float strength)
        {
            globalMaskingStrength = Mathf.Clamp01(strength);
            EventLogger.LogStatic($"<color=cyan>[MaskingEffectController]</color> Global masking strength set to {globalMaskingStrength:F2}");
        }

        /// <summary>
        /// マスキング半径の設定
        /// </summary>
        public void SetMaskingRadius(float radius)
        {
            maskingRadius = Mathf.Max(0f, radius);
        }

        /// <summary>
        /// 動的マスキングの有効/無効
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
        /// 特定位置でのマスキング効果を計箁E        /// </summary>
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
        /// AudioSourceにマスキング効果を適用
        /// </summary>
        public bool ApplyMaskingToAudioSource(AudioSource audioSource, float customMaskingStrength = -1f)
        {
            if (audioSource == null || playerTransform == null)
                return false;

            float maskingStrength = customMaskingStrength >= 0f ? customMaskingStrength : 
                CalculateMaskingAtPosition(audioSource.transform.position);

            if (maskingStrength > AudioConstants.MIN_AUDIBLE_VOLUME)
            {
                // マスキング効果を音量に反映
                float originalVolume = GetOriginalVolume(audioSource);
                float maskedVolume = originalVolume * (1f - maskingStrength);
                audioSource.volume = maskedVolume;

                // キャチE��ュを更新
                maskingStrengthCache[audioSource] = maskingStrength;
                return true;
            }

            return false;
        }

        /// <summary>
        /// 手動でマスキング効果をトリガー
        /// </summary>
        public void TriggerMaskingAtPosition(Vector3 position, float duration = 2f, float strength = 1f)
        {
            StartCoroutine(ManualMaskingCoroutine(position, duration, strength));
        }

        /// <summary>
        /// 全マスキング効果�E停止
        /// </summary>
        public void StopAllMaskingEffects()
        {
            StopMaskingDetection();
            
            // 全AudioSourceのマスキング効果をリセチE��
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
        /// マスキング検�Eの開姁E        /// </summary>
        private void StartMaskingDetection()
        {
            // ※ServiceLocator専用実装 - IAudioUpdateServiceを取得
            if (asterivo.Unity60.Core.FeatureFlags.UseServiceLocator)
            {
                try
                {
                    var audioUpdateService = asterivo.Unity60.Core.ServiceLocator.GetService<asterivo.Unity60.Core.Audio.Interfaces.IAudioUpdateService>();
                    if (audioUpdateService is AudioUpdateCoordinator coordinator && coordinator.enabled)
                    {
                        // 協調更新シスチE��に登録�E�イベント駁E���E�E                        coordinator.OnAudioSystemSync += OnAudioSystemSync;
                        EventLogger.LogStatic("<color=cyan>[MaskingEffectController]</color> Registered with AudioUpdateCoordinator via ServiceLocator");
                        return;
                    }
                }
                catch (System.Exception ex)
                {
                    ServiceLocator.GetService<IEventLogger>().LogError($"[MaskingEffectController] Failed to get IAudioUpdateService from ServiceLocator: {ex.Message}");
                }
            }
            
            // フォールバック: 直接検索
            var fallbackCoordinator = FindFirstObjectByType<AudioUpdateCoordinator>();
            if (fallbackCoordinator != null && fallbackCoordinator.enabled)
            {
                fallbackCoordinator.OnAudioSystemSync += OnAudioSystemSync;
                EventLogger.LogStatic("<color=cyan>[MaskingEffectController]</color> Registered with AudioUpdateCoordinator via fallback");
                return;
            }
            
            // フォールバック�E�従来の検�EシスチE��
            if (maskingUpdateCoroutine == null)
            {
                maskingUpdateCoroutine = StartCoroutine(MaskingDetectionCoroutine());
            }
        }

        /// <summary>
        /// マスキング検�Eの停止
        /// </summary>
        private void StopMaskingDetection()
        {
            // ※ServiceLocator専用実装 - IAudioUpdateServiceからの登録解除
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
            
            // フォールバック: 直接検索での登録解除
            var fallbackCoordinator = FindFirstObjectByType<AudioUpdateCoordinator>();
            if (fallbackCoordinator != null)
            {
                fallbackCoordinator.OnAudioSystemSync -= OnAudioSystemSync;
            }
            
            // 従来のコルーチンの停止
            if (maskingUpdateCoroutine != null)
            {
                StopCoroutine(maskingUpdateCoroutine);
                maskingUpdateCoroutine = null;
            }
        }

        /// <summary>
        /// マスキング検�Eのメインコルーチン
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
        /// マスキング対象AudioSourceの更新
        /// </summary>
        private void UpdateMaskingAudioSources()
        {
            if (playerTransform == null) return;

            // 付近のAudioSourceを検索
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
        /// AudioSourceが�Eスキング対象として有効かチェチE��
        /// </summary>
        private bool IsValidMaskingSource(AudioSource audioSource)
        {
            // タグチェチE��
            bool hasValidTag = maskingAudioTags.Length == 0 || maskingAudioTags.Contains(audioSource.tag);
            
            // レイヤーチェチE��
            bool hasValidLayer = ((1 << audioSource.gameObject.layer) & stealthSoundLayerMask) != 0;
            
            // 音量チェチE���E�非常に小さぁE��は除外！E            bool hasAudibleVolume = audioSource.volume > AudioConstants.MIN_AUDIBLE_VOLUME;
            
            return hasValidTag && hasValidLayer && hasAudibleVolume;
        }

        /// <summary>
        /// マスキング効果の処理
        /// </summary>
        private void ProcessMaskingEffects()
        {
            bool wasActive = isMaskingActive;
            currentMaskingStrength = 0f;

            if (trackingAudioSources.Count > 0)
            {
                isMaskingActive = true;
                
                // 最も強ぁE�Eスキング効果を取征E                currentMaskingStrength = trackingAudioSources.Max(source => source.maskingStrength);
                lastMaskingPosition = trackingAudioSources
                    .OrderByDescending(source => source.maskingStrength)
                    .First().audioSource.transform.position;

                // ステルス系システムに通知
                if (stealthCoordinator != null)
                {
                    stealthCoordinator.NotifyMaskingEffect(lastMaskingPosition, currentMaskingStrength, maskingRadius);
                }
            }
            else
            {
                isMaskingActive = false;
            }

            // 状態変更時�Eイベント発火
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

            // マスキング音響検�EイベンチE            if (isMaskingActive && maskingSoundDetectedEvent != null)
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
        /// 手動マスキング効果�Eコルーチン
        /// </summary>
        private IEnumerator ManualMaskingCoroutine(Vector3 position, float duration, float strength)
        {
            EventLogger.LogStatic($"<color=cyan>[MaskingEffectController]</color> Manual masking triggered at {position} for {duration}s");

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
        /// AudioSourceの允E�E音量を取征E        /// </summary>
        private float GetOriginalVolume(AudioSource audioSource)
        {
            // 允E�E音量�E保存してぁE��ぁE��め、現在の音量を基準とする
            // 実際のプロジェクトでは、AudioSourceの允E��ータを保持する仕絁E��が忁E��E            return Mathf.Max(audioSource.volume, 0.1f);
        }

        /// <summary>
        /// AudioSourceの音量を允E��戻ぁE        /// </summary>
        private void RestoreOriginalVolume(AudioSource audioSource)
        {
            // 実際の元の音量データ保持方法に依存
            // ここでは簡単な復元処理
            if (maskingStrengthCache.ContainsKey(audioSource))
            {
                maskingStrengthCache.Remove(audioSource);
            }
        }

        #endregion

        #region Helper Classes

        /// <summary>
        /// マスキング対象AudioSource情報
        /// </summary>
        [System.Serializable]
        private class MaskingAudioSource
        {
            public AudioSource audioSource;
            public float distance;
            public float maskingStrength;
        }

        /// <summary>
        /// AudioUpdateCoordinatorからの同期コールバック
        /// </summary>
        private void OnAudioSystemSync(AudioSystemSyncData syncData)
        {
            // 効率的なマスキング処理に既に最適化されたデータを使用
            if (syncData.nearbyAudioSources.Count > 0)
            {
                currentMaskingStrength = syncData.currentMaskingStrength;
                
                // バッチ処理でマスキング効果を適用
                foreach (var audioSource in syncData.nearbyAudioSources)
                {
                    if (audioSource != null && audioSource.isPlaying)
                    {
                        ApplyMaskingToAudioSource(audioSource, currentMaskingStrength);
                    }
                }

                // 状態更新
                isMaskingActive = currentMaskingStrength > AudioConstants.MIN_AUDIBLE_VOLUME;
                activeMaskingSources = syncData.nearbyAudioSources.Count;
                lastMaskingPosition = syncData.playerPosition;

                // ステルス系システムに通知
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

            // マスキング範囲の可視化
            Gizmos.color = isMaskingActive ? Color.yellow : Color.gray;
            Gizmos.DrawWireSphere(playerTransform.position, maskingRadius);

            // アクチE��ブなマスキングソースの可視化
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