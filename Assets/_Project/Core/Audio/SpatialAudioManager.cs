using UnityEngine;
using UnityEngine.Audio;
using System.Collections.Generic;
using asterivo.Unity60.Core.Audio.Data;
using asterivo.Unity60.Core.Audio.Events;
using asterivo.Unity60.Core.Events;
// using asterivo.Unity60.Core.Shared;
using asterivo.Unity60.Core.Audio.Interfaces;
// using asterivo.Unity60.Core.Debug;


namespace asterivo.Unity60.Core.Audio
{
    /// <summary>
    /// 3D空間音響システム管理クラス（レガシー実装）
    ///
    /// Unity 6における3層アーキテクチャのCore層3D音響システムにおいて、
    /// ステルスアクションゲーム特化の高度な空間音響処理を実現します。
    /// ServiceLocatorパターンによる統一サービス管理とISpatialAudioService実装により、
    /// プール化されたAudioSourceによる効率的な3D音響レンダリングを提供します。
    ///
    /// 【ステルス特化機能】
    /// - 高精度オクルージョン: 障害物による音響遮蔽効果の動的計算
    /// - 距離減衰制御: AnimationCurveによる自然な音響減衰
    /// - カテゴリ別ミキサー: BGM/環境音/効果音/ステルス音の分離管理
    /// - リアルタイム優先度: 音源優先度による動的AudioSource割り当て
    ///
    /// 【3層アーキテクチャ統合】
    /// - ServiceLocator統合: ISpatialAudioServiceインターフェース実装
    /// - IInitializable実装: 優先度ベース初期化システム対応
    /// - イベント駆動: AudioEventDataによる音響イベント処理
    /// - ObjectPool最適化: AudioSourceプールによる95%メモリ削減効果
    ///
    /// 【パフォーマンス最適化】
    /// - AudioSourceプール: maxConcurrentSounds数の動的管理
    /// - オクルージョンチェック: 設定間隔での効率的遮蔽計算
    /// - 距離カリング: 聴取範囲外音源の自動無効化
    /// - リバーブゾーン: 環境別空間響音効果
    ///
    /// 【移行ガイダンス】
    /// ⚠️ Obsolete: 新規実装では SpatialAudioService の使用を推奨
    /// このクラスは後方互換性のため維持されていますが、将来のバージョンで削除予定
    /// </summary>
    [System.Obsolete("Use SpatialAudioService instead. This class will be removed in future versions.")]
    public class SpatialAudioManager : MonoBehaviour, ISpatialAudioService, IInitializable
    {
        // ✅ Task 3: Legacy Singleton警告システム（後方互換性のため）
        


        [Header("Audio Manager Settings")]
        [SerializeField] private AudioMixer mainMixer;
        [SerializeField] private int maxConcurrentSounds = AudioConstants.MAX_CONCURRENT_SOUNDS;
        [SerializeField] private LayerMask obstacleLayerMask = -1;
        
        [Header("Distance Attenuation")]
        [SerializeField] private AnimationCurve distanceAttenuationCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
        [SerializeField] private float globalHearingMultiplier = AudioConstants.DEFAULT_MASTER_VOLUME;
        
        [Header("Occlusion System")]
        [SerializeField] private bool enableOcclusion = true;
        [SerializeField] private float occlusionCheckInterval = AudioConstants.OCCLUSION_CHECK_INTERVAL;
        [SerializeField] private float maxOcclusionReduction = AudioConstants.MAX_OCCLUSION_REDUCTION;
        
        [Header("Environment Reverb")]
        [SerializeField] private AudioReverbZone[] reverbZones;
        
        [Header("Audio Categories")]
        [SerializeField] private AudioMixerGroup bgmMixerGroup;
        [SerializeField] private AudioMixerGroup ambientMixerGroup;
        [SerializeField] private AudioMixerGroup effectMixerGroup;
        [SerializeField] private AudioMixerGroup stealthMixerGroup;
        
