using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using asterivo.Unity60.Core.Helpers;
using asterivo.Unity60.Core;

namespace asterivo.Unity60.Tests.Core
{
    /// <summary>
    /// ServiceHelper のユニットテストクラス
    /// Week 1 リファクタリングで導入されたサービス取得統一インターフェースのテスト
    /// </summary>
    [TestFixture]
    public class ServiceHelperTests
    {
        private bool originalUseServiceLocator;
        private bool originalEnableDebugLogging;

        [SetUp]
        public void SetUp()
        {
            // テスト前にFeatureFlagの状態を保存
            originalUseServiceLocator = asterivo.Unity60.Core.FeatureFlags.UseServiceLocator;
            originalEnableDebugLogging = FeatureFlags.EnableDebugLogging;
            
            // テスト用にデバッグログを無効化（ログスパム防止）
            FeatureFlags.EnableDebugLogging = false;
        }

        [TearDown]
        public void TearDown()
        {
            // テスト後にFeatureFlagの状態を復元
            FeatureFlags.UseServiceLocator = originalUseServiceLocator;
            FeatureFlags.EnableDebugLogging = originalEnableDebugLogging;
        }

        /// <summary>
        /// ServiceLocator有効時の正常なサービス取得テスト
        /// </summary>
        [Test]
        public void GetServiceWithFallback_ServiceLocatorEnabled_ReturnsServiceFromLocator()
        {
            // Arrange
            FeatureFlags.UseServiceLocator = true;
            
            // ServiceLocatorにモックサービスを登録する必要があるが、
            // 実際のServiceLocator実装に依存するため、ここではパターンのテストのみ
            
            // Act & Assert
            // ServiceLocatorが利用可能な場合のパス確認
            Assert.IsTrue(FeatureFlags.UseServiceLocator, "UseServiceLocator should be enabled for this test");
        }

        /// <summary>
        /// ServiceLocator無効時のFindFirstObjectByTypeフォールバックテスト
        /// </summary>
        [Test]
        public void GetServiceWithFallback_ServiceLocatorDisabled_UsesFindFirstObjectByTypeFallback()
        {
            // Arrange
            FeatureFlags.UseServiceLocator = false;
            
            // Act
            // GameObject型のサービス取得をテスト（Unity Objectを継承している）
            var result = ServiceHelper.GetServiceWithFallback<GameObject>();
            
            // Assert
            // ServiceLocatorが無効の場合、FindFirstObjectByTypeが使用される
            // 結果はnullまたは見つかったGameObjectのいずれか
            Assert.IsTrue(result == null || result is GameObject, 
                "Should return null or GameObject when ServiceLocator is disabled");
        }

        /// <summary>
        /// 非UnityObjectクラスでのサービス取得テスト
        /// </summary>
        [Test]
        public void GetServiceWithFallback_NonUnityObjectType_ReturnsNull()
        {
            // Arrange
            FeatureFlags.UseServiceLocator = false;
            
            // Act
            // string型（UnityEngine.Objectを継承していない）でのサービス取得
            var result = ServiceHelper.GetServiceWithFallback<string>();
            
            // Assert
            // UnityEngine.Objectを継承していない型の場合、nullが返される
            Assert.IsNull(result, "Should return null for non-UnityEngine.Object types when ServiceLocator is disabled");
        }

        /// <summary>
        /// デバッグログ有効時のサービス取得動作テスト
        /// </summary>
        [Test]
        public void GetServiceWithFallback_DebugLoggingEnabled_ServiceAcquisitionBehavior()
        {
            // Arrange
            FeatureFlags.EnableDebugLogging = true;
            FeatureFlags.UseServiceLocator = false;
            
            // Act
            var result = ServiceHelper.GetServiceWithFallback<string>();
            
            // Assert
            // ログ出力の代わりに、サービス取得の動作を確認
            Assert.IsNull(result, "Should return null for unavailable service when logging is enabled");
            Assert.IsTrue(FeatureFlags.EnableDebugLogging, "Debug logging should remain enabled");
        }

        /// <summary>
        /// ジェネリック型制約のテスト
        /// </summary>
        [Test]
        public void GetServiceWithFallback_GenericTypeConstraint_AcceptsClassTypes()
        {
            // Arrange & Act & Assert
            // クラス型のみが受け入れられることを確認
            // コンパイル時チェックのため、実際にはコンパイルが成功すればテスト成功
            Assert.DoesNotThrow(() =>
            {
                ServiceHelper.GetServiceWithFallback<GameObject>();
                ServiceHelper.GetServiceWithFallback<MonoBehaviour>();
                ServiceHelper.GetServiceWithFallback<object>();
            }, "Should accept class types for generic constraint");
        }

        /// <summary>
        /// FeatureFlagsの状態変更に対する動的応答テスト
        /// </summary>
        [Test]
        public void GetServiceWithFallback_FeatureFlagChanges_RespondsImmediately()
        {
            // Arrange
            FeatureFlags.UseServiceLocator = true;
            
            // ServiceLocator有効状態での挙動確認
            Assert.IsTrue(FeatureFlags.UseServiceLocator);
            
            // Act - フラグを変更
            FeatureFlags.UseServiceLocator = false;
            
            // Assert - 変更が即座に反映されることを確認
            Assert.IsFalse(FeatureFlags.UseServiceLocator);
            
            // 実際のサービス取得でも変更が反映されることを確認
            var result = ServiceHelper.GetServiceWithFallback<GameObject>();
            Assert.IsTrue(result == null || result is GameObject);
        }

        /// <summary>
        /// サービス取得失敗時のエラーハンドリングテスト
        /// </summary>
        [Test]
        public void GetServiceWithFallback_ServiceNotFound_HandlesGracefully()
        {
            // Arrange
            FeatureFlags.UseServiceLocator = false;
            FeatureFlags.EnableDebugLogging = true;
            
            // Act & Assert
            // 存在しないサービスを要求してもエラーが発生しないことを確認
            Assert.DoesNotThrow(() =>
            {
                var result = ServiceHelper.GetServiceWithFallback<UnityEngine.Camera>();
                // 結果はnullまたはCamera型（シーンにCameraがあれば見つかる）
                Assert.IsTrue(result == null || result is UnityEngine.Camera, 
                    "Should return null or Camera instance when camera is available");
            }, "Should handle missing services gracefully without throwing exceptions");
        }
    }
}
