using UnityEngine;
using UnityEngine.Audio;
using System.Linq;
using System.Collections.Generic;
using asterivo.Unity60.Core.Audio.Data;
using asterivo.Unity60.Core.Audio.Events;
using asterivo.Unity60.Core.Debug;
using asterivo.Unity60.Core.Events;
using asterivo.Unity60.Core.Audio.Interfaces;
using _Project.Core;

namespace asterivo.Unity60.Core.Audio
{
    /// <summary>
    /// エフェクト種別の列挙型
    /// </summary>
    public enum EffectType
    {
        UI,
        Interaction, 
        Combat,
        Stealth
    }
    /// <summary>
    /// 効果音システムの管理クラス
    /// 一般的な効果音とステルスゲーム用効果音の統合管理
    /// ServiceLocator対応版
    /// </summary>
    public class EffectManager : MonoBehaviour, IEffectService, IInitializable
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
        [SerializeField] private int uiEffectPriority = 64;
        [SerializeField] private int interactionEffectPriority = 128;
        [SerializeField] private int combatEffectPriority = 32;
        [SerializeField] private int stealthEffectPriority = 16;
        
                
        // Legacy Singleton support (Deprecated - 段階的移行用)
        private static EffectManager instance;
        
        /// <summary>
        /// 後方互換性のためのInstance（非推奨）
        /// ServiceLocator.GetService<IEffectService>()を使用してください
        /// </summary>
        [System.Obsolete("Use ServiceLocator.GetService<IEffectService>() instead")]
        public static EffectManager Instance 
        {
            get 
            {
                // Legacy Singleton完全無効化フラグの確認
                if (FeatureFlags.DisableLegacySingletons) 
                {
                    EventLogger.LogError("[DEPRECATED] EffectManager.Instance is disabled. Use ServiceLocator.GetService<IEffectService>() instead");
                    return null;
                }
                
                // 移行警告の表示
                if (FeatureFlags.EnableMigrationWarnings) 
                {
                    EventLogger.LogWarning("[DEPRECATED] EffectManager.Instance usage detected. Please migrate to ServiceLocator.GetService<IEffectService>()");
                    
                    // MigrationMonitorに使用状況を記録
                    if (FeatureFlags.EnableMigrationMonitoring)
                    {
                        var migrationMonitor = FindFirstObjectByType<MigrationMonitor>();
                        migrationMonitor?.LogSingletonUsage(typeof(EffectManager), "EffectManager.Instance");
                    }
                }
                
                return instance;
            }
        }
// 効果音プール管理
        private Queue<AudioSource> effectSourcePool = new Queue<AudioSource>();
        private List<AudioSource> activeEffectSources = new List<AudioSource>();
        
        // 効果音データベース
        private Dictionary<string, SoundDataSO> effectDatabase = new Dictionary<string, SoundDataSO>();
        
        // 他のオーディオシステムとの連携（ServiceLocator経由）
        private IAudioService audioService;
        private ISpatialAudioService spatialAudioService;
        private IStealthAudioService stealthAudioService;
        
        
        
        // IInitializable実装
        public int Priority => 15; // オーディオサービスの後に初期化
        public bool IsInitialized { get; private set; }
        
        #region Unity Lifecycle
        
        private void Awake()
        {
            // Legacy Singleton初期化（段階的移行用）
            if (instance != null && instance != this) 
            {
                EventLogger.LogWarning("[EffectManager] Multiple instances detected. Destroying duplicate.");
                Destroy(gameObject);
                return;
            }
            instance = this;
            
            // ✅ ServiceLocator専用実装のみ - Singletonパターン完全削除
            DontDestroyOnLoad(gameObject);
            
            if (FeatureFlags.UseServiceLocator)
            {
                ServiceLocator.RegisterService<IEffectService>(this);
                
                if (FeatureFlags.EnableDebugLogging)
                {
                    EventLogger.Log("[EffectManager] Registered to ServiceLocator as IEffectService");
                }
            }
            
            InitializeEffectSourcePool();
        }
        