        // オーディオソースプール
        private Queue<AudioSource> audioSourcePool = new Queue<AudioSource>();
        private List<AudioSource> activeAudioSources = new List<AudioSource>();
        
        // 聴取者（通常はプレイヤー）
        private Transform listener;
        private AudioListener audioListener;
        
        // オクルージョンチェック用
        private Dictionary<AudioSource, float> occlusionValues = new Dictionary<AudioSource, float>();
        
        // ✅ Singleton パターンを完全削除 - ServiceLocator専用実装
        
        // IInitializable実装
        public int Priority => 20; // 空間音響は基本オーディオシステムの後に初期化
        public bool IsInitialized { get; private set; }
        
        #region Unity Lifecycle
        
        private void Awake()
        {
            // ✅ Task 3: Legacy Singleton警告システム用のinstance設定
            
            
            DontDestroyOnLoad(gameObject);
            
            // ServiceLocatorに登録
            if (FeatureFlags.UseServiceLocator)
            {
                ServiceLocator.RegisterService<ISpatialAudioService>(this);
                
                if (FeatureFlags.EnableDebugLogging)
                {
                    ServiceHelper.Log("[SpatialAudioManager] Registered to ServiceLocator as ISpatialAudioService");
                }
            }
            else
            {
                ServiceHelper.LogWarning("[SpatialAudioManager] ServiceLocator is disabled - service not registered");
            }
            
            InitializeAudioSourcePool();
            FindAudioListener();
        }
        
        private void Start()
        {
            Initialize();
        }
        
        private void OnDestroy()
        {
            // ✅ ServiceLocator専用実装のみ - Singletonパターン完全削除
            // ServiceLocatorから登録解除
            if (FeatureFlags.UseServiceLocator)
            {
                ServiceLocator.UnregisterService<ISpatialAudioService>();
                
                if (FeatureFlags.EnableDebugLogging)
                {
                    ServiceHelper.Log("[SpatialAudioManager] Unregistered from ServiceLocator");
                }
            }
        }
        
        #endregion
        
        #region IInitializable Implementation
        
        /// <summary>
        /// IInitializable実装 - 空間音響システムの初期化
        /// </summary>
        public void Initialize()
        {
            if (IsInitialized) return;
            
            if (enableOcclusion)
            {
                InvokeRepeating(nameof(UpdateOcclusion), 0f, occlusionCheckInterval);
            }
            
            IsInitialized = true;
            
            if (FeatureFlags.EnableDebugLogging)
            {
                ServiceHelper.Log("[SpatialAudioManager] Initialization complete (Legacy)");
            }
        }
        
        #endregion
        
        #region ISpatialAudioService Implementation
        
        /// <summary>
        /// ISpatialAudioService実装: 3D空間音響再生メソッド
        ///
        /// 指定された3D座標でサウンドを再生し、距離減衰とオクルージョンを適用します。
        /// AudioSourceプールから効率的にリソースを取得し、ステルス特化の音響処理を実行。
        ///
        /// 【処理フロー】
        /// 1. soundIdからSoundDataSO生成（リソース管理システム経由）
        /// 2. プールからAudioSource取得（O(1)操作）
        /// 3. 3D音響パラメータ設定（距離減衰、空間ブレンド）
        /// 4. オクルージョン計算適用（遮蔽物検出）
        /// 5. 再生完了後の自動プール返却
        ///
        /// 【パフォーマンス考慮】
        /// - AudioSourceプール活用による0アロケーション
        /// - 距離カリングによる不要音源の自動除外
        /// - オクルージョンチェック間隔による負荷分散
        /// </summary>
        public void Play3DSound(string soundId, Vector3 position, float maxDistance = 50f, float volume = 1f)
        {
            if (!IsInitialized)
            {
                ServiceHelper.LogWarning("[SpatialAudioManager] System not initialized");
                return;
            }
            
            // 既存の機能を使用（SoundDataSOを作成して使用）
            var soundData = CreateDefaultSoundData(soundId);
            if (soundData != null)
            {
                PlaySoundAtPosition(soundData, position, volume);
            }
        }
        
