using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEditor;
using asterivo.Unity60.Core.Setup;

namespace asterivo.Unity60.Tests.Core.Editor
{
    /// <summary>
    /// TASK-003.4 ジャンル選択システム テストスイート
    /// 
    /// 6つのゲームジャンル（FPS/TPS/Platformer/Stealth/Adventure/Strategy）の
    /// 選択システムとGameGenre ScriptableObjectの統合テスト
    /// 
    /// テスト対象：
    /// - GameGenre ScriptableObjectの基本機能
    /// - GenreManager の初期化・管理機能
    /// - ジャンルテンプレートの存在確認
    /// - SetupWizardWindow との連携テスト
    /// </summary>
    public class GenreSelectionSystemTests
    {
        private GenreManager genreManager;
        private GameGenreType[] expectedGenres;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            // 期待されるジャンル一覧
            expectedGenres = new GameGenreType[]
            {
                GameGenreType.FPS,
                GameGenreType.TPS,
                GameGenreType.Platformer,
                GameGenreType.Stealth,
                GameGenreType.Adventure,
                GameGenreType.Strategy
            };
        }

        [SetUp]
        public void SetUp()
        {
            // GenreManagerの初期化
            var managers = Resources.FindObjectsOfTypeAll<GenreManager>();
            if (managers.Length > 0)
            {
                genreManager = managers[0];
            }
            else
            {
                // GenreManagerがない場合は作成
                genreManager = ScriptableObject.CreateInstance<GenreManager>();
            }
            
            genreManager.Initialize();
        }

        [TearDown]
        public void TearDown()
        {
            if (genreManager != null && !AssetDatabase.Contains(genreManager))
            {
                Object.DestroyImmediate(genreManager);
            }
        }

        #region 基本機能テスト

        [Test]
        public void GenreManager_ShouldInitializeSuccessfully()
        {
            // Arrange & Act
            genreManager.Initialize();

            // Assert
            Assert.IsNotNull(genreManager, "GenreManager should not be null after initialization");
            Assert.Greater(genreManager.GenreCount, 0, "GenreManager should load at least one genre");
        }

        [Test]
        public void GenreManager_ShouldLoadAllExpectedGenres()
        {
            // Arrange & Act
            var supportedGenres = genreManager.GetSupportedGenreTypes();

            // Assert
            Assert.IsNotNull(supportedGenres, "Supported genres should not be null");
            
            foreach (var expectedGenre in expectedGenres)
            {
                Assert.Contains(expectedGenre, supportedGenres, 
                    $"Genre {expectedGenre} should be supported");
            }
        }

        [Test]
        public void GameGenre_AllTemplatesShouldExist()
        {
            // Arrange & Act & Assert
            foreach (var genreType in expectedGenres)
            {
                var genre = genreManager.GetGenre(genreType);
                Assert.IsNotNull(genre, $"Genre template for {genreType} should exist");
                
                // 基本プロパティの確認
                Assert.AreEqual(genreType, genre.GenreType, 
                    $"Genre type should match for {genreType}");
                Assert.IsNotEmpty(genre.DisplayName, 
                    $"Display name should not be empty for {genreType}");
                Assert.IsNotEmpty(genre.Description, 
                    $"Description should not be empty for {genreType}");
            }
        }

        #endregion

        #region ジャンル固有テスト

        [Test]
        public void FPSGenre_ShouldHaveCorrectConfiguration()
        {
            // Arrange & Act
            var fpsGenre = genreManager.GetGenre(GameGenreType.FPS);

            // Assert
            Assert.IsNotNull(fpsGenre, "FPS genre should exist");
            Assert.AreEqual("First Person Shooter", fpsGenre.DisplayName, 
                "FPS display name should be correct");
            
            // FPS固有の設定確認
            Assert.IsTrue(fpsGenre.CameraConfig.firstPersonView, 
                "FPS should use first person view");
            Assert.Greater(fpsGenre.MovementConfig.runSpeed, fpsGenre.MovementConfig.walkSpeed, 
                "Run speed should be greater than walk speed");
        }

        [Test]
        public void StealthGenre_ShouldHaveCorrectConfiguration()
        {
            // Arrange & Act
            var stealthGenre = genreManager.GetGenre(GameGenreType.Stealth);

            // Assert
            Assert.IsNotNull(stealthGenre, "Stealth genre should exist");
            Assert.AreEqual("Stealth Action", stealthGenre.DisplayName, 
                "Stealth display name should be correct");
            
            // ステルス固有の設定確認
            Assert.IsTrue(stealthGenre.AIConfig.visualSensorEnabled, 
                "Stealth should have visual sensor enabled");
            Assert.IsTrue(stealthGenre.AudioConfig.use3DAudio, 
                "Stealth should use 3D audio");
        }

        [Test]
        public void AdventureGenre_ShouldHaveCorrectConfiguration()
        {
            // Arrange & Act
            var adventureGenre = genreManager.GetGenre(GameGenreType.Adventure);

            // Assert
            Assert.IsNotNull(adventureGenre, "Adventure genre should exist");
            Assert.AreEqual("Adventure Game", adventureGenre.DisplayName, 
                "Adventure display name should be correct");
            
            // アドベンチャー固有の設定確認
            Assert.IsFalse(adventureGenre.CameraConfig.firstPersonView, 
                "Adventure should not use first person view");
            Assert.IsTrue(adventureGenre.AudioConfig.useEnvironmentalAudio, 
                "Adventure should use environmental audio");
        }

