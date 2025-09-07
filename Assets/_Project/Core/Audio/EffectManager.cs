using UnityEngine;
using UnityEngine.Audio;
using System.Collections.Generic;
using asterivo.Unity60.Core.Audio.Data;
using asterivo.Unity60.Core.Audio.Events;
using asterivo.Unity60.Core.Debug;
using asterivo.Unity60.Core.Events;

namespace asterivo.Unity60.Core.Audio
{
    /// <summary>
    /// 効果音システムの管理クラス
    /// 一般的な効果音とステルスゲーム用効果音の統合管理
    /// </summary>
    public class EffectManager : MonoBehaviour
    {
        [Header("Effect Manager Settings")]
        [SerializeField] private int maxConcurrentEffects = 16;
        [SerializeField] private AudioMixerGroup effectMixerGroup;
        
        [Header("Effect Categories")]
        [SerializeField] private bool enableUIEffects = true;
        [SerializeField] private bool enableInteractionEffects = true;
        [SerializeField] private bool enableCombatEffects = true;
        [SerializeField] private bool enableStealthEffects = true;
        
        [Header("Volume Control")]
        [SerializeField] private float uiEffectVolume = 0.8f;
        [SerializeField] private float interactionEffectVolume = 1.0f;
        [SerializeField] private float combatEffectVolume = 1.2f;
        [SerializeField] private float stealthEffectVolume = 0.7f;
        
        [Header("Priority Settings")]
        // TODO: ApplyEffectTypeSettings内でAudioSource.priorityの設定に使用予定
        [SerializeField] private int uiEffectPriority = 64;
        [SerializeField] private int interactionEffectPriority = 128;
        [SerializeField] private int combatEffectPriority = 32;
        [SerializeField] private int stealthEffectPriority = 16;
        
        // 効果音プール管理
        private Queue<AudioSource> effectSourcePool = new Queue<AudioSource>();
        private List<AudioSource> activeEffectSources = new List<AudioSource>();
        
        // 効果音データベース
        private Dictionary<string, SoundDataSO> effectDatabase = new Dictionary<string, SoundDataSO>();
        
        // 他のオーディオシステムとの連携
        private AudioManager audioManager;
        private SpatialAudioManager spatialAudioManager;
        private StealthAudioCoordinator stealthCoordinator;
        
        private static EffectManager instance;
        public static EffectManager Instance => instance;
        
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
            
            InitializeEffectSourcePool();
        }
        
        private void Start()
        {
            // 他のオーディオシステムとの連携を確立
            audioManager = AudioManager.Instance;
            spatialAudioManager = SpatialAudioManager.Instance;
            stealthCoordinator = StealthAudioCoordinator.Instance;
            
            LoadEffectDatabase();
        }
        
        private void OnDestroy()
        {
            if (instance == this)
            {
                instance = null;
            }
        }
        
        #endregion
        
        #region Public Interface
        
        /// <summary>
        /// 効果音を再生（一般的なインターフェース）
        /// </summary>
        public AudioSource PlayEffect(string effectID, Vector3 position = default, float volumeMultiplier = 1f)
        {
            if (!effectDatabase.ContainsKey(effectID))
            {
                EventLogger.LogWarning($"[EffectManager] Effect '{effectID}' not found in database");
                return null;
            }
            
            var soundData = effectDatabase[effectID];
            var eventData = AudioEventData.CreateEffectDefault(effectID);
            eventData.worldPosition = position;
            eventData.volume *= volumeMultiplier;
            
            return PlayEffectInternal(soundData, eventData);
        }
        
        /// <summary>
        /// UI効果音の再生
        /// </summary>
        public AudioSource PlayUIEffect(string effectID, float volumeMultiplier = 1f)
        {
            if (!enableUIEffects) return null;
            
            var eventData = AudioEventData.CreateUIDefault(effectID);
            eventData.volume = uiEffectVolume * volumeMultiplier;
            
            return PlayCategorizedEffect(effectID, eventData, EffectType.UI);
        }
        