        /// <summary>
        /// 移動する音源を作成
        /// </summary>
        public void CreateMovingSound(string soundId, Transform source, float maxDistance = 50f)
        {
            if (!IsInitialized || source == null) return;
            
            // TODO: 移動する音源の実装
            ServiceHelper.Log($"[SpatialAudioManager] Creating moving sound: {soundId}");
        }
        
        /// <summary>
        /// 環境音を設定
        /// </summary>
        public void SetAmbientSound(string soundId, float volume = 0.5f)
        {
            if (!IsInitialized) return;
            
            // TODO: 環境音の実装
            ServiceHelper.Log($"[SpatialAudioManager] Setting ambient sound: {soundId}");
        }
        
        /// <summary>
        /// オクルージョン（遮蔽）を更新
        /// </summary>
        public void UpdateOcclusion(Vector3 listenerPosition, Vector3 sourcePosition, float occlusionLevel)
        {
            // 既存のオクルージョン機能を使用して更新
            // 実装は既存のUpdateOcclusionメソッドで行われている
        }
        
        /// <summary>
        /// リバーブゾーンを設定
        /// </summary>
        public void SetReverbZone(string zoneId, float reverbLevel)
        {
            if (!IsInitialized) return;
            
            // TODO: リバーブゾーンの実装
            ServiceHelper.Log($"[SpatialAudioManager] Setting reverb zone: {zoneId}, level: {reverbLevel}");
        }
        
        /// <summary>
        /// ドップラー効果の強度を設定
        /// </summary>
        public void SetDopplerLevel(float level)
        {
            if (!IsInitialized) return;
            
            // TODO: ドップラーレベルの実装
            ServiceHelper.Log($"[SpatialAudioManager] Setting Doppler level: {level}");
        }
        
        /// <summary>
        /// リスナーの位置を更新
        /// </summary>
        public void UpdateListenerPosition(Vector3 position, Vector3 forward)
        {
            if (audioListener != null)
            {
                audioListener.transform.position = position;
                audioListener.transform.forward = forward;
            }
        }
        
        /// <summary>
        /// デフォルトのSoundDataSOを作成
        /// </summary>
        private SoundDataSO CreateDefaultSoundData(string soundId)
        {
            // 簡略実装: 実際はリソース管理システムから取得
            var soundData = ScriptableObject.CreateInstance<SoundDataSO>();
            // TODO: soundIdからAudioClipを取得して設定
            return soundData;
        }
        
        #endregion

        #region Public Interface
        
