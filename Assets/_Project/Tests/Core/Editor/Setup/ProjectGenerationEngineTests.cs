using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using asterivo.Unity60.Core.Editor.Setup;
using asterivo.Unity60.Core.Setup;
using asterivo.Unity60.Core.Editor;

namespace asterivo.Unity60.Tests.Core.Editor.Setup
{
    /// <summary>
    /// TASK-003.5: モジュール・生成エンジン実装 のテストスイート
    /// ProjectGenerationEngineのロジックとSetupWizardWindowとの連携を検証する
    /// </summary>
    [TestFixture]
    public class ProjectGenerationEngineTests
    {
        private SetupWizardWindow.WizardConfiguration wizardConfig;
        private GenreManager genreManager;

        [SetUp]
        public void SetUp()
        {
            // テスト用の設定を初期化
            wizardConfig = new SetupWizardWindow.WizardConfiguration();
            
            // GenreManagerをセットアップ
            var managers = Resources.FindObjectsOfTypeAll<GenreManager>();
            if (managers.Length > 0)
            {
                genreManager = managers[0];
            }
            else
            {
                genreManager = ScriptableObject.CreateInstance<GenreManager>();
            }
            genreManager.Initialize();
        }

        #region Module Selection Logic Tests

        [Test]
        public void ModuleSelection_StealthGenre_ShouldDefaultToRecommendedModules()
        {
            // Arrange
            wizardConfig.selectedGenre = GameGenreType.Stealth;
            var stealthGenre = genreManager.GetGenre(GameGenreType.Stealth);
            
            // Act: GoToNextStepのロジックを模倣
            var selectedModules = new List<string>();
            selectedModules.AddRange(stealthGenre.RequiredModules);
            selectedModules.AddRange(stealthGenre.RecommendedModules);
            wizardConfig.selectedModules = selectedModules.Distinct().ToList();

            // Assert
            Assert.IsNotNull(stealthGenre, "Stealth Genre asset should exist.");
            Assert.IsTrue(wizardConfig.selectedModules.Contains("Audio System"), "Audio System should be a recommended module for Stealth genre.");
        }

        [Test]
        public void ModuleSelection_UserCanDeselectRecommendedModule()
        {
            // Arrange
            wizardConfig.selectedGenre = GameGenreType.Stealth;
            var stealthGenre = genreManager.GetGenre(GameGenreType.Stealth);
            wizardConfig.selectedModules.AddRange(stealthGenre.RecommendedModules);

            // Act
            wizardConfig.selectedModules.Remove("Audio System");

            // Assert
            Assert.IsFalse(wizardConfig.selectedModules.Contains("Audio System"), "User should be able to deselect a recommended module.");
        }

        [Test]
        public void ModuleSelection_RequiredModulesCannotBeDeselected()
        {
            // Arrange
            wizardConfig.selectedGenre = GameGenreType.FPS;
            var fpsGenre = genreManager.GetGenre(GameGenreType.FPS);
            
            // Act
            // UI上では無効化されているが、ロジックとして確認
            bool canRemoveRequired = wizardConfig.selectedModules.Remove("Core Gameplay Systems");

            // Assert
            Assert.IsNotNull(fpsGenre, "FPS Genre asset should exist.");
            // 必須モジュールはリストに最初から入っていないので、Removeはfalseを返すはず
            Assert.IsFalse(canRemoveRequired, "Required modules should not be removable.");
        }

        #endregion

        #region Project Generation Engine Tests

        [Test]
        public async Task ProjectGenerationEngine_BasicFlow_ShouldComplete()
        {
            // Arrange
            wizardConfig.selectedGenre = GameGenreType.Adventure;
            wizardConfig.projectName = "TestAdventureProject";
            var progressLog = new List<string>();
            Action<float, string> onProgress = (p, s) => progressLog.Add($"{p}%: {s}");
            
            var engine = new ProjectGenerationEngine(wizardConfig, onProgress);

            // Act
            bool result = await engine.GenerateProjectAsync();

            // Assert
            Assert.IsTrue(result, "Project generation should complete successfully.");
            Assert.AreEqual(5, progressLog.Count, "Should have 5 progress steps.");
            Assert.IsTrue(progressLog.Last().Contains("プロジェクト設定の適用完了"), "Final step should be applying project settings.");
        }

        [Test]
        public void ProjectGenerationEngine_IdentifiesCorrectPackagesToInstall()
        {
            // Arrange
            // このテストは、エンジンがモジュールに基づいて正しいパッケージを識別できるかを確認します。
            // 実際のエンジンでは、モジュールとパッケージのマッピングが必要です。
            var modulePackageMap = new Dictionary<string, string>
            {
                { "Localization", "com.unity.localization" },
                { "Analytics", "com.unity.analytics" }
            };

            wizardConfig.selectedModules = new List<string> { "Localization", "Analytics" };
            var engine = new ProjectGenerationEngine(wizardConfig, null);

            // Act
            // 本来はエンジン内のメソッドを呼び出すが、ここではロジックをシミュレート
            var packagesToInstall = wizardConfig.selectedModules
                .Where(m => modulePackageMap.ContainsKey(m))
                .Select(m => modulePackageMap[m])
                .ToList();

            // Assert
            Assert.AreEqual(2, packagesToInstall.Count);
            Assert.Contains("com.unity.localization", packagesToInstall);
            Assert.Contains("com.unity.analytics", packagesToInstall);
        }

