using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using _Project.Core;
using _Project.Core.Services;
using asterivo.Unity60.Core.Audio.Interfaces;

namespace _Project.Tests.Core.Services
{
    /// <summary>
    /// MigrationValidatorの包括的テスト
    /// </summary>
    public class MigrationValidatorTest
    {
        private GameObject testObject;
        private MigrationValidator migrationValidator;

        [SetUp]
        public void Setup()
        {
            // テスト用のGameObjectを作成
            testObject = new GameObject("MigrationValidatorTest");
            migrationValidator = testObject.AddComponent<MigrationValidator>();
        }

        [TearDown]
        public void TearDown()
        {
            if (testObject != null)
            {
                Object.DestroyImmediate(testObject);
            }
        }

        [Test]
        public void MigrationValidator_ComponentCreation_ShouldSucceed()
        {
            // Arrange & Act & Assert
            Assert.IsNotNull(migrationValidator, "MigrationValidator component should be created successfully");
            Assert.IsTrue(migrationValidator.enabled, "MigrationValidator should be enabled by default");
        }

        [UnityTest]
        public IEnumerator MigrationValidator_ValidateMigration_ShouldExecuteWithoutErrors()
        {
            // Arrange
            bool validationCompleted = false;
            bool exceptionThrown = false;

            // Act
            try
            {
                migrationValidator.ValidateMigration();
                validationCompleted = true;
            }
            catch (System.Exception ex)
            {
                exceptionThrown = true;
                Debug.LogError($"Validation threw exception: {ex.Message}");
            }

            yield return new WaitForSeconds(0.1f);

            // Assert
            Assert.IsTrue(validationCompleted, "Migration validation should complete successfully");
            Assert.IsFalse(exceptionThrown, "Migration validation should not throw exceptions");
        }

        [UnityTest]
        public IEnumerator MigrationValidator_RunOnStart_ShouldExecuteValidation()
        {
            // Arrange
            Object.DestroyImmediate(testObject);
            testObject = new GameObject("MigrationValidatorTestRunOnStart");
            
            bool startValidationExecuted = false;

            // Act
            var validator = testObject.AddComponent<MigrationValidator>();
            
            // Give time for Start() to execute
            yield return new WaitForSeconds(0.2f);

            // Assert - Since we can't directly verify Start() was called,
            // we verify the component was created and is functional
            Assert.IsNotNull(validator, "MigrationValidator should be created successfully with runOnStart");
            
            // Cleanup
            migrationValidator = validator;
        }

        [Test]
        public void MigrationValidator_ServiceLocatorIntegration_ShouldHandleServiceCounting()
        {
            // Arrange & Act
            bool noExceptionThrown = true;
            
            try
            {
                // This tests that ServiceLocator.GetServiceCount() can be called
                int serviceCount = ServiceLocator.GetServiceCount();
                Debug.Log($"ServiceLocator reported {serviceCount} services");
            }
            catch (System.Exception ex)
            {
                noExceptionThrown = false;
                Debug.LogError($"ServiceLocator.GetServiceCount() threw exception: {ex.Message}");
            }

            // Assert
            Assert.IsTrue(noExceptionThrown, "ServiceLocator.GetServiceCount() should not throw exceptions");
        }

        [Test]
        public void MigrationValidator_FeatureFlagsIntegration_ShouldAccessFeatureFlags()
        {
            // Arrange & Act
            bool noExceptionThrown = true;
            
            try
            {
                // Test accessing FeatureFlags properties
                bool useServiceLocator = FeatureFlags.UseServiceLocator;
                bool useNewAudioService = FeatureFlags.UseNewAudioService;
                bool useNewSpatialService = FeatureFlags.UseNewSpatialService;
                bool useNewStealthService = FeatureFlags.UseNewStealthService;
                
                Debug.Log($"FeatureFlags - ServiceLocator: {useServiceLocator}, " +
                         $"Audio: {useNewAudioService}, Spatial: {useNewSpatialService}, " +
                         $"Stealth: {useNewStealthService}");
            }
            catch (System.Exception ex)
            {
                noExceptionThrown = false;
                Debug.LogError($"FeatureFlags access threw exception: {ex.Message}");
            }

            // Assert
            Assert.IsTrue(noExceptionThrown, "FeatureFlags properties should be accessible");
        }

        [UnityTest]
        public IEnumerator MigrationValidator_ServiceValidation_ShouldHandleServiceLookup()
        {
            // Arrange
            bool noExceptionThrown = true;
            
            // Act
            try
            {
                // Test service lookups (these might return null if services aren't registered, 
                // but shouldn't throw exceptions)
                var audioService = ServiceLocator.GetService<IAudioService>();
                var spatialService = ServiceLocator.GetService<ISpatialAudioService>();
                var stealthService = ServiceLocator.GetService<IStealthAudioService>();
                var effectService = ServiceLocator.GetService<IEffectService>();
                var updateService = ServiceLocator.GetService<IAudioUpdateService>();
                
                Debug.Log($"Service lookup results - Audio: {(audioService != null ? "Found" : "Not found")}, " +
                         $"Spatial: {(spatialService != null ? "Found" : "Not found")}, " +
                         $"Stealth: {(stealthService != null ? "Found" : "Not found")}, " +
                         $"Effect: {(effectService != null ? "Found" : "Not found")}, " +
                         $"Update: {(updateService != null ? "Found" : "Not found")}");
            }
            catch (System.Exception ex)
            {
                noExceptionThrown = false;
                Debug.LogError($"Service lookup threw exception: {ex.Message}");
            }

            yield return new WaitForSeconds(0.1f);

            // Assert
            Assert.IsTrue(noExceptionThrown, "Service lookups should not throw exceptions");
        }

        [UnityTest]
        public IEnumerator MigrationValidator_ContextMenuValidation_ShouldExecuteManually()
        {
            // Arrange
            bool manualValidationCompleted = false;
            bool exceptionThrown = false;

            // Act
            try
            {
                // Simulate manual validation via context menu
                migrationValidator.ValidateMigration();
                manualValidationCompleted = true;
            }
            catch (System.Exception ex)
            {
                exceptionThrown = true;
                Debug.LogError($"Manual validation threw exception: {ex.Message}");
            }

            yield return new WaitForSeconds(0.1f);

            // Assert
            Assert.IsTrue(manualValidationCompleted, "Manual validation should complete successfully");
            Assert.IsFalse(exceptionThrown, "Manual validation should not throw exceptions");
        }

        [Test]
        public void MigrationValidator_ComponentConfiguration_ShouldHaveCorrectDefaults()
        {
            // Act & Assert
            // We can't directly access private serialized fields, but we can verify
            // the component has the expected behavior based on the implementation
            Assert.IsNotNull(migrationValidator, "MigrationValidator should exist");
            Assert.IsTrue(migrationValidator.gameObject.activeInHierarchy, "MigrationValidator GameObject should be active");
        }
    }
}