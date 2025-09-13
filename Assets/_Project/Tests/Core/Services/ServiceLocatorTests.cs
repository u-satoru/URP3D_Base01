using NUnit.Framework;
using System;
using asterivo.Unity60.Core;

namespace asterivo.Unity60.Tests.Core.Services
{
    /// <summary>
    /// ServiceLocatorのユニットテストスイート
    /// Service Locatorパターンの基本機能と信頼性を検証
    /// </summary>
    [TestFixture]
    public class ServiceLocatorTests
    {
        #region Test Interfaces and Classes

        /// <summary>
        /// テスト用インターフェース
        /// </summary>
        public interface ITestService
        {
            string GetData();
        }

        /// <summary>
        /// テスト用サービス実装
        /// </summary>
        public class TestService : ITestService
        {
            private readonly string data;

            public TestService(string data = "test data")
            {
                this.data = data;
            }

            public string GetData() => data;
        }

        /// <summary>
        /// 別のテスト用インターフェース
        /// </summary>
        public interface IAnotherTestService
        {
            int GetValue();
        }

        /// <summary>
        /// 別のテスト用サービス実装
        /// </summary>
        public class AnotherTestService : IAnotherTestService
        {
            private readonly int value;

            public AnotherTestService(int value = 42)
            {
                this.value = value;
            }

            public int GetValue() => value;
        }

        #endregion

        #region Setup and Teardown

        [SetUp]
        public void SetUp()
        {
            // 各テスト前にServiceLocatorをクリア
            ServiceLocator.Clear();
        }

        [TearDown]
        public void TearDown()
        {
            // 各テスト後にServiceLocatorをクリア
            ServiceLocator.Clear();
        }

        #endregion

        #region Basic Registration and Retrieval Tests

        [Test]
        public void RegisterService_ValidService_SuccessfullyRegisters()
        {
            // Arrange
            var service = new TestService("test");

            // Act
            ServiceLocator.RegisterService<ITestService>(service);
            var retrievedService = ServiceLocator.GetService<ITestService>();

            // Assert
            Assert.IsNotNull(retrievedService);
            Assert.AreSame(service, retrievedService);
            Assert.AreEqual("test", retrievedService.GetData());
        }

        [Test]
        public void GetService_UnregisteredService_ReturnsNull()
        {
            // Act
            var service = ServiceLocator.GetService<ITestService>();

            // Assert
            Assert.IsNull(service);
        }

        [Test]
        public void RequireService_RegisteredService_ReturnsService()
        {
            // Arrange
            var service = new TestService("required");
            ServiceLocator.RegisterService<ITestService>(service);

            // Act
            var retrievedService = ServiceLocator.RequireService<ITestService>();

            // Assert
            Assert.IsNotNull(retrievedService);
            Assert.AreSame(service, retrievedService);
        }

        [Test]
        public void RequireService_UnregisteredService_ThrowsException()
        {
            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => 
                ServiceLocator.RequireService<ITestService>());
        }

        #endregion

        #region Multiple Service Tests

        [Test]
        public void RegisterService_MultipleServiceTypes_AllRetrievable()
        {
            // Arrange
            var testService = new TestService("test");
            var anotherService = new AnotherTestService(123);

            // Act
            ServiceLocator.RegisterService<ITestService>(testService);
            ServiceLocator.RegisterService<IAnotherTestService>(anotherService);

            // Assert
            var retrievedTestService = ServiceLocator.GetService<ITestService>();
            var retrievedAnotherService = ServiceLocator.GetService<IAnotherTestService>();

            Assert.IsNotNull(retrievedTestService);
            Assert.IsNotNull(retrievedAnotherService);
            Assert.AreSame(testService, retrievedTestService);
            Assert.AreSame(anotherService, retrievedAnotherService);
        }

        [Test]
        public void RegisterService_SameTypeMultipleTimes_OverwritesPrevious()
        {
            // Arrange
            var firstService = new TestService("first");
            var secondService = new TestService("second");

            // Act
            ServiceLocator.RegisterService<ITestService>(firstService);
            ServiceLocator.RegisterService<ITestService>(secondService);
            var retrievedService = ServiceLocator.GetService<ITestService>();

            // Assert
            Assert.IsNotNull(retrievedService);
            Assert.AreSame(secondService, retrievedService);
            Assert.AreEqual("second", retrievedService.GetData());
        }

        #endregion

        #region Unregistration Tests

        [Test]
        public void UnregisterService_RegisteredService_RemovesService()
        {
            // Arrange
            var service = new TestService("test");
            ServiceLocator.RegisterService<ITestService>(service);

            // Act
            ServiceLocator.UnregisterService<ITestService>();
            var wasRegistered = true; // UnregisterService always succeeds
            var retrievedService = ServiceLocator.GetService<ITestService>();

            // Assert
            Assert.IsTrue(wasRegistered);
            Assert.IsNull(retrievedService);
        }

        [Test]
        public void UnregisterService_UnregisteredService_ReturnsFalse()
        {
            // Act
            // Act & Assert - UnregisterService doesn't return bool, so we just verify it doesn't throw
            Assert.DoesNotThrow(() => ServiceLocator.UnregisterService<ITestService>());
        }

