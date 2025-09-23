using UnityEngine;
using UnityEngine.Audio;
using System.Linq;
using System.Collections.Generic;
using asterivo.Unity60.Core.Audio.Data;
using asterivo.Unity60.Core.Audio.Events;
// using asterivo.Unity60.Core.Debug;
using asterivo.Unity60.Core.Events;
using asterivo.Unity60.Core;
using asterivo.Unity60.Core.Audio.Interfaces;


namespace asterivo.Unity60.Core.Audio
{
    /// <summary>
    /// エフェクト種別の列挙垁E    /// </summary>
    public enum EffectType
    {
        UI,
        Interaction, 
        Combat,
        Stealth
    }
    /// <summary>
    /// 効果音シスチE��の管琁E��ラス
    /// 一般皁E��効果音とスチE��スゲーム用効果音の統合管琁E    /// ServiceLocator対応版
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
        
                
// 効果音プ�Eル管琁E        private Queue<AudioSource> effectSourcePool = new Queue<AudioSource>();
        private List<AudioSource> activeEffectSources = new List<AudioSource>();
        
        // 効果音チE�Eタベ�Eス
        private Dictionary<string, SoundDataSO> effectDatabase = new Dictionary<string, SoundDataSO>();
        
        // 他�EオーチE��オシスチE��との連携�E�EerviceLocator経由�E�E        private IAudioService audioService;
        private ISpatialAudioService spatialAudioService;
        private IStealthAudioService stealthAudioService;
        
        
        
        // IInitializable実裁E        public int Priority => 15; // オーチE��オサービスの後に初期匁E        public bool IsInitialized { get; private set; }
        
        #region Unity Lifecycle
        
