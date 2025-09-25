using NUnit.Framework;
using asterivo.Unity60.Core;
using asterivo.Unity60.Core.Services;
using asterivo.Unity60.Core.Events;
using asterivo.Unity60.Core.Commands;
using asterivo.Unity60.Core.Debug;

namespace asterivo.Unity60.Tests.Core
{
    /// <summary>
    /// Core層の基本的な機能が正しく動作することを検証する簡単なテスト
    /// Phase 2.3の検証用
    /// </summary>
    [TestFixture]
    public class SimpleValidationTest
    {
        [Test]
        public void Core_NamespaceExists()
        {
            // Core層の名前空間が存在することを確認
            Assert.IsNotNull(typeof(ServiceLocator));
            Assert.IsNotNull(typeof(GameEvent));
            Assert.IsNotNull(typeof(ICommand));
            Assert.IsNotNull(typeof(ProjectDebug));
        }

        [Test]
        public void ServiceLocator_BasicOperations()
        {
            // ServiceLocatorの基本的な操作が可能か確認
            ServiceLocator.Clear();

            // サービスの登録と取得
            var testService = new TestServiceImpl();
            ServiceLocator.RegisterService<ITestService>(testService);

            var retrievedService = ServiceLocator.GetService<ITestService>();
            Assert.IsNotNull(retrievedService);
            Assert.AreEqual(testService, retrievedService);

            ServiceLocator.Clear();
        }

        [Test]
        public void ProjectDebug_CanLog()
        {
            // ProjectDebugがログ出力可能か確認
            Assert.DoesNotThrow(() => {
                ProjectDebug.Log("Test message");
                ProjectDebug.LogWarning("Test warning");
                ProjectDebug.LogError("Test error");
            });
        }

        // テスト用のインターフェースと実装
        private interface ITestService
        {
            string GetName();
        }

        private class TestServiceImpl : ITestService
        {
            public string GetName() => "TestService";
        }
    }
}