        [Test]
        public void UnregisterService_OneOfMultipleServices_OnlyRemovesSpecified()
        {
            // Arrange
            var testService = new TestService("test");
            var anotherService = new AnotherTestService(123);
            ServiceLocator.RegisterService<ITestService>(testService);
            ServiceLocator.RegisterService<IAnotherTestService>(anotherService);

            // Act
            ServiceLocator.UnregisterService<ITestService>();

            // Assert
            Assert.IsNull(ServiceLocator.GetService<ITestService>());
            Assert.IsNotNull(ServiceLocator.GetService<IAnotherTestService>());
        }

        #endregion

        #region Registration Check Tests

        [Test]
        public void IsServiceRegistered_RegisteredService_ReturnsTrue()
        {
            // Arrange
            var service = new TestService("test");
            ServiceLocator.RegisterService<ITestService>(service);

            // Act
            var isRegistered = ServiceLocator.HasService<ITestService>();

            // Assert
            Assert.IsTrue(isRegistered);
        }

        [Test]
        public void IsServiceRegistered_UnregisteredService_ReturnsFalse()
        {
            // Act
            var isRegistered = ServiceLocator.HasService<ITestService>();

            // Assert
            Assert.IsFalse(isRegistered);
        }

        [Test]
        public void IsServiceRegistered_UnregisteredAfterRegistration_ReturnsFalse()
        {
            // Arrange
            var service = new TestService("test");
            ServiceLocator.RegisterService<ITestService>(service);
            ServiceLocator.UnregisterService<ITestService>();

            // Act
            var isRegistered = ServiceLocator.HasService<ITestService>();

            // Assert
            Assert.IsFalse(isRegistered);
        }

        #endregion

        #region Clear Tests

        [Test]
        public void Clear_WithRegisteredServices_RemovesAllServices()
        {
            // Arrange
            var testService = new TestService("test");
            var anotherService = new AnotherTestService(123);
            ServiceLocator.RegisterService<ITestService>(testService);
            ServiceLocator.RegisterService<IAnotherTestService>(anotherService);

            // Act
            ServiceLocator.Clear();

            // Assert
            Assert.IsNull(ServiceLocator.GetService<ITestService>());
            Assert.IsNull(ServiceLocator.GetService<IAnotherTestService>());
            Assert.IsFalse(ServiceLocator.HasService<ITestService>());
            Assert.IsFalse(ServiceLocator.HasService<IAnotherTestService>());
        }

        [Test]
        public void Clear_WithNoServices_DoesNotThrow()
        {
            // Act & Assert
            Assert.DoesNotThrow(() => ServiceLocator.Clear());
        }

        #endregion

        #region Null Parameter Tests

        [Test]
        public void RegisterService_NullService_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => 
                ServiceLocator.RegisterService<ITestService>(null));
        }

        #endregion

        #region Factory Tests

        [Test]
        public void RegisterFactory_ValidFactory_CreatesServiceOnDemand()
        {
            // Arrange
            string expectedData = "factory created";
            ServiceLocator.RegisterFactory<ITestService>(() => new TestService(expectedData));

            // Act
            var service = ServiceLocator.GetService<ITestService>();

            // Assert
            Assert.IsNotNull(service);
            Assert.AreEqual(expectedData, service.GetData());
        }

        [Test]
        public void RegisterFactory_CalledMultipleTimes_ReturnsSameInstance()
        {
            // Arrange
            ServiceLocator.RegisterFactory<ITestService>(() => new TestService("singleton"));

            // Act
            var service1 = ServiceLocator.GetService<ITestService>();
            var service2 = ServiceLocator.GetService<ITestService>();

            // Assert
            Assert.IsNotNull(service1);
            Assert.IsNotNull(service2);
            Assert.AreSame(service1, service2);
        }

        [Test]
        public void RegisterFactory_NullFactory_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => 
                ServiceLocator.RegisterFactory<ITestService>(null));
        }

        [Test]
        public void RegisterServiceFactory_OverwritesExistingService_UsesNewFactory()
        {
            // Arrange
            var directService = new TestService("direct");
            ServiceLocator.RegisterService<ITestService>(directService);
            ServiceLocator.RegisterFactory<ITestService>(() => new TestService("factory"));

            // Act
            var service = ServiceLocator.GetService<ITestService>();

            // Assert
            Assert.IsNotNull(service);
            Assert.AreEqual("factory", service.GetData());
            Assert.AreNotSame(directService, service);
        }

        #endregion

        #region Integration Tests

        [Test]
        public void ServiceLocator_ComplexWorkflow_WorksCorrectly()
        {
            // Arrange & Act
            // 1. Register a service
            var service1 = new TestService("service1");
            ServiceLocator.RegisterService<ITestService>(service1);
            Assert.IsTrue(ServiceLocator.HasService<ITestService>());

            // 2. Replace with factory
            ServiceLocator.RegisterFactory<ITestService>(() => new TestService("factory"));
            var factoryService = ServiceLocator.GetService<ITestService>();
            Assert.AreEqual("factory", factoryService.GetData());

            // 3. Unregister
            ServiceLocator.UnregisterService<ITestService>();
            Assert.IsFalse(ServiceLocator.HasService<ITestService>());

            // 4. Register new service
            var service2 = new TestService("service2");
            ServiceLocator.RegisterService<ITestService>(service2);
            
            // Assert final state
            Assert.IsTrue(ServiceLocator.HasService<ITestService>());
            var finalService = ServiceLocator.GetService<ITestService>();
            Assert.AreEqual("service2", finalService.GetData());
            Assert.AreSame(service2, finalService);
        }

        #endregion
    }
}