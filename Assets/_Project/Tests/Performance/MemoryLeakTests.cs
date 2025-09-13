using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using System;
using asterivo.Unity60.Core;
using asterivo.Unity60.Core.Audio;
using asterivo.Unity60.Core;
using asterivo.Unity60.Core.Audio.Interfaces;

namespace asterivo.Unity60.Tests.Performance
{
    /// <summary>
    /// メモリリーク検証テストスイート
    /// Step 4.1: ServiceLocatorシステムでのメモリリーク検出と防止
    /// </summary>
    [TestFixture]
    public class MemoryLeakTests 
    {
        private const long MAX_MEMORY_INCREASE_BYTES = 1024 * 1024; // 1MB
        private const long MAX_SCENE_MEMORY_INCREASE_BYTES = 512 * 1024; // 512KB
        
        [SetUp]
        public void SetUp()
        {
            ServiceLocator.Clear();
            
            // ガベージコレクションを実行してクリーンな状態にする
            System.GC.Collect();
            System.GC.WaitForPendingFinalizers();
            System.GC.Collect();
        }
        
        [TearDown]
        public void TearDown()
        {
            ServiceLocator.Clear();
            
            // テスト後のクリーンアップ
            System.GC.Collect();
            System.GC.WaitForPendingFinalizers();
            System.GC.Collect();
        }
        
        /// <summary>
        /// ServiceLocatorの複数回登録と解除でメモリリークがないことを検証
        /// サービスの登録→解除を繰り返してもメモリ使用量が増加しないことを確認
        /// </summary>
        [Test]
        public void ServiceLocator_NoMemoryLeaks_AfterMultipleRegistrations() 
        {
            // 複数回のサービス登録でメモリリークがないことを検証
            long initialMemory = System.GC.GetTotalMemory(true);
            
            for (int i = 0; i < 100; i++) 
            {
                ServiceLocator.Clear();
                
                // ダミーサービスを登録
                var dummyService = new GameObject($"DummyService_{i}").AddComponent<DummyAudioService>();
                ServiceLocator.RegisterService<IAudioService>(dummyService);
                
                // サービスが正しく登録されていることを確認
                var retrievedService = ServiceLocator.GetService<IAudioService>();
                Assert.IsNotNull(retrievedService, $"Service should be retrievable at iteration {i}");
                Assert.AreSame(dummyService, retrievedService, $"Retrieved service should be the same instance at iteration {i}");
                
                UnityEngine.Object.DestroyImmediate(dummyService.gameObject);
            }
            
            ServiceLocator.Clear();
            
            // 強制的にガベージコレクションを実行
            System.GC.Collect();
            System.GC.WaitForPendingFinalizers();
            System.GC.Collect();
            
            long finalMemory = System.GC.GetTotalMemory(true);
            long memoryDiff = finalMemory - initialMemory;
            
            Assert.Less(memoryDiff, MAX_MEMORY_INCREASE_BYTES, 
                       $"Memory increase should be less than {MAX_MEMORY_INCREASE_BYTES / 1024}KB. Actual increase: {memoryDiff} bytes ({memoryDiff / 1024}KB)");
            
            UnityEngine.Debug.Log($"[MemoryLeak] Multiple registrations memory change: {memoryDiff} bytes ({memoryDiff / 1024:F1}KB)");
        }
        
        /// <summary>
        /// シーン遷移パターンでのメモリリーク検証
        /// ゲームでよくあるシーン切り替え時のServiceLocator使用パターンをテスト
        /// </summary>
        [UnityTest]
        public IEnumerator ServiceLocator_NoMemoryLeaks_AfterSceneTransitions()
        {
            // シーン遷移でメモリリークがないことを検証
            long initialMemory = System.GC.GetTotalMemory(true);
            
            for (int i = 0; i < 5; i++) 
            {
                // シーンの読み込みとServiceLocator使用をシミュレート
                ServiceLocator.Clear();
                
                var audioManager = new GameObject("AudioManager").AddComponent<DummyAudioService>();
                ServiceLocator.RegisterService<IAudioService>(audioManager);
                
                // サービスの使用をシミュレート
                for (int j = 0; j < 10; j++)
                {
                    var service = ServiceLocator.GetService<IAudioService>();
                    Assert.IsNotNull(service, $"Service should be available during simulation {i}-{j}");
                    service.PlaySound($"test_sound_{j}");
                }
                
                yield return null;
                
                // シーンクリーンアップをシミュレート
                UnityEngine.Object.DestroyImmediate(audioManager.gameObject);
                ServiceLocator.Clear();
                
                yield return null;
            }
            
            // ガベージコレクション強制実行
            System.GC.Collect();
            System.GC.WaitForPendingFinalizers();
            System.GC.Collect();
            
            yield return null;
            
            long finalMemory = System.GC.GetTotalMemory(true);
            long memoryDiff = finalMemory - initialMemory;
            
            Assert.Less(memoryDiff, MAX_SCENE_MEMORY_INCREASE_BYTES, 
                       $"Scene transition memory increase should be less than {MAX_SCENE_MEMORY_INCREASE_BYTES / 1024}KB. Actual increase: {memoryDiff} bytes ({memoryDiff / 1024}KB)");
            
            UnityEngine.Debug.Log($"[MemoryLeak] Scene transition memory change: {memoryDiff} bytes ({memoryDiff / 1024:F1}KB)");
        }
        
