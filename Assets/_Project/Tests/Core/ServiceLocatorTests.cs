using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using asterivo.Unity60.Core;
using asterivo.Unity60.Core;
using System;

namespace asterivo.Unity60.Tests.Core
{
    /// <summary>
    /// ServiceLocator の包括的なテストクラス
    /// サービスの登録、取得、ファクトリメソッド、エラーハンドリングの動作を検証
    /// </summary>
    [TestFixture]
    public class ServiceLocatorTests
    {
        // テスト用のダミーサービスインターフェース
        private interface ITestService
        {
            string GetName();
            int GetValue();
        }
        
        private interface IAnotherService
        {
            bool IsActive();
        }
        
        // テスト用のダミーサービス実装
        private class TestServiceImpl : ITestService
        {
            public string Name { get; set; }
            public int Value { get; set; }
            
            public TestServiceImpl(string name = "TestService", int value = 42)
            {
                Name = name;
                Value = value;
            }
            
            public string GetName() => Name;
            public int GetValue() => Value;
        }
        
        private class AnotherServiceImpl : IAnotherService
        {
            public bool Active { get; set; }
            
            public AnotherServiceImpl(bool active = true)
            {
                Active = active;
            }
            
            public bool IsActive() => Active;
        }
        
        // コンストラクタ引数を持つサービス（ファクトリテスト用）
        private class ComplexService : ITestService
        {
            private readonly string prefix;
            private readonly int multiplier;
            
            public ComplexService(string prefix, int multiplier)
            {
                this.prefix = prefix;
                this.multiplier = multiplier;
            }
            
            public string GetName() => $"{prefix}_Complex";
            public int GetValue() => 100 * multiplier;
        }

        [SetUp]
        public void SetUp()
        {
            // 各テスト前にServiceLocatorをクリア
            ServiceLocator.Clear();
        }

        [TearDown]
        public void TearDown()
        {
            // 各テスト後もServiceLocatorをクリア
            ServiceLocator.Clear();
        }

        #region Service Registration Tests

        /// <summary>
        /// サービスの基本的な登録動作テスト
        /// </summary>
        [Test]
        public void ServiceLocator_RegisterService_StoresServiceCorrectly()
        {
            // Arrange
            var testService = new TestServiceImpl("RegisterTest", 123);
            
            // Act
            ServiceLocator.RegisterService<ITestService>(testService);
            
            // Assert
            Assert.AreEqual(1, ServiceLocator.GetServiceCount(), "サービス数が正しくない");
            Assert.IsTrue(ServiceLocator.HasService<ITestService>(), "サービスが登録されていない");
        }

        /// <summary>
        /// 同じ型のサービスの重複登録テスト
        /// </summary>
        [Test]
        public void ServiceLocator_RegisterService_ReplacesPreviousService()
        {
            // Arrange
            var firstService = new TestServiceImpl("First", 1);
            var secondService = new TestServiceImpl("Second", 2);
            
            // Act - 重複登録（警告は出るが正常に置き換わる）
            ServiceLocator.RegisterService<ITestService>(firstService);
            ServiceLocator.RegisterService<ITestService>(secondService);
            
            var retrievedService = ServiceLocator.GetService<ITestService>();
            
            // Assert
            Assert.AreEqual(1, ServiceLocator.GetServiceCount(), "重複登録でサービス数が増加している");
            Assert.AreEqual("Second", retrievedService.GetName(), "新しいサービスに置き換わっていない");
            Assert.AreEqual(2, retrievedService.GetValue(), "新しいサービスの値が正しくない");
        }

        /// <summary>
        /// 複数の異なる型のサービス登録テスト
        /// </summary>
        [Test]
        public void ServiceLocator_RegisterMultipleServices_StoresAllCorrectly()
        {
            // Arrange
            var testService = new TestServiceImpl("Multi1", 100);
            var anotherService = new AnotherServiceImpl(true);
            
            // Act
            ServiceLocator.RegisterService<ITestService>(testService);
            ServiceLocator.RegisterService<IAnotherService>(anotherService);
            
            // Assert
            Assert.AreEqual(2, ServiceLocator.GetServiceCount(), "複数サービスが正しく登録されていない");
            Assert.IsTrue(ServiceLocator.HasService<ITestService>(), "ITestServiceが登録されていない");
            Assert.IsTrue(ServiceLocator.HasService<IAnotherService>(), "IAnotherServiceが登録されていない");
        }

        #endregion

        #region Service Retrieval Tests

