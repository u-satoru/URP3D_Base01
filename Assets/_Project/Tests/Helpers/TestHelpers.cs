using UnityEngine;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using asterivo.Unity60.Core;
using asterivo.Unity60.Core.Audio.Interfaces;

namespace asterivo.Unity60.Tests.Helpers
{
    /// <summary>
    /// テストヘルパークラス
    /// テスト実行時の共通処理とユーティリティ機能を提供
    /// </summary>
    public static class TestHelpers
    {
        #region GameObject Management

        private static List<GameObject> createdGameObjects = new List<GameObject>();

        /// <summary>
        /// テスト用GameObjectを作成し、自動クリーンアップ対象に追加
        /// </summary>
        public static GameObject CreateTestGameObject(string name = "TestObject")
        {
            var go = new GameObject(name);
            createdGameObjects.Add(go);
            return go;
        }

        /// <summary>
        /// コンポーネント付きテスト用GameObjectを作成
        /// </summary>
        public static T CreateTestGameObjectWithComponent<T>(string name = "TestObject") where T : Component
        {
            var go = CreateTestGameObject(name);
            return go.AddComponent<T>();
        }

        /// <summary>
        /// 作成したテスト用GameObjectをすべてクリーンアップ
        /// </summary>
        public static void CleanupTestGameObjects()
        {
            foreach (var go in createdGameObjects)
            {
                if (go != null)
                {
                    Object.DestroyImmediate(go);
                }
            }
            createdGameObjects.Clear();
        }

        #endregion

        #region ServiceLocator Test Support

        /// <summary>
        /// テスト用にServiceLocatorをクリーンな状態に初期化
        /// </summary>
        public static void InitializeServiceLocatorForTest()
        {
            ServiceLocator.Clear();
        }

        /// <summary>
        /// テスト用にサービスを登録
        /// </summary>
        public static void RegisterTestService<T>(T service) where T : class
        {
            ServiceLocator.RegisterService<T>(service);
        }

        /// <summary>
        /// テスト用サービスの登録を確認
        /// </summary>
        public static bool IsTestServiceRegistered<T>() where T : class
        {
            return ServiceLocator.HasService<T>();
        }

        #endregion

        #region Mock Object Creation

        /// <summary>
        /// MockAudioServiceを作成
        /// </summary>
        public static MockAudioService CreateMockAudioService()
        {
            return new MockAudioService();
        }

        /// <summary>
        /// MockSpatialAudioServiceを作成
        /// </summary>
        public static MockSpatialAudioService CreateMockSpatialAudioService()
        {
            return new MockSpatialAudioService();
        }

        /// <summary>
        /// MockStealthAudioServiceを作成
        /// </summary>
        public static MockStealthAudioService CreateMockStealthAudioService()
        {
            return new MockStealthAudioService();
        }

        #endregion

        #region Extended Assertions

        /// <summary>
        /// Vector3の近似比較
        /// </summary>
        public static void AssertVector3Approximately(Vector3 expected, Vector3 actual, float tolerance = 0.01f)
        {
            Assert.That(Vector3.Distance(expected, actual), Is.LessThan(tolerance),
                $"Expected: {expected}, Actual: {actual}, Tolerance: {tolerance}");
        }

        /// <summary>
        /// 範囲内の値かどうかを検証
        /// </summary>
        public static void AssertInRange(float value, float min, float max, string message = "")
        {
            Assert.That(value, Is.InRange(min, max), 
                $"Value {value} is not in range [{min}, {max}]. {message}");
        }

        /// <summary>
        /// パフォーマンステスト用の時間測定
        /// </summary>
        public static void AssertExecutionTime(System.Action action, float maxTimeMs, string description = "")
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            action();
            stopwatch.Stop();
            
            var elapsedMs = stopwatch.ElapsedMilliseconds;
            Assert.That(elapsedMs, Is.LessThan(maxTimeMs), 
                $"{description} took {elapsedMs}ms, expected less than {maxTimeMs}ms");
        }

        #endregion

        #region Test Setup Helpers