        /// <summary>
        /// インタラクション効果音の再生
        /// </summary>
        public AudioSource PlayInteractionEffect(string effectID, Vector3 position, float volumeMultiplier = 1f)
        {
            if (!enableInteractionEffects) return null;
            
            var eventData = AudioEventData.CreateEffectDefault(effectID);
            eventData.worldPosition = position;
            eventData.volume = interactionEffectVolume * volumeMultiplier;
            eventData.affectsStealthGameplay = true;
            
            return PlayCategorizedEffect(effectID, eventData, EffectType.Interaction);
        }
        
        /// <summary>
        /// 戦闘効果音の再生
        /// </summary>
        public AudioSource PlayCombatEffect(string effectID, Vector3 position, float volumeMultiplier = 1f)
        {
            if (!enableCombatEffects) return null;
            
            var eventData = AudioEventData.CreateEffectDefault(effectID);
            eventData.worldPosition = position;
            eventData.volume = combatEffectVolume * volumeMultiplier;
            eventData.priority = 0.9f; // 高優先度
            eventData.affectsStealthGameplay = true;
            eventData.layerPriority = 80;
            
            return PlayCategorizedEffect(effectID, eventData, EffectType.Combat);
        }
        
        /// <summary>
        /// ステルス効果音の再生
        /// </summary>
        public AudioSource PlayStealthEffect(string effectID, Vector3 position, float hearingRadius, 
            SurfaceMaterial surface = SurfaceMaterial.Default, float volumeMultiplier = 1f)
        {
            if (!enableStealthEffects) return null;
            
            var eventData = AudioEventData.CreateStealthDefault(effectID);
            eventData.worldPosition = position;
            eventData.volume = stealthEffectVolume * volumeMultiplier;
            eventData.hearingRadius = hearingRadius;
            eventData.surfaceType = surface;
            
            return PlayCategorizedEffect(effectID, eventData, EffectType.Stealth);
        }
        
        /// <summary>
        /// すべての効果音を停止
        /// </summary>
        public void StopAllEffects()
        {
            foreach (var source in activeEffectSources.ToArray())
            {
                if (source != null && source.isPlaying)
                {
                    ReturnToPool(source);
                }
            }
        }
        
        /// <summary>
        /// 特定のカテゴリの効果音を停止
        /// </summary>
        public void StopEffectsByType(EffectType effectType)
        {
            // この実装では簡略化のため、全体停止を行う
            // 実際の実装では、各AudioSourceにタグを付けて分類する必要がある
            foreach (var source in activeEffectSources.ToArray())
            {
                if (source != null && source.isPlaying)
                {
                    // 効果音タイプによる判定（AudioSourceの名前またはタグで識別）
                    string sourceId = source.gameObject.name;
                    bool shouldStop = false;
                    
                    switch (effectType)
                    {
                        case EffectType.UI:
                            shouldStop = sourceId.Contains("UI") || source.spatialBlend == 0f;
                            break;
                        case EffectType.Combat:
                            shouldStop = sourceId.Contains("Combat") || source.priority < 64;
                            break;
                        case EffectType.Interaction:
                            shouldStop = sourceId.Contains("Interaction");
                            break;
                        case EffectType.Stealth:
                            shouldStop = sourceId.Contains("Stealth") || source.maxDistance > 20f;
                            break;
                    }
                    
                    if (shouldStop)
                    {
                        ReturnToPool(source);
                    }
                }
            }
        }
        
        #endregion
        
        #region Private Methods
        
        /// <summary>
        /// 効果音プールの初期化
        /// </summary>
        private void InitializeEffectSourcePool()
        {
            for (int i = 0; i < maxConcurrentEffects; i++)
            {
                var go = new GameObject($"PooledEffectSource_{i}");
                go.transform.SetParent(transform);
                
                var audioSource = go.AddComponent<AudioSource>();
                audioSource.playOnAwake = false;
                audioSource.spatialBlend = 1f; // デフォルトは3D
                audioSource.outputAudioMixerGroup = effectMixerGroup;
                
                effectSourcePool.Enqueue(audioSource);
            }
        }
        