        /// <summary>
        /// サービスの基本的な取得動作テスト
        /// </summary>
        [Test]
        public void ServiceLocator_GetService_RetrievesCorrectService()
        {
            // Arrange
            var testService = new TestServiceImpl("GetTest", 456);
            ServiceLocator.RegisterService<ITestService>(testService);
            
            // Act
            var retrievedService = ServiceLocator.GetService<ITestService>();
            
            // Assert
            Assert.IsNotNull(retrievedService, "サービスが取得できない");
            Assert.AreEqual("GetTest", retrievedService.GetName(), "取得したサービスの名前が正しくない");
            Assert.AreEqual(456, retrievedService.GetValue(), "取得したサービスの値が正しくない");
            Assert.AreSame(testService, retrievedService, "同じサービスインスタンスが返されていない");
        }

        /// <summary>
        /// 未登録サービスの取得テスト
        /// </summary>
        [Test]
        public void ServiceLocator_GetService_UnregisteredService_ReturnsNull()
        {
            // Act - 未登録サービスの取得（警告は出るがnullが返される）
            var retrievedService = ServiceLocator.GetService<ITestService>();
            
            // Assert
            Assert.IsNull(retrievedService, "未登録サービスがnull以外を返している");
        }

        /// <summary>
        /// 必須サービス取得（RequireService）の正常動作テスト
        /// </summary>
        [Test]
        public void ServiceLocator_RequireService_ReturnsService()
        {
            // Arrange
            var testService = new TestServiceImpl("Required", 789);
            ServiceLocator.RegisterService<ITestService>(testService);
            
            // Act
            var retrievedService = ServiceLocator.RequireService<ITestService>();
            
            // Assert
            Assert.IsNotNull(retrievedService, "RequireServiceがnullを返した");
            Assert.AreEqual("Required", retrievedService.GetName(), "RequireServiceで取得したサービスが正しくない");
        }

        /// <summary>
        /// 必須サービス取得（RequireService）の例外スローテスト
        /// </summary>
        [Test]
        public void ServiceLocator_RequireService_UnregisteredService_ThrowsException()
        {
            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() => ServiceLocator.RequireService<ITestService>());
            Assert.That(exception.Message, Contains.Substring("ITestService"), "例外メッセージに型名が含まれていない");
        }

        #endregion

        #region Factory Method Tests

        /// <summary>
        /// ファクトリメソッドの登録と実行テスト
        /// </summary>
        [Test]
        public void ServiceLocator_RegisterFactory_CreatesServiceLazily()
        {
            // Arrange
            bool factoryWasCalled = false;
            Func<ITestService> factory = () => {
                factoryWasCalled = true;
                return new TestServiceImpl("FromFactory", 999);
            };
            
            // Act - ファクトリ登録
            ServiceLocator.RegisterFactory<ITestService>(factory);
            
            // Assert - まだファクトリは呼ばれていない
            Assert.IsFalse(factoryWasCalled, "ファクトリが登録時点で実行されている");
            Assert.IsTrue(ServiceLocator.HasService<ITestService>(), "ファクトリが登録されているべき");
            
            // Act - サービス取得（ファクトリ実行）
            var service = ServiceLocator.GetService<ITestService>();
            
            // Assert - ファクトリが実行され、サービスが作成されている
            Assert.IsTrue(factoryWasCalled, "サービス取得時にファクトリが実行されていない");
            Assert.IsNotNull(service, "ファクトリからサービスが作成されていない");
            Assert.AreEqual("FromFactory", service.GetName(), "ファクトリから作成されたサービスが正しくない");
            Assert.AreEqual(999, service.GetValue(), "ファクトリから作成されたサービスの値が正しくない");
        }

        /// <summary>
        /// ファクトリで作成されたサービスのキャッシュ動作テスト
        /// </summary>
        [Test]
        public void ServiceLocator_Factory_CachesCreatedService()
        {
            // Arrange
            int factoryCallCount = 0;
            Func<ITestService> factory = () => {
                factoryCallCount++;
                return new TestServiceImpl($"Cached_{factoryCallCount}", factoryCallCount * 10);
            };
            
            ServiceLocator.RegisterFactory<ITestService>(factory);
            
            // Act - 複数回サービス取得
            var service1 = ServiceLocator.GetService<ITestService>();
            var service2 = ServiceLocator.GetService<ITestService>();
            var service3 = ServiceLocator.GetService<ITestService>();
            
            // Assert - ファクトリは1回のみ実行され、同じインスタンスが返される
            Assert.AreEqual(1, factoryCallCount, "ファクトリが複数回実行されている");
            Assert.AreSame(service1, service2, "キャッシュされたサービスが同じインスタンスでない");
            Assert.AreSame(service2, service3, "キャッシュされたサービスが同じインスタンスでない");
            Assert.AreEqual("Cached_1", service1.GetName(), "キャッシュされたサービスの名前が正しくない");
        }

