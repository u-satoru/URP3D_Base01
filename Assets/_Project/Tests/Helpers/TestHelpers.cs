using UnityEngine;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using asterivo.Unity60.Core;
using asterivo.Unity60.Core.Audio.Interfaces;
using asterivo.Unity60.Core.Data;

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

        #region Audio System Test Support

        /// <summary>
        /// Audio系テスト用の包括的セットアップ
        /// </summary>
        public static AudioTestContext SetupAudioTestContext()
        {
            SetupTestScene();

            // AudioListenerの作成
            var listenerGO = CreateTestGameObject("TestAudioListener");
            var audioListener = listenerGO.AddComponent<AudioListener>();

            // MainCameraの作成（AudioListenerが必要）
            var cameraGO = CreateTestGameObject("TestMainCamera");
            var camera = cameraGO.AddComponent<UnityEngine.Camera>();
            camera.tag = "MainCamera";

            return new AudioTestContext
            {
                AudioListener = audioListener,
                MainCamera = camera
            };
        }

        /// <summary>
        /// Audio系テスト用のモックサービス一式を登録
        /// </summary>
        public static AudioMockServices RegisterAudioMockServices()
        {
            var audioService = CreateMockAudioService();
            var spatialAudioService = CreateMockSpatialAudioService();
            var stealthAudioService = CreateMockStealthAudioService();

            ServiceLocator.RegisterService<IAudioService>(audioService);
            ServiceLocator.RegisterService<ISpatialAudioService>(spatialAudioService);
            ServiceLocator.RegisterService<IStealthAudioService>(stealthAudioService);

            return new AudioMockServices
            {
                AudioService = audioService,
                SpatialAudioService = spatialAudioService,
                StealthAudioService = stealthAudioService
            };
        }

        /// <summary>
        /// Audio系コンポーネントの動作検証
        /// </summary>
        public static void AssertAudioComponentsWorking(GameObject audioObject)
        {
            var audioSource = audioObject.GetComponent<AudioSource>();
            if (audioSource != null)
            {
                // AudioSourceの基本設定確認
                Assert.That(audioSource.volume, Is.InRange(0f, 1f), "AudioSource volume should be in valid range");
                Assert.That(audioSource.pitch, Is.InRange(0.1f, 3f), "AudioSource pitch should be in valid range");
            }

            var audioListener = Object.FindObjectOfType<AudioListener>();
            Assert.IsNotNull(audioListener, "AudioListener should exist in scene for spatial audio tests");
        }

        /// <summary>
        /// Audio系サービスの統合状態を検証
        /// </summary>
        public static void AssertAudioServicesIntegrated()
        {
            Assert.IsTrue(ServiceLocator.HasService<IAudioService>(), "AudioService should be registered");
            Assert.IsTrue(ServiceLocator.HasService<ISpatialAudioService>(), "SpatialAudioService should be registered");
            Assert.IsTrue(ServiceLocator.HasService<IStealthAudioService>(), "StealthAudioService should be registered");
        }

        /// <summary>
        /// Audio系のパフォーマンス閾値検証
        /// </summary>
        public static void AssertAudioPerformanceThresholds(System.Action audioAction, float maxTimeMs = 5f, int maxAllocationsKB = 100)
        {
            // メモリ使用量測定開始
            long memoryBefore = System.GC.GetTotalMemory(false);
            
            // 実行時間測定
            AssertExecutionTime(audioAction, maxTimeMs, $"Audio operation should complete within {maxTimeMs}ms");
            
            // メモリ使用量測定終了
            System.GC.Collect();
            System.GC.WaitForPendingFinalizers();
            System.GC.Collect();
            
            long memoryAfter = System.GC.GetTotalMemory(true);
            long memoryDelta = (memoryAfter - memoryBefore) / 1024; // KB単位
            
            Assert.That(memoryDelta, Is.LessThan(maxAllocationsKB), 
                $"Audio operation should not allocate more than {maxAllocationsKB}KB (actual: {memoryDelta}KB)");
        }

        /// <summary>
        /// 3D音響のポジション精度検証
        /// </summary>
        public static void AssertSpatialAudioPositioning(Vector3 expectedPosition, Vector3 actualPosition, float toleranceMeters = 0.1f)
        {
            float distance = Vector3.Distance(expectedPosition, actualPosition);
            Assert.That(distance, Is.LessThan(toleranceMeters), 
                $"Spatial audio position should be within {toleranceMeters}m tolerance (actual distance: {distance:F3}m)");
        }

        /// <summary>
        /// オーディオ音量レベルの検証
        /// </summary>
        public static void AssertAudioVolumeLevel(float actualVolume, float expectedVolume, float tolerance = 0.05f)
        {
            Assert.That(actualVolume, Is.EqualTo(expectedVolume).Within(tolerance), 
                $"Audio volume should be {expectedVolume} ± {tolerance} (actual: {actualVolume})");
        }

        #endregion

        #region Test Report Generation

        /// <summary>
        /// テスト実行結果からMarkdownレポートを生成
        /// </summary>
        public static string GenerateMarkdownTestReport(TestReportData reportData)
        {
            var report = new System.Text.StringBuilder();
            
            report.AppendLine("# Audio System Test Verification Report");
            report.AppendLine();
            report.AppendLine($"**Generated**: {System.DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            report.AppendLine($"**Test Environment**: Unity {UnityEngine.Application.unityVersion}");
            report.AppendLine($"**Total Tests**: {reportData.TotalTests}");
            report.AppendLine($"**Passed**: {reportData.PassedTests}");
            report.AppendLine($"**Failed**: {reportData.FailedTests}");
            report.AppendLine($"**Success Rate**: {(reportData.PassedTests / (float)reportData.TotalTests * 100):F1}%");
            report.AppendLine();

            if (reportData.FailedTests > 0)
            {
                report.AppendLine("## ⚠️ Failed Tests");
                foreach (var failure in reportData.Failures)
                {
                    report.AppendLine($"- **{failure.TestName}**: {failure.ErrorMessage}");
                }
                report.AppendLine();
            }

            report.AppendLine("## Performance Metrics");
            report.AppendLine($"- **Average Execution Time**: {reportData.AverageExecutionTime:F2}ms");
            report.AppendLine($"- **Memory Usage**: {reportData.MemoryUsageKB}KB");
            report.AppendLine($"- **Performance Score**: {reportData.PerformanceScore}/100");
            report.AppendLine();

            report.AppendLine("## Recommendations");
            report.AppendLine(reportData.Recommendations ?? "All tests completed successfully.");
            report.AppendLine();

            report.AppendLine("## Next Actions");
            report.AppendLine(reportData.NextActions ?? "Continue with regular testing schedule.");

            return report.ToString();
        }

        /// <summary>
        /// テスト結果をXMLとMarkdown両形式で保存
        /// </summary>
        public static void SaveTestResults(TestReportData reportData, string baseFileName = "audio-system-test")
        {
            var resultsDir = "Assets/_Project/Tests/Results";
            if (!System.IO.Directory.Exists(resultsDir))
            {
                System.IO.Directory.CreateDirectory(resultsDir);
            }

            // XML形式で保存（CI/CD用）
            var xmlPath = System.IO.Path.Combine(resultsDir, $"{baseFileName}-results.xml");
            var xmlContent = GenerateNUnitXmlReport(reportData);
            System.IO.File.WriteAllText(xmlPath, xmlContent);

            // Markdown形式で保存（人間可読用）
            var markdownPath = System.IO.Path.Combine(resultsDir, $"{baseFileName}-verification.md");
            var markdownContent = GenerateMarkdownTestReport(reportData);
            System.IO.File.WriteAllText(markdownPath, markdownContent);

            Debug.Log($"Test results saved to {xmlPath} and {markdownPath}");
        }

        /// <summary>
        /// NUnit標準XML形式でレポートを生成
        /// </summary>
        private static string GenerateNUnitXmlReport(TestReportData reportData)
        {
            var xml = new System.Text.StringBuilder();
            xml.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
            xml.AppendLine($"<test-results name=\"{reportData.TestSuiteName}\" total=\"{reportData.TotalTests}\" " +
                          $"errors=\"0\" failures=\"{reportData.FailedTests}\" " +
                          $"not-run=\"0\" inconclusive=\"0\" ignored=\"0\" skipped=\"0\" invalid=\"0\" " +
                          $"date=\"{System.DateTime.Now:yyyy-MM-dd}\" time=\"{System.DateTime.Now:HH:mm:ss}\">");
            
            xml.AppendLine($"  <environment unity-version=\"{UnityEngine.Application.unityVersion}\" " +
                          $"platform=\"{UnityEngine.Application.platform}\" />");
            
            xml.AppendLine($"  <culture-info current-culture=\"{System.Globalization.CultureInfo.CurrentCulture.Name}\" " +
                          $"current-uiculture=\"{System.Globalization.CultureInfo.CurrentUICulture.Name}\" />");
            
            xml.AppendLine($"  <test-suite type=\"Assembly\" name=\"{reportData.TestSuiteName}\" executed=\"True\" " +
                          $"result=\"{(reportData.FailedTests == 0 ? "Success" : "Failure")}\" " +
                          $"success=\"{(reportData.FailedTests == 0 ? "True" : "False")}\" " +
                          $"time=\"{reportData.AverageExecutionTime / 1000:F3}\" asserts=\"{reportData.TotalAssertions}\">");
            
            xml.AppendLine("    <results>");
            
            // Individual test results would be added here in a real implementation
            foreach (var failure in reportData.Failures)
            {
                xml.AppendLine($"      <test-case name=\"{failure.TestName}\" executed=\"True\" " +
                              $"result=\"Failure\" success=\"False\">");
                xml.AppendLine($"        <failure>");
                xml.AppendLine($"          <message><![CDATA[{failure.ErrorMessage}]]></message>");
                xml.AppendLine($"        </failure>");
                xml.AppendLine($"      </test-case>");
            }
            
            xml.AppendLine("    </results>");
            xml.AppendLine("  </test-suite>");
            xml.AppendLine("</test-results>");
            
            return xml.ToString();
        }

        #endregion
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
            LastAlertLevel = AlertLevel.Relaxed;
            LastMaskingLevel = 0f;
            NotifyAuditorySensorsCallCount = 0;
            LastSensorOrigin = Vector3.zero;
            LastSensorRadius = 0f;
            LastSensorIntensity = 0f;
            LastStealthLevel = 0f;
        }

        // Track objective complete sound calls
        public int PlayObjectiveCompleteSoundCallCount { get; private set; }
        public bool LastObjectiveCompleteWithBonus { get; private set; }

        public void PlayObjectiveCompleteSound(bool withBonus)
        {
            PlayObjectiveCompleteSoundCallCount++;
            LastObjectiveCompleteWithBonus = withBonus;
        }
    }

    #endregion

    #region Support Classes for Audio Testing

    /// <summary>
    /// Audio系テスト用のコンテキスト情報
    /// </summary>
    public class AudioTestContext
    {
        public AudioListener AudioListener { get; set; }
        public UnityEngine.Camera MainCamera { get; set; }
    }

    /// <summary>
    /// Audio系のモックサービス一式
    /// </summary>
    public class AudioMockServices
    {
        public MockAudioService AudioService { get; set; }
        public MockSpatialAudioService SpatialAudioService { get; set; }
        public MockStealthAudioService StealthAudioService { get; set; }

        /// <summary>
        /// すべてのモックサービスをリセット
        /// </summary>
        public void ResetAll()
        {
            AudioService?.Reset();
            SpatialAudioService?.Reset();
            StealthAudioService?.Reset();
        }
    }

    /// <summary>
    /// テストレポート用のデータ構造
    /// </summary>
    public class TestReportData
    {
        public string TestSuiteName { get; set; } = "AudioSystemTests";
        public int TotalTests { get; set; }
        public int PassedTests { get; set; }
        public int FailedTests { get; set; }
        public float AverageExecutionTime { get; set; } // milliseconds
        public long MemoryUsageKB { get; set; }
        public int PerformanceScore { get; set; } = 100;
        public int TotalAssertions { get; set; }
        public List<TestFailure> Failures { get; set; } = new List<TestFailure>();
        public string Recommendations { get; set; }
        public string NextActions { get; set; }
    }

    /// <summary>
    /// テスト失敗情報
    /// </summary>
    public class TestFailure
    {
        public string TestName { get; set; }
        public string ErrorMessage { get; set; }
        public string StackTrace { get; set; }
    }

    #endregion
}