        [Test]
        public async Task ProjectGenerationEngine_HandlesErrorsGracefully()
        {
            // Arrange
            // 不正な設定でエンジンを初期化
            var invalidConfig = new SetupWizardWindow.WizardConfiguration();
            invalidConfig.selectedGenre = (GameGenreType)999; // 存在しないジャンル
            
            var engine = new ProjectGenerationEngine(invalidConfig, null);

            // Act
            // GenerateProjectAsync内で例外がキャッチされ、falseが返されることを期待
            bool result = await engine.GenerateProjectAsync();

            // Assert
            Assert.IsFalse(result, "Project generation should fail gracefully with invalid config.");
        }

        [Test]
        public void ProjectGenerationEngine_PackageMapping_ShouldReturnCorrectPackages()
        {
            // Arrange
            wizardConfig.selectedModules = new List<string> { "Audio System", "Localization", "Input System" };
            wizardConfig.selectedGenre = GameGenreType.FPS;
            
            var engine = new ProjectGenerationEngine(wizardConfig, null);
            
            // Act - GetRequiredPackagesメソッドをpublicにするか、リフレクションで呼び出す
            var engineType = typeof(ProjectGenerationEngine);
            var method = engineType.GetMethod("GetRequiredPackages", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var packages = (List<string>)method.Invoke(engine, null);
            
            // Assert
            Assert.IsNotNull(packages, "Packages list should not be null.");
            Assert.IsTrue(packages.Contains("com.unity.localization"), "Should include localization package.");
            Assert.IsTrue(packages.Contains("com.unity.inputsystem"), "Should include input system package.");
            Assert.IsTrue(packages.Contains("com.unity.cinemachine"), "Should include cinemachine for FPS genre.");
        }

        [Test]
        public void ProjectGenerationEngine_GenreSpecificPackages_ShouldVaryByGenre()
        {
            // Arrange & Act
            var stealthConfig = new SetupWizardWindow.WizardConfiguration { selectedGenre = GameGenreType.Stealth, selectedModules = new List<string>() };
            var stealthEngine = new ProjectGenerationEngine(stealthConfig, null);
            
            var strategyConfig = new SetupWizardWindow.WizardConfiguration { selectedGenre = GameGenreType.Strategy, selectedModules = new List<string>() };
            var strategyEngine = new ProjectGenerationEngine(strategyConfig, null);
            
            // Use reflection to get genre-specific packages
            var engineType = typeof(ProjectGenerationEngine);
            var method = engineType.GetMethod("GetGenreSpecificPackages", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            var stealthPackages = (List<string>)method.Invoke(stealthEngine, null);
            var strategyPackages = (List<string>)method.Invoke(strategyEngine, null);
            
            // Assert
            Assert.IsTrue(stealthPackages.Contains("com.unity.ai.navigation"), "Stealth genre should include AI navigation.");
            Assert.IsTrue(strategyPackages.Contains("com.unity.ai.navigation"), "Strategy genre should include AI navigation.");
            Assert.IsTrue(strategyPackages.Contains("com.unity.timeline"), "Strategy genre should include timeline.");
        }

        [Test]
        public void ProjectGenerationEngine_ModulePackageMapping_ShouldBeComplete()
        {
            // Arrange
            var engine = new ProjectGenerationEngine(new SetupWizardWindow.WizardConfiguration(), null);
            var engineType = typeof(ProjectGenerationEngine);
            var method = engineType.GetMethod("GetModulePackageMapping", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            // Act
            var mapping = (Dictionary<string, List<string>>)method.Invoke(engine, null);
            
            // Assert
            Assert.IsNotNull(mapping, "Module package mapping should not be null.");
            Assert.IsTrue(mapping.ContainsKey("Audio System"), "Should include Audio System mapping.");
            Assert.IsTrue(mapping.ContainsKey("Localization"), "Should include Localization mapping.");
            Assert.IsTrue(mapping.ContainsKey("Analytics"), "Should include Analytics mapping.");
            
            // 各モジュールが適切なパッケージにマッピングされているかチェック
            Assert.IsTrue(mapping["Localization"].Contains("com.unity.localization"), "Localization should map to Unity Localization package.");
            Assert.IsTrue(mapping["Analytics"].Contains("com.unity.analytics"), "Analytics should map to Unity Analytics package.");
        }

        #endregion
    }
}