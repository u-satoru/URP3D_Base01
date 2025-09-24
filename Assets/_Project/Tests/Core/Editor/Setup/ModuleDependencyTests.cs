using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;

namespace asterivo.Unity60.Tests.Core.Editor.Setup
{
    /// <summary>
    /// TASK-003.5: 依存関係解決システムのテストスイート
    /// </summary>
    [TestFixture]
    public class ModuleDependencyTests
    {
        private List<TestModule> allModules;

        /// <summary>
        /// テスト用のモジュール定義
        /// </summary>
        private class TestModule
        {
            public string Name { get; set; }
            public List<string> Dependencies { get; set; } = new List<string>();
        }

        /// <summary>
        /// テスト用の依存関係解決クラス（スタブ）
        /// </summary>
        private class DependencyResolver
        {
            private readonly Dictionary<string, TestModule> moduleMap;

            public DependencyResolver(IEnumerable<TestModule> modules)
            {
                moduleMap = modules.ToDictionary(m => m.Name);
            }

            public List<string> Resolve(IEnumerable<string> selectedModules)
            {
                var resolved = new HashSet<string>();
                foreach (var moduleName in selectedModules)
                {
                    ResolveDependenciesRecursive(moduleName, resolved, new HashSet<string>());
                }
                return resolved.ToList();
            }

            private void ResolveDependenciesRecursive(string moduleName, HashSet<string> resolved, HashSet<string> visiting)
            {
                if (!moduleMap.ContainsKey(moduleName)) return; // 存在しないモジュールは無視
                if (visiting.Contains(moduleName)) throw new System.Exception($"Circular dependency detected: {moduleName}");
                if (resolved.Contains(moduleName)) return;

                visiting.Add(moduleName);

                foreach (var dependency in moduleMap[moduleName].Dependencies)
                {
                    ResolveDependenciesRecursive(dependency, resolved, visiting);
                }

                visiting.Remove(moduleName);
                resolved.Add(moduleName);
            }
        }

        [SetUp]
        public void SetUp()
        {
            // テスト用のモジュールリストを定義
            allModules = new List<TestModule>
            {
                new TestModule { Name = "Core" },
                new TestModule { Name = "Audio System", Dependencies = { "Core" } },
                new TestModule { Name = "Stealth Audio", Dependencies = { "Audio System" } },
                new TestModule { Name = "Localization" },
                new TestModule { Name = "UI System", Dependencies = { "Localization", "Core" } },
                new TestModule { Name = "Analytics" },
                new TestModule { Name = "Advanced AI", Dependencies = { "Core" } },
                // 循環参照テスト用
                new TestModule { Name = "ModuleA", Dependencies = { "ModuleB" } },
                new TestModule { Name = "ModuleB", Dependencies = { "ModuleA" } }
            };
        }

        [Test]
        public void DependencyResolver_SingleModule_ResolvesCorrectly()
        {
            // Arrange
            var resolver = new DependencyResolver(allModules);
            var selection = new List<string> { "Audio System" };

            // Act
            var resolved = resolver.Resolve(selection);

            // Assert
            Assert.AreEqual(2, resolved.Count);
            Assert.Contains("Core", resolved);
            Assert.Contains("Audio System", resolved);
        }

        [Test]
        public void DependencyResolver_TransitiveDependencies_ResolvesCorrectly()
        {
            // Arrange
            var resolver = new DependencyResolver(allModules);
            var selection = new List<string> { "Stealth Audio" };

            // Act
            var resolved = resolver.Resolve(selection);

            // Assert
            Assert.AreEqual(3, resolved.Count);
            Assert.Contains("Core", resolved);
            Assert.Contains("Audio System", resolved);
            Assert.Contains("Stealth Audio", resolved);
        }

        [Test]
        public void DependencyResolver_MultipleModules_ResolvesAllDependencies()
        {
            // Arrange
            var resolver = new DependencyResolver(allModules);
            var selection = new List<string> { "UI System", "Stealth Audio" };

            // Act
            var resolved = resolver.Resolve(selection);

            // Assert
            Assert.AreEqual(4, resolved.Count);
            Assert.Contains("Core", resolved);
            Assert.Contains("Localization", resolved);
            Assert.Contains("UI System", resolved);
            Assert.Contains("Audio System", resolved);
            Assert.Contains("Stealth Audio", resolved);
        }

        [Test]
        public void DependencyResolver_NoDependencies_ReturnsSelf()
        {
            // Arrange
            var resolver = new DependencyResolver(allModules);
            var selection = new List<string> { "Analytics" };

            // Act
            var resolved = resolver.Resolve(selection);

            // Assert
            Assert.AreEqual(1, resolved.Count);
            Assert.Contains("Analytics", resolved);
        }

        [Test]
        public void DependencyResolver_CircularDependency_ThrowsException()
        {
            // Arrange
            var resolver = new DependencyResolver(allModules);
            var selection = new List<string> { "ModuleA" };

            // Act & Assert
            Assert.Throws<System.Exception>(() => resolver.Resolve(selection));
        }
    }
}