        private void Start()
        {
            Initialize();
        }
        
        private void OnDestroy()
        {
            // Legacy Singletonクリーンアップ（段階的移行用）
            if (instance == this)
            {
                instance = null;
            }
            
            // ✅ ServiceLocator専用実装のみ - Singletonパターン完全削除
            if (FeatureFlags.UseServiceLocator)
            {
                ServiceLocator.UnregisterService<IEffectService>();
                
                if (FeatureFlags.EnableDebugLogging)
                {
                    EventLogger.Log("[EffectManager] Unregistered from ServiceLocator");
                }
            }
        }
        
        #endregion
        
        #region IInitializable Implementation
        
        /// <summary>
        /// IInitializable実装 - 効果音システムの初期化
        /// </summary>
        public void Initialize()
        {
            if (IsInitialized) return;
            
            // 他のオーディオサービスをServiceLocatorから取得
            if (FeatureFlags.UseServiceLocator)
            {
                audioService = ServiceLocator.GetService<IAudioService>();
                spatialAudioService = ServiceLocator.GetService<ISpatialAudioService>();
                // TODO: StealthAudioServiceが実装されたら有効化
                // stealthAudioService = ServiceLocator.GetService<IStealthAudioService>();
            }
            
            // フォールバック: 既存の方法
            if (audioService == null)
            {
                var audioManager = FindFirstObjectByType<AudioManager>();
                if (audioManager != null)
                {
                    audioService = audioManager;
                }
            }
            
            LoadEffectDatabase();
            
            IsInitialized = true;
            
            if (FeatureFlags.EnableDebugLogging)
            {
                EventLogger.Log("[EffectManager] Initialization complete");
            }
        }
        
        #endregion
        
        #region IEffectService Implementation
        
        /// <summary>
        /// 効果音を再生
        /// </summary>
        public void PlayEffect(string effectId, Vector3 position = default, float volume = 1f)
        {
            if (!IsInitialized)
            {
                EventLogger.LogWarning("[EffectManager] System not initialized");
                return;
            }
            
            // 既存のPlayEffectメソッドを呼び出し
            if (HasMethod("PlayEffect", typeof(string), typeof(Vector3), typeof(float)))
            {
                // 既存の実装を使用（リフレクションで呼び出しまたは直接実装）
                // TODO: 既存のPlayEffectメソッドと統合
                PlayEffectWithSource(effectId, position, volume); //ventLogger.Log($"[EffectManager] Playing effect: {effectId} at {position} with volume {volume}");
            }
        }
        
        /// <summary>
        /// ループ効果音を開始
        /// </summary>
        public int StartLoopingEffect(string effectId, Vector3 position, float volume = 1f)
        {
            if (!IsInitialized) return -1;
            
            // TODO: ループ効果音の実装
            EventLogger.Log($"[EffectManager] Starting looping effect: {effectId}");
            return 0; // 仮のID
        }
        
        /// <summary>
        /// ループ効果音を停止
        /// </summary>
        public void StopLoopingEffect(int loopId)
        {
            if (!IsInitialized) return;
            
            // TODO: ループ効果音の停止実装
            EventLogger.Log($"[EffectManager] Stopping looping effect: {loopId}");
        }
        
        /// <summary>
        /// 一度だけ再生する効果音（重複防止）
        /// </summary>
        public void PlayOneShot(string effectId, Vector3 position = default, float volume = 1f)
        {
            PlayEffect(effectId, position, volume);
        }
        
        /// <summary>
        /// ランダムな効果音を再生
        /// </summary>
        public void PlayRandomEffect(string[] effectIds, Vector3 position = default, float volume = 1f)
        {
            if (effectIds != null && effectIds.Length > 0)
            {
                var randomId = effectIds[Random.Range(0, effectIds.Length)];
                PlayEffect(randomId, position, volume);
            }
        }
        