        /// <summary>
        /// 大量のサービス登録と参照でのメモリ動作テスト
        /// 高負荷時のメモリ管理の安定性を検証
        /// </summary>
        [Test]
        public void ServiceLocator_MemoryStability_UnderHighLoad()
        {
            long initialMemory = System.GC.GetTotalMemory(true);
            
            const int highLoadCount = 1000;
            
            // 大量のサービス登録と取得
            for (int i = 0; i < highLoadCount; i++)
            {
                if (i % 100 == 0) // 100回ごとにServiceLocatorをクリア
                {
                    ServiceLocator.Clear();
                }
                
                var dummyService = new GameObject($"HighLoadService_{i}").AddComponent<DummyAudioService>();
                ServiceLocator.RegisterService<IAudioService>(dummyService);
                
                // サービスの取得と使用
                var service = ServiceLocator.GetService<IAudioService>();
                Assert.IsNotNull(service, $"Service should be available at high load iteration {i}");
                
                service.PlaySound($"high_load_sound_{i}");
                
                UnityEngine.Object.DestroyImmediate(dummyService.gameObject);
            }
            
            ServiceLocator.Clear();
            
            // メモリクリーンアップ
            System.GC.Collect();
            System.GC.WaitForPendingFinalizers();
            System.GC.Collect();
            
            long finalMemory = System.GC.GetTotalMemory(true);
            long memoryDiff = finalMemory - initialMemory;
            
            // 高負荷でも合理的なメモリ使用量増加に収まることを確認
            Assert.Less(memoryDiff, MAX_MEMORY_INCREASE_BYTES * 2, 
                       $"High load memory increase should be less than {MAX_MEMORY_INCREASE_BYTES * 2 / 1024}KB. Actual: {memoryDiff} bytes");
            
            UnityEngine.Debug.Log($"[MemoryLeak] High load memory change: {memoryDiff} bytes ({memoryDiff / 1024:F1}KB)");
        }
        
        /// <summary>
        /// サービス参照の循環参照チェック
        /// ServiceLocatorが循環参照を作らないことを確認
        /// </summary>
        [Test]
        public void ServiceLocator_NoCircularReferences_BetweenServices()
        {
            long initialMemory = System.GC.GetTotalMemory(true);
            
            // 複数のサービスを相互参照させる
            var audioService = new GameObject("AudioService").AddComponent<DummyAudioService>();
            var spatialService = new GameObject("SpatialService").AddComponent<DummySpatialAudioService>();
            var effectService = new GameObject("EffectService").AddComponent<DummyEffectService>();
            
            ServiceLocator.RegisterService<IAudioService>(audioService);
            ServiceLocator.RegisterService<ISpatialAudioService>(spatialService);
            ServiceLocator.RegisterService<IEffectService>(effectService);
            
            // 相互参照をシミュレート
            for (int i = 0; i < 50; i++)
            {
                var audio = ServiceLocator.GetService<IAudioService>();
                var spatial = ServiceLocator.GetService<ISpatialAudioService>();
                var effect = ServiceLocator.GetService<IEffectService>();
                
                Assert.IsNotNull(audio, $"AudioService should be available at iteration {i}");
                Assert.IsNotNull(spatial, $"SpatialAudioService should be available at iteration {i}");
                Assert.IsNotNull(effect, $"EffectService should be available at iteration {i}");
            }
            
            // クリーンアップ
            UnityEngine.Object.DestroyImmediate(audioService.gameObject);
            UnityEngine.Object.DestroyImmediate(spatialService.gameObject);
            UnityEngine.Object.DestroyImmediate(effectService.gameObject);
            ServiceLocator.Clear();
            
            System.GC.Collect();
            System.GC.WaitForPendingFinalizers();
            System.GC.Collect();
            
            long finalMemory = System.GC.GetTotalMemory(true);
            long memoryDiff = finalMemory - initialMemory;
            
            Assert.Less(memoryDiff, MAX_MEMORY_INCREASE_BYTES / 2, 
                       $"Circular reference test memory increase should be minimal. Actual: {memoryDiff} bytes");
            
            UnityEngine.Debug.Log($"[MemoryLeak] Circular reference test memory change: {memoryDiff} bytes");
        }
        