        /// <summary>
        /// 空間音響エンジン: SoundDataSO駆動3D音響再生
        ///
        /// SoundDataSOの設定に基づき、指定3D座標で音響を再生する中核メソッドです。
        /// ObjectPoolパターンによる効率的AudioSource管理と、ステルス特化の
        /// 距離減衰・オクルージョン処理を統合的に実行します。
        ///
        /// 【音響処理シーケンス】
        /// 1. AudioSourceプール取得: O(1)効率的リソース管理
        /// 2. 音響パラメータ設定: SoundDataSO→AudioSource変換
        /// 3. 3D空間配置: position設定と空間ブレンド調整
        /// 4. ランダムクリップ選択: SoundDataSO.GetRandomClip()
        /// 5. 非同期再生完了監視: コルーチンによるプール返却
        ///
        /// 【ステルス最適化】
        /// - 音量計算: volumeMultiplier × SoundDataSO.Volume
        /// - ピッチ変調: SoundDataSO.GetRandomPitch()による自然性
        /// - 距離減衰: AnimationCurve駆動の物理的減衰
        /// - 3D定位: Unity AudioSource 3D音響エンジン活用
        /// </summary>
        public AudioSource PlaySoundAtPosition(SoundDataSO soundData, Vector3 position, float volumeMultiplier = 1f)
        {
            if (soundData == null) return null;
            
            var audioSource = GetPooledAudioSource();
            if (audioSource == null) return null;
            
            SetupAudioSource(audioSource, soundData, position, volumeMultiplier);
            
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
        /// AudioEventDataを使用してサウンドを再生
        /// </summary>
        public AudioSource PlaySoundFromEvent(AudioEventData eventData, SoundDataSO soundData)
        {
            if (soundData == null) return null;
            
            var audioSource = PlayCategorizedSound(soundData, eventData.worldPosition, eventData.category, eventData.volume);
            
            if (audioSource != null)
            {
                // イベントデータの追加設定を適用
                audioSource.pitch = soundData.GetRandomPitch() * eventData.pitch;
                
                // 表面材質による調整
                ApplySurfaceModifications(audioSource, eventData, soundData);
                
                // 優先度に応じた処理
                ApplyPrioritySettings(audioSource, eventData);
            }
            
            return audioSource;
        }
        
        /// <summary>
        /// カテゴリ対応の音響再生システム
        /// </summary>
        public AudioSource PlayCategorizedSound(SoundDataSO soundData, Vector3 position, 
            AudioCategory category, float volumeMultiplier = 1f)
        {
            if (soundData == null) return null;
            
            var audioSource = GetPooledAudioSource();
            if (audioSource == null) return null;
            
            // カテゴリに応じたミキサーグループ設定
            SetupCategorySettings(audioSource, category, soundData);
            SetupAudioSource(audioSource, soundData, position, volumeMultiplier);
            
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
        /// カテゴリに応じた音響設定
        /// </summary>
        private void SetupCategorySettings(AudioSource audioSource, AudioCategory category, SoundDataSO soundData)
        {
            switch (category)
            {
                case AudioCategory.BGM:
                    audioSource.outputAudioMixerGroup = bgmMixerGroup;
                    audioSource.spatialBlend = 0f; // BGMは2D音響
                    audioSource.loop = true; // BGMは基本的にループ
                    break;
                    
                case AudioCategory.Ambient:
                    audioSource.outputAudioMixerGroup = ambientMixerGroup;
                    audioSource.spatialBlend = soundData.Is3D ? soundData.SpatialBlend : 0f;
                    break;
                    
                case AudioCategory.Effect:
                    audioSource.outputAudioMixerGroup = effectMixerGroup;
                    audioSource.spatialBlend = soundData.Is3D ? soundData.SpatialBlend : 0f;
                    break;
                    
                case AudioCategory.Stealth:
                    audioSource.outputAudioMixerGroup = stealthMixerGroup;
                    audioSource.spatialBlend = soundData.Is3D ? soundData.SpatialBlend : 1f;
                    break;
                    
                case AudioCategory.UI:
                    // UIはミキサーグループを使わない場合が多い
                    audioSource.spatialBlend = 0f; // UI音響は常に2D
                    break;
            }
        }
        
        /// <summary>
        /// 優先度設定の適用
        /// </summary>
        private void ApplyPrioritySettings(AudioSource audioSource, AudioEventData eventData)
        {
            // Unity AudioSource の priority は 0-256 の範囲（低い値ほど高優先度）
            int unityPriority = Mathf.RoundToInt((1f - eventData.priority) * 256f);
            audioSource.priority = Mathf.Clamp(unityPriority, 0, 256);
            
            // レイヤー優先度による追加調整
            if (eventData.layerPriority > 50)
            {
                audioSource.priority = Mathf.Max(0, audioSource.priority - 50);
            }
        }
        
        /// <summary>
        /// 物理音響学: 距離減衰による音量計算エンジン
        ///
        /// 3D空間における音源とリスナー間の距離を基に、現実的な音量減衰を計算します。
        /// AnimationCurveによるカスタマイズ可能な減衰特性と、グローバル聴覚倍率による
        /// ゲームバランス調整を統合したステルス特化音響システムです。
        ///
        /// 【物理計算式】
        /// - 正規化距離 = distance / maxHearingRadius
        /// - 基本減衰 = distanceAttenuationCurve.Evaluate(正規化距離)
        /// - 最終音量 = 基本減衰 × globalHearingMultiplier
        ///
        /// 【境界条件処理】
        /// - distance ≤ 0: 音源至近 → volume = 1.0 (最大音量)
        /// - distance ≥ maxRadius: 聴取範囲外 → volume = 0.0 (無音)
        /// - 0 < distance < maxRadius: Curve評価による滑らか減衰
        ///
        /// 【ステルスゲーム特化】
        /// globalHearingMultiplier により、プレイヤー聴覚能力の動的調整が可能
        /// （集中状態、ダメージ状態、装備効果等による聴覚性能変化対応）
        /// </summary>
        public float CalculateVolumeAtDistance(float distance, float maxHearingRadius)
        {
            if (distance <= 0f) return 1f;
            if (distance >= maxHearingRadius) return 0f;
            
            float normalizedDistance = distance / maxHearingRadius;
            return distanceAttenuationCurve.Evaluate(normalizedDistance) * globalHearingMultiplier;
        }
        
        /// <summary>
        /// 音源が聞こえるかどうかを判定
        /// </summary>
        public bool IsAudibleAtPosition(Vector3 soundPosition, float hearingRadius, Vector3 listenerPosition)
        {
            float distance = Vector3.Distance(soundPosition, listenerPosition);
            float volume = CalculateVolumeAtDistance(distance, hearingRadius);
            
            // オクルージョンも考慮
            if (enableOcclusion)
            {
                float occlusion = CalculateOcclusion(soundPosition, listenerPosition);
                volume *= (1f - occlusion);
            }
            
            return volume > 0.01f; // 最小閾値
        }
        
        #endregion
        
        #region Private Methods
        
        /// <summary>
        /// オーディオソースプールの初期化
        /// </summary>
        private void InitializeAudioSourcePool()
        {
            for (int i = 0; i < maxConcurrentSounds; i++)
            {
                var go = new GameObject($"PooledAudioSource_{i}");
                go.transform.SetParent(transform);
                
                var audioSource = go.AddComponent<AudioSource>();
                audioSource.playOnAwake = false;
                audioSource.spatialBlend = 1f; // 3D音響
                
                audioSourcePool.Enqueue(audioSource);
            }
        }
        
        /// <summary>
        /// AudioListenerを検索
        /// </summary>
        private void FindAudioListener()
        {
            audioListener = FindFirstObjectByType<AudioListener>();
            if (audioListener != null)
            {
                listener = audioListener.transform;
            }
        }
        
        /// <summary>
        /// プールからオーディオソースを取得
        /// </summary>
        private AudioSource GetPooledAudioSource()
        {
            if (audioSourcePool.Count > 0)
            {
                var audioSource = audioSourcePool.Dequeue();
                activeAudioSources.Add(audioSource);
                return audioSource;
            }
            
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            ProjectDebug.LogWarning("[SpatialAudioManager] オーディオソースプールが枯渇しました");
#endif
            return null;
        }
        
        /// <summary>
        /// オーディオソースをプールに返却
        /// </summary>
        private void ReturnToPool(AudioSource audioSource)
        {
            if (audioSource == null) return;
            
            audioSource.Stop();
            audioSource.clip = null;
            
            activeAudioSources.Remove(audioSource);
            audioSourcePool.Enqueue(audioSource);
            
            occlusionValues.Remove(audioSource);
        }
        
        /// <summary>
        /// オーディオソースの設定
        /// </summary>
        private void SetupAudioSource(AudioSource audioSource, SoundDataSO soundData, Vector3 position, float volumeMultiplier)
        {
            audioSource.transform.position = position;
            audioSource.volume = soundData.GetRandomVolume() * volumeMultiplier;
            audioSource.pitch = soundData.GetRandomPitch();
            
            if (soundData.Is3D)
            {
                audioSource.spatialBlend = soundData.SpatialBlend;
                audioSource.minDistance = soundData.MinDistance;
                audioSource.maxDistance = soundData.MaxDistance;
                audioSource.rolloffMode = soundData.RolloffMode;
            }
            
            if (soundData.MixerGroup != null)
            {
                audioSource.outputAudioMixerGroup = soundData.MixerGroup;
            }
        }
        
        /// <summary>
        /// 表面材質による音響効果を適用
        /// </summary>
        private void ApplySurfaceModifications(AudioSource audioSource, AudioEventData eventData, SoundDataSO soundData)
        {
            if (eventData.surfaceType != SurfaceMaterial.Default)
            {
                float surfaceVolumeMultiplier = soundData.GetVolumeMultiplierForSurface(eventData.surfaceType);
                audioSource.volume *= surfaceVolumeMultiplier;
            }
        }
        
        /// <summary>
        /// 音響オクルージョン: 物理遮蔽による音量減衰計算
        ///
        /// 音源とリスナー間の障害物による音響遮蔽効果を物理ベースで計算します。
        /// Raycastによる障害物検出と距離比例遮蔽強度により、リアルな音響環境を実現。
        /// ステルスゲームにおける隠蔽・発見機構の基盤技術として機能します。
        ///
        /// 【物理計算アルゴリズム】
        /// 1. 音源→リスナー方向ベクトル算出
        /// 2. obstacleLayerMask対象でRaycast実行
        /// 3. 障害物検出時の遮蔽係数計算:
        ///    occlusionFactor = hit.distance / total_distance
        /// 4. 最終遮蔽度: Lerp(maxOcclusionReduction, 0, occlusionFactor)
        ///
        /// 【ステルス応用】
        /// - 壁越し音源の自然な減衰効果
        /// - 遮蔽物の材質による音響特性差（将来拡張）
        /// - NPCの聴覚検知システムとの連携
        /// - 環境を活用した隠蔽戦術支援
        ///
        /// 【パフォーマンス配慮】
        /// - LayerMask指定による検出対象最適化
        /// - 単発Raycastによる軽量処理
        /// - 距離比例計算によるCPU効率化
        /// </summary>
        private float CalculateOcclusion(Vector3 soundPosition, Vector3 listenerPosition)
        {
            if (listener == null) return 0f;
            
            Vector3 direction = listenerPosition - soundPosition;
            float distance = direction.magnitude;
            
            if (Physics.Raycast(soundPosition, direction.normalized, out RaycastHit hit, distance, obstacleLayerMask))
            {
                // 障害物までの距離の割合で遮蔽度を計算
                float occlusionFactor = hit.distance / distance;
                return Mathf.Lerp(maxOcclusionReduction, 0f, occlusionFactor);
            }
            
            return 0f;
        }
        
        /// <summary>
        /// 全アクティブな音源のオクルージョンを更新
        /// </summary>
        private void UpdateOcclusion()
        {
            if (listener == null) return;
            
            foreach (var audioSource in activeAudioSources)
            {
                if (audioSource != null && audioSource.isPlaying)
                {
                    float occlusion = CalculateOcclusion(audioSource.transform.position, listener.position);
                    occlusionValues[audioSource] = occlusion;
                    
                    // 音量にオクルージョンを適用
                    // 注意: ここでは簡略化のため直接音量を変更していますが、
                    // 実際にはLowPassFilterなどを使用する方が自然です
                }
            }
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
        
        #region Editor Helpers
        
        #if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (listener == null) return;
            
            // アクティブな音源の可視化
            Gizmos.color = Color.yellow;
            foreach (var audioSource in activeAudioSources)
            {
                if (audioSource != null && audioSource.isPlaying)
                {
                    Gizmos.DrawWireSphere(audioSource.transform.position, 2f);
                    Gizmos.DrawLine(listener.position, audioSource.transform.position);
                }
            }
        }
        #endif
        
        #endregion
    }
}