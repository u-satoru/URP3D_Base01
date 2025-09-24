using UnityEngine;
using UnityEditor;
using asterivo.Unity60.Core.Setup;
using System.Linq;

namespace asterivo.Unity60.Tests.Core.Editor
{
    /// <summary>
    /// GenreSelectionSystem手動検証用ツール
    /// Test Runnerが利用できない環境でのテスト実行・検証
    /// </summary>
    public static class GenreSystemManualTest
    {
        [MenuItem("asterivo.Unity60/Tests/Run Genre System Tests")]
        public static void RunManualTests()
        {
            Debug.Log("=== Genre System Manual Tests ===");
            
            bool allPassed = true;
            int totalTests = 0;
            int passedTests = 0;
            
            // Test 1: GenreManager初期化テスト
            totalTests++;
            if (TestGenreManagerInitialization())
            {
                passedTests++;
                Debug.Log("✅ GenreManager Initialization Test: PASSED");
            }
            else
            {
                allPassed = false;
                Debug.LogError("❌ GenreManager Initialization Test: FAILED");
            }
            
            // Test 2: 全ジャンル読み込みテスト  
            totalTests++;
            if (TestAllGenresLoaded())
            {
                passedTests++;
                Debug.Log("✅ All Genres Loaded Test: PASSED");
            }
            else
            {
                allPassed = false;
                Debug.LogError("❌ All Genres Loaded Test: FAILED");
            }
            
            // Test 3: ジャンル固有設定テスト
            totalTests++;
            if (TestGenreSpecificSettings())
            {
                passedTests++;
                Debug.Log("✅ Genre Specific Settings Test: PASSED");
            }
            else
            {
                allPassed = false;
                Debug.LogError("❌ Genre Specific Settings Test: FAILED");
            }
            
            // Test 4: 設定妥当性テスト
            totalTests++;
            if (TestConfigurationValidity())
            {
                passedTests++;
                Debug.Log("✅ Configuration Validity Test: PASSED");
            }
            else
            {
                allPassed = false;
                Debug.LogError("❌ Configuration Validity Test: FAILED");
            }
            
            // Test 5: パフォーマンステスト
            totalTests++;
            if (TestPerformance())
            {
                passedTests++;
                Debug.Log("✅ Performance Test: PASSED");
            }
            else
            {
                allPassed = false;
                Debug.LogError("❌ Performance Test: FAILED");
            }
            
            // 結果サマリー
            Debug.Log($"=== Test Results Summary ===");
            Debug.Log($"Total Tests: {totalTests}");
            Debug.Log($"Passed: {passedTests}");
            Debug.Log($"Failed: {totalTests - passedTests}");
            Debug.Log($"Success Rate: {(passedTests * 100 / totalTests):F1}%");
            
            if (allPassed)
            {
                Debug.Log("🎉 All tests PASSED! Genre Selection System is working correctly.");
            }
            else
            {
                Debug.LogWarning("⚠️ Some tests FAILED. Please check the logs above.");
            }
        }
        
        private static bool TestGenreManagerInitialization()
        {
            try
            {
                var managers = Resources.FindObjectsOfTypeAll<GenreManager>();
                if (managers.Length == 0)
                {
                    Debug.LogError("No GenreManager found");
                    return false;
                }
                
                var genreManager = managers[0];
                genreManager.Initialize();
                
                return genreManager.GenreCount > 0;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"GenreManager initialization failed: {ex.Message}");
                return false;
            }
        }
        