        /// <summary>
        /// WeakReferenceパターンのメモリ動作テスト
        /// ServiceLocatorが適切にオブジェクトを解放することを確認
        /// </summary>
        [Test]
        public void ServiceLocator_WeakReference_ProperGarbageCollection()
        {
            var weakReferences = new System.Collections.Generic.List<System.WeakReference>();
            
            // サービスを作成してWeakReferenceで追跡
            for (int i = 0; i < 10; i++)
            {
                var service = new GameObject($"WeakRefService_{i}").AddComponent<DummyAudioService>();
                ServiceLocator.RegisterService<IAudioService>(service);
                
                weakReferences.Add(new System.WeakReference(service));
                
                UnityEngine.Object.DestroyImmediate(service.gameObject);
                ServiceLocator.Clear();
            }
            
            // ガベージコレクションを強制実行
            System.GC.Collect();
            System.GC.WaitForPendingFinalizers();
            System.GC.Collect();
            
            // WeakReferenceが無効になっていることを確認（オブジェクトが正しく解放された）
            int aliveObjects = 0;
            foreach (var weakRef in weakReferences)
            {
                if (weakRef.IsAlive)
                {
                    aliveObjects++;
                }
            }
            
            Assert.LessOrEqual(aliveObjects, 2, 
                              $"Most weak references should be dead after GC. Alive objects: {aliveObjects}");
            
            UnityEngine.Debug.Log($"[MemoryLeak] WeakReference test: {aliveObjects}/{weakReferences.Count} objects still alive");
        }
    }
    
    #region Test Helper Classes
    
    /// <summary>
    /// テスト用ダミーオーディオサービス
    /// 実際のAudioManagerの代替として使用
    /// </summary>
    public class DummyAudioService : MonoBehaviour, IAudioService, IInitializable
    {
        public int Priority => 0;
        public bool IsInitialized => true;
        
        public void Initialize() 
        {
            // テスト用の初期化処理
        }
        
        public void PlaySound(string soundId, Vector3 position = default, float volume = 1f) 
        {
            // ダミー実装 - 実際の音声再生は行わない
        }
        
        public void StopSound(string soundId) 
        {
            // ダミー実装
        }
        
        public void StopAllSounds() 
        {
            // ダミー実装
        }
        
        public float GetMasterVolume() => 1f;
        
        public void SetMasterVolume(float volume) 
        {
            // ダミー実装
        }
        
        public float GetBGMVolume() => 1f;
        public float GetAmbientVolume() => 1f;
        public float GetEffectVolume() => 1f;
        
        public void SetCategoryVolume(string category, float volume) 
        {
            // ダミー実装
        }
        
        public bool IsPlaying(string soundId) => false;
        
        public void Pause() 
        {
            // ダミー実装
        }
        
        public void Resume() 
        {
            // ダミー実装
        }
    }
    
    /// <summary>
    /// テスト用ダミー空間オーディオサービス
    /// </summary>
    public class DummySpatialAudioService : MonoBehaviour, ISpatialAudioService, IInitializable
    {
        public int Priority => 0;
        public bool IsInitialized => true;
        
        public void Initialize() { }
        public void Play3DSound(string soundId, Vector3 position, float maxDistance = 50f, float volume = 1f) { }
        public void CreateMovingSound(string soundId, Transform source, float maxDistance = 50f) { }
        public void SetAmbientSound(string soundId, float volume = 0.5f) { }
        public void UpdateOcclusion(Vector3 listenerPosition, Vector3 sourcePosition, float occlusionLevel) { }
        public void SetReverbZone(string zoneId, float reverbLevel) { }
        public void SetDopplerLevel(float level) { }
        public void UpdateListenerPosition(Vector3 position, Vector3 forward) { }
    }
    
    /// <summary>
    /// テスト用ダミーオーディオアップデートサービス
    /// </summary>
    public class DummyAudioUpdateService : MonoBehaviour, IAudioUpdateService, IInitializable
    {
        public int Priority => 0;
        public bool IsInitialized => true;
        
        public float UpdateInterval { get; set; } = 0.1f;
        public bool IsCoordinatedUpdateEnabled => true;
        
        // public event System.Action<AudioSystemSyncData> OnAudioSystemSync; // Temporarily commented out for architecture verification
        
        public void Initialize() { }
        // public void RegisterUpdatable(IAudioUpdatable updatable) { } // Temporarily commented out for architecture verification
        // public void UnregisterUpdatable(IAudioUpdatable updatable) { } // Temporarily commented out for architecture verification
        public void StartCoordinatedUpdates() { }
        public void StopCoordinatedUpdates() { }
        public List<AudioSource> GetNearbyAudioSources(Vector3 center, float radius) => new List<AudioSource>();
        public void ForceRebuildSpatialCache() { }
        // public AudioCoordinatorStats GetPerformanceStats() => new AudioCoordinatorStats(); // Temporarily commented out for architecture verification
    }
    
    /// <summary>
    /// テスト用ダミーエフェクトサービス
    /// </summary>
    public class DummyEffectService : MonoBehaviour, IEffectService, IInitializable
    {
        public int Priority => 0;
        public bool IsInitialized => true;
        
        public void Initialize() { }
        public void PlayEffect(string effectId, Vector3 position = default, float volume = 1f) { }
        public int StartLoopingEffect(string effectId, Vector3 position, float volume = 1f) => 0;
        public void StopLoopingEffect(int loopId) { }
        public void PlayOneShot(string effectId, Vector3 position = default, float volume = 1f) { }
        public void PlayRandomEffect(string[] effectIds, Vector3 position = default, float volume = 1f) { }
        public void SetEffectPitch(string effectId, float pitch) { }
        public void PreloadEffects(string[] effectIds) { }
        public void ClearEffectPool() { }
    }
    
    #endregion
}