        /// <summary>
        /// 効果音データベースの読み込み
        /// 複数のパスから効果音データを収集し、カテゴリ別に整理します
        /// </summary>
        private void LoadEffectDatabase()
        {
            effectDatabase.Clear();
            
            // 複数のResourcesパスから効果音を読み込み
            string[] resourcePaths = {
                "Audio/Effects",
                "Audio/Effects/UI",
                "Audio/Effects/Combat",
                "Audio/Effects/Interaction",
                "Audio/Effects/Stealth",
                "Audio/Effects/Environment"
            };
            
            int totalLoaded = 0;
            
            foreach (string path in resourcePaths)
            {
                var effectSounds = Resources.LoadAll<SoundDataSO>(path);
                foreach (var sound in effectSounds)
                {
                    if (sound != null)
                    {
                        // 重複チェック（異なるパスに同名ファイルがある場合の処理）
                        if (effectDatabase.ContainsKey(sound.name))
                        {
                            EventLogger.LogWarning($"[EffectManager] Duplicate effect sound found: {sound.name} in {path}");
                            continue;
                        }
                        
                        effectDatabase[sound.name] = sound;
                        totalLoaded++;
                    }
                }
            }
            
            // ScriptableObjectsフォルダからも検索（プロジェクト固有のサウンドデータ）
            var customEffects = Resources.LoadAll<SoundDataSO>("ScriptableObjects/Audio/Effects");
            foreach (var effect in customEffects)
            {
                if (effect != null && !effectDatabase.ContainsKey(effect.name))
                {
                    effectDatabase[effect.name] = effect;
                    totalLoaded++;
                }
            }
            
            // デフォルト効果音の作成（必要最小限のサウンド）
            CreateDefaultEffectsIfNeeded();
            
            EventLogger.Log($"[EffectManager] Loaded {totalLoaded} effect sounds from Resources. " +
                          $"Total effects in database: {effectDatabase.Count}");
                          
            // デバッグ情報：利用可能な効果音リストを出力
            #if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (Application.isEditor)
            {
                System.Text.StringBuilder sb = new System.Text.StringBuilder();
                sb.AppendLine("[EffectManager] Available Effects:");
                foreach (var kvp in effectDatabase)
                {
                    sb.AppendLine($"  - {kvp.Key}");
                }
                EventLogger.Log(sb.ToString());
            }
            #endif
        }
        
        /// <summary>
        /// 基本的な効果音が見つからない場合にデフォルトを作成
        /// </summary>
        private void CreateDefaultEffectsIfNeeded()
        {
            // 必須の効果音IDリスト
            string[] requiredEffects = {
                "ui_click",
                "ui_hover",
                "footstep_default",
                "item_pickup",
                "door_open",
                "door_close"
            };
            
            int created = 0;
            foreach (string effectId in requiredEffects)
            {
                if (!effectDatabase.ContainsKey(effectId))
                {
                    // デフォルトのSoundDataSOを動的作成（実行時のみ）
                    var defaultSound = ScriptableObject.CreateInstance<SoundDataSO>();
                    defaultSound.name = effectId;
                    // 他のデフォルト設定は SoundDataSO の初期値を使用
                    
                    effectDatabase[effectId] = defaultSound;
                    created++;
                    
                    EventLogger.LogWarning($"[EffectManager] Created default effect: {effectId}");
                }
            }
            
            if (created > 0)
            {
                EventLogger.LogWarning($"[EffectManager] Created {created} default effects. " +
                                     "Consider adding proper SoundDataSO assets for these effects.");
            }
        }
        
        /// <summary>
        /// カテゴリ別効果音再生の内部処理
        /// </summary>
        private AudioSource PlayCategorizedEffect(string effectID, AudioEventData eventData, EffectType effectType)
        {
            if (!effectDatabase.ContainsKey(effectID))
            {
                EventLogger.LogWarning($"[EffectManager] Effect '{effectID}' not found in database");
                return null;
            }
            
            var soundData = effectDatabase[effectID];
            
            // 効果音タイプに応じた追加設定
            ApplyEffectTypeSettings(eventData, effectType);
            
            return PlayEffectInternal(soundData, eventData);
        }
        