        private void Awake()
        {
            // ✁EServiceLocator専用実裁E�Eみ - Singletonパターン完�E削除
            DontDestroyOnLoad(gameObject);
            
            if (FeatureFlags.UseServiceLocator)
            {
                ServiceLocator.RegisterService<IEffectService>(this);
                
                if (FeatureFlags.EnableDebugLogging)
                {
                    EventLogger.LogStatic("[EffectManager] Registered to ServiceLocator as IEffectService");
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
            // ✁EServiceLocator専用実裁E�Eみ - Singletonパターン完�E削除
            if (FeatureFlags.UseServiceLocator)
            {
                ServiceLocator.UnregisterService<IEffectService>();
                
                if (FeatureFlags.EnableDebugLogging)
                {
                    EventLogger.LogStatic("[EffectManager] Unregistered from ServiceLocator");
                }
            }
        }
        
        #endregion
        
        #region IInitializable Implementation
        
        /// <summary>
        /// IInitializable実裁E- 効果音シスチE��の初期匁E        /// </summary>
        public void Initialize()
        {
            if (IsInitialized) return;
            
            // 他�EオーチE��オサービスをServiceLocatorから取征E            if (FeatureFlags.UseServiceLocator)
            {
                audioService = ServiceLocator.GetService<IAudioService>();
                spatialAudioService = ServiceLocator.GetService<ISpatialAudioService>();
                // TODO: StealthAudioServiceが実裁E��れたら有効匁E                // stealthAudioService = ServiceLocator.GetService<IStealthAudioService>();
            }
            
            // フォールバック: 既存�E方況E            if (audioService == null)
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
                EventLogger.LogStatic("[EffectManager] Initialization complete");
            }
        }
        
        #endregion
        
        #region IEffectService Implementation
        
        /// <summary>
        /// 効果音を�E甁E        /// </summary>
        public void PlayEffect(string effectId, Vector3 position = default, float volume = 1f)
        {
            if (!IsInitialized)
            {
                ServiceLocator.GetService<IEventLogger>().LogWarning("[EffectManager] System not initialized");
                return;
            }
            
            // 既存�EPlayEffectメソチE��を呼び出ぁE            if (HasMethod("PlayEffect", typeof(string), typeof(Vector3), typeof(float)))
            {
                // 既存�E実裁E��使用�E�リフレクションで呼び出しまた�E直接実裁E��E                // TODO: 既存�EPlayEffectメソチE��と統吁E                PlayEffectWithSource(effectId, position, volume); //ventLogger.Log($"[EffectManager] Playing effect: {effectId} at {position} with volume {volume}");
            }
        }
        
        /// <summary>
        /// ループ効果音を開姁E        /// </summary>
        public int StartLoopingEffect(string effectId, Vector3 position, float volume = 1f)
        {
            if (!IsInitialized) return -1;
            
            // TODO: ループ効果音の実裁E            EventLogger.LogStatic($"[EffectManager] Starting looping effect: {effectId}");
            return 0; // 仮のID
        }
        
        /// <summary>
        /// ループ効果音を停止
        /// </summary>
        public void StopLoopingEffect(int loopId)
        {
            if (!IsInitialized) return;
            
            // TODO: ループ効果音の停止実裁E            EventLogger.LogStatic($"[EffectManager] Stopping looping effect: {loopId}");
        }
        
        /// <summary>
        /// 一度だけ�E生する効果音�E�重褁E��止�E�E        /// </summary>
        public void PlayOneShot(string effectId, Vector3 position = default, float volume = 1f)
        {
            PlayEffect(effectId, position, volume);
        }
        
        /// <summary>
        /// ランダムな効果音を�E甁E        /// </summary>
        public void PlayRandomEffect(string[] effectIds, Vector3 position = default, float volume = 1f)
        {
            if (effectIds != null && effectIds.Length > 0)
            {
                var randomId = effectIds[Random.Range(0, effectIds.Length)];
                PlayEffect(randomId, position, volume);
            }
        }
        
        /// <summary>
        /// 効果音のピッチを設宁E        /// </summary>
        public void SetEffectPitch(string effectId, float pitch)
        {
            // TODO: ピッチ設定�E実裁E            EventLogger.LogStatic($"[EffectManager] Setting pitch for {effectId}: {pitch}");
        }
        
        /// <summary>
        /// 効果音プ�Eルを�EリローチE        /// </summary>
        public void PreloadEffects(string[] effectIds)
        {
            // TODO: プリロード機�Eの実裁E            if (FeatureFlags.EnableDebugLogging)
            {
                EventLogger.LogStatic($"[EffectManager] Preloading {effectIds?.Length ?? 0} effects");
            }
        }
        
        /// <summary>
        /// 効果音プ�Eルをクリア
        /// </summary>
        public void ClearEffectPool()
        {
            // TODO: プ�Eルクリア機�Eの実裁E            if (FeatureFlags.EnableDebugLogging)
            {
                EventLogger.LogStatic("[EffectManager] Clearing effect pool");
            }
        }
        
        /// <summary>
        /// メソチE��の存在チェチE��用ヘルパ�E
        /// </summary>
        private bool HasMethod(string methodName, params System.Type[] parameterTypes)
        {
            var method = GetType().GetMethod(methodName, parameterTypes);
            return method != null;
        }
        
        #endregion

        #region Public Interface
        
        /// <summary>
        /// 効果音を�E生（一般皁E��インターフェース�E�E        /// </summary>
        public AudioSource PlayEffectWithSource(string effectID, Vector3 position = default, float volumeMultiplier = 1f)
        {
            if (!effectDatabase.ContainsKey(effectID))
            {
                ServiceLocator.GetService<IEventLogger>().LogWarning($"[EffectManager] Effect '{effectID}' not found in database");
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
        /// スチE��ス効果音の再生
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
        /// 特定�EカチE��リの効果音を停止
        /// </summary>
        public void StopEffectsByType(EffectType effectType)
        {
            // こ�E実裁E��は簡略化�Eため、�E体停止を行う
            // 実際の実裁E��は、各AudioSourceにタグを付けて刁E��する忁E��がある
            foreach (var source in activeEffectSources.ToArray())
            {
                if (source != null && source.isPlaying)
                {
                    // 効果音タイプによる判定！EudioSourceの名前また�Eタグで識別�E�E                    string sourceId = source.gameObject.name;
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
        /// 効果音プ�Eルの初期匁E        /// </summary>
        private void InitializeEffectSourcePool()
        {
            for (int i = 0; i < maxConcurrentEffects; i++)
            {
                var go = new GameObject($"PooledEffectSource_{i}");
                go.transform.SetParent(transform);
                
                var audioSource = go.AddComponent<AudioSource>();
                audioSource.playOnAwake = false;
                audioSource.spatialBlend = 1f; // チE��ォルト�E3D
                audioSource.outputAudioMixerGroup = effectMixerGroup;
                
                effectSourcePool.Enqueue(audioSource);
            }
        }
        
        /// <summary>
        /// 効果音チE�Eタベ�Eスの読み込み
        /// 褁E��のパスから効果音チE�Eタを収雁E��、カチE��リ別に整琁E��まぁE        /// </summary>
        private void LoadEffectDatabase()
        {
            effectDatabase.Clear();
            
            // 褁E��のResourcesパスから効果音を読み込み
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
                        // 重褁E��ェチE���E�異なるパスに同名ファイルがある場合�E処琁E��E                        if (effectDatabase.ContainsKey(sound.name))
                        {
                            ServiceLocator.GetService<IEventLogger>().LogWarning($"[EffectManager] Duplicate effect sound found: {sound.name} in {path}");
                            continue;
                        }
                        
                        effectDatabase[sound.name] = sound;
                        totalLoaded++;
                    }
                }
            }
            
            // ScriptableObjectsフォルダからも検索�E��Eロジェクト固有�Eサウンドデータ�E�E            var customEffects = Resources.LoadAll<SoundDataSO>("ScriptableObjects/Audio/Effects");
            foreach (var effect in customEffects)
            {
                if (effect != null && !effectDatabase.ContainsKey(effect.name))
                {
                    effectDatabase[effect.name] = effect;
                    totalLoaded++;
                }
            }
            
            // チE��ォルト効果音の作�E�E�忁E��最小限のサウンド！E            CreateDefaultEffectsIfNeeded();
            
            EventLogger.LogStatic($"[EffectManager] Loaded {totalLoaded} effect sounds from Resources. " +
                          $"Total effects in database: {effectDatabase.Count}");
                          
            // チE��チE��惁E���E�利用可能な効果音リストを出劁E            #if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (Application.isEditor)
            {
                System.Text.StringBuilder sb = new System.Text.StringBuilder();
                sb.AppendLine("[EffectManager] Available Effects:");
                foreach (var kvp in effectDatabase)
                {
                    sb.AppendLine($"  - {kvp.Key}");
                }
                EventLogger.LogStatic(sb.ToString());
            }
            #endif
        }
        
        /// <summary>
        /// 基本皁E��効果音が見つからなぁE��合にチE��ォルトを作�E
        /// </summary>
        private void CreateDefaultEffectsIfNeeded()
        {
            // 忁E���E効果音IDリスチE            string[] requiredEffects = {
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
                    // チE��ォルト�ESoundDataSOを動皁E���E�E�実行時のみ�E�E                    var defaultSound = ScriptableObject.CreateInstance<SoundDataSO>();
                    defaultSound.name = effectId;
                    // 他�EチE��ォルト設定�E SoundDataSO の初期値を使用
                    
                    effectDatabase[effectId] = defaultSound;
                    created++;
                    
                    ServiceLocator.GetService<IEventLogger>().LogWarning($"[EffectManager] Created default effect: {effectId}");
                }
            }
            
            if (created > 0)
            {
                ServiceLocator.GetService<IEventLogger>().LogWarning($"[EffectManager] Created {created} default effects. " +
                                     "Consider adding proper SoundDataSO assets for these effects.");
            }
        }
        
        /// <summary>
        /// カチE��リ別効果音再生の冁E��処琁E        /// </summary>
        private AudioSource PlayCategorizedEffect(string effectID, AudioEventData eventData, EffectType effectType)
        {
            if (!effectDatabase.ContainsKey(effectID))
            {
                ServiceLocator.GetService<IEventLogger>().LogWarning($"[EffectManager] Effect '{effectID}' not found in database");
                return null;
            }
            
            var soundData = effectDatabase[effectID];
            
            // 効果音タイプに応じた追加設宁E            ApplyEffectTypeSettings(eventData, effectType);
            
            return PlayEffectInternal(soundData, eventData);
        }
        
        /// <summary>
        /// 効果音再生の冁E��処琁E        /// </summary>
        private AudioSource PlayEffectInternal(SoundDataSO soundData, AudioEventData eventData)
        {
            // スチE��ス音響シスチE��がある場合�E、そちらに委譲
            if (spatialAudioService != null && eventData.affectsStealthGameplay)
            {
                spatialAudioService.Play3DSound(eventData.soundID, eventData.worldPosition, eventData.hearingRadius, eventData.volume);
                return null; // Spatial audio service doesn't return AudioSourceioSource);
            }
            
            // 通常の効果音再生処琁E            var audioSource = GetPooledEffectSource();
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
        /// 効果音タイプに応じた設定�E適用
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
        /// プ�Eルからエフェクトソースを取征E        /// </summary>
        private AudioSource GetPooledEffectSource()
        {
            if (effectSourcePool.Count > 0)
            {
                var audioSource = effectSourcePool.Dequeue();
                activeEffectSources.Add(audioSource);
                return audioSource;
            }
            
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            ProjectDebug.LogWarning("[EffectManager] Effect source pool exhausted");
#endif
            return null;
        }
        
        /// <summary>
        /// エフェクトソースを�Eールに返却
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
        /// エフェクトソースの設宁E        /// </summary>
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
            
            // 優先度設宁E            int unityPriority = Mathf.RoundToInt((1f - eventData.priority) * 256f);
            audioSource.priority = Mathf.Clamp(unityPriority, 0, 256);
        }
        
        /// <summary>
        /// 再生終亁E��にプ�Eルに返却するコルーチン
        /// </summary>
        private System.Collections.IEnumerator ReturnToPoolWhenFinished(AudioSource audioSource, float clipLength)
        {
            yield return new WaitForSeconds(clipLength + 0.1f);
            ReturnToPool(audioSource);
        }
        
        
        /// <summary>
        /// エフェクトタイプに基づぁE��設定を適用
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
        /// エフェクトタイプ指定で効果音を�E甁E        /// </summary>
        public void PlayEffect(string effectName, EffectType effectType, Vector3 position = default)
        {
            if (!effectDatabase.TryGetValue(effectName, out SoundDataSO soundData))
            {
                ServiceLocator.GetService<IEventLogger>().LogError($"Effect '{effectName}' not found in database");
                return;
            }

            var audioSource = GetPooledEffectSource();
            if (audioSource == null) return;

            // 基本設定を適用
            audioSource.clip = soundData.GetRandomClip();
            audioSource.volume = soundData.GetRandomVolume();
            audioSource.pitch = soundData.GetRandomPitch();

            // エフェクトタイプ固有�E設定を適用
            ApplyEffectTypeSettings(audioSource, effectType);

            // 位置設宁E            if (position != default)
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
            EventLogger.LogStatic($"Playing effect: {effectName} ({effectType}) with priority: {audioSource.priority}");
            #endif
        }
        
        /// <summary>
        /// UIサウンド�E生�EショートカチE��
        /// </summary>
        public void PlayUIEffect(string effectName)
        {
            PlayEffect(effectName, EffectType.UI);
        }
        
        /// <summary>
        /// インタラクションサウンド�E生�EショートカチE��
        /// </summary>
        public void PlayInteractionEffect(string effectName, Vector3 position = default)
        {
            PlayEffect(effectName, EffectType.Interaction, position);
        }
        
        /// <summary>
        /// 戦闘サウンド�E生�EショートカチE��
        /// </summary>
        public void PlayCombatEffect(string effectName, Vector3 position = default)
        {
            PlayEffect(effectName, EffectType.Combat, position);
        }
        
        /// <summary>
        /// スチE��スサウンド�E生�EショートカチE��
        /// </summary>
        public void PlayStealthEffect(string effectName, Vector3 position = default)
        {
            PlayEffect(effectName, EffectType.Stealth, position);
        }
        
        #endregion
        
        #region Public Status API
        
        /// <summary>
        /// 任意�E効果音が�E生中か確誁E        /// </summary>
        public bool IsPlaying()
        {
            return activeEffectSources.Any(source => source != null && source.isPlaying);
        }
        
        /// <summary>
        /// 持E��した効果音が�E生中か確認（簡略実裁E��E        /// </summary>
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
        /// 持E��したタイプ�E効果音が�E生中か確誁E        /// </summary>
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
        /// アクチE��ブな効果音の数を取征E        /// </summary>
        public int GetActiveEffectCount()
        {
            return activeEffectSources.Count(source => source != null && source.isPlaying);
        }
        
        /// <summary>
        /// アクチE��ブな効果音の最大数を取征E        /// </summary>
        public int GetMaxConcurrentEffects()
        {
            return maxConcurrentEffects;
        }
        
        /// <summary>
        /// 効果音プ�Eルの利用可能数を取征E        /// </summary>
        public int GetAvailableEffectSourceCount()
        {
            return effectSourcePool.Count;
        }
        
        /// <summary>
        /// 持E��した効果音IDがデータベ�Eスに登録されてぁE��か確誁E        /// </summary>
        public bool HasEffectData(string effectId)
        {
            return !string.IsNullOrEmpty(effectId) && effectDatabase.ContainsKey(effectId);
        }
        
        /// <summary>
        /// 登録されてぁE��効果音IDの一覧を取征E        /// </summary>
        public string[] GetRegisteredEffectIds()
        {
            return effectDatabase.Keys.ToArray();
        }
        
        /// <summary>
        /// カチE��リ別の効果音有効状態を取征E        /// </summary>
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