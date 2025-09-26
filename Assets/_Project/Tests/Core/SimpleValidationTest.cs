using NUnit.Framework;
using asterivo.Unity60.Core;
using asterivo.Unity60.Core;
using asterivo.Unity60.Core.Events;
using asterivo.Unity60.Core.Commands;
using asterivo.Unity60.Core.Debug;

namespace asterivo.Unity60.Tests.Core
{
    /// <summary>
    /// Core螻､縺ｮ蝓ｺ譛ｬ逧・↑讖溯・縺梧ｭ｣縺励￥蜍穂ｽ懊☆繧九％縺ｨ繧呈､懆ｨｼ縺吶ｋ邁｡蜊倥↑繝・せ繝・
    /// Phase 2.3縺ｮ讀懆ｨｼ逕ｨ
    /// </summary>
    [TestFixture]
    public class SimpleValidationTest
    {
        [Test]
        public void Core_NamespaceExists()
        {
            // Core螻､縺ｮ蜷榊燕遨ｺ髢薙′蟄伜惠縺吶ｋ縺薙→繧堤｢ｺ隱・
            Assert.IsNotNull(typeof(ServiceLocator));
            Assert.IsNotNull(typeof(GameEvent));
            Assert.IsNotNull(typeof(ICommand));
            Assert.IsNotNull(typeof(ProjectDebug));
        }

        [Test]
        public void ServiceLocator_BasicOperations()
        {
            // ServiceLocator縺ｮ蝓ｺ譛ｬ逧・↑謫堺ｽ懊′蜿ｯ閭ｽ縺狗｢ｺ隱・
            ServiceLocator.Clear();

            // 繧ｵ繝ｼ繝薙せ縺ｮ逋ｻ骭ｲ縺ｨ蜿門ｾ・
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
            // ProjectDebug縺後Ο繧ｰ蜃ｺ蜉帛庄閭ｽ縺狗｢ｺ隱・
            Assert.DoesNotThrow(() => {
                ProjectDebug.Log("Test message");
                ProjectDebug.LogWarning("Test warning");
                ProjectDebug.LogError("Test error");
            });
        }

        // 繝・せ繝育畑縺ｮ繧､繝ｳ繧ｿ繝ｼ繝輔ぉ繝ｼ繧ｹ縺ｨ螳溯｣・
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