        /// <summary>
        /// 標準的なテストセットアップ（各テストの開始時に呼び出し）
        /// </summary>
        public static void StandardTestSetup()
        {
            CleanupTestGameObjects();
            InitializeServiceLocatorForTest();
        }

        /// <summary>
        /// 標準的なテストクリーンアップ（各テストの終了時に呼び出し）
        /// </summary>
        public static void StandardTestTeardown()
        {
            CleanupTestGameObjects();
            ServiceLocator.Clear();
        }

        #endregion
    

/// <summary>
        /// テスト用にFeatureFlagsをリセット
        /// </summary>
        public static void ResetFeatureFlagsForTest()
        {
            // FeatureFlags.UseRefactoredArchitecture = false; // 必要に応じて有効化
        }

        /// <summary>
        /// テスト用ServiceLocatorのセットアップ
        /// </summary>
        public static void SetupTestServiceLocator()
        {
            ServiceLocator.Clear();
        }

        /// <summary>
        /// ServiceLocatorのクリーンアップ
        /// </summary>
        public static void CleanupServiceLocator()
        {
            ServiceLocator.Clear();
        }

        /// <summary>
        /// GameObjectにコンポーネントが存在することを確認
        /// </summary>
        public static void AssertHasComponent<T>(GameObject gameObject) where T : Component
        {
            var component = gameObject.GetComponent<T>();
            NUnit.Framework.Assert.IsNotNull(component, $"GameObject '{gameObject.name}' should have component '{typeof(T).Name}'.");
        }


/// <summary>
        /// テストシーンのセットアップ
        /// </summary>
        public static void SetupTestScene()
        {
            // テスト用のシーン初期化処理
            CleanupTestGameObjects();
            InitializeServiceLocatorForTest();
        }
}

    #region Mock Classes

    /// <summary>
    /// AudioServiceのモック実装
    /// </summary>
    public class MockAudioService : IAudioService
    {
        public int PlaySoundCallCount { get; private set; }
        public string LastPlayedSound { get; private set; }
        public Vector3 LastPosition { get; private set; }
        public float LastVolume { get; private set; }
        public bool ShouldReturnPlaying { get; set; } = false;
        public float MasterVolume { get; private set; } = 1f;
        public float BGMVolume { get; private set; } = 1f;
        public float AmbientVolume { get; private set; } = 1f;
        public float EffectVolume { get; private set; } = 1f;
        public Dictionary<string, float> CategoryVolumes { get; private set; } = new Dictionary<string, float>();
        public bool IsPaused { get; private set; } = false;

        public void PlaySound(string soundId, Vector3 position = default, float volume = 1f)
        {
            PlaySoundCallCount++;
            LastPlayedSound = soundId;
            LastPosition = position;
            LastVolume = volume;
        }

        public void StopSound(string soundId)
        {
            // Mock implementation
        }

        public void StopAllSounds()
        {
            // Mock implementation
        }

        public float GetMasterVolume()
        {
            return MasterVolume;
        }

        public void SetMasterVolume(float volume)
        {
            MasterVolume = volume;
        }

        public float GetBGMVolume()
        {
            return BGMVolume;
        }

        public float GetAmbientVolume()
        {
            return AmbientVolume;
        }

        public float GetEffectVolume()
        {
            return EffectVolume;
        }

        public void SetCategoryVolume(string category, float volume)
        {
            CategoryVolumes[category] = volume;
        }

        public bool IsPlaying(string soundId)
        {
            return ShouldReturnPlaying;
        }

        public void Pause()
        {
            IsPaused = true;
        }

        public void Resume()
        {
            IsPaused = false;
        }

        public void Reset()
        {
            PlaySoundCallCount = 0;
            LastPlayedSound = null;
            LastPosition = Vector3.zero;
            LastVolume = 0f;
            ShouldReturnPlaying = false;
            MasterVolume = 1f;
            BGMVolume = 1f;
            AmbientVolume = 1f;
            EffectVolume = 1f;
            CategoryVolumes.Clear();
            IsPaused = false;
        }
    }

    /// <summary>
    /// SpatialAudioServiceのモック実装
    /// </summary>
    public class MockSpatialAudioService : ISpatialAudioService
    {
        public int PlaySoundAtPositionCallCount { get; private set; }
        public Vector3 LastPosition { get; private set; }
        public string LastSpatialSound { get; private set; }
        
        // 3D Sound properties
        public int Play3DSoundCallCount { get; private set; }
        public string LastPlay3DSoundId { get; private set; }
        public Vector3 LastPlay3DPosition { get; private set; }
        public float LastPlay3DMaxDistance { get; private set; }
        public float LastPlay3DVolume { get; private set; }
        
        // Moving Sound properties
        public int CreateMovingSoundCallCount { get; private set; }
        public string LastMovingSoundId { get; private set; }
        public Transform LastMovingSource { get; private set; }
        public float LastMovingMaxDistance { get; private set; }
        
        // Ambient Sound properties
        public string LastAmbientSoundId { get; private set; }
        public float LastAmbientVolume { get; private set; }
        
        // Occlusion properties
        public int UpdateOcclusionCallCount { get; private set; }
        public Vector3 LastListenerPosition { get; private set; }
        public Vector3 LastSourcePosition { get; private set; }
        public float LastOcclusionLevel { get; private set; }
        
        // Reverb properties
        public string LastReverbZoneId { get; private set; }
        public float LastReverbLevel { get; private set; }
        
        // Doppler properties
        public float LastDopplerLevel { get; private set; }
        
        // Listener properties
        public Vector3 LastUpdateListenerPosition { get; private set; }
        public Vector3 LastUpdateListenerForward { get; private set; }

public void Play3DSound(string soundId, Vector3 position, float maxDistance = 50f, float volume = 1f)
        {
            Play3DSoundCallCount++;
            LastPlay3DSoundId = soundId;
            LastPlay3DPosition = position;
            LastPlay3DMaxDistance = maxDistance;
            LastPlay3DVolume = volume;
        }

        public void CreateMovingSound(string soundId, Transform source, float maxDistance = 50f)
        {
            CreateMovingSoundCallCount++;
            LastMovingSoundId = soundId;
            LastMovingSource = source;
            LastMovingMaxDistance = maxDistance;
        }

        public void SetAmbientSound(string soundId, float volume = 0.5f)
        {
            LastAmbientSoundId = soundId;
            LastAmbientVolume = volume;
        }

        public void UpdateOcclusion(Vector3 listenerPosition, Vector3 sourcePosition, float occlusionLevel)
        {
            UpdateOcclusionCallCount++;
            LastListenerPosition = listenerPosition;
            LastSourcePosition = sourcePosition;
            LastOcclusionLevel = occlusionLevel;
        }

        public void SetReverbZone(string zoneId, float reverbLevel)
        {
            LastReverbZoneId = zoneId;
            LastReverbLevel = reverbLevel;
        }

        public void SetDopplerLevel(float level)
        {
            LastDopplerLevel = level;
        }

        public void UpdateListenerPosition(Vector3 position, Vector3 forward)
        {
            LastUpdateListenerPosition = position;
            LastUpdateListenerForward = forward;
        }

        public void PlaySoundAtPosition(string soundName, Vector3 position)
        {
            PlaySoundAtPositionCallCount++;
            LastSpatialSound = soundName;
            LastPosition = position;
        }



        public void Reset()
        {
            PlaySoundAtPositionCallCount = 0;
            LastSpatialSound = null;
            LastPosition = Vector3.zero;
            Play3DSoundCallCount = 0;
            LastPlay3DSoundId = null;
            LastPlay3DPosition = Vector3.zero;
            LastPlay3DMaxDistance = 0f;
            LastPlay3DVolume = 0f;
            CreateMovingSoundCallCount = 0;
            LastMovingSoundId = null;
            LastMovingSource = null;
            LastMovingMaxDistance = 0f;
            LastAmbientSoundId = null;
            LastAmbientVolume = 0f;
            UpdateOcclusionCallCount = 0;
            LastListenerPosition = Vector3.zero;
            LastSourcePosition = Vector3.zero;
            LastOcclusionLevel = 0f;
            LastReverbZoneId = null;
            LastReverbLevel = 0f;
            LastDopplerLevel = 0f;
            LastUpdateListenerPosition = Vector3.zero;
            LastUpdateListenerForward = Vector3.zero;
        }
    }

    /// <summary>
    /// StealthAudioServiceのモック実装
    /// </summary>
    public class MockStealthAudioService : IStealthAudioService
    {
        public int CreateFootstepCallCount { get; private set; }
        public Vector3 LastFootstepPosition { get; private set; }
        public float LastFootstepIntensity { get; private set; }
        public string LastSurfaceType { get; private set; }
        public float EnvironmentNoiseLevel { get; private set; }
        public int EmitDetectableSoundCallCount { get; private set; }
        public Vector3 LastEmitPosition { get; private set; }
        public float LastEmitRadius { get; private set; }
        public float LastEmitIntensity { get; private set; }
        public string LastSoundType { get; private set; }
        public int PlayDistractionCallCount { get; private set; }
        public Vector3 LastDistractionPosition { get; private set; }
        public float LastDistractionRadius { get; private set; }
        public AlertLevel LastAlertLevel { get; private set; }
        public float LastMaskingLevel { get; private set; }
        public int NotifyAuditorySensorsCallCount { get; private set; }
        public Vector3 LastSensorOrigin { get; private set; }
        public float LastSensorRadius { get; private set; }
        public float LastSensorIntensity { get; private set; }
        public float LastStealthLevel { get; private set; }

        public void CreateFootstep(Vector3 position, float intensity, string surfaceType)
        {
            CreateFootstepCallCount++;
            LastFootstepPosition = position;
            LastFootstepIntensity = intensity;
            LastSurfaceType = surfaceType;
        }

        public void SetEnvironmentNoiseLevel(float level)
        {
            EnvironmentNoiseLevel = level;
        }

        public void EmitDetectableSound(Vector3 position, float radius, float intensity, string soundType)
        {
            EmitDetectableSoundCallCount++;
            LastEmitPosition = position;
            LastEmitRadius = radius;
            LastEmitIntensity = intensity;
            LastSoundType = soundType;
        }

        public void PlayDistraction(Vector3 position, float radius)
        {
            PlayDistractionCallCount++;
            LastDistractionPosition = position;
            LastDistractionRadius = radius;
        }

        public void SetAlertLevelMusic(AlertLevel level)
        {
            LastAlertLevel = level;
        }

        public void ApplyAudioMasking(float maskingLevel)
        {
            LastMaskingLevel = maskingLevel;
        }

        public void NotifyAuditorySensors(Vector3 origin, float radius, float intensity)
        {
            NotifyAuditorySensorsCallCount++;
            LastSensorOrigin = origin;
            LastSensorRadius = radius;
            LastSensorIntensity = intensity;
        }

        public void AdjustStealthAudio(float stealthLevel)
        {
            LastStealthLevel = stealthLevel;
        }

        public void Reset()
        {
            CreateFootstepCallCount = 0;
            LastFootstepPosition = Vector3.zero;
            LastFootstepIntensity = 0f;
            LastSurfaceType = null;
            EnvironmentNoiseLevel = 0f;
            EmitDetectableSoundCallCount = 0;
            LastEmitPosition = Vector3.zero;
            LastEmitRadius = 0f;
            LastEmitIntensity = 0f;
            LastSoundType = null;
            PlayDistractionCallCount = 0;
            LastDistractionPosition = Vector3.zero;
            LastDistractionRadius = 0f;
            LastAlertLevel = AlertLevel.None;
            LastMaskingLevel = 0f;
            NotifyAuditorySensorsCallCount = 0;
            LastSensorOrigin = Vector3.zero;
            LastSensorRadius = 0f;
            LastSensorIntensity = 0f;
            LastStealthLevel = 0f;
        }
    }

    #endregion
}