        /// <summary>
        /// 複雑なオブジェクト作成のファクトリテスト
        /// </summary>
        [Test]
        public void ServiceLocator_Factory_ComplexObjectCreation()
        {
            // Arrange
            Func<ITestService> complexFactory = () => new ComplexService("TestPrefix", 5);
            
            // Act
            ServiceLocator.RegisterFactory<ITestService>(complexFactory);
            var service = ServiceLocator.GetService<ITestService>();
            
            // Assert
            Assert.IsNotNull(service, "複雑なファクトリからサービスが作成されていない");
            Assert.AreEqual("TestPrefix_Complex", service.GetName(), "複雑なファクトリで作成されたサービスの名前が正しくない");
            Assert.AreEqual(500, service.GetValue(), "複雑なファクトリで作成されたサービスの値が正しくない");
        }

        /// <summary>
        /// ファクトリでnullを返すケースのエラーハンドリングテスト
        /// </summary>
        [Test]
        public void ServiceLocator_Factory_ReturnsNull_HandlesGracefully()
        {
            // Arrange
            Func<ITestService> nullFactory = () => null;
            ServiceLocator.RegisterFactory<ITestService>(nullFactory);
            
            // Act - nullを返すファクトリ（警告は出るがnullが返される）
            var service = ServiceLocator.GetService<ITestService>();
            
            // Assert
            Assert.IsNull(service, "nullを返すファクトリでnull以外が返されている");
        }

        #endregion

        #region Service Management Tests

        /// <summary>
        /// サービスの登録解除テスト
        /// </summary>
        [Test]
        public void ServiceLocator_UnregisterService_RemovesService()
        {
            // Arrange
            var testService = new TestServiceImpl("Unregister", 111);
            ServiceLocator.RegisterService<ITestService>(testService);
            Assert.IsTrue(ServiceLocator.HasService<ITestService>(), "事前条件：サービスが登録されているべき");
            
            // Act
            ServiceLocator.UnregisterService<ITestService>();
            
            // Assert
            Assert.IsFalse(ServiceLocator.HasService<ITestService>(), "サービスが登録解除されていない");
            Assert.AreEqual(0, ServiceLocator.GetServiceCount(), "サービス数が減少していない");
            
            // 登録解除後の取得確認（警告は出るがnullが返される）
            var retrievedService = ServiceLocator.GetService<ITestService>();
            Assert.IsNull(retrievedService, "登録解除後にサービスが取得できてしまう");
        }

        /// <summary>
        /// 全サービスクリアテスト
        /// </summary>
        [Test]
        public void ServiceLocator_Clear_RemovesAllServices()
        {
            // Arrange
            var testService = new TestServiceImpl("Clear1", 1);
            var anotherService = new AnotherServiceImpl(true);
            ServiceLocator.RegisterService<ITestService>(testService);
            ServiceLocator.RegisterService<IAnotherService>(anotherService);
            Assert.AreEqual(2, ServiceLocator.GetServiceCount(), "事前条件：2つのサービスが登録されているべき");
            
            // Act
            ServiceLocator.Clear();
            
            // Assert
            Assert.AreEqual(0, ServiceLocator.GetServiceCount(), "全サービスがクリアされていない");
            Assert.IsFalse(ServiceLocator.HasService<ITestService>(), "ITestServiceがクリアされていない");
            Assert.IsFalse(ServiceLocator.HasService<IAnotherService>(), "IAnotherServiceがクリアされていない");
        }

        /// <summary>
        /// サービス存在確認（HasService/IsServiceRegistered）テスト
        /// </summary>
        [Test]
        public void ServiceLocator_HasService_ReturnsCorrectStatus()
        {
            // Arrange
            var testService = new TestServiceImpl("Exist", 222);
            
            // Act & Assert - 未登録状態
            Assert.IsFalse(ServiceLocator.HasService<ITestService>(), "未登録サービスでtrueが返されている");
            Assert.IsFalse(ServiceLocator.IsServiceRegistered<ITestService>(), "IsServiceRegisteredが正しく動作していない");
            
            // Act & Assert - 登録後
            ServiceLocator.RegisterService<ITestService>(testService);
            Assert.IsTrue(ServiceLocator.HasService<ITestService>(), "登録済みサービスでfalseが返されている");
            Assert.IsTrue(ServiceLocator.IsServiceRegistered<ITestService>(), "IsServiceRegisteredが正しく動作していない");
            
            // Act & Assert - ファクトリ登録
            ServiceLocator.Clear();
            ServiceLocator.RegisterFactory<ITestService>(() => new TestServiceImpl("Factory", 333));
            Assert.IsTrue(ServiceLocator.HasService<ITestService>(), "ファクトリ登録されたサービスでfalseが返されている");
            Assert.IsTrue(ServiceLocator.IsServiceRegistered<ITestService>(), "ファクトリ登録でIsServiceRegisteredが正しく動作していない");
        }