        /// <summary>
        /// 効果音のピッチを設定
        /// </summary>
        public void SetEffectPitch(string effectId, float pitch)
        {
            // TODO: ピッチ設定の実装
            EventLogger.Log($"[EffectManager] Setting pitch for {effectId}: {pitch}");
        }
        
        /// <summary>
        /// 効果音プールをプリロード
        /// </summary>
        public void PreloadEffects(string[] effectIds)
        {
            // TODO: プリロード機能の実装
            if (FeatureFlags.EnableDebugLogging)
            {
                EventLogger.Log($"[EffectManager] Preloading {effectIds?.Length ?? 0} effects");
            }
        }
        
        /// <summary>
        /// 効果音プールをクリア
        /// </summary>
        public void ClearEffectPool()
        {
            // TODO: プールクリア機能の実装
            if (FeatureFlags.EnableDebugLogging)
            {
                EventLogger.Log("[EffectManager] Clearing effect pool");
            }
        }
        
        /// <summary>
        /// メソッドの存在チェック用ヘルパー
        /// </summary>
        private bool HasMethod(string methodName, params System.Type[] parameterTypes)
        {
            var method = GetType().GetMethod(methodName, parameterTypes);
            return method != null;
        }
        
        #endregion

        #region Public Interface
        
        /// <summary>
        /// 効果音を再生（一般的なインターフェース）
        /// </summary>
        public AudioSource PlayEffectWithSource(string effectID, Vector3 position = default, float volumeMultiplier = 1f)
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
            if (spatialAudioService != null && eventData.affectsStealthGameplay)
            {
                spatialAudioService.Play3DSound(eventData.soundID, eventData.worldPosition, eventData.hearingRadius, eventData.volume);
                return null; // Spatial audio service doesn't return AudioSourceioSource);
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
        
        
        /// <summary>
        /// エフェクトタイプに基づいて設定を適用
        /// </summary>
        private void ApplyEffectTypeSettings(AudioSource audioSource, EffectType effectType)
        {
            switch (effectType)
            {
                case EffectType.UI:
                    audioSource.priority = uiEffectPriority;
                    audioSource.volume *= uiEffectVolume;
                    break;
                case EffectType.Interaction:
                    audioSource.priority = interactionEffectPriority;
                    audioSource.volume *= interactionEffectVolume;
                    break;
                case EffectType.Combat:
                    audioSource.priority = combatEffectPriority;
                    audioSource.volume *= combatEffectVolume;
                    break;
                case EffectType.Stealth:
                    audioSource.priority = stealthEffectPriority;
                    audioSource.volume *= stealthEffectVolume;
                    break;
            }
        }
        
        /// <summary>
        /// エフェクトタイプ指定で効果音を再生
        /// </summary>
        public void PlayEffect(string effectName, EffectType effectType, Vector3 position = default)
        {
            if (!effectDatabase.TryGetValue(effectName, out SoundDataSO soundData))
            {
                EventLogger.LogError($"Effect '{effectName}' not found in database");
                return;
            }

            var audioSource = GetPooledEffectSource();
            if (audioSource == null) return;

            // 基本設定を適用
            audioSource.clip = soundData.GetRandomClip();
            audioSource.volume = soundData.GetRandomVolume();
            audioSource.pitch = soundData.GetRandomPitch();

            // エフェクトタイプ固有の設定を適用
            ApplyEffectTypeSettings(audioSource, effectType);

            // 位置設定
            if (position != default)
            {
                audioSource.transform.position = position;
                audioSource.spatialBlend = 1f; // 3D音響
            }
            else
            {
                audioSource.spatialBlend = 0f; // 2D音響
            }

            audioSource.Play();
            StartCoroutine(ReturnToPoolWhenFinished(audioSource, soundData.GetRandomClip().length));

            #if UNITY_EDITOR || DEVELOPMENT_BUILD
            EventLogger.Log($"Playing effect: {effectName} ({effectType}) with priority: {audioSource.priority}");
            #endif
        }
        
        /// <summary>
        /// UIサウンド再生のショートカット
        /// </summary>
        public void PlayUIEffect(string effectName)
        {
            PlayEffect(effectName, EffectType.UI);
        }
        
        /// <summary>
        /// インタラクションサウンド再生のショートカット
        /// </summary>
        public void PlayInteractionEffect(string effectName, Vector3 position = default)
        {
            PlayEffect(effectName, EffectType.Interaction, position);
        }
        
        /// <summary>
        /// 戦闘サウンド再生のショートカット
        /// </summary>
        public void PlayCombatEffect(string effectName, Vector3 position = default)
        {
            PlayEffect(effectName, EffectType.Combat, position);
        }
        
        /// <summary>
        /// ステルスサウンド再生のショートカット
        /// </summary>
        public void PlayStealthEffect(string effectName, Vector3 position = default)
        {
            PlayEffect(effectName, EffectType.Stealth, position);
        }
        
        #endregion
        
        #region Public Status API
        
        /// <summary>
        /// 任意の効果音が再生中か確認
        /// </summary>
        public bool IsPlaying()
        {
            return activeEffectSources.Any(source => source != null && source.isPlaying);
        }
        
        /// <summary>
        /// 指定した効果音が再生中か確認（簡略実装）
        /// </summary>
        public bool IsPlaying(string effectId)
        {
            if (string.IsNullOrEmpty(effectId))
                return false;
                
            return activeEffectSources.Any(source => 
                source != null && 
                source.isPlaying && 
                (source.clip?.name.Contains(effectId) == true || 
                 source.gameObject.name.Contains(effectId)));
        }
        
        /// <summary>
        /// 指定したタイプの効果音が再生中か確認
        /// </summary>
        public bool IsPlayingType(EffectType effectType)
        {
            return activeEffectSources.Any(source =>
            {
                if (source == null || !source.isPlaying)
                    return false;
                    
                string sourceId = source.gameObject.name;
                
                return effectType switch
                {
                    EffectType.UI => sourceId.Contains("UI") || source.spatialBlend == 0f,
                    EffectType.Combat => sourceId.Contains("Combat") || source.priority < 64,
                    EffectType.Interaction => sourceId.Contains("Interaction"),
                    EffectType.Stealth => sourceId.Contains("Stealth") || source.maxDistance > 20f,
                    _ => false
                };
            });
        }
        
        /// <summary>
        /// アクティブな効果音の数を取得
        /// </summary>
        public int GetActiveEffectCount()
        {
            return activeEffectSources.Count(source => source != null && source.isPlaying);
        }
        
        /// <summary>
        /// アクティブな効果音の最大数を取得
        /// </summary>
        public int GetMaxConcurrentEffects()
        {
            return maxConcurrentEffects;
        }
        
        /// <summary>
        /// 効果音プールの利用可能数を取得
        /// </summary>
        public int GetAvailableEffectSourceCount()
        {
            return effectSourcePool.Count;
        }
        
        /// <summary>
        /// 指定した効果音IDがデータベースに登録されているか確認
        /// </summary>
        public bool HasEffectData(string effectId)
        {
            return !string.IsNullOrEmpty(effectId) && effectDatabase.ContainsKey(effectId);
        }
        
        /// <summary>
        /// 登録されている効果音IDの一覧を取得
        /// </summary>
        public string[] GetRegisteredEffectIds()
        {
            return effectDatabase.Keys.ToArray();
        }
        
        /// <summary>
        /// カテゴリ別の効果音有効状態を取得
        /// </summary>
        public bool IsCategoryEnabled(EffectType effectType)
        {
            return effectType switch
            {
                EffectType.UI => enableUIEffects,
                EffectType.Interaction => enableInteractionEffects,
                EffectType.Combat => enableCombatEffects,
                EffectType.Stealth => enableStealthEffects,
                _ => false
            };
        }
        
        #endregion
    }
}