        /// <summary>
        /// 効果音再生の内部処理
        /// </summary>
        private AudioSource PlayEffectInternal(SoundDataSO soundData, AudioEventData eventData)
        {
            // ステルス音響システムがある場合は、そちらに委譲
            if (spatialAudioManager != null && eventData.affectsStealthGameplay)
            {
                return spatialAudioManager.PlaySoundFromEvent(eventData, soundData);
            }
            
            // 通常の効果音再生処理
            var audioSource = GetPooledEffectSource();
            if (audioSource == null) return null;
            
            SetupEffectSource(audioSource, soundData, eventData);
            
            var clip = soundData.GetRandomClip();
            if (clip != null)
            {
                audioSource.clip = clip;
                audioSource.Play();
                
                StartCoroutine(ReturnToPoolWhenFinished(audioSource, clip.length));
            }
            
            return audioSource;
        }
        
        /// <summary>
        /// 効果音タイプに応じた設定の適用
        /// </summary>
        private void ApplyEffectTypeSettings(AudioEventData eventData, EffectType effectType)
        {
            switch (effectType)
            {
                case EffectType.UI:
                    eventData.use3D = false;
                    eventData.affectsStealthGameplay = false;
                    eventData.layerPriority = 5;
                    break;
                    
                case EffectType.Interaction:
                    eventData.use3D = true;
                    eventData.affectsStealthGameplay = true;
                    eventData.layerPriority = 60;
                    break;
                    
                case EffectType.Combat:
                    eventData.use3D = true;
                    eventData.affectsStealthGameplay = true;
                    eventData.layerPriority = 80;
                    eventData.priority = 0.9f;
                    break;
                    
                case EffectType.Stealth:
                    eventData.use3D = true;
                    eventData.affectsStealthGameplay = true;
                    eventData.layerPriority = 100;
                    eventData.priority = 0.8f;
                    break;
            }
        }
        
        /// <summary>
        /// プールからエフェクトソースを取得
        /// </summary>
        private AudioSource GetPooledEffectSource()
        {
            if (effectSourcePool.Count > 0)
            {
                var audioSource = effectSourcePool.Dequeue();
                activeEffectSources.Add(audioSource);
                return audioSource;
            }
            
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            UnityEngine.Debug.LogWarning("[EffectManager] Effect source pool exhausted");
#endif
            return null;
        }
        
        /// <summary>
        /// エフェクトソースをプールに返却
        /// </summary>
        private void ReturnToPool(AudioSource audioSource)
        {
            if (audioSource == null) return;
            
            audioSource.Stop();
            audioSource.clip = null;
            
            activeEffectSources.Remove(audioSource);
            effectSourcePool.Enqueue(audioSource);
        }
        
        /// <summary>
        /// エフェクトソースの設定
        /// </summary>
        private void SetupEffectSource(AudioSource audioSource, SoundDataSO soundData, AudioEventData eventData)
        {
            audioSource.transform.position = eventData.worldPosition;
            audioSource.volume = eventData.volume;
            audioSource.pitch = eventData.pitch;
            
            if (eventData.use3D)
            {
                audioSource.spatialBlend = soundData.Is3D ? soundData.SpatialBlend : 1f;
                audioSource.minDistance = soundData.MinDistance;
                audioSource.maxDistance = soundData.MaxDistance;
                audioSource.rolloffMode = soundData.RolloffMode;
            }
            else
            {
                audioSource.spatialBlend = 0f; // 2D音響
            }
            
            // 優先度設定
            int unityPriority = Mathf.RoundToInt((1f - eventData.priority) * 256f);
            audioSource.priority = Mathf.Clamp(unityPriority, 0, 256);
        }
        
        /// <summary>
        /// 再生終了後にプールに返却するコルーチン
        /// </summary>
        private System.Collections.IEnumerator ReturnToPoolWhenFinished(AudioSource audioSource, float clipLength)
        {
            yield return new WaitForSeconds(clipLength + 0.1f);
            ReturnToPool(audioSource);
        }
        
        #endregion
    }
    
    /// <summary>
    /// 効果音の種類を定義
    /// </summary>
    public enum EffectType
    {
        UI,           // UIサウンド
        Interaction,  // インタラクション音
        Combat,       // 戦闘音
        Stealth       // ステルス音
    }
}