        #endregion

        #region Thread Safety and Error Handling Tests

        /// <summary>
        /// nullサービス登録のエラーハンドリングテスト
        /// </summary>
        [Test]
        public void ServiceLocator_RegisterService_NullService_HandlesGracefully()
        {
            // Act & Assert - nullサービスの登録で例外が発生しないことを確認
            Assert.DoesNotThrow(() => ServiceLocator.RegisterService<ITestService>(null), 
                "nullサービスの登録で例外が発生");
            
            // サービス数に変化がないことを確認
            Assert.AreEqual(1, ServiceLocator.GetServiceCount(), "nullサービス登録でサービス数が変化している");
            
            // null値が格納されていることを確認（警告は出るがnullが返される）
            var service = ServiceLocator.GetService<ITestService>();
            Assert.IsNull(service, "null登録されたサービスがnull以外を返している");
        }

        /// <summary>
        /// 複数操作の連続実行テスト
        /// </summary>
        [Test]
        public void ServiceLocator_MultipleOperations_WorkCorrectly()
        {
            // Arrange & Act - 複数の操作を連続実行
            var service1 = new TestServiceImpl("Multi1", 1);
            var service2 = new AnotherServiceImpl(false);
            
            ServiceLocator.RegisterService<ITestService>(service1);
            ServiceLocator.RegisterFactory<IAnotherService>(() => service2);
            
            var retrieved1 = ServiceLocator.GetService<ITestService>();
            var retrieved2 = ServiceLocator.GetService<IAnotherService>();
            
            ServiceLocator.UnregisterService<ITestService>();
            
            var retrieved3 = ServiceLocator.GetService<ITestService>();
            var retrieved4 = ServiceLocator.GetService<IAnotherService>();
            
            // Assert
            Assert.IsNotNull(retrieved1, "最初のサービス取得が失敗");
            Assert.IsNotNull(retrieved2, "ファクトリサービス取得が失敗");
            Assert.IsNull(retrieved3, "登録解除後のサービス取得でnullが返されていない");
            Assert.IsNotNull(retrieved4, "ファクトリサービスが削除されている");
            Assert.AreEqual(1, ServiceLocator.GetServiceCount(), "最終的なサービス数が正しくない");
        }

        #endregion

        #region Debug and Logging Tests

        /// <summary>
        /// デバッグ用サービス一覧出力テスト
        /// </summary>
        [Test]
        public void ServiceLocator_LogAllServices_OutputsCorrectInformation()
        {
            // Arrange
            var testService = new TestServiceImpl("LogTest", 444);
            var anotherService = new AnotherServiceImpl(true);
            
            ServiceLocator.RegisterService<ITestService>(testService);
            ServiceLocator.RegisterFactory<IAnotherService>(() => anotherService);
            
            // Act & Assert - ログ出力が例外を起こさないことを確認
            Assert.DoesNotThrow(() => ServiceLocator.LogAllServices(), 
                "LogAllServicesで例外が発生");
        }

        /// <summary>
        /// サービス数カウントの正確性テスト
        /// </summary>
        [Test]
        public void ServiceLocator_ServiceCount_AccurateTracking()
        {
            // Assert - 初期状態
            Assert.AreEqual(0, ServiceLocator.GetServiceCount(), "初期状態のサービス数が0でない");
            
            // Act & Assert - 段階的にサービスを追加
            ServiceLocator.RegisterService<ITestService>(new TestServiceImpl());
            Assert.AreEqual(1, ServiceLocator.GetServiceCount(), "1つ目のサービス追加後のカウントが正しくない");
            
            ServiceLocator.RegisterFactory<IAnotherService>(() => new AnotherServiceImpl());
            Assert.AreEqual(2, ServiceLocator.GetServiceCount(), "ファクトリ追加後のカウントが正しくない");
            
            ServiceLocator.UnregisterService<ITestService>();
            Assert.AreEqual(1, ServiceLocator.GetServiceCount(), "サービス削除後のカウントが正しくない");
            
            ServiceLocator.Clear();
            Assert.AreEqual(0, ServiceLocator.GetServiceCount(), "全クリア後のカウントが正しくない");
        }

        #endregion
    }
}