        #endregion

        #region 設定妥当性テスト

        [Test]
        public void AllGenres_ShouldHaveValidConfiguration()
        {
            // Arrange & Act & Assert
            foreach (var genreType in expectedGenres)
            {
                var genre = genreManager.GetGenre(genreType);
                Assert.IsNotNull(genre, $"Genre {genreType} should exist");
                
                // 設定妥当性チェック
                Assert.IsTrue(genre.ValidateConfiguration(), 
                    $"Genre {genreType} should have valid configuration");
                
                // 基本的な数値妥当性
                Assert.Greater(genre.MovementConfig.walkSpeed, 0, 
                    $"Walk speed should be positive for {genreType}");
                Assert.GreaterOrEqual(genre.MovementConfig.runSpeed, genre.MovementConfig.walkSpeed, 
                    $"Run speed should be >= walk speed for {genreType}");
                Assert.Greater(genre.CameraConfig.defaultFOV, 0, 
                    $"FOV should be positive for {genreType}");
                Assert.Greater(genre.AIConfig.maxNPCCount, 0, 
                    $"Max NPC count should be positive for {genreType}");
            }
        }

        [Test]
        public void GenreManager_DisplayNamesShouldBeUnique()
        {
            // Arrange
            var allGenres = genreManager.AvailableGenres;

            // Act
            var displayNames = allGenres.Select(g => g.DisplayName).ToArray();
            var uniqueNames = displayNames.Distinct().ToArray();

            // Assert
            Assert.AreEqual(displayNames.Length, uniqueNames.Length, 
                "All genre display names should be unique");
        }

        #endregion

        #region パフォーマンステスト

        [Test]
        public void GenreManager_InitializationShouldBeFast()
        {
            // Arrange
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            // Act
            genreManager.Initialize();
            stopwatch.Stop();

            // Assert
            Assert.Less(stopwatch.ElapsedMilliseconds, 1000, 
                "Genre manager initialization should complete within 1 second");
        }

        [Test]
        public void GenreManager_GetGenreShouldBeFast()
        {
            // Arrange
            genreManager.Initialize();
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            // Act
            for (int i = 0; i < 100; i++)
            {
                foreach (var genreType in expectedGenres)
                {
                    genreManager.GetGenre(genreType);
                }
            }
            stopwatch.Stop();

            // Assert
            Assert.Less(stopwatch.ElapsedMilliseconds, 100, 
                "Genre retrieval should be very fast (600 calls in <100ms)");
        }

        #endregion

        #region 統合テスト

        [Test]
        public void SetupWizard_ShouldIntegrateWithGenreManager()
        {
            // Arrange & Act
            // SetupWizardWindowのGenreManager統合をテスト
            var supportedGenres = genreManager.GetSupportedGenreTypes();

            // Assert
            Assert.IsNotNull(supportedGenres, "Setup wizard should get supported genres");
            Assert.AreEqual(6, supportedGenres.Length, 
                "Setup wizard should support all 6 genres");
            
            // 各ジャンルの表示データ確認
            foreach (var genreType in supportedGenres)
            {
                var displayName = genreManager.GetDisplayName(genreType);
                var description = genreManager.GetDescription(genreType);
                
                Assert.IsNotEmpty(displayName, 
                    $"Display name should be available for {genreType}");
                Assert.IsNotEmpty(description, 
                    $"Description should be available for {genreType}");
            }
        }

        [UnityTest]
        public IEnumerator GenreSelection_ShouldWorkInPlayMode()
        {
            // Arrange
            yield return new WaitForEndOfFrame();

            // Act
            genreManager.Initialize();
            var selectedGenre = genreManager.GetGenre(GameGenreType.Adventure);

            // Assert
            Assert.IsNotNull(selectedGenre, "Should be able to select genre in play mode");
            Assert.AreEqual(GameGenreType.Adventure, selectedGenre.GenreType, 
                "Selected genre type should match");

            yield return null;
        }

        #endregion

        #region エラーハンドリングテスト

        [Test]
        public void GenreManager_ShouldHandleMissingGenreGracefully()
        {
            // Arrange
            genreManager.Initialize();

            // Act & Assert
            // 存在しないジャンルタイプを要求した場合のテスト
            Assert.DoesNotThrow(() =>
            {
                var nonExistentGenre = genreManager.GetGenre((GameGenreType)999);
                Assert.IsNull(nonExistentGenre, "Non-existent genre should return null");
            });
        }

        [Test]
        public void GameGenre_ShouldLogConfigurationInformationCorrectly()
        {
            // Arrange
            var testGenre = genreManager.GetGenre(GameGenreType.FPS);
            Assert.IsNotNull(testGenre, "Test genre should exist");

            // Act & Assert
            Assert.DoesNotThrow(() =>
            {
                testGenre.LogConfiguration();
            }, "LogConfiguration should not throw exceptions");
        }

        #endregion

        #region ユーティリティテスト

        [Test]
        public void GenreManager_StatisticsShouldBeAccurate()
        {
            // Arrange
            genreManager.Initialize();

            // Act & Assert
            Assert.DoesNotThrow(() =>
            {
                genreManager.LogGenreStatistics();
            }, "Statistics logging should not throw exceptions");

            // 統計情報の基本チェック
            Assert.AreEqual(6, genreManager.GenreCount, 
                "Genre count statistics should be accurate");
        }

        #endregion
    }
}