        private static bool TestAllGenresLoaded()
        {
            try
            {
                var managers = Resources.FindObjectsOfTypeAll<GenreManager>();
                if (managers.Length == 0) return false;
                
                var genreManager = managers[0];
                genreManager.Initialize();
                
                var expectedGenres = new GameGenreType[]
                {
                    GameGenreType.FPS,
                    GameGenreType.TPS,
                    GameGenreType.Platformer,
                    GameGenreType.Stealth,
                    GameGenreType.Adventure,
                    GameGenreType.Strategy
                };
                
                var supportedGenres = genreManager.GetSupportedGenreTypes();
                
                foreach (var expectedGenre in expectedGenres)
                {
                    if (!supportedGenres.Contains(expectedGenre))
                    {
                        Debug.LogError($"Missing genre: {expectedGenre}");
                        return false;
                    }
                    
                    var genre = genreManager.GetGenre(expectedGenre);
                    if (genre == null)
                    {
                        Debug.LogError($"Genre {expectedGenre} is null");
                        return false;
                    }
                }
                
                return supportedGenres.Length == expectedGenres.Length;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"All genres test failed: {ex.Message}");
                return false;
            }
        }
        
        private static bool TestGenreSpecificSettings()
        {
            try
            {
                var managers = Resources.FindObjectsOfTypeAll<GenreManager>();
                if (managers.Length == 0) return false;
                
                var genreManager = managers[0];
                genreManager.Initialize();
                
                // FPSジャンルテスト
                var fpsGenre = genreManager.GetGenre(GameGenreType.FPS);
                if (fpsGenre == null || !fpsGenre.CameraConfig.firstPersonView)
                {
                    Debug.LogError("FPS genre should use first person view");
                    return false;
                }
                
                // ステルスジャンルテスト
                var stealthGenre = genreManager.GetGenre(GameGenreType.Stealth);
                if (stealthGenre == null || !stealthGenre.AIConfig.visualSensorEnabled)
                {
                    Debug.LogError("Stealth genre should have visual sensor enabled");
                    return false;
                }
                
                return true;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Genre specific settings test failed: {ex.Message}");
                return false;
            }
        }
        
        private static bool TestConfigurationValidity()
        {
            try
            {
                var managers = Resources.FindObjectsOfTypeAll<GenreManager>();
                if (managers.Length == 0) return false;
                
                var genreManager = managers[0];
                genreManager.Initialize();
                
                var supportedGenres = genreManager.GetSupportedGenreTypes();
                
                foreach (var genreType in supportedGenres)
                {
                    var genre = genreManager.GetGenre(genreType);
                    if (genre == null) continue;
                    
                    // 基本妥当性チェック
                    if (string.IsNullOrEmpty(genre.DisplayName) || 
                        string.IsNullOrEmpty(genre.Description))
                    {
                        Debug.LogError($"Genre {genreType} has invalid basic properties");
                        return false;
                    }
                    
                    // 数値妥当性チェック
                    if (genre.MovementConfig.walkSpeed <= 0 ||
                        genre.MovementConfig.runSpeed < genre.MovementConfig.walkSpeed ||
                        genre.CameraConfig.defaultFOV <= 0 ||
                        genre.AIConfig.maxNPCCount <= 0)
                    {
                        Debug.LogError($"Genre {genreType} has invalid numeric values");
                        return false;
                    }
                }
                
                return true;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Configuration validity test failed: {ex.Message}");
                return false;
            }
        }
        
        private static bool TestPerformance()
        {
            try
            {
                var managers = Resources.FindObjectsOfTypeAll<GenreManager>();
                if (managers.Length == 0) return false;
                
                var genreManager = managers[0];
                
                // 初期化パフォーマンステスト
                var stopwatch = System.Diagnostics.Stopwatch.StartNew();
                genreManager.Initialize();
                stopwatch.Stop();
                
                if (stopwatch.ElapsedMilliseconds > 1000)
                {
                    Debug.LogError($"Initialization too slow: {stopwatch.ElapsedMilliseconds}ms (should be <1000ms)");
                    return false;
                }
                
                // アクセスパフォーマンステスト
                stopwatch.Restart();
                for (int i = 0; i < 100; i++)
                {
                    genreManager.GetGenre(GameGenreType.Adventure);
                    genreManager.GetGenre(GameGenreType.FPS);
                }
                stopwatch.Stop();
                
                if (stopwatch.ElapsedMilliseconds > 100)
                {
                    Debug.LogError($"Access too slow: {stopwatch.ElapsedMilliseconds}ms (should be <100ms)");
                    return false;
                }
                
                return true;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Performance test failed: {ex.Message}");
                return false;
            }
        }
        
        [MenuItem("asterivo.Unity60/Tests/Genre System Info")]
        public static void ShowGenreSystemInfo()
        {
            Debug.Log("=== Genre System Information ===");
            
            var managers = Resources.FindObjectsOfTypeAll<GenreManager>();
            if (managers.Length == 0)
            {
                Debug.LogWarning("No GenreManager found");
                return;
            }
            
            var genreManager = managers[0];
            genreManager.Initialize();
            
            Debug.Log($"Genre Count: {genreManager.GenreCount}");
            
            var supportedGenres = genreManager.GetSupportedGenreTypes();
            foreach (var genreType in supportedGenres)
            {
                var genre = genreManager.GetGenre(genreType);
                if (genre != null)
                {
                    Debug.Log($"Genre: {genreType} - {genre.DisplayName}");
                    Debug.Log($"  Required Modules: {genre.RequiredModules.Count}");
                    Debug.Log($"  Camera: {(genre.CameraConfig.firstPersonView ? "FPS" : "TPS")}, FOV: {genre.CameraConfig.defaultFOV}°");
                    Debug.Log($"  Movement: Walk {genre.MovementConfig.walkSpeed}m/s, Run {genre.MovementConfig.runSpeed}m/s");
                    Debug.Log($"  AI: Max NPCs {genre.AIConfig.maxNPCCount}, Detection Range {genre.AIConfig.defaultDetectionRange}m");
                }
            }
            
            Debug.Log("=== End of Genre System Information ===");
        }
    }
}
